using GSheetConv.Runtime;
using GSheetConv.Runtime.TableInject;
using UnityEngine;

namespace TestCode
{
    [CreateAssetMenu(fileName = "TI", menuName = "TestCode/TI", order = 0)]
    public class TI : ScriptableObject
    {
        // [TableInject(CSVItemEnum.TTT,"2","linkinpark")]
        public string bandName;
    }
}