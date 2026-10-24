using System;
using UnityEngine;

namespace Damdor.Typio
{
    [Serializable]
    public struct SerializableTimeSpan : ISerializationCallbackReceiver
    {
        [SerializeField] private long ticks;
        private TimeSpan timeSpan;

        public TimeSpan TimeSpan
        {
            get => timeSpan;
            set => timeSpan = value;
        }

        public SerializableTimeSpan(TimeSpan timeSpan)
        {
            this.timeSpan = timeSpan;
            ticks = timeSpan.Ticks;
        }
        
        public void OnBeforeSerialize()
        {
            ticks = timeSpan.Ticks;
        }

        public void OnAfterDeserialize()
        {
            timeSpan = new TimeSpan(ticks);
        }
        
        public static implicit operator TimeSpan(SerializableTimeSpan serializableTimeSpan) => serializableTimeSpan.TimeSpan;
        public static implicit operator SerializableTimeSpan(TimeSpan timeSpan) => new(timeSpan);
        
    }
}