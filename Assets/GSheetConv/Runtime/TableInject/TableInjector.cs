using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GSheetConv.Runtime.TableInject
{
    [DefaultExecutionOrder(-10)]
    public class TableInjector : MonoBehaviour
    {
        [SerializeField] private CSVListSO _csvList = null;

        Dictionary<CSVItemEnum, CSVItemSO> _csvItemDict = new Dictionary<CSVItemEnum, CSVItemSO>();

        private void Awake()
        {
            foreach (var csvItem in _csvList.csvItems)
            {
                _csvItemDict.TryAdd(csvItem.csvItemEnum, csvItem);
            }

            IEnumerable<MonoBehaviour> injectables = FindMonoBehaviours().Where(IsInjectable);
            foreach (var injectable in injectables)
            {
                TableInjectModule.Inject(injectable, _csvItemDict);
            }
        }

        private bool IsInjectable(MonoBehaviour mono)
        {
            MemberInfo[] members = mono.GetType().GetMembers(TableInjectModule.BindingFlags);
            return members.Any(member => Attribute.IsDefined(member, typeof(TableInjectAttribute)));
        }

        private static MonoBehaviour[] FindMonoBehaviours()
        {
            return FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None); //정렬없이 모든 모노 가져오기
        }
    }
}