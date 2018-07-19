namespace System.Web.UI.WebControls.Expressions {
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Web.Compilation;
    using System.Web.Resources;
    using System;
    using System.Web.UI;

    public class OfTypeExpression : DataSourceExpression {
        private MethodInfo _ofTypeMethod;
        private string _typeName;

        private MethodInfo OfTypeMethod {
            get {
                if (_ofTypeMethod == null) {
                    var type = GetType(TypeName);

                    _ofTypeMethod = GetOfTypeMethod(type);
                }
                return _ofTypeMethod;
            }
        }

        [DefaultValue("")]
        public string TypeName {
            get {
                return _typeName ?? String.Empty;
            }
            set {
                if (TypeName != value) {
                    _typeName = value;
                    _ofTypeMethod = null;
                }
            }
        }

        public OfTypeExpression() {
        }

        public OfTypeExpression(Type type) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }
            TypeName = type.AssemblyQualifiedName;
            _ofTypeMethod = GetOfTypeMethod(type);
        }

        // internal for unit testing
        internal OfTypeExpression(Control owner)
            : base(owner) {
        }

        private Type GetType(string typeName) {
            if (String.IsNullOrEmpty(typeName)) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    AtlasWeb.OfTypeExpression_TypeNameNotSpecified,
                    Owner.ID));
            }
            try {
                return BuildManager.GetType(typeName, true /* throwOnError */, true /* ignoreCase */);
            } catch (Exception e) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                    AtlasWeb.OfTypeExpression_CannotFindType,
                    typeName,
                    Owner.ID), e);
            }
        }

        private static MethodInfo GetOfTypeMethod(Type type) {
            Debug.Assert(type != null);
            return typeof(Queryable).GetMethod("OfType").MakeGenericMethod(new Type[] { type });
        }

        public override IQueryable GetQueryable(IQueryable query) {
            return query.Provider.CreateQuery(Expression.Call(null, OfTypeMethod, query.Expression));
        }
    }
}
