using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Damdor.Typio.Editor
{
    public enum SerializableDateTimeDrawerMode
    {
        FullTime,
        Milliseconds,
        Seconds
    }

    [CustomPropertyDrawer(typeof(SerializableDateTime))]
    public class SerializableDateTimeDrawer : PropertyDrawer
    {
        private const string Format = "yyyy-MM-dd HH:mm:ss";
        private SerializableDateTimeDrawerMode mode;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var ticksProp = property.FindPropertyRelative("ticks");
            var kindProp = property.FindPropertyRelative("kind");
            var dt = new DateTime(ticksProp.longValue, (DateTimeKind)kindProp.intValue);

            label = EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);

            var valueRect = new Rect(position.x + 90, position.y, position.width - 100, position.height);
            var modeRect = new Rect(position.x, position.y, 90, position.height);

            var text = ToString(dt);
            var newText = EditorGUI.DelayedTextField(valueRect, text);
            if (newText != text)
            {
                if (TryParse(newText, out dt))
                {
                    // Numeric modes describe a UTC instant; keep the field's existing kind.
                    if (mode != SerializableDateTimeDrawerMode.FullTime &&
                        (DateTimeKind)kindProp.intValue == DateTimeKind.Local)
                    {
                        dt = dt.ToLocalTime();
                    }
                    UpdateTicks(ticksProp, dt);
                }
            }
            mode = (SerializableDateTimeDrawerMode) EditorGUI.EnumPopup(modeRect, mode);

            EditorGUI.EndProperty();
        }

        private static void UpdateTicks(SerializedProperty prop, DateTime dt)
        {
            prop.longValue = dt.Ticks;
            prop.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        private string ToString(DateTime dateTime) => mode switch
        {
            SerializableDateTimeDrawerMode.FullTime => dateTime.ToString(Format, CultureInfo.InvariantCulture),
            SerializableDateTimeDrawerMode.Milliseconds => AsUtcOffset(dateTime).ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture),
            SerializableDateTimeDrawerMode.Seconds => AsUtcOffset(dateTime).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture),
            _ => throw new ArgumentOutOfRangeException()
        };

        private static DateTimeOffset AsUtcOffset(DateTime value) => new DateTimeOffset(
            value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : DateTime.SpecifyKind(value, DateTimeKind.Utc));

        private bool TryParse(string text, out DateTime result)
        {
            switch (mode)
            {
                case SerializableDateTimeDrawerMode.FullTime:
                    return DateTime.TryParseExact(text, Format, CultureInfo.InvariantCulture, DateTimeStyles.None,
                        out result);
                case SerializableDateTimeDrawerMode.Milliseconds:
                    return TryParseUnixTime(text, true, out result);
                case SerializableDateTimeDrawerMode.Seconds:
                    return TryParseUnixTime(text, false, out result);
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }

        private static bool TryParseUnixTime(string text, bool milliseconds, out DateTime result)
        {
            if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                try
                {
                    result = (milliseconds
                        ? DateTimeOffset.FromUnixTimeMilliseconds(value)
                        : DateTimeOffset.FromUnixTimeSeconds(value)).UtcDateTime;
                    return true;
                }
                catch (ArgumentOutOfRangeException)
                {
                    // Reject input outside DateTime's range without breaking the Inspector.
                }
            }

            result = DateTime.MinValue;
            return false;
        }
    }
}
