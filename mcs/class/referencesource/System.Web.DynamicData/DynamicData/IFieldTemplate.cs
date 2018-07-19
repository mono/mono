using System.Security.Permissions;

namespace System.Web.DynamicData {
    /// <summary>
    /// Interface that represents a field template. Though by default field templates are User Controls (ascx)
    /// they don't have to be.
    /// </summary>
    public interface IFieldTemplate {
        /// <summary>
        /// Sets the IFieldTemplateHost used by this field template to know what column it is dealing with
        /// </summary>
        /// <param name="host"></param>
        void SetHost(IFieldTemplateHost host);
    }
}
