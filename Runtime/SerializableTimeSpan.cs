using System;
using UnityEngine;

namespace Damdor.Typio
{
    /// <summary>A Unity-serializable wrapper for a <see cref="System.TimeSpan"/> with tick precision.</summary>
    /// <remarks>Supports the entire <see cref="System.TimeSpan"/> range, including negative durations. The default value is zero.</remarks>
    [Serializable]
    public struct SerializableTimeSpan
    {
        [SerializeField] private long ticks;

        /// <summary>Gets or sets the duration stored in the serialized ticks field.</summary>
        public TimeSpan TimeSpan
        {
            get => new TimeSpan(ticks);
            set => ticks = value.Ticks;
        }

        /// <summary>Creates a serializable duration from a <see cref="System.TimeSpan"/> value.</summary>
        /// <param name="timeSpan">The duration whose ticks will be stored.</param>
        public SerializableTimeSpan(TimeSpan timeSpan)
        {
            ticks = timeSpan.Ticks;
        }

        /// <summary>Converts a serializable duration to its stored <see cref="System.TimeSpan"/> value.</summary>
        /// <param name="serializableTimeSpan">The serializable duration.</param>
        /// <returns>The duration with the same ticks.</returns>
        public static implicit operator TimeSpan(SerializableTimeSpan serializableTimeSpan) => serializableTimeSpan.TimeSpan;

        /// <summary>Wraps a <see cref="System.TimeSpan"/> for Unity serialization.</summary>
        /// <param name="timeSpan">The duration to wrap.</param>
        /// <returns>A serializable duration with the same ticks.</returns>
        public static implicit operator SerializableTimeSpan(TimeSpan timeSpan) => new(timeSpan);
    }
}
