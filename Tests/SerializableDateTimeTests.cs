using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Damdor.Typio.Tests
{
    public class SerializableDateTimeTests
    {
        private static IEnumerable<DateTime> Dates
        {
            get
            {
                foreach (var kind in new[] { DateTimeKind.Unspecified, DateTimeKind.Utc, DateTimeKind.Local })
                foreach (var ticks in new[] { 0L, 1L, 638448479991234567L, DateTime.MaxValue.Ticks })
                    yield return new DateTime(ticks, kind);
            }
        }

        [Test]
        public void DefaultValue_IsMinimumUnspecifiedDate()
        {
            AssertDate(DateTime.MinValue, default(SerializableDateTime).DateTime);
        }

        [TestCaseSource(nameof(Dates))]
        public void Constructor_PreservesTicksAndKind(DateTime expected)
        {
            AssertDate(expected, new SerializableDateTime(expected).DateTime);
        }

        [TestCaseSource(nameof(Dates))]
        public void Setter_PreservesTicksAndKindThroughCallbacks(DateTime expected)
        {
            var value = new SerializableDateTime(DateTime.Now);
            value.DateTime = expected;
            AssertDate(expected, value.DateTime);
        }

        [TestCaseSource(nameof(Dates))]
        public void ImplicitConversions_PreserveTicksAndKind(DateTime expected)
        {
            SerializableDateTime wrapped = expected;
            DateTime actual = wrapped;
            AssertDate(expected, actual);
        }

        [TestCaseSource(nameof(Dates))]
        public void JsonRoundTrip_PreservesTicksAndKind(DateTime expected)
        {
            var source = new Holder { value = expected };
            var restored = JsonUtility.FromJson<Holder>(JsonUtility.ToJson(source));
            AssertDate(expected, restored.value.DateTime);
        }

        [Test]
        public void LegacyJson_WithoutKind_RestoresUnspecifiedDate()
        {
            var restored = JsonUtility.FromJson<Holder>("{\"value\":{\"ticks\":638448479991234567}}");
            AssertDate(new DateTime(638448479991234567L), restored.value.DateTime);
        }

        [Test]
        public void StructCopy_CanBeChangedIndependently()
        {
            SerializableDateTime original = new DateTime(123L, DateTimeKind.Utc);
            var copy = original;
            copy.DateTime = new DateTime(456L, DateTimeKind.Local);
            AssertDate(new DateTime(123L, DateTimeKind.Utc), original.DateTime);
            AssertDate(new DateTime(456L, DateTimeKind.Local), copy.DateTime);
        }

        [Test]
        public void BoxedCallbacks_PreserveValue()
        {
            var expected = new DateTime(638448479991234567L, DateTimeKind.Utc);
            object boxed = new SerializableDateTime(expected);
            AssertDate(expected, ((SerializableDateTime)boxed).DateTime);
        }

        internal static void AssertDate(DateTime expected, DateTime actual)
        {
            Assert.That(actual.Ticks, Is.EqualTo(expected.Ticks));
            Assert.That(actual.Kind, Is.EqualTo(expected.Kind));
        }

        [Serializable]
        public class Holder
        {
            public SerializableDateTime value;
        }
    }
}
