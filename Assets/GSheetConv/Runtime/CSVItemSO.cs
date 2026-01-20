using System;
using System.Collections.Generic;
using UnityEngine;

namespace GSheetConv.Runtime
{
    public class CSVItemSO : ScriptableObject
    {
        [HideInInspector] public CSVItemEnum csvItemEnum;
        public string title;
        public string url;
        public List<string> headers;
        public List<string> keys;
        public List<string> rows;
        public string keyName;
        [HideInInspector] public int keyIndex;

        public void SetKey(string key)
        {
            if (headers == null || rows == null)
            {
                Debug.LogError("CSVItemSO: Headers or Rows are not initialized.");
                return;
            }
            
            key = key.ToUpper().Trim();

            if (!headers.Contains(key))
            {
                keyIndex = 0;
                keyName = headers[0];
                Debug.LogWarning($"CSVItemSO: Key '{key}' does not exist in headers. Defaulting to index 0.");
            }
            else
            {
                keyName = key;
                keyIndex = headers.IndexOf(key);
            }
            keys = new List<string>();
            foreach (var r in rows)
            {
                var row = r.Split(',');
                if (keyIndex >= row.Length)
                {
                    Debug.LogError(
                        $"CSVItemSO: Key index '{keyIndex}' is out of range for the row. Skipping this row.");
                    continue;
                }
                keys.Add(row[keyIndex].ToUpper());
            }
        }

        public string GetValue(string header, string key)
        {
            if (headers == null)
            {
                Debug.LogError("CSVItemSO: Headers are not initialized.");
                return null;
            }
            if (rows == null)
            {
                Debug.LogError("CSVItemSO: Rows are not initialized.");
                return null;
            }

            header = header.ToUpper().Trim();
            key = key.ToUpper().Trim();
            
            if (!headers.Contains(header))
            {
                Debug.LogError($"CSVItemSO: Header '{header}' does not exist." +
                               $"Available headers: {string.Join(", ", headers)}");
                return null;
            }

            int columnIndex = headers.IndexOf(header);
            if (columnIndex == -1)
            {
                Debug.LogError($"CSVItemSO: Header '{header}' index is invalid.");
                return null;
            }
            int rowIndex = keys.IndexOf(key);
            if (rowIndex == -1)
            {
                Debug.LogError($"CSVItemSO: Key '{key}' does not exist." +
                               $"Available keys: {string.Join(", ", keys)}");
                return null;
            }
            var row = rows[rowIndex].Split(',');
            if (columnIndex >= row.Length)
            {
                Debug.LogError(
                    $"CSVItemSO: Column index '{columnIndex}' is out of range for the row.");
                return null;
            }
            return row[columnIndex];
        }
    }
}