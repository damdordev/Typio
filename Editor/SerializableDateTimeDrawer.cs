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
            var dt = new DateTime(ticksProp.longValue);
        
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
            SerializableDateTimeDrawerMode.Milliseconds => ((long) dateTime.Subtract(DateTime.UnixEpoch).TotalMilliseconds).ToString(),
            SerializableDateTimeDrawerMode.Seconds => ((long) dateTime.Subtract(DateTime.UnixEpoch).TotalSeconds).ToString(),
            _ => throw new ArgumentOutOfRangeException()
        };

        private bool TryParse(string text, out DateTime result)
        {
            switch (mode)
            {
                case SerializableDateTimeDrawerMode.FullTime:
                    return DateTime.TryParseExact(text, Format, CultureInfo.InvariantCulture, DateTimeStyles.None,
                        out result);
                case SerializableDateTimeDrawerMode.Milliseconds:
                    if (long.TryParse(text, out var valueMilliseconds))
                    {
                        result = DateTime.UnixEpoch.AddMilliseconds(valueMilliseconds);
                        return true;
                    }

                    result = DateTime.MinValue;
                    return false;
                case SerializableDateTimeDrawerMode.Seconds:
                    if (long.TryParse(text, out var valueSeconds))
                    {
                        result = DateTime.UnixEpoch.AddSeconds(valueSeconds);
                        return true;
                    }

                    result = DateTime.MinValue;
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }
    }
}