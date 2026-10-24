using System;
using System.Globalization;
using System.Reflection;
using Damdor.Typio.Editor;
using NUnit.Framework;

namespace Damdor.Typio.Tests
{
    public class PropertyDrawerTests
    {
        private static readonly long[] TickValues = { long.MinValue, -1L, 0L, 1L, 123456789012345L, long.MaxValue };

        [TestCaseSource(nameof(TickValues))]
        public void DurationFormats_RoundTripWithTickPrecision(long ticks)
        {
            var drawer = new SerializableTimeSpanPropertyDrawer();
            foreach (SerializableTimeSpanDrawerMode mode in Enum.GetValues(typeof(SerializableTimeSpanDrawerMode)))
            {
                SetMode(drawer, mode);
                var text = Format(drawer, new TimeSpan(ticks));
                Assert.That(Parse<TimeSpan>(drawer, text, out var result), Is.True, mode.ToString());
                Assert.That(result.Ticks, Is.EqualTo(ticks), mode.ToString());
            }
        }

        [TestCase("pl-PL")]
        [TestCase("en-US")]
        public void DurationNumericInput_UsesInvariantCulture(string culture)
        {
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                var drawer = new SerializableTimeSpanPropertyDrawer();
                SetMode(drawer, SerializableTimeSpanDrawerMode.Seconds);
                Assert.That(Parse<TimeSpan>(drawer, "1.25", out var value), Is.True);
                Assert.That(value.Ticks, Is.EqualTo(12500000L));
                Assert.That(Format(drawer, value), Is.EqualTo("1.25"));
                Assert.That(Parse<TimeSpan>(drawer, "1,25", out _), Is.False);
            }
            finally
            {
                CultureInfo.CurrentCulture = previous;
            }
        }

        [TestCase("NaN")]
        [TestCase("Infinity")]
        [TestCase("1e100")]
        [TestCase("922337203685478")]
        [TestCase("79228162514264337593543950335")]
        [TestCase("")]
        public void DurationNumericInput_RejectsInvalidOrOverflowingValues(string text)
        {
            var drawer = new SerializableTimeSpanPropertyDrawer();
            SetMode(drawer, SerializableTimeSpanDrawerMode.Seconds);
            Assert.That(Parse<TimeSpan>(drawer, text, out _), Is.False);
        }

        [TestCase(SerializableDateTimeDrawerMode.Seconds, "253402300800")]
        [TestCase(SerializableDateTimeDrawerMode.Seconds, "-62135596801")]
        [TestCase(SerializableDateTimeDrawerMode.Milliseconds, "253402300800000")]
        [TestCase(SerializableDateTimeDrawerMode.Milliseconds, "-62135596800001")]
        [TestCase(SerializableDateTimeDrawerMode.Seconds, "9223372036854775807")]
        [TestCase(SerializableDateTimeDrawerMode.Seconds, "abc")]
        public void UnixInput_RejectsInvalidOrOutOfRangeValues(SerializableDateTimeDrawerMode mode, string text)
        {
            var drawer = new SerializableDateTimeDrawer();
            SetMode(drawer, mode);
            Assert.That(Parse<DateTime>(drawer, text, out _), Is.False);
        }

        [TestCase(SerializableDateTimeDrawerMode.Seconds, "0", 621355968000000000L)]
        [TestCase(SerializableDateTimeDrawerMode.Seconds, "-1", 621355967990000000L)]
        [TestCase(SerializableDateTimeDrawerMode.Seconds, "253402300799", 3155378975990000000L)]
        [TestCase(SerializableDateTimeDrawerMode.Milliseconds, "-62135596800000", 0L)]
        [TestCase(SerializableDateTimeDrawerMode.Milliseconds, "253402300799999", 3155378975999990000L)]
        public void UnixInput_ParsesUtcWithinRange(SerializableDateTimeDrawerMode mode, string text, long ticks)
        {
            var drawer = new SerializableDateTimeDrawer();
            SetMode(drawer, mode);
            Assert.That(Parse<DateTime>(drawer, text, out var result), Is.True);
            SerializableDateTimeTests.AssertDate(new DateTime(ticks, DateTimeKind.Utc), result);
            Assert.That(Format(drawer, result), Is.EqualTo(text));
        }

        [TestCase(SerializableDateTimeDrawerMode.Seconds)]
        [TestCase(SerializableDateTimeDrawerMode.Milliseconds)]
        public void UnixFormatting_FloorsFractionBeforeEpoch(SerializableDateTimeDrawerMode mode)
        {
            var drawer = new SerializableDateTimeDrawer();
            SetMode(drawer, mode);
            Assert.That(Format(drawer, new DateTime(621355967999999999L, DateTimeKind.Utc)), Is.EqualTo("-1"));
        }

        [Test]
        public void UnixFormatting_LocalAndUtcDescribeTheSameInstant()
        {
            var drawer = new SerializableDateTimeDrawer();
            SetMode(drawer, SerializableDateTimeDrawerMode.Seconds);
            var utc = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);
            Assert.That(Format(drawer, utc.ToLocalTime()), Is.EqualTo(Format(drawer, utc)));
        }

        // Exercise the parsing used by OnGUI without synthesizing IMGUI events.
        private static void SetMode(object drawer, object mode) =>
            drawer.GetType().GetField("mode", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(drawer, mode);

        private static string Format<T>(object drawer, T value) => (string)drawer.GetType()
            .GetMethod("ToString", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(T) }, null)
            .Invoke(drawer, new object[] { value });

        private static bool Parse<T>(object drawer, string text, out T value)
        {
            var args = new object[] { text, default(T) };
            var method = drawer.GetType().GetMethod("TryParse", BindingFlags.NonPublic | BindingFlags.Instance,
                null, new[] { typeof(string), typeof(T).MakeByRefType() }, null);
            var success = (bool)method.Invoke(drawer, args);
            value = (T)args[1];
            return success;
        }
    }
}
