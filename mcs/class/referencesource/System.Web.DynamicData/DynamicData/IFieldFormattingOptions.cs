using System.Security.Permissions;

namespace System.Web.DynamicData {
    /// <summary>
    /// Interface that encapsulates common formatting fields used in multiple places
    /// </summary>
    public interface IFieldFormattingOptions {
        /// <summary>
        /// Same semantic as the same property on System.Web.UI.WebControls.BoundField
        /// </summary>
        bool ApplyFormatInEditMode { get; }

        /// <summary>
        /// Same semantic as the same property on System.Web.UI.WebControls.BoundField
        /// </summary>
        string DataFormatString { get; }

        /// <summary>
        /// Same semantic as the same property on System.Web.UI.WebControls.BoundField
        /// </summary>
        bool ConvertEmptyStringToNull { get; }

        /// <summary>
        /// Same semantic as the same property on System.Web.UI.WebControls.BoundField
        /// </summary>
        string NullDisplayText { get; }

        /// <summary>
        /// Same semantic as the same property on System.Web.UI.WebControls.BoundField
        /// </summary>
        bool HtmlEncode { get; }
    }
}
