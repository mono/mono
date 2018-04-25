using System.Web.UI.WebControls;
using System.Security.Permissions;

namespace System.Web.DynamicData {
    /// <summary>
    /// Interface implemented by objects that know how to create field temnplates
    /// </summary>
    public interface IFieldTemplateFactory {

        /// <summary>
        /// Initialize the FieldTemplateFactory, passing it the meta model that it will work with
        /// </summary>
        void Initialize(MetaModel model);

        /// <summary>
        /// Create a field template based on various pieces of data
        /// </summary>
        /// <param name="column">The MetaColumn for which the field template is needed</param>
        /// <param name="mode">The mode (Readonly, Edit, Insert) for which the field template is needed</param>
        /// <param name="uiHint">The UIHint (if any) that should affect the field template lookup</param>
        /// <returns></returns>
        IFieldTemplate CreateFieldTemplate(MetaColumn column, DataBoundControlMode mode, string uiHint);
    }
}
