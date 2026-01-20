using System;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = System.Object;

namespace GSheetConv.Runtime.TableInject
{
    public static class TableInjectModule
    {
        public const BindingFlags BindingFlags =
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic;

        // Cache field and attribute together to avoid GetCustomAttribute calls in loop
        private struct CachedFieldData
        {
            public FieldInfo Field;
            public TableInjectAttribute Attribute;
        }

        private static readonly Dictionary<Type, CachedFieldData[]> CachedFields = new Dictionary<Type, CachedFieldData[]>();

        public static bool Inject(Object injectable, Dictionary<CSVItemEnum, CSVItemSO> csvItemDict)
        {
            // Debug.Log removed for performance
            Type type = injectable.GetType();
            
            if (!CachedFields.TryGetValue(type, out CachedFieldData[] cachedFields))
            {
                var rawFields = type.GetFields(BindingFlags);
                var list = new List<CachedFieldData>(rawFields.Length);
                foreach (var f in rawFields)
                {
                    var attr = f.GetCustomAttribute<TableInjectAttribute>();
                    if (attr != null) list.Add(new CachedFieldData { Field = f, Attribute = attr });
                }
                cachedFields = list.ToArray();
                CachedFields[type] = cachedFields;
            }

            bool isModified = false;

            // Use for loop to avoid enumerator allocation
            for (int i = 0; i < cachedFields.Length; i++)
            {
                var data = cachedFields[i];
                var field = data.Field;
                var injectAttr = data.Attribute;

                if (csvItemDict.TryGetValue(injectAttr.CsvEnum, out CSVItemSO csvItem))
                {
                    string rawValue = csvItem.GetValue(injectAttr.Column, injectAttr.Key);
                    
                    if (string.IsNullOrEmpty(rawValue)) continue;

                    object convertedValue = ConvertValue(rawValue, field.FieldType);

                    if (convertedValue != null)
                    {
                        object currentValue = field.GetValue(injectable);
                        if (!Equals(currentValue, convertedValue))
                        {
                            // Debug.Log removed
                            field.SetValue(injectable, convertedValue);
                            isModified = true;
                        }
                        // Debug.LogWarning removed
                    }
                    // Debug.LogError removed to speed up execution
                }
            }

            if (isModified)
            {
#if UNITY_EDITOR
                if (injectable is UnityEngine.Object unityObject)
                {
                    EditorUtility.SetDirty(unityObject);
                    
                    // If the object is a persistent asset, force save to disk
                    if (!Application.isPlaying && AssetDatabase.Contains(unityObject))
                    {
                        AssetDatabase.SaveAssets();
                    }
                }
#endif
            }

            return isModified;
        }

        private static object ConvertValue(string value, Type targetType)
        {
            try
            {
                if (targetType == typeof(string)) return value;
                if (targetType == typeof(int)) return int.Parse(value);
                if (targetType == typeof(float)) return float.Parse(value);
                if (targetType == typeof(double)) return double.Parse(value);
                if (targetType == typeof(bool)) return bool.Parse(value);
                if (targetType.IsEnum) return Enum.Parse(targetType, value);
#if UNITY_EDITOR
                if (typeof(UnityEngine.Object).IsAssignableFrom(targetType)) 
                     return GetAssetFromPath(value, targetType);
#endif
            }
            catch (Exception)
            {
                // Debug.LogError removed
            }

            return null;
        }

#if UNITY_EDITOR
        private static UnityEngine.Object GetAssetFromPath(string path, Type type)
        {
            // Debug.Log removed. Trim path to avoid whitespace issues.
            var asset = AssetDatabase.LoadAssetAtPath(path.Trim(), type);
            if (asset == null)
            {
                // Only log if critically needed, otherwise skip to keep editor responsive
                 Debug.LogError($"Failed to load asset at path: {path} of type: {type.Name}");
            }
            return asset;
        }
        
        private static T GetAssetFromPath<T>(string path) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path.Trim());
        }
#endif
    }
}
