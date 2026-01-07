using System.IO;
using GSheetConv.Editor.SheetWindow;
using GSheetConv.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SheetManagerEditor : EditorWindow
{
    [SerializeField] private VisualTreeAsset visualTreeAsset = default;
    [SerializeField] private VisualTreeAsset itemTemplate = default;
    [SerializeField] private SheetConvertSetting setting = default;

    private SheetItemView m_LeftPanel = null;
    private SheetDetailView m_DetailView = null;

    [MenuItem("Tools/GoogleSheet/SheetManagerEditor")]
    public static void ShowWindow()
    {
        SheetManagerEditor wnd = GetWindow<SheetManagerEditor>();
        wnd.titleContent = new GUIContent("SheetManagerEditor");
        wnd.minSize = new Vector2(600, 480);
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;
        visualTreeAsset.CloneTree(root);

        InitializeSetting();

        // Left Panel
        VisualElement leftPanel = root.Q<VisualElement>("LeftPanel");
        m_LeftPanel = new SheetItemView(leftPanel, itemTemplate, setting);

        // Right Panel
        VisualElement rightPanel = root.Q<VisualElement>("RightPanel");
        m_DetailView = new SheetDetailView(rightPanel, setting);
        
        m_LeftPanel.SelectedCallback += m_DetailView.UpdateDetailView;
        m_DetailView.OnItemNameChanged += m_LeftPanel.RefreshItemList;
    }


    private void InitializeSetting()
    {
        MonoScript script = MonoScript.FromScriptableObject(this);
        string scriptPath = AssetDatabase.GetAssetPath(script);
        scriptPath = Path.GetDirectoryName(scriptPath);
        string folderPath = Path.GetDirectoryName(scriptPath)?.Replace("\\", "/");
        string settingPath = $"{folderPath}/SheetConvertSetting.asset";
        setting = AssetDatabase.LoadAssetAtPath<SheetConvertSetting>(settingPath);

        string csvPath = $"{Path.GetDirectoryName(Path.GetDirectoryName(scriptPath))?.Replace("\\", "/")}/Items";
        if (setting == null)
        {
            Debug.Log("CSV Path : " + csvPath);
            Debug.Log("Create SheetConvertSetting Asset");
            setting = CreateInstance<SheetConvertSetting>();
            setting.SavePath = csvPath;

            AssetDatabase.CreateAsset(setting, settingPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        if (setting.CSVList == null)
        {
            string csvListPath = $"{csvPath}/CSVListSO.asset";
            if (!File.Exists(csvListPath))
            {
                setting.CSVList = CreateInstance<CSVListSO>();
                AssetDatabase.CreateAsset(setting.CSVList, csvListPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
                setting.CSVList = AssetDatabase.LoadAssetAtPath<CSVListSO>(csvListPath);

            // Update the setting asset to reference the CSVListSO
            EditorUtility.SetDirty(setting);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}