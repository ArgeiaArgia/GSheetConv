using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GSheetConv.Runtime;
using GSheetConv.Runtime.TableInject;
using UnityEditor;
using UnityEngine;

namespace GSheetConv.Editor.TableAssetInject
{
    public static class TableDetectorModule
    {
        private static HashSet<Type> _injectableTypesCache;

        public static void InjectAssets()
        {
            //Test
            float startTime = Time.realtimeSinceStartup;
            
            RefreshInjectableTypeCache();

            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
            var dict = GetItemDictionary();
            int updatedCount = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                
                Type assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
                
                if (assetType != null && IsInjectableType(assetType))
                {
                    ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                    if (so != null && TableInjectModule.Inject(so, dict))
                    {
                        EditorUtility.SetDirty(so);
                        updatedCount++;
                    }
                }
            }

            if (updatedCount > 0)
            {
                AssetDatabase.SaveAssets();
                float endTime = Time.realtimeSinceStartup;
                Debug.Log($"<b>[GSheetConv]</b> {updatedCount} assets updated automatically. Time taken: {(endTime - startTime) * 1000f} ms");
            }
        }

        private static bool IsInjectableType(Type type)
        {
            return _injectableTypesCache != null && _injectableTypesCache.Any(t => t.IsAssignableFrom(type));
        }

        private static void RefreshInjectableTypeCache()
        {
            _injectableTypesCache = new HashSet<Type>();
            
            // 프로젝트 내의 모든 클래스를 훑어 어트리뷰트가 있는 클래스 목록 생성
            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes());

            foreach (var type in allTypes)
            {
                if (type.GetFields(TableInjectModule.BindingFlags)
                        .Any(f => f.GetCustomAttribute<TableInjectAttribute>() != null))
                {
                    _injectableTypesCache.Add(type);
                }
            }
        }

        public static Dictionary<CSVItemEnum, CSVItemSO> GetItemDictionary()
        {
            var dict = new Dictionary<CSVItemEnum, CSVItemSO>();
            string[] guids = AssetDatabase.FindAssets("t:CSVItemSO");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<CSVItemSO>(path);
                if (item != null && !dict.ContainsKey(item.csvItemEnum))
                {
                    dict.Add(item.csvItemEnum, item);
                }
            }
            return dict;
        }
    }
}