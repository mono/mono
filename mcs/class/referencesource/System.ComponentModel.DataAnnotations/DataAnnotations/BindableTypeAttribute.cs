namespace System.ComponentModel.DataAnnotations {
    /// <summary>
    /// Specifies that a type is considered a bindable type for automatic fields generation by controls like GridView / DetailsView.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct, AllowMultiple = false, Inherited=true)]
    public sealed class BindableTypeAttribute : Attribute {

        public BindableTypeAttribute() {
            IsBindable = true;
        }

        /// <summary>
        /// Indicates whether the type should be considered Bindable or not. The default value is true when the attribute is specified.
        /// </summary>
        public bool IsBindable {
            get;
            set;
        }
    }
}
