using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Data.Linq.Provider;
using System.Data.Linq.Mapping;
using System.Data.Linq.SqlClient;
using System.Threading;
using System.Runtime.Versioning;
using LinqToSqlShared.Mapping;
using System.Runtime.CompilerServices;

namespace System.Data.Linq.Mapping {

    internal class MappedMetaModel : MetaModel {
        ReaderWriterLock @lock = new ReaderWriterLock();
        MappingSource mappingSource;
        Type contextType;
        Type providerType;
        DatabaseMapping mapping;
        HashSet<Module> modules;
        Dictionary<string, Type> types;
        Dictionary<Type, MetaType> metaTypes;
        Dictionary<Type, MetaTable> metaTables;
        Dictionary<MetaPosition, MetaFunction> metaFunctions;
        bool fullyLoaded;

        [ResourceExposure(ResourceScope.Assembly | ResourceScope.Machine)] // mapping parameter contains various type references.
        [ResourceConsumption(ResourceScope.Assembly | ResourceScope.Machine)] // FindType method call.
        internal MappedMetaModel(MappingSource mappingSource, Type contextType, DatabaseMapping mapping) {
            this.mappingSource = mappingSource;
            this.contextType = contextType;
            this.mapping = mapping;
            this.modules = new HashSet<Module>();
            this.modules.Add(this.contextType.Module);
            this.metaTypes = new Dictionary<Type, MetaType>();
            this.metaTables = new Dictionary<Type, MetaTable>();
            this.types = new Dictionary<string, Type>();
            // Provider type
            if (this.providerType == null && !String.IsNullOrEmpty(this.mapping.Provider)) {
                this.providerType = this.FindType(this.mapping.Provider, typeof(SqlProvider).Namespace);
                if (this.providerType == null) {
                    throw Error.ProviderTypeNotFound(this.mapping.Provider);
                }
            }
            else if (this.providerType == null) {
                this.providerType = typeof(SqlProvider);
            }
            this.Init();
        }
        #region Initialization
        private void Init() {
            if (!fullyLoaded) {
                // The fullyLoaded state is required so that tools like
                // CreateDatabase can get a full view of all tables.
                @lock.AcquireWriterLock(Timeout.Infinite);
                try {
                    if (!fullyLoaded) {
                        // Initialize static tables and functions.
                        this.InitStaticTables();
                        this.InitFunctions();
                        fullyLoaded = true;
                    }
                }
                finally {
                    @lock.ReleaseWriterLock();
                }
            }
        }

        [ResourceExposure(ResourceScope.None)] // Exposure is via external mapping file/attributes.
        [ResourceConsumption(ResourceScope.Assembly | ResourceScope.Machine, ResourceScope.Assembly | ResourceScope.Machine)] // FindType method call.
        private void InitStaticTables() {
            this.InitStaticTableTypes();
            foreach (TableMapping tableMapping in this.mapping.Tables) {
                Type rowType = this.FindType(tableMapping.RowType.Name);
                if (rowType == null) {
                    throw Error.CouldNotFindTypeFromMapping(tableMapping.RowType.Name);
                }
                Type rootType = this.GetRootType(rowType, tableMapping.RowType);
                MetaTable table = new MappedTable(this, tableMapping, rootType);
                foreach (MetaType mt in table.RowType.InheritanceTypes) {
                    this.metaTypes.Add(mt.Type, mt);
                    this.metaTables.Add(mt.Type, table);
                }
            }
        }

        private void InitStaticTableTypes() {
            for (Type type = this.contextType; type != typeof(DataContext); type = type.BaseType) {
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (FieldInfo fi in fields) {
                    Type ft = fi.FieldType;
                    if (ft.IsGenericType && ft.GetGenericTypeDefinition() == typeof(Table<>)) {
                        Type rowType = ft.GetGenericArguments()[0];
                        if (!this.types.ContainsKey(rowType.Name)) {
                            this.types.Add(rowType.FullName, rowType);
                            if (!this.types.ContainsKey(rowType.Name)) {
                                this.types.Add(rowType.Name, rowType);
                            }
                            this.modules.Add(rowType.Module);
                        }
                    }
                }
                PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (PropertyInfo pi in props) {
                    Type pt = pi.PropertyType;
                    if (pt.IsGenericType && pt.GetGenericTypeDefinition() == typeof(Table<>)) {
                        Type rowType = pt.GetGenericArguments()[0];
                        if (!this.types.ContainsKey(rowType.Name)) {
                            this.types.Add(rowType.FullName, rowType);
                            if (!this.types.ContainsKey(rowType.Name)) {
                                this.types.Add(rowType.Name, rowType);
                            }
                            this.modules.Add(rowType.Module);
                        }
                    }
                }
            }
        }

        [ResourceExposure(ResourceScope.None)] // mapping instance variable is set elsewhere.
        [ResourceConsumption(ResourceScope.Assembly | ResourceScope.Machine, ResourceScope.Assembly | ResourceScope.Machine)] // For GetMethods call.
        private void InitFunctions() {
            this.metaFunctions = new Dictionary<MetaPosition, MetaFunction>();
            if (this.contextType != typeof(DataContext)) {
                foreach (FunctionMapping fmap in this.mapping.Functions) {
                    MethodInfo method = this.GetMethod(fmap.MethodName);
                    if (method == null) {
                        throw Error.MethodCannotBeFound(fmap.MethodName);
                    }
                    MappedFunction func = new MappedFunction(this, fmap, method);
                    this.metaFunctions.Add(new MetaPosition(method), func);

                    // pre-set all known function result types into metaType map
                    foreach (MetaType rt in func.ResultRowTypes) {
                        foreach (MetaType it in rt.InheritanceTypes) {
                            if (!this.metaTypes.ContainsKey(it.Type)) {
                                this.metaTypes.Add(it.Type, it);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        public override MappingSource MappingSource {
            get { return this.mappingSource; }
        }

        public override Type ContextType {
            get { return this.contextType; }
        }

        public override string DatabaseName {
            get { return this.mapping.DatabaseName; }
        }

        public override Type ProviderType {
            get { return this.providerType; }
        }

        public override IEnumerable<MetaTable> GetTables() {
            return this.metaTables.Values.Where(x => x != null).Distinct();
        }

        public override MetaTable GetTable(Type rowType) {
            if (rowType == null) {
                throw Error.ArgumentNull("rowType");
            }
            MetaTable table = null;
            this.metaTables.TryGetValue(rowType, out table);
            return table;
        }

        public override MetaType GetMetaType(Type type) {
            if (type == null) {
                throw Error.ArgumentNull("type");
            }
            MetaType mtype = null;
            @lock.AcquireReaderLock(Timeout.Infinite);
            try {
                if (this.metaTypes.TryGetValue(type, out mtype)) {
                    return mtype;
                }
            }
            finally {
                @lock.ReleaseReaderLock();
            }
            @lock.AcquireWriterLock(Timeout.Infinite);
            try {
                if (!this.metaTypes.TryGetValue(type, out mtype)) {
                    // not known, so must be unmapped type
                    mtype = new UnmappedType(this, type);
                    this.metaTypes.Add(type, mtype);
                }
            }
            finally {
                @lock.ReleaseWriterLock();
            }
            return mtype;
        }

        public override MetaFunction GetFunction(MethodInfo method) {
            if (method == null) {
                throw new ArgumentNullException("method");
            }
            MetaFunction func = null;
            this.metaFunctions.TryGetValue(new MetaPosition(method), out func);
            return func;
        }

        public override IEnumerable<MetaFunction> GetFunctions() {
            return this.metaFunctions.Values;
        }

        private Type GetRootType(Type type, TypeMapping rootMapping) {
            if (string.Compare(rootMapping.Name, type.Name, StringComparison.Ordinal) == 0
                || string.Compare(rootMapping.Name, type.FullName, StringComparison.Ordinal) == 0
                || string.Compare(rootMapping.Name, type.AssemblyQualifiedName, StringComparison.Ordinal) == 0)
                return type;
            if (type.BaseType != typeof(object)) {
                return this.GetRootType(type.BaseType, rootMapping);
            }
            throw Error.UnableToResolveRootForType(type);
        }

        [ResourceExposure(ResourceScope.Assembly | ResourceScope.Machine)] // name parameter will be found on a type.
        [ResourceConsumption(ResourceScope.Assembly | ResourceScope.Machine)] // FindType method call.
        private MethodInfo GetMethod(string name) {
            string typeName, methodName;
            this.GetTypeAndMethod(name, out typeName, out methodName);
            Type type = this.FindType(typeName);
            if (type != null) {
                return type.GetMethod(methodName, BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
            }
            return null;
        }

        private void GetTypeAndMethod(string name, out string typeName, out string methodName) {
            int dotIndex = name.LastIndexOf(".", StringComparison.CurrentCulture);
            if (dotIndex > 0) {
                typeName = name.Substring(0, dotIndex);
                methodName = name.Substring(dotIndex + 1);
            }
            else {
                typeName = this.contextType.FullName;
                methodName = name;
            }
        }

        [ResourceExposure(ResourceScope.Assembly | ResourceScope.Machine)] // name parameter is a type name.
        [ResourceConsumption(ResourceScope.Assembly | ResourceScope.Machine)] // FindType method call.
        internal Type FindType(string name) {
            return this.FindType(name, this.contextType.Namespace);
        }

        [ResourceExposure(ResourceScope.Assembly | ResourceScope.Machine)] // name parameter is a type name.
        [ResourceConsumption(ResourceScope.Assembly | ResourceScope.Machine)] // SearchForType method call.
        internal Type FindType(string name, string defaultNamespace) {
            Type result = null;
            string name2 = null;
            @lock.AcquireReaderLock(Timeout.Infinite);
            try {
                if (this.types.TryGetValue(name, out result)) {
                    return result;
                }
                name2 = name.Contains(".") ? null : defaultNamespace + "." + name;
                if (name2 != null && this.types.TryGetValue(name2, out result)) {
                    return result;
                }
            }
            finally {
                @lock.ReleaseReaderLock();
            }
            // don't block anyone while we search for the correct type
            Type foundResult = this.SearchForType(name);

            if (foundResult == null && name2 != null) {
                foundResult = this.SearchForType(name2);
            }
            if (foundResult != null) {
                @lock.AcquireWriterLock(Timeout.Infinite);
                try {
                    if (this.types.TryGetValue(name, out result)) {
                        return result; 
                    }
                    this.types.Add(name, foundResult);
                    return foundResult;
                }
                finally {
                    @lock.ReleaseWriterLock();
                }
            }
            return null;
        }

        [ResourceExposure(ResourceScope.Assembly | ResourceScope.Machine)] // name parameter is a type name.
        [ResourceConsumption(ResourceScope.Assembly | ResourceScope.Machine)] // SearchForType method call.
        private Type SearchForType(string name) {
            // Try search for type using case sensitive.
            Type type = SearchForType(name, false);
            if (type != null) {
                return type;
            }

            // Try search for type using case in-sensitive.
            return SearchForType(name, true);
        }

       [ResourceExposure(ResourceScope.Assembly | ResourceScope.Machine)] // name parameter is a type name.
       [ResourceConsumption(ResourceScope.Assembly | ResourceScope.Machine)] // Assembly.GetLoadedModules method call.
       private Type SearchForType(string name, bool ignoreCase) {
            // first try default system lookup
            Type type = Type.GetType(name, false, ignoreCase);
            if (type != null) {
                return type;
            }

            // try all known modules (modules that other statically known types were found in)
            foreach (Module module in this.modules) {
                type = module.GetType(name, false, ignoreCase);
                if (type != null) {
                    return type;
                }
            }

            // try all loaded modules (is there a better way?)
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (Module module in a.GetLoadedModules()) {
                    type = module.GetType(name, false, ignoreCase);
                    if (type != null) {
                        return type;
                    }
                }
            }

            return null;
        }
    }

    internal sealed class MappedTable : MetaTable {
        MappedMetaModel model;
        TableMapping mapping;
        MetaType rowType;
        bool hasMethods;
        MethodInfo insertMethod;
        MethodInfo updateMethod;
        MethodInfo deleteMethod;

        [ResourceExposure(ResourceScope.Assembly | ResourceScope.Machine)] // Parameter contains various type references.
        [ResourceConsumption(ResourceScope.Assembly | ResourceScope.Machine)] // MappedRootType constructor call.
        internal MappedTable(MappedMetaModel model, TableMapping mapping, Type rowType) {
            this.model = model;
            this.mapping = mapping;
            this.rowType = new MappedRootType(model, this, mapping.RowType, rowType);
        }
        public override MetaModel Model {
            get { return this.model; }
        }
        public override string TableName {
            get { return this.mapping.TableName; }
        }
        public override MetaType RowType {
            get { return this.rowType; }
        }
        public override MethodInfo InsertMethod {
            get {
                this.InitMethods();
                return this.insertMethod;
            }
        }
        public override MethodInfo UpdateMethod {
            get {
                this.InitMethods();
                return this.updateMethod;
            }
        }
        public override MethodInfo DeleteMethod {
            get {
                this.InitMethods();
                return this.deleteMethod;
            }
        }
        private void InitMethods() {
            if (!this.hasMethods) {
                this.insertMethod = MethodFinder.FindMethod(
                    this.model.ContextType,
                    "Insert" + rowType.Name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    new Type[] { rowType.Type }
                    );
                this.updateMethod = MethodFinder.FindMethod(
                    this.model.ContextType,
                    "Update" + rowType.Name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    new Type[] { rowType.Type }
                    );
                this.deleteMethod = MethodFinder.FindMethod(
                    this.model.ContextType,
                    "Delete" + rowType.Name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    new Type[] { rowType.Type }
                    );
                this.hasMethods = true;
            }
        }
    }

    internal sealed class MappedRootType : MappedType {
        Dictionary<Type, MetaType> derivedTypes;
        Dictionary<object, MetaType> inheritanceCodes;
        ReadOnlyCollection<MetaType> inheritanceTypes;
        MetaType inheritanceDefault;
        bool hasInheritance;

        [ResourceExposure(ResourceScope.Assembly | ResourceScope.Machine)] // Various parameters can contain type names.
        [ResourceConsumption(ResourceScope.Assembly | ResourceScope.Machine)] // InitInheritedType method call.
        public MappedRootType(MappedMetaModel model, MappedTable table, TypeMapping typeMapping, Type type)
            : base(model, table, typeMapping, type, null) {
            if (typeMapping == null)
                throw Error.ArgumentNull("typeMapping");

            if (typeMapping.InheritanceCode != null || typeMapping.DerivedTypes.Count > 0) {
                if (this.Discriminator == null) {
                    throw Error.NoDiscriminatorFound(type.Name);
                }
                this.hasInheritance = true;
                if (!MappingSystem.IsSupportedDiscriminatorType(this.Discriminator.Type)) {
                    throw Error.DiscriminatorClrTypeNotSupported(this.Discriminator.DeclaringType.Name, this.Discriminator.Name, this.Discriminator.Type);
                }
                this.derivedTypes = new Dictionary<Type, MetaType>();
                this.inheritanceCodes = new Dictionary<object, MetaType>();
                this.InitInheritedType(typeMapping, this);
            }

            if (this.inheritanceDefault == null && (this.inheritanceCode != null || this.inheritanceCodes != null && this.inheritanceCodes.Count > 0))
                throw Error.InheritanceHierarchyDoesNotDefineDefault(type);

            if (this.derivedTypes != null) {
                this.inheritanceTypes = this.derivedTypes.Values.ToList().AsReadOnly();
            }
            else {
                this.inheritanceTypes = new MetaType[] { this }.ToList().AsReadOnly();
            }

            this.Validate();
        }

        private void Validate() {
            Dictionary<object, string> memberToColumn = new Dictionary<object, string>();
            foreach (MetaType type in this.InheritanceTypes) {
                // NB: Table node in XML can have only one Type node -- enforced by XSD

                foreach (MetaDataMember mem in type.PersistentDataMembers) {
                    if (mem.IsDeclaredBy(type)) {
                        if (mem.IsDiscriminator && !this.HasInheritance) {
                            throw Error.NonInheritanceClassHasDiscriminator(type);
                        }
                        if (!mem.IsAssociation) {
                            // validate that no database column is mapped twice
                            if (!string.IsNullOrEmpty(mem.MappedName)) {
                                string column;
                                object dn = InheritanceRules.DistinguishedMemberName(mem.Member);
                                if (memberToColumn.TryGetValue(dn, out column)) {
                                    if (column != mem.MappedName) {
                                        throw Error.MemberMappedMoreThanOnce(mem.Member.Name);
                                    }
                                }
                                else {
                                    memberToColumn.Add(dn, mem.MappedName);
                                }
                            }
                        }
                    }
                }
            }
        }

        [ResourceExposure(ResourceScope.Assembly | ResourceScope.Machine)] // typeMap parameter's Name property.
        [ResourceConsumption(ResourceScope.Assembly | ResourceScope.Machine)] // FindType method call.
        private MetaType InitDerivedTypes(TypeMapping typeMap) {
            Type type = ((MappedMetaModel)Model).FindType(typeMap.Name);
            if (type == null)
                throw Error.CouldNotFindRuntimeTypeForMapping(typeMap.Name);
            MappedType rowType = new MappedType(this.Model, this.Table, typeMap, type, this);
            return this.InitInheritedType(typeMap, rowType);
        }

        [ResourceExposure(ResourceScope.Assembly | ResourceScope.Machine)] // typeMap parameter's Name property.
        [ResourceConsumption(ResourceScope.Assembly | ResourceScope.Machine)] // InitDerivedTypes method call.
        private MetaType InitInheritedType(TypeMapping typeMap, MappedType type) {
            this.derivedTypes.Add(type.Type, type);

            if (typeMap.InheritanceCode != null) { // Mapping with no inheritance code: For example, an unmapped intermediate class in a hierarchy.
                if (this.Discriminator == null)
                    throw Error.NoDiscriminatorFound(type.Name);

                if (type.Type.IsAbstract)
                    throw Error.AbstractClassAssignInheritanceDiscriminator(type.Type);

                object keyValue = DBConvert.ChangeType(typeMap.InheritanceCode, this.Discriminator.Type);
                foreach (object d in inheritanceCodes.Keys) {
                    // if the keys are equal, or if they are both strings containing only spaces
                    // they are considered equal
                    if ((keyValue.GetType() == typeof(string) && ((string)keyValue).Trim().Length == 0 &&
                        d.GetType() == typeof(string) && ((string)d).Trim().Length == 0) ||
                        object.Equals(d, keyValue)) {
                        throw Error.InheritanceCodeUsedForMultipleTypes(keyValue);
                    }
                }
                if (type.inheritanceCode != null)
                    throw Error.InheritanceTypeHasMultipleDiscriminators(type);
                type.inheritanceCode = keyValue;
                this.inheritanceCodes.Add(keyValue, type);
                if (typeMap.IsInheritanceDefault) {
                    if (this.inheritanceDefault != null)
                        throw Error.InheritanceTypeHasMultipleDefaults(type);
                    this.inheritanceDefault = type;
                }
            }

            // init sub-inherited types
            foreach (TypeMapping tm in typeMap.DerivedTypes) {
                this.InitDerivedTypes(tm);
            }

            return type;
        }

        public override bool HasInheritance {
            get { return this.hasInheritance; }
        }

        public override bool HasInheritanceCode {
            get { return this.InheritanceCode != null; }
        }

        public override ReadOnlyCollection<MetaType> InheritanceTypes {
            get { return this.inheritanceTypes; }
        }

        public override MetaType GetInheritanceType(Type type) {
            if (type == this.Type)
                return this;
            MetaType metaType = null;
            if (this.derivedTypes != null) {
                this.derivedTypes.TryGetValue(type, out metaType);
            }
            return metaType;
        }

        public override MetaType InheritanceDefault {
            get { return this.inheritanceDefault; }
        }
    }

    internal class MappedType : MetaType {
        MetaModel model;
        MetaTable table;
        Type type;
        TypeMapping typeMapping;
        Dictionary<object, MetaDataMember> dataMemberMap;
        ReadOnlyCollection<MetaDataMember> dataMembers;
        ReadOnlyCollection<MetaDataMember> persistentDataMembers;
        ReadOnlyCollection<MetaDataMember> identities;
        MetaDataMember dbGeneratedIdentity;
        MetaDataMember version;
        MetaDataMember discriminator;
        MetaType inheritanceRoot;
        bool inheritanceBaseSet;
        MetaType inheritanceBase;
        internal object inheritanceCode;
        ReadOnlyCollection<MetaType> derivedTypes;
        ReadOnlyCollection<MetaAssociation> associations;
        bool hasMethods;
        bool hasAnyLoadMethod;
        bool hasAnyValidateMethod;
        MethodInfo onLoadedMethod;
        MethodInfo onValidateMethod;

        object locktarget = new object(); // Hold locks on private object rather than public MetaType.

        internal MappedType(MetaModel model, MetaTable table, TypeMapping typeMapping, Type type, MetaType inheritanceRoot) {
            this.model = model;
            this.table = table;
            this.typeMapping = typeMapping;
            this.type = type;
            this.inheritanceRoot = inheritanceRoot != null ? inheritanceRoot : this;
            this.InitDataMembers();

            this.identities = this.dataMembers.Where(m => m.IsPrimaryKey).ToList().AsReadOnly();
            this.persistentDataMembers = this.dataMembers.Where(m => m.IsPersistent).ToList().AsReadOnly();
        }
        #region Initialization
        private void ValidatePrimaryKeyMember(MetaDataMember mm) {
            //if the type is a sub-type, no member in the type can be primary key
            if (mm.IsPrimaryKey && this.inheritanceRoot != this && mm.Member.DeclaringType == this.type) {
                throw (Error.PrimaryKeyInSubTypeNotSupported(this.type.Name, mm.Name));
            }
        }

        private void InitMethods() {
            if (!this.hasMethods) {
                this.onLoadedMethod = MethodFinder.FindMethod(
                    this.Type,
                    "OnLoaded",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    Type.EmptyTypes,
                    false
                    );
                this.onValidateMethod = MethodFinder.FindMethod(
                    this.Type,
                    "OnValidate",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    new[] { typeof(ChangeAction) },
                    false
                    );

                this.hasAnyLoadMethod = (this.onLoadedMethod != null) || (this.InheritanceBase != null && this.InheritanceBase.HasAnyLoadMethod);
                this.hasAnyValidateMethod = (this.onValidateMethod != null) || (this.InheritanceBase != null && this.InheritanceBase.HasAnyValidateMethod);

                this.hasMethods = true;
            }
        }
        
        private void InitDataMembers() {
            if (this.dataMembers == null) {
                Dictionary<object, MetaDataMember> map = new Dictionary<object, MetaDataMember>();
                List<MetaDataMember> dMembers = new List<MetaDataMember>();
                int ordinal = 0;
                BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

                // Map of valid mapped names.
                Dictionary<string, MemberMapping> names = new Dictionary<string, MemberMapping>();
                Type currentType = this.type;
                for (TypeMapping tm = this.typeMapping; tm != null; tm = tm.BaseType) {
                    foreach (MemberMapping mmap in tm.Members) {
                        names[mmap.MemberName + ":" + currentType.Name] = mmap;
                    }
                    currentType = currentType.BaseType;
                }

                HashSet<string> namesSeen = new HashSet<string>(); // Keep track of which names from the mapping file have been seen.
                FieldInfo[] fis = TypeSystem.GetAllFields(this.type, flags).ToArray();
                if (fis != null) {
                    foreach (FieldInfo fi in fis) {
                        MemberMapping mmap;
                        string name = fi.Name + ":" + fi.DeclaringType.Name;
                        if (names.TryGetValue(name, out mmap)) {
                            namesSeen.Add(name);
                            object dn = InheritanceRules.DistinguishedMemberName(fi);
                            MetaDataMember mm;
                            if (!map.TryGetValue(dn, out mm)) {
                                mm = new MappedDataMember(this, fi, mmap, ordinal);
                                map.Add(InheritanceRules.DistinguishedMemberName(mm.Member), mm);
                                dMembers.Add(mm);
                                this.InitSpecialMember(mm);
                            }
                            ValidatePrimaryKeyMember(mm);
                            ordinal++;
                        }
                    }
                }

                PropertyInfo[] pis = TypeSystem.GetAllProperties(this.type, flags).ToArray();
                if (pis != null) {
                    foreach (PropertyInfo pi in pis) {
                        MemberMapping mmap;
                        string name = pi.Name + ":" + pi.DeclaringType.Name;
                        if (names.TryGetValue(name, out mmap)) {
                            namesSeen.Add(name);
                            MetaDataMember mm;
                            object dn = InheritanceRules.DistinguishedMemberName(pi);
                            if (!map.TryGetValue(dn, out mm)) {
                                mm = new MappedDataMember(this, pi, mmap, ordinal);
                                map.Add(InheritanceRules.DistinguishedMemberName(mm.Member), mm);
                                dMembers.Add(mm);
                                this.InitSpecialMember(mm);
                            }
                            ValidatePrimaryKeyMember(mm);
                            ordinal++;
                        }
                    }
                }

                this.dataMembers = dMembers.AsReadOnly();
                this.dataMemberMap = map;

                // Finally, make sure that all types in the mapping file were consumed.
                foreach(string name in namesSeen) {
                    names.Remove(name);
                }
                foreach(var orphan in names) {
                    Type aboveRoot = inheritanceRoot.Type.BaseType;
                    while (aboveRoot!=null) {
                        foreach(MemberInfo mi in aboveRoot.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)) {
                            if(String.Compare(mi.Name, orphan.Value.MemberName, StringComparison.Ordinal)==0) {
                                throw Error.MappedMemberHadNoCorrespondingMemberInType(orphan.Value.MemberName, type.Name);
                            }
                        }
                        aboveRoot = aboveRoot.BaseType;
                    }
                }
            }
        }
        private void InitSpecialMember(MetaDataMember mm) {
            // Can only have one auto gen member that is also an identity member,
            // except if that member is a computed column (since they are implicitly auto gen)
            if (mm.IsDbGenerated && mm.IsPrimaryKey && string.IsNullOrEmpty(mm.Expression)) {
                if (this.dbGeneratedIdentity != null) {
                    throw Error.TwoMembersMarkedAsPrimaryKeyAndDBGenerated(mm.Member, this.dbGeneratedIdentity.Member);
                }
                this.dbGeneratedIdentity = mm;
            }
            if (mm.IsPrimaryKey && !MappingSystem.IsSupportedIdentityType(mm.Type))
            {
                throw Error.IdentityClrTypeNotSupported(mm.DeclaringType, mm.Name, mm.Type);
            }
            if (mm.IsVersion) {
                if (this.version != null) {
                    throw Error.TwoMembersMarkedAsRowVersion(mm.Member, this.version.Member);
                }
                this.version = mm;
            }
            if (mm.IsDiscriminator) {
                if (this.discriminator != null) {
                    if (!InheritanceRules.AreSameMember(this.discriminator.Member, mm.Member)) {
                        throw Error.TwoMembersMarkedAsInheritanceDiscriminator(mm.Member, this.discriminator.Member);
                    }
                }
                else {
                    this.discriminator = mm;
                }
            }
        }
        #endregion
        public override MetaModel Model {
            get { return this.model; }
        }
        public override MetaTable Table {
            get { return this.table; }
        }
        public override Type Type {
            get { return this.type; }
        }
        public override string Name {
            get { return this.type.Name; }
        }
        public override bool IsEntity {
            get {
                if (this.table != null) {
                    return table.RowType.IdentityMembers.Count > 0;
                }
                return false;
            }

        }
        public override bool CanInstantiate {
            get { return !this.type.IsAbstract && (this == this.InheritanceRoot || this.HasInheritanceCode); }
        }
        public override MetaDataMember DBGeneratedIdentityMember {
            get { return this.dbGeneratedIdentity; }
        }
        public override MetaDataMember VersionMember {
            get { return this.version; }
        }
        public override MetaDataMember Discriminator {
            get { return this.discriminator; }
        }
        public override bool HasUpdateCheck {
            get {
                foreach (MetaDataMember member in this.PersistentDataMembers) {
                    if (member.UpdateCheck != UpdateCheck.Never) {
                        return true;
                    }
                }
                return false;
            }
        }
        public override bool HasInheritance {
            get { return this.inheritanceRoot.HasInheritance; }
        }
        public override object InheritanceCode {
            get { return this.inheritanceCode; }
        }
        public override bool HasInheritanceCode {
            get { return this.InheritanceCode != null; }
        }
        public override bool IsInheritanceDefault {
            get { return this.InheritanceDefault == this; }
        }
        public override MetaType InheritanceDefault {
            get {
                if (this.inheritanceRoot == this)
                    throw Error.CannotGetInheritanceDefaultFromNonInheritanceClass();
                return this.InheritanceRoot.InheritanceDefault;
            }
        }
        public override MetaType InheritanceRoot {
            get { return this.inheritanceRoot; }
        }
        public override MetaType InheritanceBase {
            get {
                // LOCKING: Cannot initialize at construction
                if (!this.inheritanceBaseSet && this.inheritanceBase == null) {
                    lock (this.locktarget) {
                        if (this.inheritanceBase == null) {
                            this.inheritanceBase = InheritanceBaseFinder.FindBase(this);
                            this.inheritanceBaseSet = true;
                        }
                    }
                }
                return this.inheritanceBase;
            }
        }
        public override ReadOnlyCollection<MetaType> InheritanceTypes {
            get { return this.inheritanceRoot.InheritanceTypes; }
        }
        public override ReadOnlyCollection<MetaType> DerivedTypes {
            get {
                // LOCKING: Cannot initialize at construction because derived types
                // won't exist yet.
                if (this.derivedTypes == null) {
                    lock (this.locktarget) {
                        if (this.derivedTypes == null) {
                            List<MetaType> dTypes = new List<MetaType>();
                            foreach (MetaType mt in this.InheritanceTypes) {
                                if (mt.Type.BaseType == this.type)
                                    dTypes.Add(mt);
                            }
                            this.derivedTypes = dTypes.AsReadOnly();
                        }
                    }
                }
                return this.derivedTypes;
            }
        }
        public override MetaType GetInheritanceType(Type inheritanceType) {
            foreach (MetaType mt in this.InheritanceTypes)
                if (mt.Type == inheritanceType)
                    return mt;
            return null;
        }
        public override MetaType GetTypeForInheritanceCode(object key) {
            if (this.InheritanceRoot.Discriminator.Type == typeof(string)) {
                string skey = (string)key;
                foreach (MetaType mt in this.InheritanceRoot.InheritanceTypes) {
                    if (string.Compare((string)mt.InheritanceCode, skey, StringComparison.OrdinalIgnoreCase) == 0)
                        return mt;
                }
            }
            else {
                foreach (MetaType mt in this.InheritanceRoot.InheritanceTypes) {
                    if (object.Equals(mt.InheritanceCode, key))
                        return mt;
                }
            }
            return null;
        }
        public override ReadOnlyCollection<MetaDataMember> DataMembers {
            get { return this.dataMembers; }
        }
        public override ReadOnlyCollection<MetaDataMember> PersistentDataMembers {
            get { return this.persistentDataMembers; }
        }
        public override ReadOnlyCollection<MetaDataMember> IdentityMembers {
            get { return this.identities; }
        }
        public override ReadOnlyCollection<MetaAssociation> Associations {
            get {
                // LOCKING: Associations are late-expanded so that cycles are broken.
                if (this.associations == null) {
                    lock (this.locktarget) {
                        if (this.associations == null) {
                            this.associations = this.dataMembers.Where(m => m.IsAssociation).Select(m => m.Association).ToList().AsReadOnly();
                        }
                    }
                }
                return this.associations;
            }
        }
        public override MetaDataMember GetDataMember(MemberInfo mi) {
            if (mi == null)
                throw Error.ArgumentNull("mi");
            MetaDataMember mm;
            if (this.dataMemberMap.TryGetValue(InheritanceRules.DistinguishedMemberName(mi), out mm)) {
                return mm;
            } else {
                if (mi.DeclaringType.IsInterface) {
                    throw Error.MappingOfInterfacesMemberIsNotSupported(mi.DeclaringType.Name, mi.Name);
                } else { //the member is not mapped in the base class
                    throw Error.UnmappedClassMember(mi.DeclaringType.Name, mi.Name);
                }
            }
        }

        public override MethodInfo OnLoadedMethod {
            get {
                this.InitMethods();
                return this.onLoadedMethod;
            }
        }

        public override MethodInfo OnValidateMethod {
            get {
                this.InitMethods();
                return this.onValidateMethod;
            }
        }
        public override bool HasAnyValidateMethod {
            get {
                this.InitMethods();
                return this.hasAnyValidateMethod;
            }
        }
        public override bool HasAnyLoadMethod {
            get {
                this.InitMethods();
                return this.hasAnyLoadMethod;
            }
        }

        public override string ToString() {
            return this.Name;
        }
    }

    internal sealed class MappedDataMember : MetaDataMember {
        MetaType declaringType;
        MemberInfo member;
        MemberInfo storageMember;
        int ordinal;
        Type type;
        bool hasAccessors;
        MetaAccessor accPublic;
        MetaAccessor accPrivate;
        MetaAccessor accDefValue;
        MetaAccessor accDefSource;
        MemberMapping memberMap;
        MappedAssociation assoc;
        bool isNullableType;
        bool isDeferred;
        bool isPrimaryKey;
        bool isVersion;
        bool isDBGenerated;
        bool isDiscriminator;
        bool canBeNull = true;
        string dbType;
        string expression;
        string mappedName;
        UpdateCheck updateCheck = UpdateCheck.Never;
        AutoSync autoSync = AutoSync.Never;
        object locktarget = new object(); // Hold locks on private object rather than public MetaType.
        bool hasLoadMethod;
        MethodInfo loadMethod;

        internal MappedDataMember(MetaType declaringType, MemberInfo mi, MemberMapping map, int ordinal) {
            this.declaringType = declaringType;
            this.member = mi;
            this.ordinal = ordinal;
            this.type = TypeSystem.GetMemberType(mi);
            this.isNullableType = TypeSystem.IsNullableType(this.type);
            this.memberMap = map;
            if (this.memberMap != null && this.memberMap.StorageMemberName != null) {
                MemberInfo[] mis = mi.DeclaringType.GetMember(this.memberMap.StorageMemberName, BindingFlags.Instance | BindingFlags.NonPublic);
                if (mis == null || mis.Length != 1) {
                    throw Error.BadStorageProperty(this.memberMap.StorageMemberName, mi.DeclaringType, mi.Name);
                }
                this.storageMember = mis[0];
            }
            Type storageType = this.storageMember != null ? TypeSystem.GetMemberType(this.storageMember) : this.type;
            this.isDeferred = IsDeferredType(storageType);
            ColumnMapping cmap = map as ColumnMapping;
            if (cmap != null && cmap.IsDbGenerated && cmap.IsPrimaryKey) {
                // auto-gen identities must be synced on insert
                if ((cmap.AutoSync != AutoSync.Default) && (cmap.AutoSync != AutoSync.OnInsert)) {
                    throw Error.IncorrectAutoSyncSpecification(mi.Name);
                }
            }
            if (cmap != null) {
                this.isPrimaryKey = cmap.IsPrimaryKey;
                this.isVersion = cmap.IsVersion;
                this.isDBGenerated = cmap.IsDbGenerated || !string.IsNullOrEmpty(cmap.Expression) || this.isVersion;
                this.isDiscriminator = cmap.IsDiscriminator;
                this.canBeNull = cmap.CanBeNull == null ? this.isNullableType || !this.type.IsValueType : (bool)cmap.CanBeNull;
                this.dbType = cmap.DbType;
                this.expression = cmap.Expression;
                this.updateCheck = cmap.UpdateCheck;
                // auto-gen keys are always and only synced on insert
                if (this.IsDbGenerated && this.IsPrimaryKey) {
                    this.autoSync = AutoSync.OnInsert;
                }
                else if (cmap.AutoSync != AutoSync.Default) {
                    // if the user has explicitly set it, use their value
                    this.autoSync = cmap.AutoSync;
                }
                else if (this.IsDbGenerated) {
                    // database generated members default to always
                    this.autoSync = AutoSync.Always;
                }
            }
            this.mappedName = this.memberMap.DbName != null ? this.memberMap.DbName : this.member.Name;
        }
        private void InitAccessors() {
            if (!this.hasAccessors) {
                lock (this.locktarget) {
                    if (!this.hasAccessors) {
                        if (this.storageMember != null) {
                            this.accPrivate = MakeMemberAccessor(this.member.ReflectedType, this.storageMember, null);
                            if (this.isDeferred) {
                                MakeDeferredAccessors(this.member.ReflectedType, this.accPrivate, out this.accPrivate, out this.accDefValue, out this.accDefSource);
                            }
                            this.accPublic = MakeMemberAccessor(this.member.ReflectedType, this.member, this.accPrivate);
                        }
                        else {
                            this.accPublic = this.accPrivate = MakeMemberAccessor(this.member.ReflectedType, this.member, null);
                            if (this.isDeferred) {
                                MakeDeferredAccessors(this.member.ReflectedType, this.accPrivate, out this.accPrivate, out this.accDefValue, out this.accDefSource);
                            }
                        }
                        this.hasAccessors = true;
                    }
                }
            }
        }
        public override MetaType DeclaringType {
            get { return this.declaringType; }
        }
        public override bool IsDeclaredBy(MetaType metaType) {
            if (metaType == null) {
                throw Error.ArgumentNull("metaType");
            }
            return metaType.Type == this.member.DeclaringType;
        }
        public override MemberInfo Member {
            get { return this.member; }
        }
        public override MemberInfo StorageMember {
            get { return this.storageMember; }
        }
        public override string Name {
            get { return this.member.Name; }
        }
        public override int Ordinal {
            get { return this.ordinal; }
        }
        public override Type Type {
            get { return this.type; }
        }
        public override MetaAccessor MemberAccessor {
            get {
                this.InitAccessors();
                return this.accPublic;
            }
        }
        public override MetaAccessor StorageAccessor {
            get { 
                this.InitAccessors();
                return this.accPrivate; 
            }
        }
        public override MetaAccessor DeferredValueAccessor {
            get {
                this.InitAccessors();
                return this.accDefValue;
            }
        }
        public override MetaAccessor DeferredSourceAccessor {
            get {
                this.InitAccessors();
                return this.accDefSource;
            }
        }
        public override bool IsDeferred {
            get { return this.isDeferred; }
        }
        public override bool IsPersistent {
            get { return this.memberMap != null; }
        }
        public override bool IsAssociation {
            get { return this.memberMap is AssociationMapping; }
        }
        public override bool IsPrimaryKey {
            get { return this.isPrimaryKey; }
        }
        /// <summary>
        /// Returns true if the member is explicitly marked as auto gen, or if the
        /// member is computed or generated by the database server.
        /// </summary>
        public override bool IsDbGenerated {
            get { return this.isDBGenerated; }
        }
        public override bool IsVersion {
            get { return this.isVersion; }
        }
        public override bool IsDiscriminator {
            get { return this.isDiscriminator; }
        }
        public override bool CanBeNull {
            get { return this.canBeNull; }
        }
        public override string DbType {
            get { return this.dbType; }
        }
        public override string Expression {
            get { return this.expression; }
        }
        public override string MappedName {
            get { return this.mappedName; }
        }
        public override UpdateCheck UpdateCheck {
            get { return this.updateCheck; }
        }
        public override AutoSync AutoSync {
            get { return this.autoSync; }
        }
        public override MetaAssociation Association {
            get {
                if (this.IsAssociation) {
                    // LOCKING: This deferral isn't an optimization. It can't be done in the constructor
                    // because there may be loops in the association graph.
                    if (this.assoc == null) {
                        lock (this.locktarget) {
                            if (this.assoc == null) {
                                this.assoc = new MappedAssociation(this, (AssociationMapping)this.memberMap);
                            }
                        }
                    }
                }
                return this.assoc;
            }
        }
        public override MethodInfo LoadMethod {
            get {
                if (this.hasLoadMethod == false && this.IsDeferred) {
                    // defer searching for this access method until we really need to know
                    this.loadMethod = MethodFinder.FindMethod(
                        ((MappedMetaModel)this.declaringType.Model).ContextType,
                        "Load" + this.member.Name,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        new Type[] { this.DeclaringType.Type }
                        );
                    this.hasLoadMethod = true;
                }
                return this.loadMethod;
            }
        }
        private bool IsDeferredType(Type clrType) {
            if (clrType == null || clrType == typeof(object)) {
                return false;
            }
            if (clrType.IsGenericType) {
                Type gtype = clrType.GetGenericTypeDefinition();
                return gtype == typeof(Link<>) ||
                    typeof(EntitySet<>).IsAssignableFrom(gtype) ||
                    typeof(EntityRef<>).IsAssignableFrom(gtype) ||
                    IsDeferredType(clrType.BaseType);
            }
            return false;
        }
        private static MetaAccessor MakeMemberAccessor(Type accessorType, MemberInfo mi, MetaAccessor storage) {
            FieldInfo fi = mi as FieldInfo;
            MetaAccessor acc = null;
            if (fi != null) {
                acc = FieldAccessor.Create(accessorType, fi);
            }
            else {
                PropertyInfo pi = (PropertyInfo)mi;
                acc = PropertyAccessor.Create(accessorType, pi, storage);
            }
            return acc;
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void MakeDeferredAccessors(
            Type declaringType, MetaAccessor accessor,
            out MetaAccessor accessorValue, out MetaAccessor accessorDeferredValue, out MetaAccessor accessorDeferredSource
            ) {
            if (accessor.Type.IsGenericType) {
                Type gtype = accessor.Type.GetGenericTypeDefinition();
                Type itemType = accessor.Type.GetGenericArguments()[0];
                if (gtype == typeof(Link<>)) {
                    accessorValue = CreateAccessor(typeof(LinkValueAccessor<,>).MakeGenericType(declaringType, itemType), accessor);
                    accessorDeferredValue = CreateAccessor(typeof(LinkDefValueAccessor<,>).MakeGenericType(declaringType, itemType), accessor);
                    accessorDeferredSource = CreateAccessor(typeof(LinkDefSourceAccessor<,>).MakeGenericType(declaringType, itemType), accessor);
                    return;
                }
                else if (typeof(EntityRef<>).IsAssignableFrom(gtype)) {
                    accessorValue = CreateAccessor(typeof(EntityRefValueAccessor<,>).MakeGenericType(declaringType, itemType), accessor);
                    accessorDeferredValue = CreateAccessor(typeof(EntityRefDefValueAccessor<,>).MakeGenericType(declaringType, itemType), accessor);
                    accessorDeferredSource = CreateAccessor(typeof(EntityRefDefSourceAccessor<,>).MakeGenericType(declaringType, itemType), accessor);
                    return;
                }
                else if (typeof(EntitySet<>).IsAssignableFrom(gtype)) {
                    accessorValue = CreateAccessor(typeof(EntitySetValueAccessor<,>).MakeGenericType(declaringType, itemType), accessor);
                    accessorDeferredValue = CreateAccessor(typeof(EntitySetDefValueAccessor<,>).MakeGenericType(declaringType, itemType), accessor);
                    accessorDeferredSource = CreateAccessor(typeof(EntitySetDefSourceAccessor<,>).MakeGenericType(declaringType, itemType), accessor);
                    return;
                }
            }
            throw Error.UnhandledDeferredStorageType(accessor.Type);
        }
        private static MetaAccessor CreateAccessor(Type accessorType, params object[] args) {
            return (MetaAccessor)Activator.CreateInstance(accessorType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, args, null);
        }
    }

    internal class MappedAssociation : MetaAssociationImpl {
        MappedDataMember thisMember;
        MetaDataMember otherMember;
        MetaType otherType;
        ReadOnlyCollection<MetaDataMember> thisKey;
        ReadOnlyCollection<MetaDataMember> otherKey;
        bool isMany;
        bool isForeignKey;
        bool isNullable;
        bool thisKeyIsPrimaryKey;
        bool otherKeyIsPrimaryKey;
        AssociationMapping assocMap;

        internal MappedAssociation(MappedDataMember mm, AssociationMapping assocMap) {
            this.thisMember = mm;
            this.assocMap = assocMap;
            this.Init();
            this.InitOther();
            //validate the number of ThisKey columns is the same as the number of OtherKey columns
            if (this.thisKey.Count != this.otherKey.Count && this.thisKey.Count > 0 && this.otherKey.Count > 0) {
                throw Error.MismatchedThisKeyOtherKey(thisMember.Name, thisMember.DeclaringType.Name);
            }
        }
        #region Initialization
        private void Init() {
            this.isMany = TypeSystem.IsSequenceType(this.thisMember.Type);
            this.thisKey = (this.assocMap.ThisKey != null)
                ? MakeKeys(this.thisMember.DeclaringType, this.assocMap.ThisKey)
                : this.thisMember.DeclaringType.IdentityMembers;
            // this association refers to the parent if thisKey is not our own identity
            this.thisKeyIsPrimaryKey = AreEqual(this.thisKey, this.thisMember.DeclaringType.IdentityMembers);
            this.isForeignKey = this.assocMap.IsForeignKey;

            // if any key members are not nullable, the association is not nullable
            this.isNullable = true;
            foreach (MetaDataMember mm in this.thisKey) {
                if (mm == null)
                    throw Error.UnexpectedNull("MetaDataMember");

                if (!mm.CanBeNull) {
                    this.isNullable = false;
                    break;
                }
            }

            // validate DeleteOnNull specification
            if (assocMap.DeleteOnNull == true) {
                if (!(isForeignKey && !isMany && !isNullable)) {
                    throw Error.InvalidDeleteOnNullSpecification(thisMember);
                }
            }
        }
        private void InitOther() {
            if (this.otherType == null) {
                Type ot = this.isMany ? TypeSystem.GetElementType(this.thisMember.Type) : this.thisMember.Type;
                this.otherType = this.thisMember.DeclaringType.Model.GetMetaType(ot);
                System.Diagnostics.Debug.Assert(this.otherType.IsEntity);
                this.otherKey = (assocMap.OtherKey != null)
                    ? MakeKeys(this.otherType, this.assocMap.OtherKey)
                    : this.otherType.IdentityMembers;
                this.otherKeyIsPrimaryKey = AreEqual(this.otherKey, this.otherType.IdentityMembers);
                foreach (MetaDataMember omm in this.otherType.DataMembers) {
                    if (omm.IsAssociation && omm != this.thisMember && omm.MappedName == this.thisMember.MappedName) {
                        this.otherMember = omm;
                        break;
                    }
                }
            }
        }
        #endregion
        public override MetaDataMember ThisMember {
            get { return this.thisMember; }
        }
        public override ReadOnlyCollection<MetaDataMember> ThisKey {
            get { return this.thisKey; }
        }
        public override MetaDataMember OtherMember {
            get { return this.otherMember; }
        }
        public override ReadOnlyCollection<MetaDataMember> OtherKey {
            get { return this.otherKey; }
        }
        public override MetaType OtherType {
            get { return this.otherType; }
        }
        public override bool IsMany {
            get { return this.isMany; }
        }
        public override bool IsForeignKey {
            get { return this.isForeignKey; }
        }
        public override bool IsUnique {
            get { return this.assocMap.IsUnique; }
        }
        public override bool IsNullable {
            get { return this.isNullable; }
        }
        public override bool ThisKeyIsPrimaryKey {
            get { return this.thisKeyIsPrimaryKey; }
        }
        public override bool OtherKeyIsPrimaryKey {
            get { return this.otherKeyIsPrimaryKey; }
        }
        public override string DeleteRule {
            get {
                return this.assocMap.DeleteRule;
            }
        }
        public override bool DeleteOnNull {
            get {
                return this.assocMap.DeleteOnNull;
            }
        }
    }

    class MappedFunction : MetaFunction {
        MetaModel model;
        FunctionMapping map;
        MethodInfo method;
        ReadOnlyCollection<MetaParameter> parameters;
        MetaParameter returnParameter;
        ReadOnlyCollection<MetaType> rowTypes;
        static ReadOnlyCollection<MetaParameter> _emptyParameters = new List<MetaParameter>(0).AsReadOnly();
        static ReadOnlyCollection<MetaType> _emptyTypes = new List<MetaType>(0).AsReadOnly();

        [ResourceExposure(ResourceScope.Assembly | ResourceScope.Machine)] // map parameter contains type names.
        [ResourceConsumption(ResourceScope.Assembly | ResourceScope.Machine)] // FindType method call.
        internal MappedFunction(MappedMetaModel model, FunctionMapping map, MethodInfo method) {
            this.model = model;
            this.map = map;
            this.method = method;
            this.rowTypes = _emptyTypes;

            if (map.Types.Count == 0 && this.method.ReturnType == typeof(IMultipleResults)) {
                throw Error.NoResultTypesDeclaredForFunction(method.Name);
            }
            else if (map.Types.Count > 1 && this.method.ReturnType != typeof(IMultipleResults)) {
                throw Error.TooManyResultTypesDeclaredForFunction(method.Name);
            }
            else if (map.Types.Count == 1 && this.method.ReturnType != typeof(IMultipleResults)) {
                Type elementType = TypeSystem.GetElementType(method.ReturnType);
                this.rowTypes = new List<MetaType>(1) { this.GetMetaType(map.Types[0], elementType) }.AsReadOnly();
            }
            else if (map.Types.Count > 0) {
                List<MetaType> rowTypes = new List<MetaType>();
                foreach (TypeMapping rtm in map.Types) {
                    Type elementType = model.FindType(rtm.Name);
                    if (elementType == null) {
                        throw Error.CouldNotFindElementTypeInModel(rtm.Name);
                    }
                    MetaType mt = this.GetMetaType(rtm, elementType);
                    // Only add unique meta types
                    if (!rowTypes.Contains(mt)) {
                        rowTypes.Add(mt);
                    }
                }
                this.rowTypes = rowTypes.AsReadOnly();
            }
            else if (map.FunReturn != null) {
                this.returnParameter = new MappedReturnParameter(method.ReturnParameter, map.FunReturn);
            }

            // Parameters.
            ParameterInfo[] pis = this.method.GetParameters();
            if (pis.Length > 0) {
                List<MetaParameter> mps = new List<MetaParameter>(pis.Length);
                if (this.map.Parameters.Count != pis.Length) {
                    throw Error.IncorrectNumberOfParametersMappedForMethod(this.map.MethodName);
                }
                for (int i = 0; i < pis.Length; i++) {
                    mps.Add(new MappedParameter(pis[i], this.map.Parameters[i]));
                }
                this.parameters = mps.AsReadOnly();
            }
            else {
                this.parameters = _emptyParameters;
            }
        }
        /// <summary>
        /// For the specified type, if it is a mapped type, use the Table
        /// metatype to get the correct inheritance metatype,
        /// otherwise create a new meta type.
        [ResourceExposure(ResourceScope.Assembly | ResourceScope.Machine)] // Parameter contains various type references.
        [ResourceConsumption(ResourceScope.Assembly | ResourceScope.Machine)] // MappedRootType constructor call.        
        private MetaType GetMetaType(TypeMapping tm, Type elementType) {
            MetaTable tbl = model.GetTable(elementType);
            if (tbl != null) {
                return tbl.RowType.GetInheritanceType(elementType);
            }
            return new MappedRootType((MappedMetaModel)model, null, tm, elementType);
        }
        public override ReadOnlyCollection<MetaParameter> Parameters {
            get { return this.parameters; }
        }
        public override string MappedName {
            get { return this.map.Name; }
        }
        public override MethodInfo Method {
            get { return this.method; }
        }
        public override MetaModel Model {
            get { return this.model; }
        }
        public override string Name {
            get { return this.method.Name; }
        }
        public override bool IsComposable {
            get { return this.map.IsComposable; }
        }
        public override MetaParameter ReturnParameter {
            get { return this.returnParameter; }
        }
        public override bool HasMultipleResults {
            get { return this.method.ReturnType == typeof(IMultipleResults); }
        }
        public override ReadOnlyCollection<MetaType> ResultRowTypes {
            get { return this.rowTypes; }
        }
    }

    internal sealed class MappedParameter : MetaParameter {
        private ParameterInfo parameterInfo;
        private ParameterMapping map;

        public MappedParameter(ParameterInfo parameterInfo, ParameterMapping map) {
            this.parameterInfo = parameterInfo;
            this.map = map;
        }
        public override ParameterInfo Parameter {
            get { return this.parameterInfo; }
        }
        public override string Name {
            get { return this.parameterInfo.Name; }
        }
        public override string MappedName {
            get { return this.map.Name; }
        }
        public override Type ParameterType {
            get { return this.parameterInfo.ParameterType; }
        }
        public override string DbType {
            get { return this.map.DbType; }
        }
    }

    internal sealed class MappedReturnParameter : MetaParameter {
        private ParameterInfo parameterInfo;
        private ReturnMapping map;

        public MappedReturnParameter(ParameterInfo parameterInfo, ReturnMapping map) {
            this.parameterInfo = parameterInfo;
            this.map = map;
        }
        public override ParameterInfo Parameter {
            get { return this.parameterInfo; }
        }
        public override string Name {
            get { return null; }
        }
        public override string MappedName {
            get { return null; }
        }
        public override Type ParameterType {
            get { return this.parameterInfo.ParameterType; }
        }
        public override string DbType {
            get { return this.map.DbType; }
        }
    }

    internal abstract class MetaAssociationImpl : MetaAssociation {

        private static char[] keySeparators = new char[] { ',' };
        /// <summary>
        /// Given a MetaType and a set of key fields, return the set of MetaDataMembers
        /// corresponding to the key.
        /// </summary>
        protected static ReadOnlyCollection<MetaDataMember> MakeKeys(MetaType mtype, string keyFields) {
            string[] names = keyFields.Split(keySeparators);
            MetaDataMember[] members = new MetaDataMember[names.Length];
            for (int i = 0; i < names.Length; i++) {
                names[i] = names[i].Trim();
                MemberInfo[] rmis = mtype.Type.GetMember(names[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (rmis == null || rmis.Length != 1) {
                    throw Error.BadKeyMember(names[i], keyFields, mtype.Name);
                }
                members[i] = mtype.GetDataMember(rmis[0]);
                if (members[i] == null) {
                    throw Error.BadKeyMember(names[i], keyFields, mtype.Name);
                }
            }
            return new List<MetaDataMember>(members).AsReadOnly();
        }

        /// <summary>
        /// Compare two sets of keys for equality.
        /// </summary>
        protected static bool AreEqual(IEnumerable<MetaDataMember> key1, IEnumerable<MetaDataMember> key2) {
            using (IEnumerator<MetaDataMember> e1 = key1.GetEnumerator()) {
                using (IEnumerator<MetaDataMember> e2 = key2.GetEnumerator()) {
                    bool m1, m2;
                    for (m1 = e1.MoveNext(), m2 = e2.MoveNext(); m1 && m2; m1 = e1.MoveNext(), m2 = e2.MoveNext()) {
                        if (e1.Current != e2.Current)
                            return false;
                    }
                    if (m1 != m2)
                        return false;
                }
            }
            return true;
        }

        public override string ToString() {
            return string.Format(Globalization.CultureInfo.InvariantCulture, "{0} ->{1} {2}", ThisMember.DeclaringType.Name, IsMany ? "*" : "", OtherType.Name);
        }
    }

    internal sealed class UnmappedType : MetaType {
        MetaModel model;
        Type type;
        Dictionary<object, MetaDataMember> dataMemberMap;
        ReadOnlyCollection<MetaDataMember> dataMembers;
        ReadOnlyCollection<MetaType> inheritanceTypes;
        object locktarget = new object(); // Hold locks on private object rather than public MetaType.

        private static ReadOnlyCollection<MetaType> _emptyTypes = new List<MetaType>().AsReadOnly();
        private static ReadOnlyCollection<MetaDataMember> _emptyDataMembers = new List<MetaDataMember>().AsReadOnly();
        private static ReadOnlyCollection<MetaAssociation> _emptyAssociations = new List<MetaAssociation>().AsReadOnly();

        internal UnmappedType(MetaModel model, Type type) {
            this.model = model;
            this.type = type;
        }

        public override MetaModel Model {
            get { return this.model; }
        }
        public override MetaTable Table {
            get { return null; }
        }
        public override Type Type {
            get { return this.type; }
        }
        public override string Name {
            get { return this.type.Name; }
        }
        public override bool IsEntity {
            get { return false; }
        }
        public override bool CanInstantiate {
            get { return !this.type.IsAbstract; }
        }
        public override MetaDataMember DBGeneratedIdentityMember {
            get { return null; }
        }
        public override MetaDataMember VersionMember {
            get { return null; }
        }
        public override MetaDataMember Discriminator {
            get { return null; }
        }
        public override bool HasUpdateCheck {
            get { return false; }
        }
        public override ReadOnlyCollection<MetaType> InheritanceTypes {
            get {
                if (this.inheritanceTypes == null) {
                    lock (this.locktarget) {
                        if (this.inheritanceTypes == null) {
                            this.inheritanceTypes = new MetaType[] { this }.ToList().AsReadOnly();
                        }
                    }
                }
                return this.inheritanceTypes;
            }
        }
        public override MetaType GetInheritanceType(Type inheritanceType) {
            if (inheritanceType == this.type)
                return this;
            return null;
        }
        public override ReadOnlyCollection<MetaType> DerivedTypes {
            get { return _emptyTypes; }
        }
        public override MetaType GetTypeForInheritanceCode(object key) {
            return null;
        }
        public override bool HasInheritance {
            get { return false; }
        }
        public override bool HasInheritanceCode {
            get { return false; }
        }
        public override object InheritanceCode {
            get { return null; }
        }
        public override MetaType InheritanceRoot {
            get { return this; }
        }
        public override MetaType InheritanceBase {
            get { return null; }
        }
        public override MetaType InheritanceDefault {
            get { return null; }
        }
        public override bool IsInheritanceDefault {
            get { return false; }
        }
        public override ReadOnlyCollection<MetaDataMember> DataMembers {
            get {
                this.InitDataMembers();
                return this.dataMembers;
            }
        }
        public override ReadOnlyCollection<MetaDataMember> PersistentDataMembers {
            get { return _emptyDataMembers; }
        }
        public override ReadOnlyCollection<MetaDataMember> IdentityMembers {
            get {
                this.InitDataMembers();
                return this.dataMembers;
            }
        }
        public override ReadOnlyCollection<MetaAssociation> Associations {
            get { return _emptyAssociations; }
        }
        public override MetaDataMember GetDataMember(MemberInfo mi) {
            if (mi == null)
                throw Error.ArgumentNull("mi");
            this.InitDataMembers();
            if (this.dataMemberMap == null) {
                lock (this.locktarget) {
                    if (this.dataMemberMap == null) {
                        Dictionary<object, MetaDataMember> map = new Dictionary<object, MetaDataMember>();
                        foreach (MetaDataMember mm in this.dataMembers) {
                            map.Add(InheritanceRules.DistinguishedMemberName(mm.Member), mm);
                        }
                        this.dataMemberMap = map;
                    }
                }
            }
            object dn = InheritanceRules.DistinguishedMemberName(mi);
            MetaDataMember mdm;
            this.dataMemberMap.TryGetValue(dn, out mdm);
            return mdm;
        }

        private void InitDataMembers() {
            if (this.dataMembers == null) {
                lock (this.locktarget) {
                    if (this.dataMembers == null) {
                        List<MetaDataMember> dMembers = new List<MetaDataMember>();
                        int ordinal = 0;
                        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
                        foreach (FieldInfo fi in this.type.GetFields(flags)) {
                            MetaDataMember mm = new UnmappedDataMember(this, fi, ordinal);
                            dMembers.Add(mm);
                            ordinal++;
                        }
                        foreach (PropertyInfo pi in this.type.GetProperties(flags)) {
                            MetaDataMember mm = new UnmappedDataMember(this, pi, ordinal);
                            dMembers.Add(mm);
                            ordinal++;
                        }
                        this.dataMembers = dMembers.AsReadOnly();
                    }
                }
            }
        }

        public override string ToString() {
            return this.Name;
        }

        public override MethodInfo OnLoadedMethod {
            get { return null; }
        }

        public override MethodInfo OnValidateMethod {
            get { return null; }
        }
        public override bool HasAnyValidateMethod {
            get {
                return false;
            }
        }
        public override bool HasAnyLoadMethod {
            get {
                return false;
            }
        }
    }

    internal sealed class UnmappedDataMember : MetaDataMember {
        MetaType declaringType;
        MemberInfo member;
        int ordinal;
        Type type;
        MetaAccessor accPublic;
        object lockTarget = new object();

        internal UnmappedDataMember(MetaType declaringType, MemberInfo mi, int ordinal) {
            this.declaringType = declaringType;
            this.member = mi;
            this.ordinal = ordinal;
            this.type = TypeSystem.GetMemberType(mi);
        }
        private void InitAccessors() {
            if (this.accPublic == null) {
                lock (this.lockTarget) {
                    if (this.accPublic == null) {
                        this.accPublic = MakeMemberAccessor(this.member.ReflectedType, this.member);
                    }
                }
            }
        }
        public override MetaType DeclaringType {
            get { return this.declaringType; }
        }
        public override bool IsDeclaredBy(MetaType metaType) {
            if (metaType == null) {
                throw Error.ArgumentNull("metaType");
            }
            return metaType.Type == this.member.DeclaringType;
        }
        public override MemberInfo Member {
            get { return this.member; }
        }
        public override MemberInfo StorageMember {
            get { return this.member; }
        }
        public override string Name {
            get { return this.member.Name; }
        }
        public override int Ordinal {
            get { return this.ordinal; }
        }
        public override Type Type {
            get { return this.type; }
        }
        public override MetaAccessor MemberAccessor {
            get {
                this.InitAccessors();
                return this.accPublic;
            }
        }
        public override MetaAccessor StorageAccessor {
            get {
                this.InitAccessors();
                return this.accPublic;
            }
        }
        public override MetaAccessor DeferredValueAccessor {
            get { return null; }
        }
        public override MetaAccessor DeferredSourceAccessor {
            get { return null; }
        }
        public override bool IsDeferred {
            get { return false; }
        }
        public override bool IsPersistent {
            get { return false; }
        }
        public override bool IsAssociation {
            get { return false; }
        }
        public override bool IsPrimaryKey {
            get { return false; }
        }
        public override bool IsDbGenerated {
            get { return false; }
        }
        public override bool IsVersion {
            get { return false; }
        }
        public override bool IsDiscriminator {
            get { return false; }
        }
        public override bool CanBeNull {
            get { return !this.type.IsValueType || TypeSystem.IsNullableType(this.type); }
        }
        public override string DbType {
            get { return null; }
        }
        public override string Expression {
            get { return null; }
        }
        public override string MappedName {
            get { return this.member.Name; }
        }
        public override UpdateCheck UpdateCheck {
            get { return UpdateCheck.Never; }
        }
        public override AutoSync AutoSync {
            get { return AutoSync.Never; }
        }
        public override MetaAssociation Association {
            get { return null; }
        }
        public override MethodInfo LoadMethod {
            get { return null; }
        }
        private static MetaAccessor MakeMemberAccessor(Type accessorType, MemberInfo mi) {
            FieldInfo fi = mi as FieldInfo;
            MetaAccessor acc = null;
            if (fi != null) {
                acc = FieldAccessor.Create(accessorType, fi);
            }
            else {
                PropertyInfo pi = (PropertyInfo)mi;
                acc = PropertyAccessor.Create(accessorType, pi, null);
            }
            return acc;
        }
    }
}
