using System;
using UnityEngine.UIElements;

namespace GSheetConv.Editor.SheetWindow
{
    public class SheetItem
    {
        public VisualElement Root { get; private set; }
        private Label _titleLabel;
        private Button _deleteButton;
        
        public Action<SheetItem> OnClick;
        public Action<SheetItem> OnDelete;
        
        public SheetItem(VisualElement root, string title)
        {
            Root = root;
            _titleLabel = Root.Q<Label>("SheetName");
            _deleteButton = Root.Q<Button>("DeleteBtn");
            
            _titleLabel.text = title;
            _deleteButton.clicked += OnDeleteButtonClicked;
            
            Root.RegisterCallback<ClickEvent>(OnClicked);
        }

        private void OnClicked(ClickEvent evt)
        {
            OnClick?.Invoke(this);
        }

        private void OnDeleteButtonClicked()
        {
            //ask for confirmation
            bool confirm = UnityEditor.EditorUtility.DisplayDialog("Confirm Delete", $"Are you sure you want to delete '{_titleLabel.text}'?", "Yes", "No");
            
            if (confirm)
            {
                _deleteButton.clicked -= OnDeleteButtonClicked;
                OnDelete?.Invoke(this);
            }
        }
    }
}