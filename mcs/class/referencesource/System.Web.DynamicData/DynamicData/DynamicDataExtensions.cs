namespace System.Web.DynamicData {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using System.Data.Linq;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Web;
    using System.Web.DynamicData.Util;
    using System.Web.Resources;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    /// <summary>
    /// Extension methods used by DynamicData
    /// </summary>
    public static class DynamicDataExtensions {
        public static void SetMetaTable(this INamingContainer control, MetaTable table) {
            SetMetaTableInternal(control, table, null/* defaultValues*/, new HttpContextWrapper(HttpContext.Current));
        }

        public static void SetMetaTable(this INamingContainer control, MetaTable table, IDictionary<string, object> defaultValues) {
            if (defaultValues == null) {
                throw new ArgumentNullException("defaultValues");
            }
            SetMetaTableInternal(control, table, defaultValues, new HttpContextWrapper(HttpContext.Current));
        }

        public static void SetMetaTable(this INamingContainer control, MetaTable table, object defaultValues) {
            if (defaultValues == null) {
                throw new ArgumentNullException("defaultValues");
            }
            SetMetaTableInternal(control, table, Misc.ConvertObjectToDictionary(defaultValues), new HttpContextWrapper(HttpContext.Current));
        }

        public static IDictionary<string, object> GetDefaultValues(this IDataSource dataSource) {
            return GetDefaultValues(dataSource, new HttpContextWrapper(HttpContext.Current));
        }

        public static IDictionary<string, object> GetDefaultValues(this INamingContainer control) {
            return GetDefaultValues(control, new HttpContextWrapper(HttpContext.Current));
        }

        public static MetaTable GetMetaTable(this IDataSource dataSource) {
            return GetMetaTable(dataSource, new HttpContextWrapper(HttpContext.Current));
        }

        public static bool TryGetMetaTable(this IDataSource dataSource, out MetaTable table) {
            return TryGetMetaTable(dataSource, new HttpContextWrapper(HttpContext.Current), out table);
        }

        public static MetaTable GetMetaTable(this INamingContainer control) {
            return GetMetaTable(control, new HttpContextWrapper(HttpContext.Current));
        }

        public static bool TryGetMetaTable(this INamingContainer control, out MetaTable table) {
            return TryGetMetaTable(control, new HttpContextWrapper(HttpContext.Current), out table);
        }

        internal static void ApplyFieldGenerator(INamingContainer control, MetaTable table) {
            GridView gridView = control as GridView;
            if (gridView != null && gridView.AutoGenerateColumns && gridView.ColumnsGenerator == null) {
                gridView.ColumnsGenerator = new DefaultAutoFieldGenerator(table);
            }
            else {
                DetailsView detailsView = control as DetailsView;
                if (detailsView != null && detailsView.AutoGenerateRows && detailsView.RowsGenerator == null) {
                    detailsView.RowsGenerator = new DefaultAutoFieldGenerator(table);
                }
            }
        }

        internal static DefaultValueMapping GetDefaultValueMapping(object control, HttpContextBase context) {
            IDictionary<object, MappingInfo> mapping = MetaTableHelper.GetMapping(context);
            MappingInfo mappingInfo;
            if (mapping.TryGetValue(control, out mappingInfo)) {
                return mappingInfo.DefaultValueMapping;
            }
            return null;
        }

        internal static IDictionary<string, object> GetDefaultValues(object control, HttpContextBase context) {
            DefaultValueMapping mapping = GetDefaultValueMapping(control, context);
            if (mapping != null) {
                return mapping.Values;
            }
            return null;
        }

        internal static MetaTable GetMetaTable(IDataSource dataSource, HttpContextBase context) {
            MetaTable table;
            if (TryGetMetaTable(dataSource, context, out table)) {
                return table;
            }
            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, DynamicDataResources.MetaTable_CannotGetTableFromDataSource));
        }

        internal static bool TryGetMetaTable(IDataSource dataSource, HttpContextBase context, out MetaTable table) {
            if (dataSource == null) {
                throw new ArgumentNullException("dataSource");
            }

            Debug.Assert(context != null);

            table = MetaTableHelper.GetTableFromMapping(context, dataSource);
            if (table == null) {
                var dynamicDataSource = dataSource as IDynamicDataSource;
                if (dynamicDataSource != null) {
                    table = MetaTableHelper.GetTableFromDynamicDataSource(dynamicDataSource);
                }
            }
            return table != null;
        }

        internal static MetaTable GetMetaTable(INamingContainer control, HttpContextBase context) {
            MetaTable table;
            if (!TryGetMetaTable(control, context, out table)) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, DynamicDataResources.MetaTable_CannotGetTableFromControl));
            }
            return table;
        }

        internal static bool TryGetMetaTable(INamingContainer control, HttpContextBase context, out MetaTable table) {
            if (control == null) {
                throw new ArgumentNullException("control");
            }

            table = MetaTableHelper.GetTableFromMapping(context, control);
            return table != null;
        }

        internal static void SetMetaTableInternal(INamingContainer control, MetaTable table, IDictionary<string, object> defaultValues, HttpContextBase context) {
            if (control == null) {
                throw new ArgumentNullException("control");
            }
            if (table == null) {
                throw new ArgumentNullException("table");
            }
            IDataBoundControl dataBoundControl = control as IDataBoundControl;
            IDataSource dataSource = null;
            if (dataBoundControl != null) {
                dataSource = dataBoundControl.DataSourceObject;
            }
            MetaTableHelper.SetTableInMapping(context, control, table, defaultValues);
            if (dataSource != null) {
                // If the control being mapped is a databound control then register its datasource
                MetaTableHelper.SetTableInMapping(context, dataSource, table, defaultValues);
            }
        }

        /// <summary>
        /// Return the MetaTable association with a datasource
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "This is a legacy API and cannot be changed")]
        public static MetaTable GetTable(this IDynamicDataSource dataSource) {
            return MetaTableHelper.GetTableWithFullFallback(dataSource, HttpContext.Current.ToWrapper());
        }

        /// <summary>
        /// Expand Dynamic where parameter (e.g. DynamicControlParameter, DynamicQueryStringParameter) into
        /// 'regular' parameters that the datasource can understand
        /// </summary>
        /// <param name="dataSource">The datasource which Where parameters need to be expanded</param>
        public static void ExpandDynamicWhereParameters(this IDynamicDataSource dataSource) {

            ParameterCollection whereParameters = dataSource.WhereParameters;

            // First, check if any parameters need to be expanded
            bool needProcessing = false;
            foreach (Parameter parameter in whereParameters) {
                if (parameter is IWhereParametersProvider) {
                    needProcessing = true;
                    break;
                }
            }

            // If not, don't do anything
            if (!needProcessing)
                return;

            // Make a copy of the parameters, and clear the collection
            var whereParametersCopy = new Parameter[whereParameters.Count];
            whereParameters.CopyTo(whereParametersCopy, 0);
            whereParameters.Clear();

            // Go through all the parameters and expand them
            foreach (Parameter parameter in whereParametersCopy) {
                ExpandWhereParameter(dataSource, parameter);
            }
        }

        private static void ExpandWhereParameter(IDynamicDataSource dataSource, Parameter parameter) {
            var provider = parameter as IWhereParametersProvider;
            if (provider == null) {
                // If it's a standard parameter, just add it
                dataSource.WhereParameters.Add(parameter);
            }
            else {
                // Get the list of sub-parameters and expand them recursively
                IEnumerable<Parameter> newParameters = provider.GetWhereParameters(dataSource);
                foreach (Parameter newParameter in newParameters) {
                    ExpandWhereParameter(dataSource, newParameter);
                }
            }
        }

        /// <summary>
        /// Find the containing data control, and return the data source it points to
        /// </summary>
        public static IDynamicDataSource FindDataSourceControl(this Control current) {
            return DataControlHelper.FindDataSourceControl(current);
        }

        /// <summary>
        /// Find the containing data control, and return the MetaTable associated with it, if any
        /// </summary>
        public static MetaTable FindMetaTable(this Control current) {
            return MetaTableHelper.FindMetaTable(current, HttpContext.Current.ToWrapper());
        }

        /// <summary>
        /// Find the field template for a column within the current naming container
        /// </summary>
        public static Control FindFieldTemplate(this Control control, string columnName) {
            return control.FindControl(DynamicControl.GetControlIDFromColumnName(columnName));
        }

        /// <summary>
        /// Make the SelectedIndex [....] up with the PersistedSelection. Concretely, what it means is that
        /// if you select a row and then page away (or sort), the selection remains on that row
        /// even if it's not currently visible.
        /// </summary>
        [Obsolete("Use the EnablePersistedSelection property on a databound control such as GridView or ListView.")]
        public static void EnablePersistedSelection(this BaseDataBoundControl dataBoundControl) {
            EnablePersistedSelectionInternal(dataBoundControl);
        }

        internal static void EnablePersistedSelectionInternal(BaseDataBoundControl dataBoundControl) {
            IDataBoundListControl dataBoundListControl = dataBoundControl as IDataBoundListControl;
            if (dataBoundListControl != null) {
                dataBoundListControl.EnablePersistedSelection = true;
                // 

                if (dataBoundListControl.SelectedIndex < 0) {
                    // Force the first item to be selected
                    dataBoundListControl.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        /// Set the DataLoadOptions on a Linq To Sql datasource to force all the FK entities
        /// to be directly loaded.
        /// </summary>
        /// <param name="dataSource">The data source for which we want to preload FKs</param>
        /// <param name="rowType">The type of the entities returned by the data source</param>
        public static void LoadWithForeignKeys(this LinqDataSource dataSource, Type rowType) {
            dataSource.ContextCreated += delegate(object sender, LinqDataSourceStatusEventArgs e) {
                // This only applies to a DLinq data context
                var context = e.Result as DataContext;
                if (context == null)
                    return;

                DataLoadOptions loadOptions = null;
                ParameterExpression tableParameter = null;
                System.Data.Linq.Mapping.MetaTable metaTable = context.Mapping.GetTable(rowType);
                foreach (System.Data.Linq.Mapping.MetaDataMember member in metaTable.RowType.DataMembers) {
                    if (member.IsAssociation && !member.Association.IsMany) {
                        if (member.Type.Equals(rowType)) continue;
                        if (loadOptions == null) {
                            loadOptions = new DataLoadOptions();
                            tableParameter = Expression.Parameter(rowType, "e");
                        }
                        var memberExpression = Expression.Property(tableParameter, member.Name);
                        loadOptions.LoadWith(Expression.Lambda(memberExpression, tableParameter));
                    }
                }

                if (loadOptions != null) {
                    context.LoadOptions = loadOptions;
                }
            };
        }

        public static void LoadWith<TEntity>(this LinqDataSource dataSource) {
            LoadWithForeignKeys(dataSource, typeof(TEntity));
        }

        /// <summary>
        /// Apply potential HTML encoding and formatting to a string that needs to be displayed
        /// This logic is mostly copied from BoundField.FormatDataValue, but omits the old Whidbey behavior path
        /// </summary>
        /// <param name="fieldValue">The value that should be formatted</param>
        /// <param name="formattingOptions">The IFieldFormattingOptions to use. This is useful when using options different from the column's</param>
        /// <returns>the formatted value</returns>
        public static string FormatValue(this IFieldFormattingOptions formattingOptions, object fieldValue) {

            string formattedValue = String.Empty;

            if (fieldValue != null) {
                string dataValueString = fieldValue.ToString();
                string formatting = formattingOptions.DataFormatString;
                int dataValueStringLength = dataValueString.Length;

                // If the result is still empty and ConvertEmptyStringToNull=true, replace the value with the NullDisplayText
                if (dataValueStringLength == 0 && formattingOptions.ConvertEmptyStringToNull) {
                    dataValueString = formattingOptions.NullDisplayText;
                }
                else {
                    // If there's a format string, apply it to the raw data value
                    // If there's no format string, then dataValueString already has the right value
                    if (!String.IsNullOrEmpty(formatting)) {
                        dataValueString = String.Format(CultureInfo.CurrentCulture, formatting, fieldValue);
                    }

                    // Optionally HTML encode the value (including the format string, if any was applied)
                    if (!String.IsNullOrEmpty(dataValueString) && formattingOptions.HtmlEncode) {
                        dataValueString = HttpUtility.HtmlEncode(dataValueString);
                    }
                }

                formattedValue = dataValueString;
            }
            else {
                formattedValue = formattingOptions.NullDisplayText;
            }

            return formattedValue;
        }

        /// <summary>
        /// Similar to FormatValue, but the string is to be used when the field is in edit mode
        /// </summary>
        public static string FormatEditValue(this IFieldFormattingOptions formattingOptions, object fieldValue) {
            string valueString;

            // Apply the format string to it if that flag is set.  Otherwise use it as is.
            if (formattingOptions.ApplyFormatInEditMode) {
                valueString = formattingOptions.FormatValue(fieldValue);
            }
            else {
                valueString = (fieldValue != null) ? fieldValue.ToString() : String.Empty;
            }

            // Trim any trailing spaces as they cause unwanted behavior (since we limit the input length and the
            // spaces cause the limit to be reach prematurely)
            valueString = valueString.TrimEnd();

            return valueString;
        }

        /// <summary>
        /// Return either the input value or null based on ConvertEmptyStringToNull and NullDisplayText
        /// </summary>
        /// <param name="formattingOptions">the formatting options object</param>
        /// <param name="value">The input value</param>
        /// <returns>The converted value</returns>
        public static object ConvertEditedValue(this IFieldFormattingOptions formattingOptions, string value) {
            // If it's an empty string and ConvertEmptyStringToNull is set, make it null
            if (String.IsNullOrEmpty(value) && formattingOptions.ConvertEmptyStringToNull) {
                return null;
            }

            // If it's the NullDisplayText, return null
            string nullDisplayText = formattingOptions.NullDisplayText;
            if (value == nullDisplayText && !String.IsNullOrEmpty(nullDisplayText)) {
                return null;
            }

            // Otherwise, return it unchanged
            return value;
        }

        /// <summary>
        /// If this column represents an enumeration type, this method returns that type. The caloumn can represent
        /// an enumeration type if the underlying type is an enum, or if it is decoareted with EnumDataTypeAttribute.
        /// If this column does not represent an enum, this method returns null.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "The interface is internal")]
        public static Type GetEnumType(this MetaColumn column) {
            return GetEnumType((IMetaColumn)column);
        }

        internal static Type GetEnumType(this IMetaColumn column) {
            return column.Attributes.GetAttributePropertyValue<EnumDataTypeAttribute, Type>(a => a.EnumType, null) ??
                (column.ColumnType.IsEnum ? column.ColumnType : null);
        }

        internal static bool IsEnumType(this MetaColumn column, out Type enumType) {
            enumType = column.GetEnumType();
            return enumType != null;
        }
    }
}
