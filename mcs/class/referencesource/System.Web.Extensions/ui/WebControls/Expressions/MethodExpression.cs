namespace System.Web.UI.WebControls.Expressions {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Web.Compilation;
    using System.Web.DynamicData;
    using System.Web.Resources;
    using System.Web.UI.WebControls;

    public class MethodExpression : ParameterDataSourceExpression {
        private static readonly BindingFlags MethodFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        // We'll populate a list of ways to get the type
        private Func<Type>[] typeGetters;
        
        public string TypeName {
            get {
                return (string)ViewState["TypeName"] ?? String.Empty;
            }
            set {
                ViewState["TypeName"] = value;
            }
        }
        
        public string MethodName {
            get {
                return (string)ViewState["MethodName"] ?? String.Empty;
            }
            set {
                ViewState["MethodName"] = value;
            }
        }

        public bool IgnoreIfNotFound {
            get {
                object o = ViewState["IgnoreIfNotFound"];
                return o != null ? (bool)o : false;
            }
            set {
                ViewState["IgnoreIfNotFound"] = value;
            }
        }        

        public MethodExpression() {
            // 1. If a TypeName is specified find the method on that type.
            // 2. Otherwise, if the DataSource is an IDynamicDataSource, then use context type and search for the method.
            // 3. Otherwise look for the method on the current TemplateControl (Page/UserControl) etc.
            typeGetters = new Func<Type>[] { 
                () => GetType(TypeName),
                () => GetType(DataSource),
                () => (Owner != null && Owner.TemplateControl != null) ? Owner.TemplateControl.GetType() : null
            };
        }

        private static Type GetType(string typeName) {
            if (!String.IsNullOrEmpty(typeName)) {
                return BuildManager.GetType(typeName, false /* throwOnError */, true /* ignoreCase */);
            }
            return null;
        }

        private static Type GetType(IQueryableDataSource dataSource) {
            IDynamicDataSource dynamicDataSource = dataSource as IDynamicDataSource;
            if (dynamicDataSource != null) {
                return dynamicDataSource.ContextType;
            }
            return null;
        }

        internal MethodInfo ResolveMethod() {
            if (String.IsNullOrEmpty(MethodName)) {
                throw new InvalidOperationException(AtlasWeb.MethodExpression_MethodNameMustBeSpecified);
            }

            MethodInfo methodInfo = null;
            // We allow the format string {0} in the method name
            IDynamicDataSource dataSource = DataSource as IDynamicDataSource;
            if (dataSource != null) {
                MethodName = String.Format(CultureInfo.CurrentCulture, MethodName, dataSource.EntitySetName);
            }
            else if (MethodName.Contains("{0}")) {
                // If method has a format string but no IDynamicDataSource then throw an exception
                throw new InvalidOperationException(AtlasWeb.MethodExpression_DataSourceMustBeIDynamicDataSource);
            }

            foreach (Func<Type> typeGetter in typeGetters) {
                Type type = typeGetter();
                // If the type is null continue to next fall back function
                if (type == null) {
                    continue;
                }

                methodInfo = type.GetMethod(MethodName, MethodFlags);
                if (methodInfo != null) {
                    break;
                }
            }

            return methodInfo;
        }

        public override IQueryable GetQueryable(IQueryable source) {
            if (source == null) {
                throw new ArgumentNullException("source");
            }

            MethodInfo method = ResolveMethod();

            // Get the parameter values
            IDictionary<string, object> parameterValues = GetValues();

            if (method == null) {
                if (IgnoreIfNotFound) {
                    // Unchange the IQueryable if the user set IgnoreIfNotFound
                    return source;
                }
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                    AtlasWeb.MethodExpression_MethodNotFound, MethodName));
            }

            if(!method.IsStatic) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    AtlasWeb.MethodExpression_MethodMustBeStatic, MethodName));
            }

            ParameterInfo[] parameterArray = method.GetParameters();
            if (parameterArray.Length == 0 || !parameterArray[0].ParameterType.IsAssignableFrom(source.GetType())) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.MethodExpression_FirstParamterMustBeCorrectType,
                    MethodName, source.GetType()));
            }

            object[] arguments = new object[parameterArray.Length];
            // First argument is the IQueryable
            arguments[0] = source;
            for (int i = 1; i < parameterArray.Length; ++i) {
                ParameterInfo param = parameterArray[i];
                object value;
                if (!parameterValues.TryGetValue(param.Name, out value)) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                        AtlasWeb.MethodExpression_ParameterNotFound, MethodName, param.Name));
                }

                arguments[i] = DataSourceHelper.BuildObjectValue(value, param.ParameterType, param.Name);
            }

            object result = method.Invoke(null, arguments);

            // Require the return type be the same as the parameter type
            if (result != null) {
                IQueryable queryable = result as IQueryable;
                // Check if the user did a projection (changed the T in IQuerable<T>)
                if (queryable == null || !queryable.ElementType.IsAssignableFrom(source.ElementType)) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, AtlasWeb.MethodExpression_ChangingTheReturnTypeIsNotAllowed,
                                                        source.ElementType.FullName));
                }
            }

            return (IQueryable)result;
        }
    }
}
