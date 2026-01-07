using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GSheetConv.Editor.SheetWindow
{
    public class SheetDownloader
    {
        public static async Task<string> DownloadSheetAsync(string url)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                await request.SendWebRequest();
                
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Error downloading sheet: {request.error}");
                    return null;
                }
                return request.downloadHandler.text;
            }
        }

        public static string FormatGoogleSheetUrl(string url)
        {
            int gid = 0; //default page id
            //remove edit?guid=... part
            int editIndex = url.IndexOf("/edit");
            if (editIndex >= 0)
            {
                int gidIndex = url.IndexOf("gid=");
                if (gidIndex >= 0)
                {
                    string gidStr = url.Substring(gidIndex + 4);
                    int ampIndex = gidStr.IndexOf('#');
                    if (ampIndex >= 0)
                    {
                        gidStr = gidStr.Substring(0, ampIndex);
                    }
                    int.TryParse(gidStr, out gid);
                }
                url = url.Substring(0, editIndex);
            }
            
            //append export?format=csv&gid=page
            if (!url.EndsWith("/"))
                url += "/";
            url += $"export?format=csv&gid={gid}";
            return url;
        }
    }
}