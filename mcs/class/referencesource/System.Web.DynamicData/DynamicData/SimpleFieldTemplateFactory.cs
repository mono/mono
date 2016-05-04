namespace System.Web.DynamicData {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.UI.WebControls;
    using System.Web.Hosting;

    // This FieldTemplateFactory is used for the simple cases where the user doesn't have
    // a FieldTemplate directory but wants to get basic validation logic. In a sense
    // it is a smarter version of a BoundField
    internal class SimpleFieldTemplateFactory : FieldTemplateFactory {
        private static bool? _directoryExists;

        public SimpleFieldTemplateFactory()
            : this(HostingEnvironment.VirtualPathProvider) {
        }

        internal SimpleFieldTemplateFactory(VirtualPathProvider vpp)
            : base(vpp) {
            VirtualPathProvider = vpp;
        }

        internal VirtualPathProvider VirtualPathProvider {
            get;
            set;
        }

        protected virtual bool DirectoryExists {
            get {
                if (!_directoryExists.HasValue) {
                    // This is expensive so cache it.
                    _directoryExists = VirtualPathProvider.DirectoryExists(TemplateFolderVirtualPath);
                }
                return _directoryExists.Value;
            }
        }

        public override IFieldTemplate CreateFieldTemplate(MetaColumn column, DataBoundControlMode mode, string uiHint) {
            // Call Preprocess mode so that we do set the right mode base on the the column's attributes
            mode = PreprocessMode(column, mode);
            bool readOnly = (mode == DataBoundControlMode.ReadOnly);
            // If the folder doesn't exist use the fallback
            if (!DirectoryExists) {
                return CreateFieldTemplate(readOnly, column);
            }

            // Always see check if the base found anything first then fall back to the simple field template
            IFieldTemplate fieldTemplate = base.CreateFieldTemplate(column, mode, uiHint);

            // If there was no field template found and the user specified a uiHint then use the default behavior
            if (!String.IsNullOrEmpty(uiHint)) {
                return fieldTemplate;
            }
            
            return fieldTemplate ?? CreateFieldTemplate(readOnly, column);
        }

        private IFieldTemplate CreateFieldTemplate(bool readOnly, MetaColumn column) {
            // By default we'll support checkbox fields for boolean and a textbox for
            // everything else
            if (column.ColumnType == typeof(bool)) {
                return SimpleFieldTemplateUserControl.CreateBooleanTemplate(readOnly);
            }
            return SimpleFieldTemplateUserControl.CreateTextTemplate(column, readOnly);
        }
    }
}
