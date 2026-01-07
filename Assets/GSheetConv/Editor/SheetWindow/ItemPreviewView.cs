using GSheetConv.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace GSheetConv.Editor.SheetWindow
{
    public class ItemPreviewView
    {
        private VisualElement _root;
        private CSVItemSO _currentItem;
        public ItemPreviewView(VisualElement root)
        {
            _root = root;
        }
        
        public void UpdatePreview(CSVItemSO item)
        {
            _currentItem = item;
            RefreshUI();
        }

        private void RefreshUI()
        {
            _root.Clear();
            //make column headers
            if (_currentItem == null || _currentItem.headers == null) return;
            VisualElement headerRow = new VisualElement();
            headerRow.AddToClassList("cell-container");
            foreach (var header in _currentItem.headers)
            {
                Label columnHeader = new Label(header);
                columnHeader.AddToClassList("column");
                headerRow.Add(columnHeader);
            }
            _root.Add(headerRow);

            foreach (var row in _currentItem.rows)
            {
                VisualElement dataRow = new VisualElement();
                dataRow.AddToClassList("cell-container");
                var columns = row.Split(',');
                for (int i = 0; i < _currentItem.headers.Count; i++)
                {
                    string cellValue = i < columns.Length ? columns[i] : "";
                    Label cellLabel = new Label(cellValue);
                    cellLabel.AddToClassList("row");
                    dataRow.Add(cellLabel);

                    if (i == _currentItem.keyIndex)
                        cellLabel.AddToClassList("key");
                }
                _root.Add(dataRow);
            }
        }
    }
}