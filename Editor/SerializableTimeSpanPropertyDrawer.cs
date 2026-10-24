using System;
using System.Globalization;
using UnityEditor;
using UnityEngine;

namespace Damdor.Typio.Editor
{
    public enum SerializableTimeSpanDrawerMode
    {
        Seconds,
        FullTime,
        Milliseconds,
        Minutes,
        Hours,
        Days,
    }

    [CustomPropertyDrawer(typeof(SerializableTimeSpan))]
    public class SerializableTimeSpanPropertyDrawer : PropertyDrawer
    {
        private const string Format = "g";

        private SerializableTimeSpanDrawerMode mode;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var ticksProp = property.FindPropertyRelative("ticks");
            var timeSpan = new TimeSpan(ticksProp.longValue);

            label = EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);

            var valueRect = new Rect(position.x + 90, position.y, position.width - 100, position.height);
            var modeRect = new Rect(position.x, position.y, 90, position.height);

            var text = ToString(timeSpan);
            var newText = EditorGUI.DelayedTextField(valueRect, text);
            if (newText != text)
            {
                if (TryParse(newText, out timeSpan))
                {
                    UpdateTicks(ticksProp, timeSpan);
                }
            }

            mode = (SerializableTimeSpanDrawerMode)EditorGUI.EnumPopup(modeRect, mode);

            EditorGUI.EndProperty();
        }

        private void UpdateTicks(SerializedProperty prop, TimeSpan timeSpan)
        {
            prop.longValue = timeSpan.Ticks;
            prop.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        private string ToString(TimeSpan timeSpan) => mode switch
        {
            SerializableTimeSpanDrawerMode.FullTime => timeSpan.ToString(Format, CultureInfo.InvariantCulture),
            SerializableTimeSpanDrawerMode.Milliseconds => ((long)timeSpan.TotalMilliseconds).ToString(Format, CultureInfo.InvariantCulture),
            SerializableTimeSpanDrawerMode.Seconds => ((float)timeSpan.TotalSeconds).ToString(Format, CultureInfo.InvariantCulture),
            SerializableTimeSpanDrawerMode.Minutes => ((float)timeSpan.TotalMinutes).ToString(Format, CultureInfo.InvariantCulture),
            SerializableTimeSpanDrawerMode.Hours => ((float)timeSpan.TotalHours).ToString(Format, CultureInfo.InvariantCulture),
            SerializableTimeSpanDrawerMode.Days => ((float)timeSpan.TotalDays).ToString(Format, CultureInfo.InvariantCulture),
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };

        private bool TryParse(string text, out TimeSpan result) => mode switch
        {
            SerializableTimeSpanDrawerMode.FullTime => 
                TimeSpan.TryParseExact(text, Format, CultureInfo.InvariantCulture, TimeSpanStyles.None, out result),
            SerializableTimeSpanDrawerMode.Milliseconds => 
                TryParse(text, 1, out result),
            SerializableTimeSpanDrawerMode.Seconds => 
                TryParse(text, 1000, out result),
            SerializableTimeSpanDrawerMode.Minutes =>
                TryParse(text, 60 * 1000, out result),
            SerializableTimeSpanDrawerMode.Hours =>
                TryParse(text, 60 * 60 * 1000, out result),
            SerializableTimeSpanDrawerMode.Days =>
                TryParse(text, 24 * 60 * 60 * 1000, out result),
            _ =>
                throw new ArgumentOutOfRangeException(nameof(mode))
        };

        private static bool TryParse(string text, long millisecondMultiplier, out TimeSpan result)
        {
            if (float.TryParse(text, out var valueMilliseconds))
            {
                result = TimeSpan.FromMilliseconds(millisecondMultiplier * valueMilliseconds);
                return true;
            }
            result = TimeSpan.Zero;
            return false;
        }
        
    }
}