using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Damdor.Typio.Tests
{
    public class UnitySerializationTests
    {
        [Test]
        public void SerializedPropertyEdits_AreVisibleWithoutRestoringACache()
        {
            var asset = ScriptableObject.CreateInstance<TypioSerializationTestAsset>();
            try
            {
                asset.date = new DateTime(123L, DateTimeKind.Utc);
                asset.duration = TimeSpan.FromTicks(456);
                using (var serialized = new SerializedObject(asset))
                {
                    serialized.FindProperty("date").FindPropertyRelative("ticks").longValue = 789L;
                    serialized.FindProperty("duration").FindPropertyRelative("ticks").longValue = -987L;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }

                SerializableDateTimeTests.AssertDate(new DateTime(789L, DateTimeKind.Utc), asset.date.DateTime);
                Assert.That(asset.duration.TimeSpan.Ticks, Is.EqualTo(-987L));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }

        [TestCase(DateTimeKind.Unspecified)]
        [TestCase(DateTimeKind.Utc)]
        [TestCase(DateTimeKind.Local)]
        public void AssetSaveAndReload_PreservesDateKindAndDuration(DateTimeKind kind)
        {
            var path = "Assets/TypioSerializationTest_" + Guid.NewGuid().ToString("N") + ".asset";
            var asset = ScriptableObject.CreateInstance<TypioSerializationTestAsset>();
            var expectedDate = new DateTime(638448479991234567L, kind);
            try
            {
                asset.date = expectedDate;
                asset.duration = TimeSpan.MinValue;
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                Resources.UnloadAsset(asset);
                asset = AssetDatabase.LoadAssetAtPath<TypioSerializationTestAsset>(path);

                SerializableDateTimeTests.AssertDate(expectedDate, asset.date.DateTime);
                Assert.That(asset.duration.TimeSpan, Is.EqualTo(TimeSpan.MinValue));
            }
            finally
            {
                AssetDatabase.DeleteAsset(path);
                if (asset != null && !EditorUtility.IsPersistent(asset))
                    UnityEngine.Object.DestroyImmediate(asset);
            }
        }
    }
}
