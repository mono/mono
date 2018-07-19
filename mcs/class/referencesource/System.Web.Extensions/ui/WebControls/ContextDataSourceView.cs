namespace System.Web.UI.WebControls {
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.UI;

    public abstract class ContextDataSourceView : QueryableDataSourceView {
        private string _entitySetName;
        private string _contextTypeName;

        private Type _contextType;
        private string _entityTypeName;
        private Type _entityType;
        private Type _entitySetType;

        private Control _owner;

        protected static readonly object EventContextCreating = new object();
        protected static readonly object EventContextCreated = new object();
        protected static readonly object EventContextDisposing = new object();

        protected ContextDataSourceView(DataSourceControl owner, string viewName, HttpContext context)
            : base(owner, viewName, context) {
            _owner = owner;
        }

        internal ContextDataSourceView(DataSourceControl owner, string viewName, HttpContext context, IDynamicQueryable queryable)
            : base(owner, viewName, context, queryable) {
        }

        public string EntitySetName {
            get {
                return _entitySetName ?? String.Empty;
            }
            set {
                if (_entitySetName != value) {
                    _entitySetName = value;
                    _entitySetType = null;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }


        public string EntityTypeName {
            get {
                return _entityTypeName ?? String.Empty;
            }
            set {
                if (_entityTypeName != value) {
                    _entityTypeName = value;
                    _entityType = null;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        protected override Type EntityType {
            get {
                string typeName = EntityTypeName;
                if (_entityType == null) {
                    _entityType = GetDataObjectTypeByName(typeName) ?? GetDataObjectType(EntitySetType);
                }
                return _entityType;
            }
        }

        public virtual string ContextTypeName {
            get {
                return _contextTypeName ?? String.Empty;
            }
            set {
                if (_contextTypeName != value) {
                    _contextTypeName = value;
                    _contextType = null;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        public virtual Type ContextType {
            get {
                if (_contextType == null && !String.IsNullOrEmpty(ContextTypeName)) {
                    _contextType = DataSourceHelper.GetType(ContextTypeName);
                }
                return _contextType;
            }
        }

        /// <summary>
        /// Current Context
        /// </summary>
        protected object Context {
            get;
            set;
        }

        /// <summary>
        /// Current EntitySet
        /// </summary>
        protected object EntitySet {
            get;
            private set;
        }

        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods",
            Justification = "The result of GetEntitySetType() is cached unless the EntitySetTypeName changes.")]
        protected Type EntitySetType {
            get {
                if (_entitySetType == null) {
                    _entitySetType = GetEntitySetType();
                }
                return _entitySetType;
            }
        }

        // Default implementation assumes the EntitySet is a property or field of the Context        
        protected virtual Type GetEntitySetType() {
            MemberInfo mi = GetEntitySetMember(ContextType);
            if (mi.MemberType == MemberTypes.Property) {
                return ((PropertyInfo)mi).PropertyType;
            }
            else if (mi.MemberType == MemberTypes.Field) {
                return ((FieldInfo)mi).FieldType;
            }
            // 
            throw new InvalidOperationException("EntitySet Type must be a field or property");
        }

        private MemberInfo GetEntitySetMember(Type contextType) {
            string entitySetTypeName = EntitySetName;
            if (String.IsNullOrEmpty(entitySetTypeName)) {
                // 
                return null;
            }

            MemberInfo[] members = contextType.FindMembers(MemberTypes.Field | MemberTypes.Property,
                                                           BindingFlags.Public | BindingFlags.Instance |
                                                           BindingFlags.Static, /*filter*/null, /*filterCriteria*/null);

            for (int i = 0; i < members.Length; i++) {
                if (String.Equals(members[i].Name, entitySetTypeName, StringComparison.OrdinalIgnoreCase)) {
                    return members[i];
                }
            }
            return null;
        }

        private static Type GetDataObjectTypeByName(string typeName) {
            Type entityType = null;            
            if (!String.IsNullOrEmpty(typeName)) {
                entityType = BuildManager.GetType(typeName, /*throwOnFail*/ false, /*ignoreCase*/ true);
            }
            return entityType;
        }

        protected virtual Type GetDataObjectType(Type type) {
            if (type.IsGenericType) {
                Type[] genericTypes = type.GetGenericArguments();
                if (genericTypes.Length == 1) {
                    return genericTypes[0];
                }
            }
            // 
            return typeof(object);
        }

        protected virtual ContextDataSourceContextData CreateContext(DataSourceOperation operation) {
            return null;
        }

        protected override object GetSource(QueryContext context) {
            ContextDataSourceContextData contextData = CreateContext(DataSourceOperation.Select);
            if (contextData != null) {
                // Set the current context
                Context = contextData.Context;
                EntitySet = contextData.EntitySet;
                return EntitySet;
            }

            return null;
        }

        protected override int ExecuteUpdate(IDictionary keys, IDictionary values, IDictionary oldValues) {
            ContextDataSourceContextData contextData = null;
            try {
                contextData = CreateContext(DataSourceOperation.Update);
                if (contextData != null) {
                    // Set the current context
                    Context = contextData.Context;
                    EntitySet = contextData.EntitySet;
                    return base.ExecuteUpdate(keys, values, oldValues);
                }
            }
            finally {
                DisposeContext();
            }

            return -1;
        }

        protected override int ExecuteDelete(IDictionary keys, IDictionary oldValues) {
            ContextDataSourceContextData contextData = null;
            try {
                contextData = CreateContext(DataSourceOperation.Delete);
                if (contextData != null) {
                    // Set the current context
                    Context = contextData.Context;
                    EntitySet = contextData.EntitySet;
                    return base.ExecuteDelete(keys, oldValues);
                }
            }
            finally {
                DisposeContext();
            }

            return -1;
        }

        protected override int ExecuteInsert(IDictionary values) {
            ContextDataSourceContextData contextData = null;
            try {
                contextData = CreateContext(DataSourceOperation.Insert);
                if (contextData != null) {
                    // Set the current context
                    Context = contextData.Context;
                    EntitySet = contextData.EntitySet;

                    return base.ExecuteInsert(values);
                }
            }
            finally {
                DisposeContext();
            }

            return -1;
        }

        protected virtual void DisposeContext(object dataContext) {
            if (dataContext != null) {
                IDisposable disposableObject = dataContext as IDisposable;
                if (disposableObject != null) {
                    disposableObject.Dispose();
                }
                dataContext = null;
            }
        }

        protected void DisposeContext() {
            DisposeContext(Context);
        }
    }
}
