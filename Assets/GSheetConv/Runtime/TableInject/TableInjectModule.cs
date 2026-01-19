using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace GSheetConv.Runtime.TableInject
{
    public static class TableInjectModule
    {
        public const BindingFlags BindingFlags =
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;

        public static bool Inject(Object injectable, Dictionary<CSVItemEnum, CSVItemSO> csvItemDict)
        {
            Type type = injectable.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags);
            bool isModified = false;

            foreach (var field in fields)
            {
                var injectAttr = field.GetCustomAttribute<TableInjectAttribute>();
                if (injectAttr == null) continue;

                if (csvItemDict.TryGetValue(injectAttr.CsvEnum, out CSVItemSO csvItem))
                {
                    string rawValue = csvItem.GetValue(injectAttr.Column, injectAttr.Key);
                    object convertedValue = ConvertValue(rawValue, field.FieldType);

                    if (convertedValue != null)
                    {
                        object currentValue = field.GetValue(injectable);
                        if (!Equals(currentValue, convertedValue))
                        {
                            field.SetValue(injectable, convertedValue);
                            isModified = true;
                        }
                    }
                }
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
                if (targetType == typeof(Sprite)) return GetAssetFromPath<Sprite>(value);
                if (targetType == typeof(AudioClip)) return GetAssetFromPath<AudioClip>(value);
                if (targetType == typeof(Material)) return GetAssetFromPath<Material>(value);
                if (targetType == typeof(Texture2D)) return GetAssetFromPath<Texture2D>(value);
            }
            catch (Exception e)
            {
                Debug.LogError($"Conversion failed: {value} to {targetType.Name}. Error: {e.Message}");
            }
            return null;
        }

        private static T GetAssetFromPath<T>(string path) where T : UnityEngine.Object
        {
            var asset = AssetDatabase.LoadAssetAtPath("Assets/" + path, typeof(T));
            return asset as T;
        }
    }
}