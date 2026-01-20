using System;
using GSheetConv.Runtime;
using GSheetConv.Runtime.TableInject;
using UnityEngine;

namespace TestCode
{
    public class MatTester : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;
        // [TableInject(CSVItemEnum.MATERIALTEST,"green", "MatPath"), SerializeField]
        private Material redMaterial;

        private void Start()
        {
            if (meshRenderer != null && redMaterial != null)
            {
                meshRenderer.material = redMaterial;
                Debug.Log("Applied red material to mesh renderer.");
            }
            else
            {
                Debug.LogWarning("MeshRenderer or redMaterial is not assigned.");
            }
        }
    }
}