using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq.Mapping {

    /// <summary>
    /// Attribute placed on a method mapped to a User Defined Function.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class FunctionAttribute : Attribute {
        string name;
        bool isComposable;
        public FunctionAttribute() {
        }
        public string Name {
            get { return this.name; }
            set { this.name = value; }
        }
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Composable", Justification="Spelling is correct.")]
        public bool IsComposable {
            get { return this.isComposable; }
            set { this.isComposable = value; }
        }
    }

    /// <summary>
    /// This attribute is applied to functions returning multiple result types,
    /// to declare the possible result types returned from the function.  For
    /// inheritance types, only the root type of the inheritance hierarchy need
    /// be specified.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class ResultTypeAttribute : Attribute {
        Type type;
        public ResultTypeAttribute(Type type) {
            this.type = type;
        }
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "The contexts in which this is available are fairly specific.")]
        public Type Type {
            get { return this.type; }
        }
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = false)]
    public sealed class ParameterAttribute : Attribute {
        string name;
        string dbType;
        public ParameterAttribute() {
        }
        public string Name {
            get { return this.name; }
            set { this.name = value; }
        }
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db", Justification = "Conforms to legacy spelling.")]
        public string DbType {
            get { return this.dbType; }
            set { this.dbType = value; }
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class DatabaseAttribute : Attribute {
        string name;
        public DatabaseAttribute() {
        }
        public string Name {
            get { return this.name; }
            set { this.name = value; }
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class TableAttribute : Attribute {
        string name;
        public TableAttribute() {
        }
        public string Name {
            get { return this.name; }
            set { this.name = value; }
        }
    }

    /// <summary>
    /// Class attribute used to describe an inheritance hierarchy to be mapped.
    /// For example, 
    /// 
    ///     [Table(Name = "People")]
    ///     [InheritanceMapping(Code = "P", Type = typeof(Person), IsDefault=true)]
    ///     [InheritanceMapping(Code = "C", Type = typeof(Customer))]
    ///     [InheritanceMapping(Code = "E", Type = typeof(Employee))]
    ///     class Person { ... }
    ///     
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited = false)]
    public sealed class InheritanceMappingAttribute : Attribute {
        private object code;
        private Type type;
        private bool isDefault;

        /// <summary>
        /// Discriminator value in store column for this type.
        /// </summary>
        public object Code {
            get { return this.code; }
            set { this.code = value; }
        }
        /// <summary>
        /// Type to instantiate when Key is matched.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification="The contexts in which this is available are fairly specific.")]
        public Type Type {
            get { return this.type; }
            set { this.type = value; }
        }

        /// <summary>
        /// If discriminator value in store column is unrecognized then instantiate this type.
        /// </summary>
        public bool IsDefault {
            get { return this.isDefault; }
            set { this.isDefault = value; }
        }

    }

    public abstract class DataAttribute : Attribute {
        string name;
        string storage;
        protected DataAttribute() { }
        public string Name {
            get { return this.name; }
            set { name = value; }
        }
        public string Storage {
            get { return this.storage; }
            set { this.storage = value; }
        }
    }

    public enum UpdateCheck {
        Always,
        Never,
        WhenChanged
    }

    /// <summary>
    /// Used to specify for during insert and update operations when
    /// a data member should be read back after the operation completes.
    /// </summary>
    public enum AutoSync {
        Default = 0, // Automatically choose
        Always = 1,
        Never = 2,
        OnInsert = 3,
        OnUpdate = 4 
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ColumnAttribute : DataAttribute {
        string dbtype;
        string expression;
        bool isPrimaryKey;
        bool isDBGenerated;
        bool isVersion;
        bool isDiscriminator;
        bool canBeNull = true;
        UpdateCheck check;
        AutoSync autoSync = AutoSync.Default;
        bool canBeNullSet = false;

        public ColumnAttribute() {
            check = UpdateCheck.Always;
        }
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db", Justification="Conforms to legacy spelling.")]
        public string DbType {
            get { return this.dbtype; }
            set { this.dbtype = value; }
        }
        public string Expression {
            get { return this.expression; }
            set { this.expression = value; }
        }
        public bool IsPrimaryKey {
            get { return this.isPrimaryKey; }
            set { this.isPrimaryKey = value; }
        }
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db", Justification = "Conforms to legacy spelling.")]
        public bool IsDbGenerated {
            get { return this.isDBGenerated; }
            set { this.isDBGenerated = value; }
        }
        public bool IsVersion {
            get { return this.isVersion; }
            set { this.isVersion = value; }
        }
        public UpdateCheck UpdateCheck {
            get { return this.check; }
            set { this.check = value; }
        }
        public AutoSync AutoSync {
            get { return this.autoSync; }
            set { this.autoSync = value; }
        }
        public bool IsDiscriminator {
            get { return this.isDiscriminator; }
            set { isDiscriminator = value; }
        }
        public bool CanBeNull { 
            get {return this.canBeNull;}
            set {
                this.canBeNullSet = true;
                this.canBeNull = value;
            }
        }
        internal bool CanBeNullSet {
            get {return this.canBeNullSet;}
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class AssociationAttribute : DataAttribute {
        string thisKey;
        string otherKey;
        bool isUnique;
        bool isForeignKey;
        bool deleteOnNull;
        string deleteRule;
        
        public AssociationAttribute() { }
        public string ThisKey {
            get { return this.thisKey; }
            set { this.thisKey = value; }
        }
        public string OtherKey {
            get { return this.otherKey; }
            set { this.otherKey = value; }
        }
        public bool IsUnique {
            get { return this.isUnique; }
            set { this.isUnique = value; }
        }
        public bool IsForeignKey {
            get { return this.isForeignKey; }
            set { this.isForeignKey = value; }
        }
        public string DeleteRule {
            get { return this.deleteRule; }
            set { this.deleteRule = value; }
        }
        public bool DeleteOnNull {
            get { return this.deleteOnNull; }
            set { this.deleteOnNull = value; }
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ProviderAttribute : Attribute {
        Type providerType;

        public ProviderAttribute() {
        }

        public ProviderAttribute(Type type) {
            this.providerType = type;
        }

        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "The contexts in which this is available are fairly specific.")]
        public Type Type {
            get { return this.providerType; }
        }
    }
}
