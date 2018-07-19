using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Globalization;

namespace LinqToSqlShared.Mapping {
    /// <summary>
    /// DatabaseMapping and related classes represent a parsed version of the
    /// XML mapping string. This unvalidated intermediate representation is 
    /// necessary because unused mappings are intentially never validated.
    /// </summary>
    internal class DatabaseMapping {
        string databaseName;
        string provider;
        List<TableMapping> tables;
        List<FunctionMapping> functions;

        internal DatabaseMapping() {
            this.tables = new List<TableMapping>();
            this.functions = new List<FunctionMapping>();
        }

        internal string DatabaseName {
            get { return this.databaseName; }
            set { this.databaseName = value; }
        }

        internal string Provider {
            get { return this.provider; }
            set { this.provider = value; }
        }

        internal List<TableMapping> Tables {
            get { return this.tables; }
        }

        internal List<FunctionMapping> Functions {
            get { return this.functions; }
        }

        internal TableMapping GetTable(string tableName) {
            foreach (TableMapping tmap in this.tables) {
                if (string.Compare(tmap.TableName, tableName, StringComparison.Ordinal) == 0)
                    return tmap;
            }
            return null;
        }

        internal TableMapping GetTable(Type rowType) {
            foreach (TableMapping tableMap in this.tables) {
                if (this.IsType(tableMap.RowType, rowType)) {
                    return tableMap;
                }
            }
            return null;
        }

        private bool IsType(TypeMapping map, Type type) {
            if (string.Compare(map.Name, type.Name, StringComparison.Ordinal) == 0
                || string.Compare(map.Name, type.FullName, StringComparison.Ordinal) == 0
                || string.Compare(map.Name, type.AssemblyQualifiedName, StringComparison.Ordinal) == 0)
                return true;
            foreach (TypeMapping subMap in map.DerivedTypes) {
                if (this.IsType(subMap, type))
                    return true;
            }
            return false;
        }

        internal FunctionMapping GetFunction(string functionName) {
            foreach (FunctionMapping fmap in this.functions) {
                if (string.Compare(fmap.Name, functionName, StringComparison.Ordinal) == 0)
                    return fmap;
            }
            return null;
        }
    }

    /// <summary>
    /// Constants in the mapping schema.
    /// </summary>
    class XmlMappingConstant {
        internal const string Association = "Association";
        internal const string AutoSync = "AutoSync";
        internal const string Column = "Column";
        internal const string Database = "Database";
        internal const string DbType = "DbType";
        internal const string DeleteRule = "DeleteRule";
        internal const string DeleteOnNull = "DeleteOnNull";
        internal const string Direction = "Direction";
        internal const string ElementType = "ElementType";
        internal const string Expression = "Expression";
        internal const string False = "false";
        internal const string Function = "Function";
        internal const string InheritanceCode = "InheritanceCode";
        internal const string IsComposable = "IsComposable";
        internal const string IsDbGenerated = "IsDbGenerated";
        internal const string IsDiscriminator = "IsDiscriminator";
        internal const string IsPrimaryKey = "IsPrimaryKey";
        internal const string IsInheritanceDefault = "IsInheritanceDefault";
        internal const string IsForeignKey = "IsForeignKey";
        internal const string IsUnique = "IsUnique";
        internal const string IsVersion = "IsVersion";
        internal const string MappingNamespace = "http://schemas.microsoft.com/linqtosql/mapping/2007";
        internal const string Member = "Member";
        internal const string Method = "Method";
        internal const string Name = "Name";
        internal const string CanBeNull = "CanBeNull";
        internal const string OtherKey = "OtherKey";
        internal const string Parameter = "Parameter";
        internal const string Provider = "Provider";
        internal const string Return = "Return";
        internal const string Storage = "Storage";
        internal const string Table = "Table";
        internal const string ThisKey = "ThisKey";
        internal const string True = "true";
        internal const string Type = "Type";
        internal const string UpdateCheck = "UpdateCheck";
    }

    internal class TableMapping {
        string tableName;
        string member;
        TypeMapping rowType;

        internal TableMapping() {
        }

        internal string TableName {
            get { return this.tableName; }
            set { this.tableName = value; }
        }

        internal string Member {
            get { return this.member; }
            set { this.member = value; }
        }

        internal TypeMapping RowType {
            get { return this.rowType; }
            set { this.rowType = value; }
        }
    }

    internal class FunctionMapping {
        string name;
        string methodName;
        bool isComposable;
        List<ParameterMapping> parameters;
        List<TypeMapping> types;
        ReturnMapping funReturn;

        internal FunctionMapping() {
            this.parameters = new List<ParameterMapping>();
            this.types = new List<TypeMapping>();
        }

        internal string Name {
            get { return this.name; }
            set { this.name = value; }
        }

        internal string MethodName {
            get { return this.methodName; }
            set { this.methodName = value; }
        }

        internal bool IsComposable {
            get { return this.isComposable; }
            set { this.isComposable = value; }
        }

        internal string XmlIsComposable {
            get { return this.isComposable ? XmlMappingConstant.True : null; }
            set { this.isComposable = (value != null) ? bool.Parse(value) : false; }
        }

        internal List<ParameterMapping> Parameters {
            get { return this.parameters; }
        }

        internal List<TypeMapping> Types {
            get { return this.types; }
        }

        internal ReturnMapping FunReturn {
            get { return this.funReturn; }
            set { this.funReturn = value; }
        }
    }

    internal enum MappingParameterDirection {
        In,
        Out,
        InOut
    }

    internal class ParameterMapping {
        string name;
        string parameterName;
        string dbType;
        MappingParameterDirection direction;

        internal string Name {
            get { return this.name; }
            set { this.name = value; }
        }

        internal string ParameterName {
            get { return this.parameterName; }
            set { this.parameterName = value; }
        }

        internal string DbType {
            get { return this.dbType; }
            set { this.dbType = value; }
        }

        public string XmlDirection {
            get { return this.direction == MappingParameterDirection.In ? null : this.direction.ToString(); }
            set {
                this.direction = (value == null)
                    ? MappingParameterDirection.In
                    : (MappingParameterDirection)Enum.Parse(typeof(MappingParameterDirection), value, true);
            }
        }

        public MappingParameterDirection Direction {
            get { return this.direction; }
            set { this.direction = value; }
        }
    }

    internal class ReturnMapping {
        string dbType;

        internal string DbType {
            get { return this.dbType; }
            set { this.dbType = value; }
        }
    }


    internal class TypeMapping {
        string name;
        TypeMapping baseType;
        List<MemberMapping> members;
        string inheritanceCode;
        bool isInheritanceDefault;
        List<TypeMapping> derivedTypes;

        internal TypeMapping() {
            this.members = new List<MemberMapping>();
            this.derivedTypes = new List<TypeMapping>();
        }

        internal TypeMapping BaseType {
            get { return this.baseType; }
            set { this.baseType = value; }
        }

        internal string Name {
            get { return this.name; }
            set { this.name = value; }
        }

        internal List<MemberMapping> Members {
            get { return this.members; }
        }

        internal string InheritanceCode {
            get { return this.inheritanceCode; }
            set { this.inheritanceCode = value; }
        }

        internal bool IsInheritanceDefault {
            get { return this.isInheritanceDefault; }
            set { this.isInheritanceDefault = value; }
        }

        internal string XmlIsInheritanceDefault {
            get { return this.isInheritanceDefault ? XmlMappingConstant.True : null; }
            set { this.isInheritanceDefault = (value != null) ? bool.Parse(value) : false; }
        }

        internal List<TypeMapping> DerivedTypes {
            get { return this.derivedTypes; }
        }
    }

    internal abstract class MemberMapping {
        string name;
        string member;
        string storageMember;

        internal MemberMapping() {
        }

        internal string DbName {
            get { return this.name; }
            set { this.name = value; }
        }

        internal string MemberName {
            get { return this.member; }
            set { this.member = value; }
        }

        internal string StorageMemberName {
            get { return this.storageMember; }
            set { this.storageMember = value; }
        }
    }

    internal sealed class ColumnMapping : MemberMapping {
        string dbType;
        string expression;
        bool isPrimaryKey;
        bool isDBGenerated;
        bool isVersion;
        bool isDiscriminator;
        bool? canBeNull = null;
        UpdateCheck updateCheck;
        AutoSync autoSync;

        internal ColumnMapping() {
        }

        internal string DbType {
            get { return this.dbType; }
            set { this.dbType = value; }
        }

        internal bool? CanBeNull {
            get { return this.canBeNull; }
            set { this.canBeNull = value; }
        }

        internal string XmlCanBeNull {
            get {
                if (this.canBeNull == null) return null;
                return this.canBeNull == true ? null : XmlMappingConstant.False;
            }
            set { this.canBeNull = (value != null) ? bool.Parse(value) : true; }
        }

        internal string Expression {
            get { return this.expression; }
            set { this.expression = value; }
        }

        internal bool IsPrimaryKey {
            get { return this.isPrimaryKey; }
            set { this.isPrimaryKey = value; }
        }

        internal string XmlIsPrimaryKey {
            get { return this.isPrimaryKey ? XmlMappingConstant.True : null; }
            set { this.isPrimaryKey = (value != null) ? bool.Parse(value) : false; }
        }

        internal bool IsDbGenerated {
            get { return this.isDBGenerated; }
            set { this.isDBGenerated = value; }
        }

        internal string XmlIsDbGenerated {
            get { return this.isDBGenerated ? XmlMappingConstant.True : null; }
            set { this.isDBGenerated = (value != null) ? bool.Parse(value) : false; }
        }

        internal bool IsVersion {
            get { return this.isVersion; }
            set { this.isVersion = value; }
        }

        internal string XmlIsVersion {
            get { return this.isVersion ? XmlMappingConstant.True : null; }
            set { this.isVersion = (value != null) ? bool.Parse(value) : false; }
        }

        internal bool IsDiscriminator {
            get { return this.isDiscriminator; }
            set { this.isDiscriminator = value; }
        }

        internal string XmlIsDiscriminator {
            get { return this.isDiscriminator ? XmlMappingConstant.True : null; }
            set { this.isDiscriminator = (value != null) ? bool.Parse(value) : false; }
        }

        internal UpdateCheck UpdateCheck {
            get { return this.updateCheck; }
            set { this.updateCheck = value; }
        }

        internal string XmlUpdateCheck {
            get { return this.updateCheck != UpdateCheck.Always ? this.updateCheck.ToString() : null; }
            set { this.updateCheck = (value == null) ? UpdateCheck.Always : (UpdateCheck)Enum.Parse(typeof(UpdateCheck), value); }
        }

        internal AutoSync AutoSync {
            get { return this.autoSync; }
            set { this.autoSync = value; }
        }

        internal string XmlAutoSync {
            get { return this.autoSync != AutoSync.Default ? this.autoSync.ToString() : null; }
            set { this.autoSync = (value != null) ? (AutoSync)Enum.Parse(typeof(AutoSync), value) : AutoSync.Default; }
        }
    }

    internal sealed class AssociationMapping : MemberMapping {
        string thisKey;
        string otherKey;
        string deleteRule;
        bool deleteOnNull;
        bool isForeignKey;
        bool isUnique;

        internal AssociationMapping() {
        }

        internal string ThisKey {
            get { return this.thisKey; }
            set { this.thisKey = value; }
        }

        internal string OtherKey {
            get { return this.otherKey; }
            set { this.otherKey = value; }
        }

        internal string DeleteRule {
            get { return this.deleteRule; }
            set { this.deleteRule = value; }
        }

        internal bool DeleteOnNull {
            get { return this.deleteOnNull; }
            set { this.deleteOnNull = value; }
        }

        internal bool IsForeignKey {
            get { return this.isForeignKey; }
            set { this.isForeignKey = value; }
        }

        internal string XmlIsForeignKey {
            get { return this.isForeignKey ? XmlMappingConstant.True : null; }
            set { this.isForeignKey = (value != null) ? bool.Parse(value) : false; }
        }

        internal string XmlDeleteOnNull {
            get { return this.deleteOnNull ? XmlMappingConstant.True : null; }
            set { this.deleteOnNull = (value != null) ? bool.Parse(value) : false; }
        }

        internal bool IsUnique {
            get { return this.isUnique; }
            set { this.isUnique = value; }
        }

        internal string XmlIsUnique {
            get { return this.isUnique ? XmlMappingConstant.True : null; }
            set { this.isUnique = (value != null) ? bool.Parse(value) : false; }
        }
    }

    /// <summary>
    /// Shared rules governing the mapping system.
    /// </summary>
    internal static class MappingSystem {
        /// <summary>
        /// Return true if this is a clr type supported as an inheritance discriminator.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsSupportedDiscriminatorType(Type type) {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                type = type.GetGenericArguments()[0];
            }
            switch (Type.GetTypeCode(type)) {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Char:
                case TypeCode.String:
                case TypeCode.Boolean:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Return true if this is a clr type supported as an inheritance discriminator.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsSupportedDiscriminatorType(SqlDbType type) {
            switch (type) {
                case SqlDbType.BigInt:
                case SqlDbType.Bit:
                case SqlDbType.Char:
                case SqlDbType.Int:
                case SqlDbType.NChar:
                case SqlDbType.NVarChar:
                case SqlDbType.SmallInt:
                case SqlDbType.TinyInt:
                case SqlDbType.VarChar:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Return true if this is a CLR type supported as an identity member.  Since identity
        /// management (caching) relies on key members being hashable, only types implementing
        /// GetHashCode are supported.  Also, the runtime relies on identity members being comparable,
        /// so only types implementing Equals are supported.
        /// </summary>
        internal static bool IsSupportedIdentityType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments()[0];
            }
            if (type == typeof(Guid) || type == typeof(DateTime) || type == typeof(DateTimeOffset) ||
                type == typeof(TimeSpan) || type == typeof(Binary))
            {
                return true;
            }
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Char:
                case TypeCode.String:
                case TypeCode.Boolean:
                case TypeCode.Decimal:
                case TypeCode.Single:
                case TypeCode.Double:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Types that do not support comparison cannot be used as primary keys.  The
        /// database will restrict this, but we can't rely on that, since it is possible
        /// to create a key mapping to a column that isn't truly a key in the DB.
        /// </summary>
        internal static bool IsSupportedIdentityType(SqlDbType type)
        {
            switch (type) {
                case SqlDbType.NText:
                case SqlDbType.Image:
                case SqlDbType.Xml:
                case SqlDbType.Text:
                case SqlDbType.Variant:
                case SqlDbType.Udt:
                    return false;
                default:
                    return true;
            }
        }

    }
}
