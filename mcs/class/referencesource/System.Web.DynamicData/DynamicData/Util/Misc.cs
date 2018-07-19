using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.DynamicData.Util;
using System.Web.Resources;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace System.Web.DynamicData {
    internal static class Misc {
        public static HttpContextWrapper ToWrapper(this HttpContext context) {
            return new HttpContextWrapper(context);
        }

        public static object GetRealDataItem(object dataItem) {
            if (dataItem is ICustomTypeDescriptor) {
                // Unwrap EF object
                dataItem = ((ICustomTypeDescriptor)dataItem).GetPropertyOwner(null);
            }
            return dataItem;
        }

        // Walks the type hierachy up to endingType (assuming startingType is a subtype of starting type)
        // trying to find a meta table.
        public static MetaTable GetTableFromTypeHierarchy(Type entityType) {
            if (entityType == null) {
                throw new ArgumentNullException("entityType");
            }

            Type type = entityType;
            while (type != null) {
                MetaTable table;
                if (MetaTable.TryGetTable(type, out table)) {
                    return table;
                }
                type = type.BaseType;
            }

            return null;
        }

        public static Type RemoveNullableFromType(Type type) {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        internal static bool IsColumnInDictionary(IMetaColumn column, IDictionary<string, object> values) {
            if (column == null) {
                throw new ArgumentNullException("column");
            }
            if (values == null) {
                throw new ArgumentNullException("values");
            }
            IMetaForeignKeyColumn foreignKeyColumn = column as IMetaForeignKeyColumn;
            if (foreignKeyColumn != null) {
                return foreignKeyColumn.ForeignKeyNames.All(fkName => values.ContainsKey(fkName));
            }
            return values.ContainsKey(column.Name);
        }

        internal static IDictionary<string, object> ConvertObjectToDictionary(object instance) {
            if (instance == null) {
                throw new ArgumentNullException("instance");
            }
            Dictionary<string, object> values = new Dictionary<string, object>();
            var props = TypeDescriptor.GetProperties(instance);
            foreach (PropertyDescriptor p in props) {
                values[p.Name] = p.GetValue(instance);
            }
            return values;
        }

        public static T ChangeType<T>(object value) {
            return (T)ChangeType(value, typeof(T));
        }

        public static object ChangeType(object value, Type type) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            if (value == null) {
                if (TypeAllowsNull(type)) {
                    return null;
                }
                return Convert.ChangeType(value, type, CultureInfo.CurrentCulture);
            }

            type = RemoveNullableFromType(type);

            if (value.GetType() == type) {
                return value;
            }

            TypeConverter converter = TypeDescriptor.GetConverter(type);
            if (converter.CanConvertFrom(value.GetType())) {
                return converter.ConvertFrom(value);
            }

            TypeConverter otherConverter = TypeDescriptor.GetConverter(value.GetType());
            if (otherConverter.CanConvertTo(type)) {
                return otherConverter.ConvertTo(value, type);
            }

            throw new InvalidOperationException(String.Format(
                            CultureInfo.CurrentCulture,
                            DynamicDataResources.Misc_CannotConvertType, value.GetType(), type));
        }

        internal static bool TypeAllowsNull(Type type) {
            return Nullable.GetUnderlyingType(type) != null || !type.IsValueType;
        }

        public static ContainerType FindContainerType(Control control) {
            if (control == null) {
                throw new ArgumentNullException("control");
            }

            Control container = control;
            // Walk up NamingContainers until we find one of the DataBound control interfaces
            while (container != null) {
                if (container is IDataBoundItemControl) {
                    return ContainerType.Item;
                }
                else if (container is IDataBoundListControl || container is Repeater) {
                    return ContainerType.List;
                }
                container = container.NamingContainer;
            }
            // Default container type is a list if none of the known 
            // interfaces are found
            return ContainerType.List;
        }

        public static IOrderedDictionary GetEnumNamesAndValues(Type enumType) {
            Debug.Assert(enumType != null);
            Debug.Assert(enumType.IsEnum);
            OrderedDictionary result = new OrderedDictionary();
            var enumEntries = from e in Enum.GetValues(enumType).OfType<object>()
                              select new EnumEntry {
                                  // 
                                  Name = Enum.GetName(enumType, e),
                                  UnderlyingValue = GetUnderlyingTypeValue(enumType, e)
                              };
            foreach (var entry in enumEntries.OrderBy(e => e.UnderlyingValue)) {
                result.Add(entry.Name, entry.UnderlyingValue.ToString());
            }
            return result;
        }

        private struct EnumEntry {
            public string Name { get; set; }
            public object UnderlyingValue { get; set; }
        }

        public static object GetUnderlyingTypeValue(Type enumType, object enumValue) {
            return Convert.ChangeType(enumValue, Enum.GetUnderlyingType(enumType), CultureInfo.InvariantCulture);
        }

        public static string GetUnderlyingTypeValueString(Type enumType, object enumValue) {
            return GetUnderlyingTypeValue(enumType, enumValue).ToString();
        }

        public static string PersistListToCommaSeparatedString(IList<object> list) {
            // Special case empty and single lists
            if (list == null || list.Count == 0)
                return String.Empty;
            if (list.Count == 1) {
                return list[0] == null ? String.Empty : list[0].ToString().TrimEnd();
            }

            var builder = new StringBuilder();
            bool first = true;
            bool hasNonNullItem = false;
            foreach (object o in list) {
                if (!first) {
                    builder.Append(",");
                }

                if (o != null) {
                    // 
                    builder.Append(o.ToString().TrimEnd());

                    hasNonNullItem = true;
                }
                first = false;
            }

            // If all the parts are null, return empty string instead of the comma separated list
            if (!hasNonNullItem)
                return String.Empty;

            return builder.ToString();
        }

        // 
        public static object[] GetKeyValues(IList<MetaColumn> keyMembers, object entity) {
            object[] values = new object[keyMembers.Count];

            int index = 0;
            foreach (MetaColumn pkMember in keyMembers) {
                values[index++] = DataBinder.GetPropertyValue(entity, pkMember.Name);
            }

            return values;
        }

        public static string[] ParseCommaSeparatedString(string stringList) {
            // 
            return stringList.Split(',');
        }

        public static IQueryable BuildSortQueryable(IQueryable query, IMetaTable table) {
            IMetaColumn sortColumn = table.SortColumn;
            if (sortColumn.IsCustomProperty) {
                // An extra property can't be optimized on server
                // 
                var data = query.OfType<object>().AsEnumerable();
                Func<object, object> lambda = row => DataBinder.GetPropertyValue(row, sortColumn.Name);
                if (table.SortDescending) {
                    query = data.OrderByDescending<object, object>(lambda).AsQueryable();
                }
                else {
                    query = data.OrderBy<object, object>(lambda).AsQueryable();
                }
            }
            else {
                // Build custom expression to optimize sorting on server
                // 
                var parameter = Expression.Parameter(query.ElementType, "row");
                LambdaExpression lambda = null;
                IMetaForeignKeyColumn foreignKeyColumn = sortColumn as IMetaForeignKeyColumn;
                if (foreignKeyColumn != null) {
                    // e.g. product => product.Category.CategoryName
                    var foreignKeySortColumn = foreignKeyColumn.ParentTable.SortColumn;
                    lambda = Expression.Lambda(Expression.Property(Expression.Property(parameter, sortColumn.Name), foreignKeySortColumn.Name), parameter);
                }
                else {
                    // e.g. product => product.ProductName
                    lambda = Expression.Lambda(Expression.Property(parameter, sortColumn.Name), parameter);
                }
                string ordering = table.SortDescending ? "OrderByDescending" : "OrderBy";
                var expression = Expression.Call(typeof(Queryable), ordering, new Type[] { query.ElementType, lambda.Body.Type }, query.Expression, lambda);
                query = query.Provider.CreateQuery(expression);
            }
            return query;
        }

        // Fill a ListItemCollection with all the entries from a table
        public static void FillListItemCollection(IMetaTable table, ListItemCollection listItemCollection) {
            foreach (var o in table.GetQuery()) {
                string text = table.GetDisplayString(o);
                string value = table.GetPrimaryKeyString(o);
                listItemCollection.Add(new ListItem(text, value.TrimEnd()));
            }
        }

        internal static void ExtractValuesFromBindableControls(IOrderedDictionary dictionary, Control container) {
            IBindableControl bindableControl = container as IBindableControl;
            if (bindableControl != null) {
                bindableControl.ExtractValues(dictionary);
            }
            foreach (Control childControl in container.Controls) {
                ExtractValuesFromBindableControls(dictionary, childControl);
            }
        }

        /// <devdoc>
        /// Walks up the stack of NamingContainers starting at 'control' to find a control with the ID 'controlID'.
        /// Copied from DataBoundControlHelper.FindControl (System.Web)
        /// </devdoc>
        public static Control FindControl(Control control, string controlID) {
            Debug.Assert(control != null, "control should not be null");
            //Debug.Assert(!String.IsNullOrEmpty(controlID), "controlID should not be empty");
            Control currentContainer = control;
            Control foundControl = null;

            if (control == control.Page) {
                // If we get to the Page itself while we're walking up the
                // hierarchy, just return whatever item we find (if anything)
                // since we can't walk any higher.
                return control.FindControl(controlID);
            }

            while (foundControl == null && currentContainer != control.Page) {
                currentContainer = currentContainer.NamingContainer;
                if (currentContainer == null) {
                    throw new HttpException(String.Format(CultureInfo.CurrentCulture,
                            DynamicDataResources.Misc_NoNamingContainer,
                            control.GetType().Name, control.ID));
                }
                foundControl = currentContainer.FindControl(controlID);
            }

            return foundControl;
        }

        public static string GetRouteValue(string key) {
            RequestContext requestContext = DynamicDataRouteHandler.GetRequestContext(HttpContext.Current);
            object value;
            if (!requestContext.RouteData.Values.TryGetValue(key, out value)) {
                return null;
            }

            return value as string;
        }

        public static string SanitizeQueryStringValue(object value) {
            if (value == null)
                return null;

            string strValue = value.ToString();

            // Trim trailing spaces, as they are typically meaningless, and make the url look ugly
            return strValue.TrimEnd();
        }

        internal static long CombineHashCodes(object o1, object o2) {
            // Start with a seed (obtained from String.GetHashCode implementation)
            long combinedHash = 5381;

            combinedHash = AddHashCode(combinedHash, o1);
            combinedHash = AddHashCode(combinedHash, o2);

            return combinedHash;
        }

        // Return a single hash code for 3 objects
        internal static long CombineHashCodes(object o1, object o2, object o3) {
            // Start with a seed (obtained from String.GetHashCode implementation)
            long combinedHash = 5381;

            combinedHash = AddHashCode(combinedHash, o1);
            combinedHash = AddHashCode(combinedHash, o2);
            combinedHash = AddHashCode(combinedHash, o3);

            return combinedHash;
        }

        private static long AddHashCode(long currentHash, object o) {
            if (o == null)
                return currentHash;

            return ((currentHash << 5) + currentHash) ^ o.GetHashCode();
        }
    }
}

