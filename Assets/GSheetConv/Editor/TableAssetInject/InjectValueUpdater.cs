using UnityEditor;

namespace GSheetConv.Editor.TableAssetInject
{
    public class InjectValueUpdater
    {
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptReloaded()
        {
            EditorApplication.delayCall += HandleScriptReloaded;
        }

        private static void HandleScriptReloaded()
        {
            TableDetectorModule.InjectAssets();
        }
    }
}