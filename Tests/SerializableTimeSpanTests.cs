using System;
using NUnit.Framework;
using UnityEngine;

namespace Damdor.Typio.Tests
{
    public class SerializableTimeSpanTests
    {
        private static readonly long[] TickValues = { long.MinValue, -123456789012345L, -1L, 0L, 1L, 123456789012345L, long.MaxValue };

        [Test]
        public void DefaultValue_IsZero()
        {
            Assert.That(default(SerializableTimeSpan).TimeSpan, Is.EqualTo(TimeSpan.Zero));
        }

        [TestCaseSource(nameof(TickValues))]
        public void Constructor_PreservesTicks(long ticks)
        {
            Assert.That(new SerializableTimeSpan(new TimeSpan(ticks)).TimeSpan.Ticks, Is.EqualTo(ticks));
        }

        [TestCaseSource(nameof(TickValues))]
        public void Setter_PreservesTicksThroughCallbacks(long ticks)
        {
            var value = new SerializableTimeSpan(TimeSpan.FromDays(1));
            value.TimeSpan = new TimeSpan(ticks);
            Assert.That(value.TimeSpan.Ticks, Is.EqualTo(ticks));
        }

        [TestCaseSource(nameof(TickValues))]
        public void ImplicitConversions_PreserveTicks(long ticks)
        {
            SerializableTimeSpan wrapped = new TimeSpan(ticks);
            TimeSpan actual = wrapped;
            Assert.That(actual.Ticks, Is.EqualTo(ticks));
        }

        [TestCaseSource(nameof(TickValues))]
        public void JsonRoundTrip_PreservesTicks(long ticks)
        {
            var source = new Holder { value = new TimeSpan(ticks) };
            var restored = JsonUtility.FromJson<Holder>(JsonUtility.ToJson(source));
            Assert.That(restored.value.TimeSpan.Ticks, Is.EqualTo(ticks));
        }

        [Test]
        public void StructCopy_CanBeChangedIndependently()
        {
            SerializableTimeSpan original = TimeSpan.FromTicks(123);
            var copy = original;
            copy.TimeSpan = TimeSpan.FromTicks(456);
            Assert.That(original.TimeSpan.Ticks, Is.EqualTo(123));
            Assert.That(copy.TimeSpan.Ticks, Is.EqualTo(456));
        }

        [Test]
        public void Setter_UpdatesSerializedValueBeforeCallbacks()
        {
            var value = new SerializableTimeSpan(TimeSpan.FromSeconds(1));
            value.TimeSpan = TimeSpan.FromSeconds(2);
            Assert.That(value.TimeSpan, Is.EqualTo(TimeSpan.FromSeconds(2)));
        }

        [Serializable]
        public class Holder
        {
            public SerializableTimeSpan value;
        }
    }
}
