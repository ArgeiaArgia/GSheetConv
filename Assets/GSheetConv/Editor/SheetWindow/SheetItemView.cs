using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GSheetConv.Editor.TableAssetInject;
using GSheetConv.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GSheetConv.Editor.SheetWindow
{
    public class SheetItemView
    {
        private SheetConvertSetting _setting;

        private VisualTreeAsset _itemTemplate = default;
        private Button _addButton;
        private Button _updateAllButton;
        private ScrollView _itemListView;
        private AddSheetWindow _addSheetWindow;

        private Dictionary<SheetItem, CSVItemSO> _sheetItems = new();
        
        public Action<CSVItemSO> SelectedCallback;
        public Action RefreshRequested;

        public SheetItemView(VisualElement leftPanel, VisualTreeAsset itemTemplate, SheetConvertSetting setting)
        {
            _itemTemplate = itemTemplate;
            _addButton = leftPanel.Q<Button>("AddBtn");
            _updateAllButton = leftPanel.Q<Button>("UpdateAllBtn");
            _itemListView = leftPanel.Q<ScrollView>("ItemView");
            _setting = setting;

            RefreshItemList();

            _addButton.clicked += HandleAddButtonClicked;
            _updateAllButton.clicked += HandleUpdateAllButtonClicked;
        }

        private async void HandleUpdateAllButtonClicked()
        {
            _updateAllButton.text = "Updating All...";
            await Task.WhenAll(_setting.CSVList.csvItems.Select(item => SheetUpdateModule.Update(item)).ToArray());
            _updateAllButton.text = "Update All";
            TableDetectorModule.InjectAssets();
            RefreshRequested?.Invoke();
        }

        public void RefreshItemList()
        {
            _itemListView.Clear();
            if (_setting.CSVList == null) return;
            foreach (var csvItem in _setting.CSVList.csvItems)
            {
                CreateItemUI(csvItem);
            }
        }

        private void CreateItemUI(CSVItemSO csvItem)
        {
            VisualElement itemElement = _itemTemplate.CloneTree();
            SheetItem sheetItem = new SheetItem(itemElement, csvItem.title);
            sheetItem.OnClick += HandleItemSelect;
            sheetItem.OnDelete += HandleItemDelete;
            _sheetItems.Add(sheetItem, csvItem);
            _itemListView.Add(itemElement);
        }

        private void HandleItemSelect(SheetItem obj)
        {
            if (_sheetItems.TryGetValue(obj, out CSVItemSO csvItem))
            {
                SelectedCallback?.Invoke(csvItem);
            }
        }

        private void HandleItemDelete(SheetItem obj)
        {
            if (_sheetItems.TryGetValue(obj, out CSVItemSO csvItem))
            {
                _setting.CSVList.csvItems.Remove(csvItem);
                _sheetItems.Remove(obj);
                EditorUtility.SetDirty(_setting.CSVList);
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(csvItem));
                AssetDatabase.SaveAssets();

                DeleteEnum();
            }

            _itemListView.Remove(obj.Root);
            obj.Root.RemoveFromHierarchy();
        }

        private void DeleteEnum()
        {
            string enumString = string.Join(", ",
                _setting.CSVList.csvItems.Select(so =>
                {
                    return $"{so.name.ToUpper().Replace(' ', '_')} = {(int)so.csvItemEnum}";
                }));
            if (!string.IsNullOrEmpty(enumString))
                enumString += ", ";
            enumString += $"END = {(int)CSVItemEnum.END}";
            var scriptPath = AssetDatabase.GetAssetPath(_setting.CSVList);
            string dirName = Path.GetDirectoryName(scriptPath);
            if (dirName != null)
            {
                var path = Directory.GetParent(dirName)?.FullName;
                if (path != null)
                {
                    var code = string.Format(CodeFormat.CSVEnumFormat, "CSVItemEnum", enumString);
                    File.WriteAllText($"{path}/Runtime/CSVItemEnum.cs", code);
                }
            }
        }

        private void HandleAddButtonClicked()
        {
            if (_addSheetWindow == null)
                _addSheetWindow = ScriptableObject.CreateInstance<AddSheetWindow>();

            _addSheetWindow.titleContent = new GUIContent("Add New Sheet");
            _addSheetWindow.minSize = new Vector2(350, 225);
            _addSheetWindow.maxSize = new Vector2(350, 225);
            _addSheetWindow.Show();

            _addSheetWindow.OnAddSheet += OnAddSheet;
        }

        private void OnAddSheet(CSVItemSO newCsvItem)
        {
            CreateItemUI(newCsvItem);
        }
    }
}