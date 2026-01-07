using System.Collections.Generic;
using System.Threading.Tasks;
using GSheetConv.Runtime;
using UnityEditor;

namespace GSheetConv.Editor.SheetWindow
{
    public static class SheetUpdateModule
    {
        public static async Task Update(CSVItemSO item)
        {
            var url = SheetDownloader.FormatGoogleSheetUrl(item.url);
            var result = await SheetDownloader.DownloadSheetAsync(url);
            if (result != null)
            {
                var readRes = result.Split('\n');
                var headers = readRes[0].Split(',');
                item.headers = new List<string>();
                foreach (var h in headers)
                {
                    item.headers.Add(h.ToUpper().Trim());
                }
                item.rows = new List<string>();
                for (int i = 1; i < readRes.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(readRes[i]))
                        item.rows.Add(readRes[i]);
                }
                item.SetKey(item.keyName);
                EditorUtility.SetDirty(item);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Failed to update the sheet. Please check the URL and try again.",
                    "OK");
            }
        }
    }
}