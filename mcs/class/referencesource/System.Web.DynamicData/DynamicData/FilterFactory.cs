using System.Diagnostics;
using System.Globalization;
using System.Web.Compilation;
using System.Web.Hosting;

namespace System.Web.DynamicData {
    public class FilterFactory {
        private const string s_defaultFiltersFolder = "Filters";

        private const string s_booleanFilter = "Boolean";
        private const string s_foreignKeyFilter = "ForeignKey";
        private const string s_enumerationFilter = "Enumeration";

        private TemplateFactory _templateFactory;

        public FilterFactory() {
            _templateFactory = new TemplateFactory(s_defaultFiltersFolder);
        }

        // for testing purposes
        internal FilterFactory(VirtualPathProvider vpp)
            : this() {
            _templateFactory.VirtualPathProvider = vpp;
        }

        internal string FilterFolderVirtualPath {
            get {
                return _templateFactory.TemplateFolderVirtualPath;
            }
            set {
                _templateFactory.TemplateFolderVirtualPath = value;
            }
        }

        internal void Initialize(MetaModel model) {
            Debug.Assert(model != null);
            _templateFactory.Model = model;
        }

        private string GetDefaultFilterControlName(MetaColumn column) {
            if (column is MetaForeignKeyColumn) {
                return s_foreignKeyFilter;
            }
            else if (column.ColumnType == typeof(bool)) {
                return s_booleanFilter;
            }
            else if (column.GetEnumType() != null) {
                return s_enumerationFilter;
            }
            else {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    Resources.DynamicDataResources.FilterFactory_ColumnHasNoDefaultFilter,
                    column.Name,
                    column.Table.Name));
            }
        }

        public virtual QueryableFilterUserControl CreateFilterControl(MetaColumn column, string filterUIHint) {
            if (column == null) {
                throw new ArgumentNullException("column");
            }
            string filterTemplatePath = GetFilterVirtualPathWithCaching(column, filterUIHint);
            Debug.Assert(filterTemplatePath != null);

            QueryableFilterUserControl filter = (QueryableFilterUserControl)BuildManager.CreateInstanceFromVirtualPath(
                filterTemplatePath, typeof(QueryableFilterUserControl));

            return filter;
        }

        // internal for unit testing
        internal string GetFilterVirtualPathWithCaching(MetaColumn column, string filterUIHint) {
            Debug.Assert(column != null);
            long cacheKey = Misc.CombineHashCodes(column, filterUIHint);

            return _templateFactory.GetTemplatePath(cacheKey, delegate() {
                return GetFilterVirtualPath(column, filterUIHint);
            });
        }

        public virtual string GetFilterVirtualPath(MetaColumn column, string filterUIHint) {
            if (column == null) {
                throw new ArgumentNullException("column");
            }

            string filterControlName = BuildFilterVirtualPath(column, filterUIHint);
            string filterTemplatePath = VirtualPathUtility.Combine(FilterFolderVirtualPath, filterControlName + ".ascx");
            return filterTemplatePath;
        }

        private string BuildFilterVirtualPath(MetaColumn column, string filterUIHint) {
            string filterControlName = null;
            if (!String.IsNullOrEmpty(filterUIHint)) {
                filterControlName = filterUIHint;
            }
            else if (!String.IsNullOrEmpty(column.FilterUIHint)) {
                filterControlName = column.FilterUIHint;
            }

            filterControlName = filterControlName ?? GetDefaultFilterControlName(column);
            return filterControlName;
        }
    }
}
