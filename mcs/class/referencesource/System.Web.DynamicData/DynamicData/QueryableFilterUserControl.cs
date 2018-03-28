using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Collections;
using System.Web.UI.WebControls.Expressions;
using System.Web.DynamicData.Util;

namespace System.Web.DynamicData {
    public abstract class QueryableFilterUserControl : UserControl {
        private HttpContextBase _context;
        private DefaultValueMapping _defaultValueMapping;

        // internal for unit tests
        internal protected MetaColumn Column { get; private set; }
        private IQueryableDataSource QueryableDataSource { get; set; }

        public abstract IQueryable GetQueryable(IQueryable source);

        internal void Initialize(MetaColumn column, IQueryableDataSource iQueryableDataSource, HttpContextBase context) {
            QueryableDataSource = iQueryableDataSource;
            Column = column;
            _context = context ?? new HttpContextWrapper(Context);
        }

        public event EventHandler FilterChanged;

        /// <summary>
        /// Returns the data control that handles the filter inside the filter template. Can be null if the filter template does not override it.
        /// </summary>
        public virtual Control FilterControl {
            get {
                return null;
            }
        }

        /// <summary>
        /// Populate a ListControl with all the items in the foreign table (or true/false for boolean fields)
        /// </summary>
        /// <param name="listControl"></param>
        public void PopulateListControl(ListControl listControl) {
            Type enumType;
            if (Column is MetaForeignKeyColumn) {
                MetaTable FilterTable = ((MetaForeignKeyColumn)Column).ParentTable;
                Misc.FillListItemCollection(FilterTable, listControl.Items);
            }
            else if (Column.IsEnumType(out enumType)) {
                Debug.Assert(enumType != null);
                FillEnumListControl(listControl, enumType);
            }
        }

        private void FillEnumListControl(ListControl list, Type enumType) {
            foreach (DictionaryEntry entry in Misc.GetEnumNamesAndValues(enumType)) {
                list.Items.Add(new ListItem((string)entry.Key, (string)entry.Value));
            }
        }

        /// <summary>
        /// Raises the FilterChanged event. This is necessary to notify the data source that the filter selection
        /// has changed and the query needs to be reevaluated.
        /// </summary>
        protected void OnFilterChanged() {
            // Clear the default value for this column
            ClearDefaultValues();

            EventHandler eventHandler = FilterChanged;
            if (eventHandler != null) {
                eventHandler(this, EventArgs.Empty);
            }
            QueryableDataSource.RaiseViewChanged();
        }

        private void ClearDefaultValues() {
            IDictionary<string, object> defaultValues = DefaultValues;
            if (defaultValues != null) {
                MetaForeignKeyColumn foreignKeyColumn = Column as MetaForeignKeyColumn;
                if (foreignKeyColumn != null) {
                    foreach (var fkName in foreignKeyColumn.ForeignKeyNames) {
                        defaultValues.Remove(fkName);
                    }
                }
                else {
                    defaultValues.Remove(Column.Name);
                }
            }
        }

        public string DefaultValue {
            get {
                IDictionary<string, object> defaultValues = DefaultValues;
                if (defaultValues == null || !DefaultValueMapping.Contains(Column)) {
                    return null;
                }

                MetaForeignKeyColumn foreignKeyColumn = Column as MetaForeignKeyColumn;
                if (foreignKeyColumn != null) {
                    return foreignKeyColumn.GetForeignKeyString(DefaultValueMapping.Instance);
                }

                object value;
                if (defaultValues.TryGetValue(Column.Name, out value)) {
                    return Misc.ChangeType<string>(value);
                }
                return null;
            }
        }

        public IDictionary<string, object> DefaultValues {
            get {
                // Get the default values mapped for this datasource if any
                if (DefaultValueMapping != null) {
                    return DefaultValueMapping.Values;
                }
                return null;
            }
        }

        private DefaultValueMapping DefaultValueMapping {
            get {
                if (_defaultValueMapping == null) {
                    Debug.Assert(_context != null);
                    _defaultValueMapping = DynamicDataExtensions.GetDefaultValueMapping(QueryableDataSource, _context);
                }
                return _defaultValueMapping;
            }
        }

        // Method copied from ExpressionHelper
        private static Expression CreatePropertyExpression(Expression parameterExpression, string propertyName) {
            Expression propExpression = null;
            string[] props = propertyName.Split('.');
            foreach (var p in props) {
                if (propExpression == null) {
                    propExpression = Expression.PropertyOrField(parameterExpression, p);
                }
                else {
                    propExpression = Expression.PropertyOrField(propExpression, p);
                }
            }
            return propExpression;
        }

        public static IQueryable ApplyEqualityFilter(IQueryable source, string propertyName, object value) {
            ParameterExpression parameterExpression = Expression.Parameter(source.ElementType, String.Empty);
            Expression propertyExpression = CreatePropertyExpression(parameterExpression, propertyName);
            if (Nullable.GetUnderlyingType(propertyExpression.Type) != null && value != null) {
                propertyExpression = Expression.Convert(propertyExpression, Misc.RemoveNullableFromType(propertyExpression.Type));
            }
            value = Misc.ChangeType(value, propertyExpression.Type);
            Expression compareExpression = Expression.Equal(propertyExpression, Expression.Constant(value));
            LambdaExpression lambda = Expression.Lambda(compareExpression, parameterExpression);
            MethodCallExpression whereCall = Expression.Call(typeof(Queryable), "Where", new Type[] { source.ElementType }, source.Expression, Expression.Quote(lambda));
            return source.Provider.CreateQuery(whereCall);
        }
    }
}
