using GSheetConv.Runtime;
using GSheetConv.Runtime.TableInject;
using UnityEngine;

namespace TestCode
{
    [CreateAssetMenu(fileName = "MatInjectTest", menuName = "SO/MatInjectTest", order = 0)]
    public class MatInjectTest : ScriptableObject
    {
        // [TableInject(CSVItemEnum.MATERIALTEST,"Green", "MatPath")]
        private Material redMaterial;
    }
}