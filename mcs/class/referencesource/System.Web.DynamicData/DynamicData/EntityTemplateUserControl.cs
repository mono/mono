using System.Security.Permissions;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace System.Web.DynamicData {
    /// <summary>
    /// Used as the base type for UserControls acting as entity templates.
    /// </summary>
    public class EntityTemplateUserControl : UserControl {
        public virtual ContainerType ContainerType {
            get {
                return Misc.FindContainerType(this);
            }
        }

        public string ValidationGroup { get; internal set; }
        public DataBoundControlMode Mode { get; internal set; }
        public MetaTable Table { get; internal set; }        
    }
}
