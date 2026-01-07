using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GSheetConv.Editor.SheetWindow;
using GSheetConv.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public static class CodeFormat
{
    public static readonly string CSVEnumFormat =
@"
namespace GSheetConv.Runtime
{{
    public enum {0}
    {{
        {1}
    }}
}}
";
}

public class AddSheetWindow : EditorWindow
{
    [SerializeField] private VisualTreeAsset visualTreeAsset = default;
    [SerializeField] private SheetConvertSetting setting = default;

    private TextField _urlField;
    private Button _validBtn;
    private TextField _titleField;
    private Button _duplicateBtn;
    private Button _addBtn;

    //Settings
    private TextField _keyField;

    private bool _isUrlValid = false;
    private bool _isTitleValid = false;

    private const string ValidClass = "valid";
    private const string InvalidClass = "invalid";
    private const string DefaultText = "Valid Check";
    private const string ValidText = "Available";
    private const string InvalidText = "Unavailable";

    private string[] _readCSV;

    public delegate void OnAddSheetDelegate(CSVItemSO newCSVItem);

    public OnAddSheetDelegate OnAddSheet;

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        visualTreeAsset.CloneTree(root);

        if (setting == null)
        {
            MonoScript script = MonoScript.FromScriptableObject(this);
            string scriptPath = AssetDatabase.GetAssetPath(script);
            string folderPath = System.IO.Path.GetDirectoryName(scriptPath)?.Replace("\\", "/");
            string settingPath = $"{folderPath}/SheetConvertSetting.asset";
            setting = AssetDatabase.LoadAssetAtPath<SheetConvertSetting>(settingPath);
        }

        GetUIElements();
    }

    private void GetUIElements()
    {
        _urlField = rootVisualElement.Q<TextField>("UrlField");
        _validBtn = rootVisualElement.Q<Button>("ValidBtn");
        _titleField = rootVisualElement.Q<TextField>("TitleField");
        _duplicateBtn = rootVisualElement.Q<Button>("DuplicateBtn");
        _addBtn = rootVisualElement.Q<Button>("AddBtn");

        _keyField = rootVisualElement.Q<TextField>("KeyName");

        _validBtn.clicked += OnValidButtonClicked;
        _urlField.RegisterValueChangedCallback(OnUrlFieldChanged);

        _duplicateBtn.clicked += OnDuplicateBtnClicked;
        _titleField.RegisterValueChangedCallback(OnTitleFieldChanged);

        _addBtn.clicked += OnAddButtonClicked;
    }

    private void OnAddButtonClicked()
    {
        string url = _urlField.text;
        url = SheetDownloader.FormatGoogleSheetUrl(url);
        string title = _titleField.text;
        string keyName = _keyField.text;

        var newCsvItem = CreateCSVItem(title, _urlField.text, keyName);

        setting.CSVList.AddCSVItem(newCsvItem);
        EditorUtility.SetDirty(setting.CSVList);

        OnAddSheet?.Invoke(newCsvItem);
        
        this.Close();
    }
    
    private CSVItemSO CreateCSVItem(string title, string url, string keyName)
    {
        CSVItemSO newCsvItem = ScriptableObject.CreateInstance<CSVItemSO>();
        int enumCount = CreateEnum(title);
        newCsvItem.csvItemEnum = (CSVItemEnum)enumCount;
        newCsvItem.name = title;
        newCsvItem.title = title;
        newCsvItem.url = url;
        SetHeaderAndRows(newCsvItem, keyName);
        // Save CSVItemSO asset
        string assetPath = $"{setting.SavePath}/{title}.asset";
        AssetDatabase.CreateAsset(newCsvItem, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return newCsvItem;
    }

    private int CreateEnum(string newCsv)
    {
        int nextEnumValue = (int)CSVItemEnum.END;
        string enumString = string.Join(", ", setting.CSVList.csvItems.Select(so =>
        {
            return $"{so.name.ToUpper().Replace(' ', '_')} = {(int)so.csvItemEnum}";
        }));
        if (!string.IsNullOrEmpty(enumString))
            enumString += ", ";
        enumString += $"{newCsv.ToUpper().Replace(' ', '_')} = {nextEnumValue}";
        enumString += $", END = {nextEnumValue + 1}";
        string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
        string dirName = Path.GetDirectoryName(scriptPath);
        if (dirName != null)
        {
            var fullName = Directory.GetParent(dirName)?.FullName;
            if (fullName != null)
            {
                var parentDirectory = Directory.GetParent(fullName);
                if (parentDirectory != null)
                {
                    var path = parentDirectory.FullName;
                    var code = string.Format(CodeFormat.CSVEnumFormat, "CSVItemEnum", enumString);
                    var filePath = Path.Combine(path, "Runtime", "CSVItemEnum.cs");
                    File.WriteAllText(filePath, code);
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return nextEnumValue;
    }

    private void SetHeaderAndRows(CSVItemSO item, string keyName)
    {
        var headers = _readCSV[0].Split(',');
        item.headers = new List<string>();
        foreach (var h in headers)
        {
            item.headers.Add(h.ToUpper().Trim());
        }
        item.rows = new List<string>();
        for (int i = 1; i < _readCSV.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(_readCSV[i]))
                item.rows.Add(_readCSV[i]);
        }
        item.SetKey(keyName);
    }

    private async void OnValidButtonClicked()
    {
        try
        {
            string url = _urlField.text;
        
            url = SheetDownloader.FormatGoogleSheetUrl(url);
            _validBtn.text = "Checking...";
        
            foreach (var item in setting.CSVList.csvItems)
            {
                var itemUrl = SheetDownloader.FormatGoogleSheetUrl(item.url);
                if (itemUrl == url)
                {
                    _isUrlValid = false;
                    _validBtn.text = InvalidText;
                    _validBtn.AddToClassList(InvalidClass);
                    _addBtn.SetEnabled(false);
                    EditorUtility.DisplayDialog("URL Duplicate", $"Another Sheet(\"{item.title}\") is already using the same URL", "OK");
                    return;
                }
            }
        
            string csv = await SheetDownloader.DownloadSheetAsync(url);
            _isUrlValid = csv != null;
            if (csv != null)
            {
                _validBtn.text = ValidText;
                _validBtn.AddToClassList(ValidClass);
                _addBtn.SetEnabled(_isUrlValid && _isTitleValid);
                _readCSV = csv.Split('\n');
            }
            else
            {
                _validBtn.text = InvalidText;
                _validBtn.AddToClassList(InvalidClass);
                _addBtn.SetEnabled(false);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            _isUrlValid = false;
            _validBtn.text = InvalidText;
            _validBtn.AddToClassList(InvalidClass);
            _addBtn.SetEnabled(false);
        }
    }

    private void OnUrlFieldChanged(ChangeEvent<string> evt)
    {
        _validBtn.text = DefaultText;
        _validBtn.RemoveFromClassList(ValidClass);
        _validBtn.RemoveFromClassList(InvalidClass);
        _addBtn.SetEnabled(false);
    }

    private void OnDuplicateBtnClicked()
    {
        string title = _titleField.text;
        var list = setting.CSVList;
        _isTitleValid = true;
        foreach (var item in list.csvItems)
        {
            if (item.name.ToUpper() == title.ToUpper())
            {
                _isTitleValid = false;
                _duplicateBtn.text = InvalidText;
                _duplicateBtn.AddToClassList(InvalidClass);
                _addBtn.SetEnabled(false);
                break;
            }
        }

        if (_isTitleValid)
        {
            _duplicateBtn.text = ValidText;
            _duplicateBtn.AddToClassList(ValidClass);
            _addBtn.SetEnabled(_isUrlValid && _isTitleValid);
        }
    }

    private void OnTitleFieldChanged(ChangeEvent<string> evt)
    {
        _duplicateBtn.text = DefaultText;
        _duplicateBtn.RemoveFromClassList(ValidClass);
        _duplicateBtn.RemoveFromClassList(InvalidClass);
        _addBtn.SetEnabled(false);
    }
}