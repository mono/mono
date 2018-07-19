using System.Diagnostics;
using System.Globalization;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Web.UI.WebControls;

namespace System.Web.DynamicData {
    public class EntityTemplateFactory {
        private const string s_defaultTemplateName = "Default";
        
        private TemplateFactory _factory;
        private Func<string, EntityTemplateUserControl> _templateInstantiator;

        public EntityTemplateFactory()
            : this(CreateEntityTemplateInstance, /* trackChanges */ true) {
        }

        // for unit testing
        internal EntityTemplateFactory(Func<string, EntityTemplateUserControl> templateInstantiator, VirtualPathProvider vpp)
            : this(templateInstantiator, /* trackChanges */ false) {
            _factory.VirtualPathProvider = vpp;
        }

        private EntityTemplateFactory(Func<string, EntityTemplateUserControl> templateInstantiator, bool trackChanges) {
            _factory = new TemplateFactory("EntityTemplates", trackChanges);
            _templateInstantiator = templateInstantiator;
        }

        internal string TemplateFolderVirtualPath {
            get {
                return _factory.TemplateFolderVirtualPath;
            }
            set {
                _factory.TemplateFolderVirtualPath = value;
            }
        }

        private static EntityTemplateUserControl CreateEntityTemplateInstance(string path) {
            return (EntityTemplateUserControl)BuildManager.CreateInstanceFromVirtualPath(
                    path, typeof(EntityTemplateUserControl));
        }

        public virtual EntityTemplateUserControl CreateEntityTemplate(MetaTable table, DataBoundControlMode mode, string uiHint) {
            if (table == null) {
                throw new ArgumentNullException("table");
            }

            string entityTemplatePath = GetEntityTemplateVirtualPathWithCaching(table, mode, uiHint);
            if (entityTemplatePath == null) {
                return null;
            }

            return _templateInstantiator(entityTemplatePath);
        }

        private string GetEntityTemplateVirtualPathWithCaching(MetaTable table, DataBoundControlMode mode, string uiHint) {
            long cacheKey = Misc.CombineHashCodes(table, mode, uiHint);

            return _factory.GetTemplatePath(cacheKey, delegate() {
                return GetEntityTemplateVirtualPath(table, mode, uiHint);
            });
        }

        public virtual string GetEntityTemplateVirtualPath(MetaTable table, DataBoundControlMode mode, string uiHint) {
            if (table == null) {
                throw new ArgumentNullException("table");
            }

            // Fallback order is as follows (where CustomProducts is the uiHint)
            //    CustomProducts_Insert
            //    CustomProducts_Edit
            //    Products_Insert
            //    Products_Edit
            //    Default_Insert
            //    Default_Edit
            //    CustomProducts_ReadOnly
            //    Products_ReadOnly
            //    Default_ReadOnly
            //
            // If nothing matches null is returned

            return GetVirtualPathFallback(table, mode, uiHint, DataBoundControlMode.Edit) ??
                GetVirtualPathFallback(table, DataBoundControlMode.ReadOnly, uiHint, DataBoundControlMode.ReadOnly);
        }

        private string GetVirtualPathFallback(MetaTable table, DataBoundControlMode mode, string uiHint, DataBoundControlMode minModeToFallBack) {
            if (mode < minModeToFallBack) {
                return null;
            }

            // the strategy is to go over each candidate name and try to find an existing template for
            // each mode between 'mode' and 'minModeToFallBack'
            // note that GetVirtualPathForMode will return null for empty names (e.g. when the uiHint is not specified)
            string[] fallbackNames = new string[] { uiHint, table.Name, s_defaultTemplateName };
            foreach (var name in fallbackNames) {
                for (var currentMode = mode; currentMode >= minModeToFallBack; currentMode--) {
                    string virtualPath = GetVirtualPathForMode(name, currentMode);
                    if (virtualPath != null) {
                        return virtualPath;
                    }
                }
            }
            return null;
        }

        private string GetVirtualPathForMode(string candidateName, DataBoundControlMode mode) {
            if (String.IsNullOrEmpty(candidateName)) {
                return null;
            } else {
                string templatePath = BuildEntityTemplateVirtualPath(candidateName, mode);
                return _factory.FileExists(templatePath) ? templatePath : null;
            }
        }

        public virtual string BuildEntityTemplateVirtualPath(string templateName, DataBoundControlMode mode) {
            if (templateName == null) {
                throw new ArgumentNullException("templateName");
            }

            string modeString = mode == DataBoundControlMode.ReadOnly ? String.Empty : "_" + mode.ToString();
            return String.Format(CultureInfo.InvariantCulture, this.TemplateFolderVirtualPath + "{0}{1}.ascx", templateName, modeString);
        }

        internal void Initialize(MetaModel model) {
            Debug.Assert(model != null);
            _factory.Model = model;
        }
    }
}
