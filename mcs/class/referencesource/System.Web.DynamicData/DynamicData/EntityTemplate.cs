using System.ComponentModel;
using System.Security.Permissions;
using System.Web.UI;

namespace System.Web.DynamicData {
    [ParseChildren(true)]
    [PersistChildren(false)]
    public class EntityTemplate : Control {
        [Browsable(false)]
        [DefaultValue(null)]
        [PersistenceMode(PersistenceMode.InnerProperty)]
        [TemplateContainer(typeof(INamingContainer))]
        public virtual ITemplate ItemTemplate { get; set; }
    }
}
