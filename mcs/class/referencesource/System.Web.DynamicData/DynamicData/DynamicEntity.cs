using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Web.DynamicData.Util;
using System.Web.Resources;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace System.Web.DynamicData {
    [ToolboxBitmap(typeof(DynamicEntity), "DynamicEntity.bmp")]
    public class DynamicEntity : Control {
        private HttpContextBase _context;

        [
        DefaultValue(DataBoundControlMode.ReadOnly),
        Category("Behavior"),
        ResourceDescription("DynamicEntity_Mode")
        ]
        public DataBoundControlMode Mode {
            get {
                var value = ViewState["Mode"];
                return value != null ? (DataBoundControlMode)value : DataBoundControlMode.ReadOnly;
            }
            set {
                ViewState["Mode"] = value;
            }
        }

        [
        DefaultValue(""),
        Category("Behavior"),
        ResourceDescription("DynamicControlFieldCommon_UIHint")
        ]
        public string UIHint {
            get {
                return (string)ViewState["UIHint"] ?? String.Empty;
            }
            set {
                ViewState["UIHint"] = value;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(""),
        Themeable(false),
        ResourceDescription("DynamicControlFieldCommon_ValidationGroup")
        ]
        public string ValidationGroup {
            get {
                return (string)ViewState["ValidationGroup"] ?? String.Empty;
            }
            set {
                ViewState["ValidationGroup"] = value;
            }
        }

        private new HttpContextBase Context {
            get {
                return _context ?? new HttpContextWrapper(HttpContext.Current);
            }
        }

        public DynamicEntity() {
        }

        // for unit testing
        internal DynamicEntity(HttpContextBase context)
            : this() {
            _context = context;
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            if (DesignMode) {
                return;
            }

            MetaTable table = MetaTableHelper.FindMetaTable(this, Context);
            if (table == null) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    DynamicDataResources.DynamicEntity_ControlNeedsToExistInAContextSupportingDynamicData,
                    this.ID));
            }

            EntityTemplateFactory entityTemplateFactory = table.Model.EntityTemplateFactory;
            EntityTemplateUserControl entityTemplateControl = entityTemplateFactory.CreateEntityTemplate(table, Mode, UIHint);
            if (entityTemplateControl == null) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    DynamicDataResources.DynamicEntity_CantFindTemplate,
                    table.Name,
                    entityTemplateFactory.TemplateFolderVirtualPath));
            }

            entityTemplateControl.Mode = Mode;
            entityTemplateControl.ValidationGroup = ValidationGroup;
            entityTemplateControl.Table = table;
            Controls.Add(entityTemplateControl);
        }

        protected override void Render(HtmlTextWriter writer) {
            if (DesignMode) {
                writer.Write("[" + GetType().Name + "]");
            }
            else {
                base.Render(writer);
            }
        }
    }
}
