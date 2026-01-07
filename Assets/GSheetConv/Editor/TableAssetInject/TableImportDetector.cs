using System.Reflection;
using GSheetConv.Runtime;
using GSheetConv.Runtime.TableInject;
using UnityEditor;
using UnityEngine;

namespace GSheetConv.Editor.TableAssetInject
{
    public class TableImportDetector : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (string str in importedAssets)
            {
                if (str.EndsWith(".asset"))
                {
                    System.Type assetType = AssetDatabase.GetMainAssetTypeAtPath(str);
        
                    if (assetType == typeof(CSVItemSO))
                    {
                        Debug.Log("Detected CSVItemSO import, injecting all assets.");
                        TableDetectorModule.InjectAssets();
                    }
                    else
                    {
                        //Check if asset has TableInject attributes
                        ScriptableObject data = AssetDatabase.LoadAssetAtPath<ScriptableObject>(str);
                        if (data != null)
                        {
                            MemberInfo[] members = data.GetType().GetMembers(TableInjectModule.BindingFlags);
                            foreach (var member in members)
                            {
                                var injectAttr = member.GetCustomAttribute<TableInjectAttribute>();
                                if (injectAttr != null)
                                {
                                    if (injectAttr.IsInitialized)
                                        break;
                                    TableInjectModule.Inject(data, TableDetectorModule.GetItemDictionary());
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        
    }
}