using System;

namespace GSheetConv.Runtime.TableInject
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class TableInjectAttribute: Attribute
    {
        public CSVItemEnum CsvEnum { get; }
        public string Column { get; }
        public string Key { get; }
        public bool IsInitialized { get; set; } = false;
        
        public TableInjectAttribute(CSVItemEnum csvEnum, string key,  string column)
        {
            CsvEnum = csvEnum;
            Column = column;
            Key = key;
        }
    }
}