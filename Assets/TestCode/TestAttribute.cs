using System;

namespace TestCode
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class TestAttribute : Attribute
    {
        private int a { get; }

        public TestAttribute(int a)
        {
            this.a = a;
        }
    }
}