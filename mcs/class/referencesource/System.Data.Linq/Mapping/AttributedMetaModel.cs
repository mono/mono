using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Data.Linq.Provider;
using System.Data.Linq.SqlClient;
using System.Threading;
using LinqToSqlShared.Mapping;
using System.Runtime.CompilerServices;

namespace System.Data.Linq.Mapping {

    internal static class MethodFinder {
        internal static MethodInfo FindMethod(Type type, string name, BindingFlags flags, Type[] argTypes) {
            return FindMethod(type, name, flags, argTypes, true);
        }

        internal static MethodInfo FindMethod(Type type, string name, BindingFlags flags, Type[] argTypes, bool allowInherit) {
            for (; type != typeof(object); type = type.BaseType) {
                MethodInfo mi = type.GetMethod(name, flags | BindingFlags.DeclaredOnly, null, argTypes, null);
                if (mi != null || !allowInherit) {
                    return mi;
                }
            }
            return null;
        }
    }

    internal static class InheritanceBaseFinder {
        internal static MetaType FindBase(MetaType derivedType) {
            if (derivedType.Type == typeof(object)) {
                return null;
            }

            var clrType = derivedType.Type; // start
            var rootClrType = derivedType.InheritanceRoot.Type; // end
            var metaTable = derivedType.Table;
            MetaType metaType = null;

            while (true) {
                if (clrType == typeof(object) || clrType == rootClrType) {
                    return null;
                }

                clrType = clrType.BaseType;
                metaType = derivedType.InheritanceRoot.GetInheritanceType(clrType);

                if (metaType != null) {
                    return metaType;
                }
            }
        }
    }

    internal class AttributedMetaModel : MetaModel {
        ReaderWriterLock @lock = new ReaderWriterLock();
        MappingSource mappingSource;
        Type contextType;
        Type providerType;
        Dictionary<Type, MetaType> metaTypes;
        Dictionary<Type, MetaTable> metaTables;
        ReadOnlyCollection<MetaTable> staticTables;
        Dictionary<MetaPosition, MetaFunction> metaFunctions;
        string dbName;
        bool initStaticTables;
        bool initFunctions;

        internal AttributedMetaModel(MappingSource mappingSource, Type contextType) {
            this.mappingSource = mappingSource;
            this.contextType = contextType;
            this.metaTypes = new Dictionary<Type, MetaType>();
            this.metaTables = new Dictionary<Type, MetaTable>();
            this.metaFunctions = new Dictionary<MetaPosition, MetaFunction>();

            // Provider type
            ProviderAttribute[] attrs = (ProviderAttribute[])this.contextType.GetCustomAttributes(typeof(ProviderAttribute), true);
            if (attrs != null && attrs.Length == 1) { // Provider attribute is !AllowMultiple
                this.providerType = attrs[0].Type;
            } else {
                this.providerType = typeof(SqlProvider);
            }

            // Database name 
            DatabaseAttribute[] das = (DatabaseAttribute[])this.contextType.GetCustomAttributes(typeof(DatabaseAttribute), false);
            this.dbName = (das != null && das.Length > 0) ? das[0].Name : this.contextType.Name;
        }

        public override MappingSource MappingSource {
            get { return this.mappingSource; }
        }

        public override Type ContextType {
            get { return this.contextType; }
        }

        public override string DatabaseName {
            get { return this.dbName; }
        }

        public override Type ProviderType {
            get { return this.providerType; }
        }

        public override IEnumerable<MetaTable> GetTables() {
            this.InitStaticTables();
            if (this.staticTables.Count > 0) {
                return this.staticTables;
            }
            else {
                @lock.AcquireReaderLock(Timeout.Infinite);
                try {
                    return this.metaTables.Values.Where(x => x != null).Distinct();
                }
                finally {
                    @lock.ReleaseReaderLock();
                }
            }
        }
        #region Initialization
        private void InitStaticTables() {
            if (!this.initStaticTables) {
                @lock.AcquireWriterLock(Timeout.Infinite);
                try {
                    if (!this.initStaticTables) {
                        HashSet<MetaTable> tables = new HashSet<MetaTable>();
                        for (Type type = this.contextType; type != typeof(DataContext); type = type.BaseType) {
                            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                            foreach (FieldInfo fi in fields) {
                                Type ft = fi.FieldType;
                                if (ft.IsGenericType && ft.GetGenericTypeDefinition() == typeof(Table<>)) {
                                    Type rowType = ft.GetGenericArguments()[0];
                                    tables.Add(this.GetTableNoLocks(rowType));
                                }
                            }
                            PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                            foreach (PropertyInfo pi in props) {
                                Type pt = pi.PropertyType;
                                if (pt.IsGenericType && pt.GetGenericTypeDefinition() == typeof(Table<>)) {
                                    Type rowType = pt.GetGenericArguments()[0];
                                    tables.Add(this.GetTableNoLocks(rowType));
                                }
                            }
                        }
                        this.staticTables = new List<MetaTable>(tables).AsReadOnly();
                        this.initStaticTables = true;
                    }
                }
                finally {
                    @lock.ReleaseWriterLock();
                }
            }
        }

        private void InitFunctions() {
            if (!this.initFunctions) {
                @lock.AcquireWriterLock(Timeout.Infinite);
                try {
                    if (!this.initFunctions) {
                        if (this.contextType != typeof(DataContext)) {
                            for (Type type = this.contextType; type != typeof(DataContext); type = type.BaseType) {
                                foreach (MethodInfo mi in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)) {
                                    if (IsUserFunction(mi)) {
                                        if (mi.IsGenericMethodDefinition) {
                                            // Added this constraint because XML mapping model didn't support mapping sprocs to generic method.
                                            // The attribute mapping model was, however, able to support it. This check is for parity between 
                                            // the two models.
                                            throw Error.InvalidUseOfGenericMethodAsMappedFunction(mi.Name);
                                        }
                                        MetaPosition mp = new MetaPosition(mi);
                                        if (!this.metaFunctions.ContainsKey(mp)) {
                                            MetaFunction metaFunction = new AttributedMetaFunction(this, mi);
                                            this.metaFunctions.Add(mp, metaFunction);

                                            // pre-set all known function result types into metaType map
                                            foreach (MetaType rt in metaFunction.ResultRowTypes) {
                                                foreach (MetaType it in rt.InheritanceTypes) {
                                                    if (!this.metaTypes.ContainsKey(it.Type)) {
                                                        this.metaTypes.Add(it.Type, it);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        this.initFunctions = true;
                    }
                }
                finally {
                    @lock.ReleaseWriterLock();
                }
            }
        }

        private static bool IsUserFunction(MethodInfo mi) {
            return Attribute.GetCustomAttribute(mi, typeof(FunctionAttribute), false) != null;
        }
        #endregion

        public override MetaTable GetTable(Type rowType) {
            if (rowType == null) {
                throw Error.ArgumentNull("rowType");
            }
            MetaTable table;
            @lock.AcquireReaderLock(Timeout.Infinite);
            try {
                if (this.metaTables.TryGetValue(rowType, out table)) {
                    return table;
                }
            }
            finally {
                @lock.ReleaseReaderLock();
            }
            @lock.AcquireWriterLock(Timeout.Infinite);
            try {
                table = this.GetTableNoLocks(rowType);
            }
            finally {
                @lock.ReleaseWriterLock();
            }
            return table;
        }

        internal MetaTable GetTableNoLocks(Type rowType) {
            MetaTable table;
            if (!this.metaTables.TryGetValue(rowType, out table)) {
                Type root = GetRoot(rowType) ?? rowType;
                TableAttribute[] attrs = (TableAttribute[])root.GetCustomAttributes(typeof(TableAttribute), true);
                if (attrs.Length == 0) {
                    this.metaTables.Add(rowType, null);
                }
                else {
                    if (!this.metaTables.TryGetValue(root, out table)) {
                        table = new AttributedMetaTable(this, attrs[0], root);
                        foreach (MetaType mt in table.RowType.InheritanceTypes) {
                            this.metaTables.Add(mt.Type, table);
                        }
                    }
                    // catch case of derived type that is not part of inheritance
                    if (table.RowType.GetInheritanceType(rowType) == null) {
                        this.metaTables.Add(rowType, null);
                        return null;
                    }
                }
            }
            return table;
        }

        private static Type GetRoot(Type derivedType) {
            while (derivedType != null && derivedType != typeof(object)) {
                TableAttribute[] attrs = (TableAttribute[])derivedType.GetCustomAttributes(typeof(TableAttribute), false);
                if (attrs.Length > 0)
                    return derivedType;
                derivedType = derivedType.BaseType;
            }
            return null;
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
            // Attributed meta model allows us to learn about tables we did not
            // statically know about
            MetaTable tab = this.GetTable(type);
            if (tab != null) {
                return tab.RowType.GetInheritanceType(type);
            }
            this.InitFunctions();
            @lock.AcquireWriterLock(Timeout.Infinite);
            try {
                if (!this.metaTypes.TryGetValue(type, out mtype)) {
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
                throw Error.ArgumentNull("method");
            }
            this.InitFunctions();
            MetaFunction function = null;
            this.metaFunctions.TryGetValue(new MetaPosition(method), out function);
            return function;
        }

        public override IEnumerable<MetaFunction> GetFunctions() {
            this.InitFunctions();
            return this.metaFunctions.Values.ToList().AsReadOnly();
        }
    }

    internal sealed class AttributedMetaTable : MetaTable {
        AttributedMetaModel model;
        string tableName;
        MetaType rowType;
        bool hasMethods;
        MethodInfo insertMethod;
        MethodInfo updateMethod;
        MethodInfo deleteMethod;

        internal AttributedMetaTable(AttributedMetaModel model, TableAttribute attr, Type rowType) {
            this.model = model;
            this.tableName = string.IsNullOrEmpty(attr.Name) ? rowType.Name : attr.Name;
            this.rowType = new AttributedRootType(model, this, rowType);
        }

        public override MetaModel Model {
            get { return this.model; }
        }

        public override string TableName {
            get { return this.tableName; }
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

    internal sealed class AttributedRootType : AttributedMetaType {
        Dictionary<Type, MetaType> types;
        Dictionary<object, MetaType> codeMap;
        ReadOnlyCollection<MetaType> inheritanceTypes;
        MetaType inheritanceDefault;

        internal AttributedRootType(AttributedMetaModel model, AttributedMetaTable table, Type type)
            : base(model, table, type, null) {

            // check for inheritance and create all other types
            InheritanceMappingAttribute[] inheritanceInfo = (InheritanceMappingAttribute[])type.GetCustomAttributes(typeof(InheritanceMappingAttribute), true);
            if (inheritanceInfo.Length > 0) {
                if (this.Discriminator == null) {
                    throw Error.NoDiscriminatorFound(type);
                }
                if (!MappingSystem.IsSupportedDiscriminatorType(this.Discriminator.Type)) {
                    throw Error.DiscriminatorClrTypeNotSupported(this.Discriminator.DeclaringType.Name, this.Discriminator.Name, this.Discriminator.Type);
                }
                this.types = new Dictionary<Type, MetaType>();
                this.types.Add(type, this); // add self
                this.codeMap = new Dictionary<object, MetaType>();

                // initialize inheritance types
                foreach (InheritanceMappingAttribute attr in inheritanceInfo) {
                    if (!type.IsAssignableFrom(attr.Type)) {
                        throw Error.InheritanceTypeDoesNotDeriveFromRoot(attr.Type, type);
                    }
                    if (attr.Type.IsAbstract) {
                        throw Error.AbstractClassAssignInheritanceDiscriminator(attr.Type);
                    }
                    AttributedMetaType mt = this.CreateInheritedType(type, attr.Type);
                    if (attr.Code == null) {
                        throw Error.InheritanceCodeMayNotBeNull();
                    }
                    if (mt.inheritanceCode != null) {
                        throw Error.InheritanceTypeHasMultipleDiscriminators(attr.Type);
                    }
                    object codeValue = DBConvert.ChangeType(attr.Code, this.Discriminator.Type);                
                    foreach (object d in codeMap.Keys) {
                        // if the keys are equal, or if they are both strings containing only spaces
                        // they are considered equal
                        if ((codeValue.GetType() == typeof(string) && ((string)codeValue).Trim().Length == 0 &&
                            d.GetType() == typeof(string) && ((string)d).Trim().Length == 0) ||
                            object.Equals(d, codeValue)) {
                            throw Error.InheritanceCodeUsedForMultipleTypes(codeValue);
                        }
                    }
                    mt.inheritanceCode = codeValue;
                    this.codeMap.Add(codeValue, mt);
                    if (attr.IsDefault) {
                        if (this.inheritanceDefault != null) {
                            throw Error.InheritanceTypeHasMultipleDefaults(type);
                        }
                        this.inheritanceDefault = mt;
                    }
                }

                if (this.inheritanceDefault == null) {
                    throw Error.InheritanceHierarchyDoesNotDefineDefault(type);
                }
            }

            if (this.types != null) {
                this.inheritanceTypes = this.types.Values.ToList().AsReadOnly();
            }
            else {
                this.inheritanceTypes = new MetaType[] { this }.ToList().AsReadOnly();
            }
            this.Validate();
        }

        private void Validate() {
            Dictionary<object, string> memberToColumn = new Dictionary<object, string>();
            foreach (MetaType type in this.InheritanceTypes) {
                if (type != this) {
                    TableAttribute[] attrs = (TableAttribute[])type.Type.GetCustomAttributes(typeof(TableAttribute), false);
                    if (attrs.Length > 0)
                        throw Error.InheritanceSubTypeIsAlsoRoot(type.Type);
                }
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

        public override bool HasInheritance {
            get { return this.types != null; }
        }
     
        private AttributedMetaType CreateInheritedType(Type root, Type type) {
            MetaType metaType;
            if (!this.types.TryGetValue(type, out metaType)) {
                metaType = new AttributedMetaType(this.Model, this.Table, type, this);
                this.types.Add(type, metaType);
                if (type != root && type.BaseType != typeof(object)) {
                    this.CreateInheritedType(root, type.BaseType);
                }
            }
            return (AttributedMetaType)metaType;
        }

        public override ReadOnlyCollection<MetaType> InheritanceTypes {
            get { return this.inheritanceTypes; }
        }

        public override MetaType GetInheritanceType(Type type) {
            if (type == this.Type)
                return this;
            MetaType metaType = null;
            if (this.types != null) {
                this.types.TryGetValue(type, out metaType);
            }
            return metaType;
        }

        public override MetaType InheritanceDefault {
            get { return this.inheritanceDefault; }
        }
    }

    internal class AttributedMetaType : MetaType {
        MetaModel model;
        MetaTable table;
        Type type;
        Dictionary<MetaPosition, MetaDataMember> dataMemberMap;
        ReadOnlyCollection<MetaDataMember> dataMembers;
        ReadOnlyCollection<MetaDataMember> persistentMembers;
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

        internal AttributedMetaType(MetaModel model, MetaTable table, Type type, MetaType inheritanceRoot) {
            this.model = model;
            this.table = table;
            this.type = type;
            this.inheritanceRoot = (inheritanceRoot != null) ? inheritanceRoot : this;
            // Not lazy-loading to simplify locking and enhance performance 
            // (because no lock will be required for the common read scenario).
            this.InitDataMembers();
            this.identities = this.dataMembers.Where(m => m.IsPrimaryKey).ToList().AsReadOnly();
            this.persistentMembers = this.dataMembers.Where(m => m.IsPersistent).ToList().AsReadOnly();
        }
        #region Initialization

        private void ValidatePrimaryKeyMember(MetaDataMember mm) {
            //if the type is a sub-type, no member declared in the type can be primary key
            if (mm.IsPrimaryKey && this.inheritanceRoot != this && mm.Member.DeclaringType == this.type) {
                throw(Error.PrimaryKeyInSubTypeNotSupported(this.type.Name, mm.Name));
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
                this.dataMemberMap = new Dictionary<MetaPosition, MetaDataMember>();

                int ordinal = 0;
                BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

                FieldInfo[] fis = TypeSystem.GetAllFields(this.type, flags).ToArray();
                if (fis != null) {
                    for (int i = 0, n = fis.Length; i < n; i++) {
                        FieldInfo fi = fis[i];
                        MetaDataMember mm = new AttributedMetaDataMember(this, fi, ordinal);
                        ValidatePrimaryKeyMember(mm);
                        // must be public or persistent
                        if (!mm.IsPersistent && !fi.IsPublic)
                            continue;
                        this.dataMemberMap.Add(new MetaPosition(fi), mm);
                        ordinal++;
                        // must be persistent for the rest
                        if (!mm.IsPersistent)
                            continue;
                        this.InitSpecialMember(mm);
                    }
                }
                
                PropertyInfo[] pis = TypeSystem.GetAllProperties(this.type, flags).ToArray();
                if (pis != null) {
                    for (int i = 0, n = pis.Length; i < n; i++) {
                        PropertyInfo pi = pis[i];
                        MetaDataMember mm = new AttributedMetaDataMember(this, pi, ordinal);
                        ValidatePrimaryKeyMember(mm);
                        // must be public or persistent
                        bool isPublic = (pi.CanRead && pi.GetGetMethod(false) != null)
                                        && (!pi.CanWrite || pi.GetSetMethod(false) != null);
                        if (!mm.IsPersistent && !isPublic)
                            continue;
                        this.dataMemberMap.Add(new MetaPosition(pi), mm);
                        ordinal++;
                        // must be persistent for the rest
                        if (!mm.IsPersistent)
                            continue;
                        this.InitSpecialMember(mm);
                    }
                }

                this.dataMembers = new List<MetaDataMember>(this.dataMemberMap.Values).AsReadOnly();
            }
        }

        private void InitSpecialMember(MetaDataMember mm) {
            // Can only have one auto gen member that is also an identity member,
            // except if that member is a computed column (since they are implicitly auto gen)
            if (mm.IsDbGenerated && mm.IsPrimaryKey && string.IsNullOrEmpty(mm.Expression)) {
                if (this.dbGeneratedIdentity != null)
                    throw Error.TwoMembersMarkedAsPrimaryKeyAndDBGenerated(mm.Member, this.dbGeneratedIdentity.Member);
                this.dbGeneratedIdentity = mm;
            }
            if (mm.IsPrimaryKey && !MappingSystem.IsSupportedIdentityType(mm.Type))
            {
                throw Error.IdentityClrTypeNotSupported(mm.DeclaringType, mm.Name, mm.Type);
            }
            if (mm.IsVersion) {
                if (this.version != null)
                    throw Error.TwoMembersMarkedAsRowVersion(mm.Member, this.version.Member);
                this.version = mm;
            }
            if (mm.IsDiscriminator) {
                if (this.discriminator!=null)
                    throw Error.TwoMembersMarkedAsInheritanceDiscriminator(mm.Member, this.discriminator.Member);
                this.discriminator = mm;
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
                foreach(MetaDataMember member in this.PersistentDataMembers) {
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
        public override bool HasInheritanceCode {
            get { return this.inheritanceCode != null; }
        }
        public override object InheritanceCode {
            get { return this.inheritanceCode; }
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
        public override MetaType InheritanceDefault {
            get { return this.InheritanceRoot.InheritanceDefault; }
        }
        public override bool IsInheritanceDefault {
            get { return this.InheritanceDefault == this; }
        }
        public override ReadOnlyCollection<MetaType> InheritanceTypes {
            get { return this.inheritanceRoot.InheritanceTypes; }
        }
        public override MetaType GetInheritanceType(Type inheritanceType) {
            if (inheritanceType == this.type)
                return this;
            return this.inheritanceRoot.GetInheritanceType(inheritanceType);
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
            get { return this.persistentMembers; }
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
            MetaDataMember mm = null;
            if (this.dataMemberMap.TryGetValue(new MetaPosition(mi), out mm)) {
                return mm;
            }
            else {
                // DON'T look to see if we are trying to get a member from an inherited type.
                // The calling code should know to look in the inherited type.
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

    internal sealed class AttributedMetaFunction : MetaFunction {
        private AttributedMetaModel model;
        private MethodInfo methodInfo;
        private FunctionAttribute functionAttrib;
        private MetaParameter returnParameter;
        private ReadOnlyCollection<MetaParameter> parameters;
        private ReadOnlyCollection<MetaType> rowTypes;
        static ReadOnlyCollection<MetaParameter> _emptyParameters = new List<MetaParameter>(0).AsReadOnly();
        static ReadOnlyCollection<MetaType> _emptyTypes = new List<MetaType>(0).AsReadOnly();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="metaType">The parent meta type.</param>
        /// <param name="mi">The method info.</param>
        public AttributedMetaFunction(AttributedMetaModel model, MethodInfo mi) {
            this.model = model;
            this.methodInfo = mi;
            this.rowTypes = _emptyTypes;

            this.functionAttrib = Attribute.GetCustomAttribute(mi, typeof(FunctionAttribute), false) as FunctionAttribute;
            System.Diagnostics.Debug.Assert(functionAttrib != null);

            // Gather up all mapped results
            ResultTypeAttribute[] attrs = (ResultTypeAttribute[])Attribute.GetCustomAttributes(mi, typeof(ResultTypeAttribute));
            if (attrs.Length == 0 && mi.ReturnType == typeof(IMultipleResults)) {
                throw Error.NoResultTypesDeclaredForFunction(mi.Name);
            }
            else if (attrs.Length > 1 && mi.ReturnType != typeof(IMultipleResults)) {
                throw Error.TooManyResultTypesDeclaredForFunction(mi.Name);
            }
            else if (attrs.Length <= 1 && mi.ReturnType.IsGenericType &&
                     (mi.ReturnType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                      mi.ReturnType.GetGenericTypeDefinition() == typeof(ISingleResult<>) ||
                      mi.ReturnType.GetGenericTypeDefinition() == typeof(IQueryable<>))) {
                Type elementType = TypeSystem.GetElementType(mi.ReturnType);
                this.rowTypes = new List<MetaType>(1) { this.GetMetaType(elementType) }.AsReadOnly();
            }
            else if (attrs.Length > 0) {
                List<MetaType> rowTypes = new List<MetaType>();
                foreach (ResultTypeAttribute rat in attrs) {
                    Type type = rat.Type;
                    MetaType mt = this.GetMetaType(type);
                    // Only add unique meta types
                    if (!rowTypes.Contains(mt)) {
                        rowTypes.Add(mt);
                    } 
                }
                this.rowTypes = rowTypes.AsReadOnly();
            }
            else {
                this.returnParameter = new AttributedMetaParameter(this.methodInfo.ReturnParameter);
            }

            // gather up all meta parameter
            ParameterInfo[] pis = mi.GetParameters();
            if (pis.Length > 0) {
                List<MetaParameter> mps = new List<MetaParameter>(pis.Length);
                for (int i = 0, n = pis.Length; i < n; i++) {
                    AttributedMetaParameter metaParam = new AttributedMetaParameter(pis[i]);
                    mps.Add(metaParam);
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
        /// </summary>
        private MetaType GetMetaType(Type type) {
            // call no-lock version of GetTable since this function is called only in constructor
            // and constructor is only called by function that already has a lock.
            MetaTable tbl = model.GetTableNoLocks(type);
            if (tbl != null) {
                return tbl.RowType.GetInheritanceType(type);
            } 
            return new AttributedRootType(model, null, type);
        }

        public override MetaModel Model {
            get { return this.model; }
        }
        public override MethodInfo Method {
            get { return this.methodInfo; }
        }
        public override string Name {
            get { return this.methodInfo.Name; }
        }
        public override string MappedName {
            get {
                if (!string.IsNullOrEmpty(this.functionAttrib.Name)) {
                    return this.functionAttrib.Name;
                }
                return this.methodInfo.Name;
            }
        }
        public override bool IsComposable {
            get { return this.functionAttrib.IsComposable; }
        }
        public override ReadOnlyCollection<MetaParameter> Parameters {
            get { return this.parameters; }
        }
        public override MetaParameter ReturnParameter {
            get { return this.returnParameter; }
        }
        public override bool HasMultipleResults {
            get { return this.methodInfo.ReturnType == typeof(IMultipleResults); }
        }
        public override ReadOnlyCollection<MetaType> ResultRowTypes {
            get { return this.rowTypes; }
        }
    }

    internal sealed class AttributedMetaParameter : MetaParameter {
        private ParameterInfo parameterInfo;
        private ParameterAttribute paramAttrib;

        public AttributedMetaParameter(ParameterInfo parameterInfo) {
            this.parameterInfo = parameterInfo;
            this.paramAttrib = Attribute.GetCustomAttribute(parameterInfo, typeof(ParameterAttribute), false) as ParameterAttribute;
        }
        public override ParameterInfo Parameter {
            get { return this.parameterInfo; }
        }
        public override string Name {
            get { return this.parameterInfo.Name; }
        }
        public override string MappedName {
            get {
                if (this.paramAttrib != null && this.paramAttrib.Name != null)
                    return this.paramAttrib.Name;
                return this.parameterInfo.Name;
            }
        }
        public override Type ParameterType {
            get { return this.parameterInfo.ParameterType; }
        }
        public override string DbType {
            get {
                if (this.paramAttrib != null && this.paramAttrib.DbType != null)
                    return this.paramAttrib.DbType;
                return null;
            }
        }
    }

    internal sealed class AttributedMetaDataMember : MetaDataMember {
        AttributedMetaType metaType;
        MemberInfo member;
        MemberInfo storageMember;
        int ordinal;
        Type type;
        Type declaringType;
        bool hasAccessors;
        MetaAccessor accPublic;
        MetaAccessor accPrivate;
        MetaAccessor accDefValue;
        MetaAccessor accDefSource;
        DataAttribute attr;
        ColumnAttribute attrColumn;
        AssociationAttribute attrAssoc;
        AttributedMetaAssociation assoc;
        bool isNullableType;
        bool isDeferred;
        object locktarget = new object(); // Hold locks on private object rather than public MetaType.
        bool hasLoadMethod;
        MethodInfo loadMethod;
        
        internal AttributedMetaDataMember(AttributedMetaType metaType, MemberInfo mi, int ordinal) {
            this.declaringType = mi.DeclaringType;
            this.metaType = metaType;
            this.member = mi;
            this.ordinal = ordinal;
            this.type = TypeSystem.GetMemberType(mi);
            this.isNullableType = TypeSystem.IsNullableType(this.type);
            this.attrColumn = (ColumnAttribute)Attribute.GetCustomAttribute(mi, typeof(ColumnAttribute));
            this.attrAssoc = (AssociationAttribute)Attribute.GetCustomAttribute(mi, typeof(AssociationAttribute));
            this.attr = (this.attrColumn != null) ? (DataAttribute)this.attrColumn : (DataAttribute)this.attrAssoc;
            if (this.attr != null && this.attr.Storage != null) {
                MemberInfo[] mis = mi.DeclaringType.GetMember(this.attr.Storage, BindingFlags.Instance | BindingFlags.NonPublic);
                if (mis == null || mis.Length != 1) {
                    throw Error.BadStorageProperty(this.attr.Storage, mi.DeclaringType, mi.Name);
                }
                this.storageMember = mis[0];
            }
            Type storageType = this.storageMember != null ? TypeSystem.GetMemberType(this.storageMember) : this.type;
            this.isDeferred = IsDeferredType(storageType);
            if (attrColumn != null && attrColumn.IsDbGenerated && attrColumn.IsPrimaryKey) {
                // auto-gen identities must be synced on insert
                if ((attrColumn.AutoSync != AutoSync.Default) && (attrColumn.AutoSync != AutoSync.OnInsert)) {
                    throw Error.IncorrectAutoSyncSpecification(mi.Name);
                }
            }
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
            get { return this.metaType; }
        }
        public override bool IsDeclaredBy(MetaType declaringMetaType) {
            if (declaringMetaType == null) {
                throw Error.ArgumentNull("declaringMetaType");
            } 
            return declaringMetaType.Type == this.declaringType;
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
            get { return this.attrColumn != null || this.attrAssoc != null; }
        }
        public override bool IsAssociation {
            get { return this.attrAssoc != null; }
        }
        public override bool IsPrimaryKey {
            get { return this.attrColumn != null && this.attrColumn.IsPrimaryKey; }
        }
        /// <summary>
        /// Returns true if the member is explicitly marked as auto gen, or if the
        /// member is computed or generated by the database server.
        /// </summary>
        public override bool IsDbGenerated {
            get {
                return this.attrColumn != null &&
                (this.attrColumn.IsDbGenerated || !string.IsNullOrEmpty(attrColumn.Expression)) || IsVersion;
            }
        }
        public override bool IsVersion {
            get { return this.attrColumn != null && this.attrColumn.IsVersion; }
        }
        public override bool IsDiscriminator {
            get { return attrColumn == null ? false : attrColumn.IsDiscriminator; }
        }
        public override bool CanBeNull {
            get {
                if (this.attrColumn == null) {
                    return true;
                }
                if (!this.attrColumn.CanBeNullSet) {
                    return this.isNullableType || !this.type.IsValueType;
                }
                return this.attrColumn.CanBeNull;
            }
        }
        public override string DbType {
            get {
                if (this.attrColumn != null) {
                    return this.attrColumn.DbType;
                }
                return null;
            }
        }
        public override string Expression {
            get {
                if (this.attrColumn != null) {
                    return this.attrColumn.Expression;
                }
                return null;
            }
        }
        public override string MappedName {
            get {
                if (this.attrColumn != null && this.attrColumn.Name != null) {
                    return this.attrColumn.Name;
                }
                if (this.attrAssoc != null && this.attrAssoc.Name != null) {
                    return this.attrAssoc.Name;
                }
                return this.member.Name;
            }
        }
        public override UpdateCheck UpdateCheck {
            get {
                if (this.attrColumn != null) {
                    return this.attrColumn.UpdateCheck;
                }
                return UpdateCheck.Never;
            }
        }
        public override AutoSync AutoSync {
            get {
                if (this.attrColumn != null) {
                    // auto-gen keys are always and only synced on insert
                    if (this.IsDbGenerated && this.IsPrimaryKey) {
                        return AutoSync.OnInsert;
                    }
                    // if the user has explicitly set it, use their value
                    if (attrColumn.AutoSync != AutoSync.Default) {
                        return attrColumn.AutoSync;
                    }
                    // database generated members default to always
                    if (this.IsDbGenerated) {
                        return AutoSync.Always;
                    }
                }
                return AutoSync.Never;
            }
        }
        public override MetaAssociation Association {
            get {
                if (this.IsAssociation) {
                    // LOCKING: This deferral isn't an optimization. It can't be done in the constructor
                    // because there may be loops in the association graph.
                    if (this.assoc == null) {
                        lock (this.locktarget) {
                            if (this.assoc == null) {
                                this.assoc = new AttributedMetaAssociation(this, this.attrAssoc);
                            }
                        }
                    }
                }
                return this.assoc;
            }
        }
        public override MethodInfo LoadMethod {
            get {
                if (this.hasLoadMethod == false && (this.IsDeferred || this.IsAssociation)) {
                    // defer searching for this access method until we really need to know
                    this.loadMethod = MethodFinder.FindMethod(
                        ((AttributedMetaModel)this.metaType.Model).ContextType,
                        "Load" + this.member.Name,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                        new Type[] { this.DeclaringType.Type }
                        );
                    this.hasLoadMethod = true;
                }
                return this.loadMethod;
            }
        }
        private bool IsDeferredType(Type entityType) {
            if (entityType == null || entityType == typeof(object)) {
                return false;
            }
            if (entityType.IsGenericType) {
                Type gtype = entityType.GetGenericTypeDefinition();
                return gtype == typeof(Link<>) ||
                    typeof(EntitySet<>).IsAssignableFrom(gtype) ||
                    typeof(EntityRef<>).IsAssignableFrom(gtype) ||
                    IsDeferredType(entityType.BaseType);
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
            Type objectDeclaringType, MetaAccessor accessor,
            out MetaAccessor accessorValue, out MetaAccessor accessorDeferredValue, out MetaAccessor accessorDeferredSource
            ) {
            if (accessor.Type.IsGenericType) {
                Type gtype = accessor.Type.GetGenericTypeDefinition();
                Type itemType = accessor.Type.GetGenericArguments()[0];
                if (gtype == typeof(Link<>)) {
                    accessorValue = CreateAccessor(typeof(LinkValueAccessor<,>).MakeGenericType(objectDeclaringType, itemType), accessor);
                    accessorDeferredValue = CreateAccessor(typeof(LinkDefValueAccessor<,>).MakeGenericType(objectDeclaringType, itemType), accessor);
                    accessorDeferredSource = CreateAccessor(typeof(LinkDefSourceAccessor<,>).MakeGenericType(objectDeclaringType, itemType), accessor);
                    return;
                }
                else if (typeof(EntityRef<>).IsAssignableFrom(gtype)) {
                    accessorValue = CreateAccessor(typeof(EntityRefValueAccessor<,>).MakeGenericType(objectDeclaringType, itemType), accessor);
                    accessorDeferredValue = CreateAccessor(typeof(EntityRefDefValueAccessor<,>).MakeGenericType(objectDeclaringType, itemType), accessor);
                    accessorDeferredSource = CreateAccessor(typeof(EntityRefDefSourceAccessor<,>).MakeGenericType(objectDeclaringType, itemType), accessor);
                    return;
                }
                else if (typeof(EntitySet<>).IsAssignableFrom(gtype)) {
                    accessorValue = CreateAccessor(typeof(EntitySetValueAccessor<,>).MakeGenericType(objectDeclaringType, itemType), accessor);
                    accessorDeferredValue = CreateAccessor(typeof(EntitySetDefValueAccessor<,>).MakeGenericType(objectDeclaringType, itemType), accessor);
                    accessorDeferredSource = CreateAccessor(typeof(EntitySetDefSourceAccessor<,>).MakeGenericType(objectDeclaringType, itemType), accessor);
                    return;
                }
            }
            throw Error.UnhandledDeferredStorageType(accessor.Type);
        }
        private static MetaAccessor CreateAccessor(Type accessorType, params object[] args) {
            return (MetaAccessor)Activator.CreateInstance(accessorType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, args, null);
        }
        public override string ToString() {
            return this.DeclaringType.ToString() + ":" + this.Member.ToString();
        }
    }

    internal class AttributedMetaAssociation : MetaAssociationImpl {
        AttributedMetaDataMember thisMember;
        MetaDataMember otherMember;
        ReadOnlyCollection<MetaDataMember> thisKey;
        ReadOnlyCollection<MetaDataMember> otherKey;
        MetaType otherType;
        bool isMany;
        bool isForeignKey;
        bool isUnique;
        bool isNullable = true;
        bool thisKeyIsPrimaryKey;
        bool otherKeyIsPrimaryKey;
        string deleteRule;
        bool deleteOnNull;

        internal AttributedMetaAssociation(AttributedMetaDataMember member, AssociationAttribute attr) {
            this.thisMember = member;

            this.isMany = TypeSystem.IsSequenceType(this.thisMember.Type);
            Type ot = this.isMany ? TypeSystem.GetElementType(this.thisMember.Type) : this.thisMember.Type;
            this.otherType = this.thisMember.DeclaringType.Model.GetMetaType(ot);
            this.thisKey = (attr.ThisKey != null) ? MakeKeys(this.thisMember.DeclaringType, attr.ThisKey) : this.thisMember.DeclaringType.IdentityMembers;
            this.otherKey = (attr.OtherKey != null) ? MakeKeys(otherType, attr.OtherKey) : this.otherType.IdentityMembers;
            this.thisKeyIsPrimaryKey = AreEqual(this.thisKey, this.thisMember.DeclaringType.IdentityMembers);
            this.otherKeyIsPrimaryKey = AreEqual(this.otherKey, this.otherType.IdentityMembers);
            this.isForeignKey = attr.IsForeignKey;

            this.isUnique = attr.IsUnique;
            this.deleteRule = attr.DeleteRule;
            this.deleteOnNull = attr.DeleteOnNull;

            // if any key members are not nullable, the association is not nullable
            foreach (MetaDataMember mm in thisKey) {
                if (!mm.CanBeNull) {
                    this.isNullable = false;
                    break;
                }
            }

            // validate DeleteOnNull specification
            if (deleteOnNull == true) {
                if( !(isForeignKey && !isMany && !isNullable) ) {
                    throw Error.InvalidDeleteOnNullSpecification(member);
                }
            }

            //validate the number of ThisKey columns is the same as the number of OtherKey columns
            if (this.thisKey.Count != this.otherKey.Count && this.thisKey.Count > 0 && this.otherKey.Count > 0) {
                throw Error.MismatchedThisKeyOtherKey(member.Name, member.DeclaringType.Name);
            }

            // determine reverse reference member
            foreach (MetaDataMember omm in this.otherType.PersistentDataMembers) {
                AssociationAttribute oattr = (AssociationAttribute)Attribute.GetCustomAttribute(omm.Member, typeof(AssociationAttribute));
                if (oattr != null) {
                    if (omm != this.thisMember && oattr.Name == attr.Name) {
                        this.otherMember = omm;
                        break;
                    }
                }
            }
        }

        public override MetaType OtherType {
            get { return this.otherType; }
        }
        public override MetaDataMember ThisMember {
            get { return this.thisMember; }
        }
        public override MetaDataMember OtherMember {
            get { return this.otherMember; }
        }
        public override ReadOnlyCollection<MetaDataMember> ThisKey {
            get { return this.thisKey; }
        }
        public override ReadOnlyCollection<MetaDataMember> OtherKey {
            get { return this.otherKey; }
        }
        public override bool ThisKeyIsPrimaryKey {
            get { return this.thisKeyIsPrimaryKey; }
        }
        public override bool OtherKeyIsPrimaryKey {
            get { return this.otherKeyIsPrimaryKey; }
        }
        public override bool IsMany {
            get { return this.isMany; }
        }
        public override bool IsForeignKey {
            get { return this.isForeignKey; }
        }
        public override bool IsUnique {
            get { return this.isUnique; }
        }
        public override bool IsNullable {
            get { return this.isNullable; }
        }
        public override string DeleteRule {
            get { return this.deleteRule; }
        }
        public override bool DeleteOnNull {
            get { return this.deleteOnNull; }
        }
    }
}
