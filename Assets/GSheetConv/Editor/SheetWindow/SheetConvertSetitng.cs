using GSheetConv.Runtime;
using UnityEngine;

namespace GSheetConv.Editor.SheetWindow
{
    [CreateAssetMenu(fileName = "SheetConvertSetting", menuName = "SO/GSheetConv/SheetConvertSetting", order = 0)]
    public class SheetConvertSetting : ScriptableObject
    {
        [field: SerializeField] public string SavePath { get; set; }
        [field: SerializeField] public CSVListSO CSVList { get; set; }
    }
}