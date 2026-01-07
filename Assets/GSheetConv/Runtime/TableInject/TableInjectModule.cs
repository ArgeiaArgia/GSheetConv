using System;
using System.Collections.Generic;
using System.Reflection;
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
                        // 현재 값과 비교하여 다를 때만 할당 (성능 최적화 핵심)
                        object currentValue = field.GetValue(injectable);
                        if (!object.Equals(currentValue, convertedValue))
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
                if (targetType == typeof(bool)) return bool.Parse(value);
            }
            catch (Exception e)
            {
                Debug.LogError($"Conversion failed: {value} to {targetType.Name}. Error: {e.Message}");
            }
            return null;
        }
    }
}