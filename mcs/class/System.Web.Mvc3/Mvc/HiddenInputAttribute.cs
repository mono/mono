namespace System.Web.Mvc {
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class HiddenInputAttribute : Attribute {
        public HiddenInputAttribute() {
            DisplayValue = true;
        }

        public bool DisplayValue { get; set; }
    }
}
