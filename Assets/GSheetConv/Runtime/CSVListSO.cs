using System.Collections.Generic;
using UnityEngine;

namespace GSheetConv.Runtime
{
    public class CSVListSO : ScriptableObject
    {
        public List<CSVItemSO> csvItems = new List<CSVItemSO>();

        public void AddCSVItem(CSVItemSO newCsvItem)=>csvItems.Add(newCsvItem);
    }
}