using System;
using UnityEngine;

namespace Damdor.Typio
{
    [Serializable]
    public struct SerializableDateTime : ISerializationCallbackReceiver
    {
        [SerializeField]
        private long ticks;

        private DateTime dateTime;

        public DateTime DateTime
        {
            get => dateTime;
            set
            {
                dateTime = value;
                ticks = dateTime.Ticks;
            }
        }

        public SerializableDateTime(DateTime dateTime)
        {
            this.dateTime = dateTime;
            ticks = dateTime.Ticks;
        }

        public void OnBeforeSerialize()
        {
            ticks = dateTime.Ticks;
        }

        public void OnAfterDeserialize()
        {
            dateTime = new DateTime(ticks);
        }

        public static implicit operator DateTime(SerializableDateTime sdt) => sdt.DateTime;
        public static implicit operator SerializableDateTime(DateTime dt) => new(dt);
    }
}