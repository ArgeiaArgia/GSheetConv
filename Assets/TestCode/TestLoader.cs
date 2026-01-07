using GSheetConv.Runtime;
using UnityEngine;

namespace TestCode
{
    public class TestLoader : MonoBehaviour
    {
        [SerializeField] private string url;
        private async void Start()
        {
            var data = await GSheetConv.Editor.SheetWindow.SheetDownloader.DownloadSheetAsync(url);
            if (data != null)
            {
                Debug.Log("Downloaded Data:\n" + data);
            }
        }
        [SerializeField] private CSVItemSO csvItem;
        [SerializeField] private string column;
        [SerializeField] private string key;

        [ContextMenu("GetValue")]
        private void TestGetValue()
        {
            if (csvItem != null)
            {
                string value = csvItem.GetValue(column, key);
                Debug.Log($"Value at column '{column}' and key '{key}': {value}");
            }
            else
            {
                Debug.LogWarning("CSVItemSO is not assigned.");
            }
        }
    }
}