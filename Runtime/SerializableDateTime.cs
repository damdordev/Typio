using System;
using UnityEngine;

namespace Damdor.Typio
{
    /// <summary>A Unity-serializable wrapper that preserves a date's ticks and <see cref="DateTime.Kind"/>.</summary>
    /// <remarks>
    /// The default value represents <see cref="System.DateTime.MinValue"/> with
    /// <see cref="DateTimeKind.Unspecified"/> kind. Data containing only ticks is read with unspecified kind.
    /// Local dates retain their wall-clock ticks and kind, but do not store a time zone or UTC offset.
    /// </remarks>
    [Serializable]
    public struct SerializableDateTime
    {
        [SerializeField]
        private long ticks;

        [SerializeField]
        private DateTimeKind kind;

        /// <summary>Gets or sets the date, preserving its ticks and kind.</summary>
        /// <exception cref="ArgumentOutOfRangeException">Serialized ticks or kind are outside the valid range.</exception>
        public DateTime DateTime
        {
            get => new(ticks, kind);
            set
            {
                ticks = value.Ticks;
                kind = value.Kind;
            }
        }

        /// <summary>Creates a serializable date from a <see cref="System.DateTime"/> value.</summary>
        /// <param name="dateTime">The date whose ticks and kind will be stored.</param>
        public SerializableDateTime(DateTime dateTime)
        {
            ticks = dateTime.Ticks;
            kind = dateTime.Kind;
        }

        /// <summary>Converts a serializable date to its stored <see cref="System.DateTime"/> value.</summary>
        /// <param name="sdt">The serializable date.</param>
        /// <returns>The date with its original ticks and kind.</returns>
        public static implicit operator DateTime(SerializableDateTime sdt) => sdt.DateTime;

        /// <summary>Wraps a <see cref="System.DateTime"/> for Unity serialization.</summary>
        /// <param name="dt">The date to wrap.</param>
        /// <returns>A serializable date with the same ticks and kind.</returns>
        public static implicit operator SerializableDateTime(DateTime dt) => new(dt);
    }
}
