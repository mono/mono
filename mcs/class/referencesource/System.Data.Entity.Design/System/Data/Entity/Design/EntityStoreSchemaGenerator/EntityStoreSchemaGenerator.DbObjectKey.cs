//---------------------------------------------------------------------
// <copyright file="EntityStoreSchemaGenerator.DbObjectKey.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System.Diagnostics;
namespace System.Data.Entity.Design
{
    public sealed partial class EntityStoreSchemaGenerator
    {
        internal enum DbObjectType
        {
            Unknown,
            Table,
            View,
            Function,
        }

        [DebuggerDisplay("Catalog={Catalog}, Schema={Schema}, Name={TableName}")]
        internal struct DbObjectKey
        {
            
            private readonly string _catalog;
            private readonly string _schema;
            private readonly string _name;
            private readonly DbObjectType _objectType;

            public DbObjectKey(object catalog, object schema, object name, DbObjectType objectType)
            {
                if (catalog != null && !Convert.IsDBNull(catalog))
                {
                    _catalog = catalog.ToString();
                }
                else
                {
                    _catalog = null;
                }

                if (schema != null && !Convert.IsDBNull(schema))
                {
                    _schema = schema.ToString();
                }
                else
                {
                    _schema = null;
                }

                if (name != null && !Convert.IsDBNull(name))
                {
                    _name = name.ToString();
                }
                else
                {
                    _name = null;
                }

                _objectType = objectType;
                Debug.Assert(_catalog != null || _schema != null || _name != null, "This is going to look like an empty one, just ue the default constructor");
            }

            public static bool operator ==(DbObjectKey lhs, DbObjectKey rhs)
            {
                return lhs.Equals(rhs);
            }
            public static bool operator !=(DbObjectKey lhs, DbObjectKey rhs)
            {
                return !(lhs == rhs);
            }
            public string Catalog
            {
                get { return _catalog; }
            }

            public string Schema
            {
                get { return _schema; }
            }

            public string TableName
            {
                get { return _name; }
            }

            public DbObjectType ObjectType
            {
                get { return _objectType; }
            }

            public override bool Equals(object obj)
            {
                if (!(obj is DbObjectKey))
                {
                    return false;
                }

                DbObjectKey key = (DbObjectKey)obj;
                if (key._catalog != _catalog)
                {
                    return false;
                }

                if (key._schema != _schema)
                {
                    return false;
                }

                if (key._name != _name)
                {
                    return false;
                }

                // objectType does not count in equality
                return true;
            }

            public override int GetHashCode()
            {
                int hashCode = 0;
                if (_catalog != null)
                {
                    hashCode ^= _catalog.GetHashCode();
                }

                if (_schema != null)
                {
                    hashCode ^= _schema.GetHashCode();
                }

                if (_name != null)
                {
                    hashCode ^= _name.GetHashCode();
                }

                // objectType does not count in equality

                return hashCode;
            }

            public override string ToString()
            {
                string value = string.Empty;
                if (Catalog != null)
                {
                    value += Catalog;
                }

                if (Schema != null)
                {
                    if (value != string.Empty)
                    {
                        value += ".";
                    }
                    value += Schema;
                }

                if (TableName != null)
                {
                    if (value != string.Empty)
                    {
                        value += ".";
                    }
                    value += TableName;
                }

                return value;
            }

            internal bool IsEmpty
            {
                get
                {
                    return _catalog == null && _schema == null && _name == null;
                }
            }
        }
    }
}
