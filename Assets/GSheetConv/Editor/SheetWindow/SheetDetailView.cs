using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GSheetConv.Editor.TableAssetInject;
using GSheetConv.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GSheetConv.Editor.SheetWindow
{
    public class SheetDetailView
    {
        private VisualElement _root;

        private SheetConvertSetting _setting;
        private CSVItemSO _currentCSVItem;

        private ItemPreviewView _itemPreviewView;

        private TextField _sheetName;
        private Button _updateButton;
        private TextField _sheetURL;
        private TextField _keyName;

        public Action OnItemNameChanged;

        public SheetDetailView(VisualElement root, SheetConvertSetting setting)
        {
            _root = root;
            _setting = setting;
            InitializeElements();
            Deselect();
        }

        private void InitializeElements()
        {
            _sheetName = _root.Q<TextField>("TitleField");
            _updateButton = _root.Q<Button>("UpdateBtn");
            _sheetURL = _root.Q<TextField>("URLField");
            _keyName = _root.Q<TextField>("KeyField");
            
            _itemPreviewView = new ItemPreviewView(_root.Q<VisualElement>("PreviewContainer"));

            _sheetName.RegisterValueChangedCallback(HandleNameChange);
            _sheetURL.RegisterCallback<ClickEvent>(HandleURLClick);
            _keyName.RegisterValueChangedCallback(HandleKeyChange);
            _updateButton.clicked += HandleUpdate;
        }

        private void HandleKeyChange(ChangeEvent<string> evt)
        {
            _currentCSVItem.SetKey(evt.newValue);
            EditorUtility.SetDirty(_currentCSVItem);
            AssetDatabase.SaveAssets();
            
            _itemPreviewView.UpdatePreview(_currentCSVItem);
        }

        private void HandleNameChange(ChangeEvent<string> evt)
        {
            foreach (var item in _setting.CSVList.csvItems)
            {
                if (item.title == evt.newValue)
                {
                    EditorUtility.DisplayDialog("Error", "A sheet with this name already exists.", "OK");
                    _sheetName.SetValueWithoutNotify(_currentCSVItem.title);
                    return;
                }
            }
            string assetPath = AssetDatabase.GetAssetPath(_currentCSVItem);
            AssetDatabase.RenameAsset(assetPath, evt.newValue);
            _currentCSVItem.title = evt.newValue;
            _currentCSVItem.name = evt.newValue;
            EditorUtility.SetDirty(_currentCSVItem);
            AssetDatabase.SaveAssets();
            ChangeEnum();
            OnItemNameChanged?.Invoke();
        }

        private void ChangeEnum()
        {
            string enumString = string.Join(", ",
                _setting.CSVList.csvItems.Select(so => $"{so.name.ToUpper().Replace(' ', '_')} = {(int)so.csvItemEnum}"));
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

            Debug.Log("Updated CSVItemEnum.cs");
        }

        private void HandleURLClick(ClickEvent evt)
        {
            Application.OpenURL(_currentCSVItem.url);
        }

        private async void HandleUpdate()
        {
            _updateButton.text = "Updating...";
            var url = SheetDownloader.FormatGoogleSheetUrl(_currentCSVItem.url);
            var result = await SheetDownloader.DownloadSheetAsync(url);
            if (result != null)
            {
                var readRes = result.Split('\n');
                var headers = readRes[0].Split(',');
                _currentCSVItem.headers = new List<string>();
                foreach (var h in headers)
                {
                    _currentCSVItem.headers.Add(h.ToUpper().Trim());
                }
                _currentCSVItem.rows = new List<string>();
                for (int i = 1; i < readRes.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(readRes[i]))
                        _currentCSVItem.rows.Add(readRes[i]);
                }
                _currentCSVItem.SetKey(_currentCSVItem.keyName);
                EditorUtility.SetDirty(_currentCSVItem);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Failed to update the sheet. Please check the URL and try again.",
                    "OK");
            }

            _updateButton.text = "Update";
            _itemPreviewView.UpdatePreview(_currentCSVItem);
            TableDetectorModule.InjectAssets();
        }

        public void UpdateDetailView(CSVItemSO csvItem)
        {
            if (csvItem != null)
                _root.style.display = DisplayStyle.Flex;

            _currentCSVItem = csvItem;
            if (_sheetName == null)
                InitializeElements();

            if (_sheetName != null) _sheetName.SetValueWithoutNotify(csvItem.title);
            if (_sheetURL != null) _sheetURL.SetValueWithoutNotify(csvItem.url);
            if (_keyName != null) _keyName.SetValueWithoutNotify(csvItem.keyName);
            _itemPreviewView.UpdatePreview(csvItem);
        }

        public void Deselect()
        {
            _currentCSVItem = null;
            _root.style.display = DisplayStyle.None;
        }
    }
}