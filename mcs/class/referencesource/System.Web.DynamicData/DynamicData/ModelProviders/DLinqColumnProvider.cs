using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace System.Web.DynamicData.ModelProviders {
    internal sealed class DLinqColumnProvider : ColumnProvider {
        private static Regex s_varCharRegEx = new Regex(@"N?(?:Var)?Char\(([0-9]+)\)", RegexOptions.IgnoreCase); // accepts char, nchar, varchar, and nvarchar

        private AttributeCollection _attributes;
        private AssociationProvider _association;
        private bool _isAssociation;

        public DLinqColumnProvider(DLinqTableProvider table, MetaDataMember member)
            : base(table) {
            Member = member;
            Name = member.Name;
            ColumnType = GetMemberType(member);
            IsPrimaryKey = member.IsPrimaryKey;
            IsGenerated = member.IsDbGenerated;
            _isAssociation = member.IsAssociation;
            IsCustomProperty = !member.IsAssociation && Member.DbType == null;
            Nullable = Member.IsAssociation ? Member.Association.IsNullable : Member.CanBeNull;
            MaxLength = ProcessMaxLength(ColumnType, Member.DbType);
            IsSortable = ProcessIsSortable(ColumnType, Member.DbType);
        }

        public override AttributeCollection Attributes {
            get {
                if (!Member.IsDiscriminator)
                    return base.Attributes;

                if (_attributes == null) {
                    List<Attribute> newAttributes = new List<Attribute>();
                    bool foundScaffoldAttribute = false;

                    foreach (Attribute attr in base.Attributes) {
                        if (attr is ScaffoldColumnAttribute) {
                            foundScaffoldAttribute = true;
                            break;
                        }
                        newAttributes.Add(attr);
                    }

                    if (foundScaffoldAttribute)
                        _attributes = base.Attributes;
                    else {
                        newAttributes.Add(new ScaffoldColumnAttribute(false));
                        _attributes = new AttributeCollection(newAttributes.ToArray());
                    }
                }

                return _attributes;
            }
        }

        // internal to facilitate unit testing
        internal static int ProcessMaxLength(Type memberType, String dbType) {
            // Only strings and chars that come in from a database have max lengths
            if (dbType == null || (memberType != typeof(string) && Misc.RemoveNullableFromType(memberType) != typeof(char)))
                return 0;

            if (dbType.StartsWith("NText", StringComparison.OrdinalIgnoreCase)) {
                return Int32.MaxValue >> 1; // see sql server 2005 spec for ntext
            }

            if (dbType.StartsWith("Text", StringComparison.OrdinalIgnoreCase)) {
                return Int32.MaxValue; // see sql server 2005 spec for text
            }

            if (dbType.StartsWith("NVarChar(MAX)", StringComparison.OrdinalIgnoreCase)) {
                return (Int32.MaxValue >> 1) - 2; // see sql server 2005 spec for nvarchar
            }

            if (dbType.StartsWith("VarChar(MAX)", StringComparison.OrdinalIgnoreCase)) {
                return Int32.MaxValue - 2; // see sql server 2005 spec for varchar
            }

            Match m = s_varCharRegEx.Match(dbType);
            if (m.Success) {
                return Int32.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
            }

            return 0;
        }

        internal static bool ProcessIsSortable(Type memberType, String dbType) {
            if (dbType == null)
                return false;

            if (memberType == typeof(string) &&
                (dbType.StartsWith("Text", StringComparison.OrdinalIgnoreCase)
                  || dbType.StartsWith("NText", StringComparison.OrdinalIgnoreCase))) {
                return false;
            }

            if (memberType == typeof(Binary) && dbType.StartsWith("Image", StringComparison.OrdinalIgnoreCase)) {
                return false;
            }

            if (memberType == typeof(XElement)) {
                return false;
            }

            return true;
        }

        internal MetaDataMember Member {
            get;
            private set;
        }

        internal void Initialize() {
            if (_isAssociation && _association == null) {
                _association = new DLinqAssociationProvider(this);
            }
        }

        internal bool ShouldRemove { get; set; }

        private static Type GetMemberType(MetaDataMember member) {
            Type type = member.Type;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(EntitySet<>)) {
                return type.GetGenericArguments()[0];
            }
            else {
                return type;
            }
        }

        #region IEntityMember Members

        public override PropertyInfo EntityTypeProperty {
            get { return (PropertyInfo)Member.Member; }
        }

        public override AssociationProvider Association {
            get {
                Initialize();

                return _association;
            }
        }

        internal new bool IsForeignKeyComponent {
            set {
                base.IsForeignKeyComponent = value;
            }
        }

        #endregion
    }
}
