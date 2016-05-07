using System.Web.UI.WebControls;
using System.Security.Permissions;

namespace System.Web.DynamicData {
    /// <summary>
    /// Interface implemented by the object that drives field template. Typically, this is the DynamicControl.
    /// </summary>
    public interface IFieldTemplateHost {
        /// <summary>
        /// The MetaColumn for which the field template is needed
        /// </summary>
        MetaColumn Column { get; }

        /// <summary>
        /// The mode (Readonly, Edit, Insert) for which the field template is needed
        /// </summary>
        DataBoundControlMode Mode { get; }

        /// <summary>
        /// The various formatting options that should be applied to the field template
        /// </summary>
        IFieldFormattingOptions FormattingOptions { get; }

        /// <summary>
        /// The validation group that the field template need to be in
        /// </summary>
        string ValidationGroup { get; }
    }
}
