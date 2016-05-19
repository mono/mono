using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Spatial;
using System.Diagnostics;
using System.Globalization;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Web.UI.WebControls;

namespace System.Web.DynamicData {
    /// <summary>
    /// Default implementation of IFieldTemplateFactory. It uses user controls for the field templates.
    /// </summary>
    public class FieldTemplateFactory : IFieldTemplateFactory {
        private const string IntegerField = "Integer";
        private const string ForeignKeyField = "ForeignKey";
        private const string ChildrenField = "Children";
        private const string ManyToManyField = "ManyToMany";
        private const string EnumerationField = "Enumeration";

        private const string EditModePathModifier = "_Edit";
        private const string InsertModePathModifier = "_Insert";

        private Dictionary<Type, string> _typesToTemplateNames;
        private Dictionary<Type, Type> _typesFallBacks;

        private TemplateFactory _templateFactory;

        /// <summary>
        /// </summary>
        public FieldTemplateFactory() {
            InitTypesToTemplateNamesTable();
            BuildTypeFallbackTable();
            _templateFactory = new TemplateFactory("FieldTemplates");
        }

        // For unit test purpose
        internal FieldTemplateFactory(VirtualPathProvider vpp)
            : this() {
            _templateFactory.VirtualPathProvider = vpp;
        }

        /// <summary>
        /// Sets the folder containing the user controls. By default, this is ~/DynamicData/FieldTemplates/
        /// </summary>
        public string TemplateFolderVirtualPath {
            get {
                return _templateFactory.TemplateFolderVirtualPath;
            }
            set {
                _templateFactory.TemplateFolderVirtualPath = value;
            }
        }

        /// <summary>
        /// The MetaModel that the factory is associated with
        /// </summary>
        public MetaModel Model {
            get {
                return _templateFactory.Model;
            }
            private set {
                _templateFactory.Model = value;
            }
        }

        private void InitTypesToTemplateNamesTable() {
            _typesToTemplateNames = new Dictionary<Type, string>();
            _typesToTemplateNames[typeof(int)] = FieldTemplateFactory.IntegerField;
            _typesToTemplateNames[typeof(string)] = DataType.Text.ToString();
        }

        private void BuildTypeFallbackTable() {
            _typesFallBacks = new Dictionary<Type, Type>();
            _typesFallBacks[typeof(float)] = typeof(decimal);
            _typesFallBacks[typeof(double)] = typeof(decimal);
            _typesFallBacks[typeof(Int16)] = typeof(int);
            _typesFallBacks[typeof(byte)] = typeof(int);
            _typesFallBacks[typeof(long)] = typeof(int);

            // Fall back to strings for most types
            _typesFallBacks[typeof(char)] = typeof(string);
            _typesFallBacks[typeof(int)] = typeof(string);
            _typesFallBacks[typeof(decimal)] = typeof(string);
            _typesFallBacks[typeof(Guid)] = typeof(string);
            _typesFallBacks[typeof(DateTime)] = typeof(string);
            _typesFallBacks[typeof(DateTimeOffset)] = typeof(string);
            _typesFallBacks[typeof(TimeSpan)] = typeof(string);
            _typesFallBacks[typeof(DbGeography)] = typeof(string);
            _typesFallBacks[typeof(DbGeometry)] = typeof(string);

            // 
        }

        private Type GetFallBackType(Type t) {
            // Check if there is a fallback type
            Type fallbackType;
            if (_typesFallBacks.TryGetValue(t, out fallbackType))
                return fallbackType;

            return null;
        }

        // Internal for unit test purpose
        internal string GetFieldTemplateVirtualPathWithCaching(MetaColumn column, DataBoundControlMode mode, string uiHint) {
            // Compute a cache key based on all the input paramaters
            long cacheKey = Misc.CombineHashCodes(uiHint, column, mode);

            Func<string> templatePathFactoryFunction = () => GetFieldTemplateVirtualPath(column, mode, uiHint);

            return _templateFactory.GetTemplatePath(cacheKey, templatePathFactoryFunction);
        }

        /// <summary>
        /// Returns the virtual path of the field template user control to be used, based on various pieces of data
        /// </summary>
        /// <param name="column">The MetaColumn for which the field template is needed</param>
        /// <param name="mode">The mode (Readonly, Edit, Insert) for which the field template is needed</param>
        /// <param name="uiHint">The UIHint (if any) that should affect the field template lookup</param>
        /// <returns></returns>
        public virtual string GetFieldTemplateVirtualPath(MetaColumn column, DataBoundControlMode mode, string uiHint) {

            mode = PreprocessMode(column, mode);

            bool hasDataTypeAttribute = column != null && column.DataTypeAttribute != null;

            // Set the UIHint in some special cases, but don't do it if we already have one or
            // if we have a DataTypeAttribute
            if (String.IsNullOrEmpty(uiHint) && !hasDataTypeAttribute) {
                // Check if it's an association
                // Or if it is an enum
                if (column is MetaForeignKeyColumn) {
                    uiHint = FieldTemplateFactory.ForeignKeyField;
                } else if (column is MetaChildrenColumn) {
                    var childrenColumn = (MetaChildrenColumn)column;
                    if (childrenColumn.IsManyToMany) {
                        uiHint = FieldTemplateFactory.ManyToManyField;
                    }
                    else {
                        uiHint = FieldTemplateFactory.ChildrenField;
                    }
                } else if (column.ColumnType.IsEnum) {
                    uiHint = FieldTemplateFactory.EnumerationField;
                }
            }

            return GetVirtualPathWithModeFallback(uiHint, column, mode);
        }

        /// <summary>
        /// Gets a chance to change the mode.  e.g. an Edit mode request can be turned into ReadOnly mode
        /// if the column is a primary key
        /// </summary>
        public virtual DataBoundControlMode PreprocessMode(MetaColumn column, DataBoundControlMode mode) {
            if (column == null) {
                throw new ArgumentNullException("column");
            }

            // Primary keys can't be edited, so put them in readonly mode.  Note that this
            // does not apply to Insert mode, which is fine
            if (column.IsPrimaryKey && mode == DataBoundControlMode.Edit) {
                mode = DataBoundControlMode.ReadOnly;
            }

            // Generated columns should never be editable/insertable
            if (column.IsGenerated) {
                mode = DataBoundControlMode.ReadOnly;
            }

            // ReadOnly columns cannot be edited nor inserted, and are always in Display mode
            if (column.IsReadOnly) {
                if (mode == DataBoundControlMode.Insert && column.AllowInitialValue) {
                    // but don't change the mode if we're in insert and an initial value is allowed
                } else {
                    mode = DataBoundControlMode.ReadOnly;
                }
            }

            // If initial value is not allowed set mode to ReadOnly
            if (mode == DataBoundControlMode.Insert && !column.AllowInitialValue) {
                mode = DataBoundControlMode.ReadOnly;
            }

            if (column is MetaForeignKeyColumn) {
                // If the foreign key is part of the primary key (e.g. Order and Product in Order_Details table),
                // change the mode to ReadOnly so that they can't be edited.
                if (mode == DataBoundControlMode.Edit && ((MetaForeignKeyColumn)column).IsPrimaryKeyInThisTable) {
                    mode = DataBoundControlMode.ReadOnly;
                }
            }

            return mode;
        }

        private string GetVirtualPathWithModeFallback(string templateName, MetaColumn column, DataBoundControlMode mode) {
            // Try not only the requested mode, but others if needed.  Basically:
            // - an edit template can default to an item template
            // - an insert template can default to an edit template, then to an item template
            for (var currentMode = mode; currentMode >= 0; currentMode--) {
                string virtualPath = GetVirtualPathForMode(templateName, column, currentMode);
                if (virtualPath != null)
                    return virtualPath;
            }

            // We couldn't locate any field template at all, so give up
            return null;
        }

        private string GetVirtualPathForMode(string templateName, MetaColumn column, DataBoundControlMode mode) {

            // If we got a template name, try it
            if (!String.IsNullOrEmpty(templateName)) {
                string virtualPath = GetVirtualPathIfExists(templateName, column, mode);
                if (virtualPath != null)
                    return virtualPath;
            }

            // Otherwise, use the column's type
            return GetVirtualPathForTypeWithFallback(column.ColumnType, column, mode);
        }

        private string GetVirtualPathForTypeWithFallback(Type fieldType, MetaColumn column, DataBoundControlMode mode) {

            string templateName;
            string virtualPath;

            // If we have a data type attribute
            if (column.DataTypeAttribute != null) {
                templateName = column.DataTypeAttribute.GetDataTypeName();

                // Try to get the path from it
                virtualPath = GetVirtualPathIfExists(templateName, column, mode);
                if (virtualPath != null)
                    return virtualPath;
            }

            // Try the actual fully qualified type name (i.e. with the namespace)
            virtualPath = GetVirtualPathIfExists(fieldType.FullName, column, mode);
            if (virtualPath != null)
                return virtualPath;

            // Try the simple type name
            virtualPath = GetVirtualPathIfExists(fieldType.Name, column, mode);
            if (virtualPath != null)
                return virtualPath;

            // If our type name table has an entry for it, try it
            if (_typesToTemplateNames.TryGetValue(fieldType, out templateName)) {
                virtualPath = GetVirtualPathIfExists(templateName, column, mode);
                if (virtualPath != null)
                    return virtualPath;
            }

            // Check if there is a fallback type
            Type fallbackType = GetFallBackType(fieldType);

            // If not, we've run out of options
            if (fallbackType == null)
                return null;

            // If so, try it
            return GetVirtualPathForTypeWithFallback(fallbackType, column, mode);
        }

        private string GetVirtualPathIfExists(string templateName, MetaColumn column, DataBoundControlMode mode) {
            // Build the path
            string virtualPath = BuildVirtualPath(templateName, column, mode);

            // Check if it exists
            if (_templateFactory.FileExists(virtualPath))
                return virtualPath;

            // If not, return null
            return null;
        }

        /// <summary>
        /// Build the virtual path to the field template user control based on the template name and mode.
        /// By default, it returns names that look like TemplateName_ModeName.ascx, in the folder specified
        /// by TemplateFolderVirtualPath.
        /// </summary>
        /// <param name="templateName"></param>
        /// <param name="column"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public virtual string BuildVirtualPath(string templateName, MetaColumn column, DataBoundControlMode mode) {
            if (String.IsNullOrEmpty(templateName)) {
                throw new ArgumentNullException("templateName");
            }

            string modePathModifier = null;
            switch (mode) {
                case DataBoundControlMode.ReadOnly:
                    modePathModifier = String.Empty;
                    break;
                case DataBoundControlMode.Edit:
                    modePathModifier = FieldTemplateFactory.EditModePathModifier;
                    break;
                case DataBoundControlMode.Insert:
                    modePathModifier = FieldTemplateFactory.InsertModePathModifier;
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }

            return String.Format(CultureInfo.InvariantCulture,
                TemplateFolderVirtualPath + "{0}{1}.ascx", templateName, modePathModifier);
        }

        #region IFieldTemplateFactory Members

        public virtual void Initialize(MetaModel model) {
            Model = model;
        }

        /// <summary>
        /// See IFieldTemplateFactory for details.
        /// </summary>
        /// <returns></returns>
        public virtual IFieldTemplate CreateFieldTemplate(MetaColumn column, DataBoundControlMode mode, string uiHint) {
            string fieldTemplatePath = GetFieldTemplateVirtualPathWithCaching(column, mode, uiHint);

            if (fieldTemplatePath == null)
                return null;

            return (IFieldTemplate)BuildManager.CreateInstanceFromVirtualPath(
                fieldTemplatePath, typeof(IFieldTemplate));
        }

        #endregion
    }
}
