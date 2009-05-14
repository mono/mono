#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Stefan Klinger
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

#region grammar
/* ----------------
default namespace = "http://schemas.microsoft.com/linqtosql/mapping/2007"
grammar {

start = element Database { Database }

Database = {
  element Table { Table }*,
  element Function { Function }*,
  attribute Name { text }?,
  attribute Provider { text }?
}

Table = {
  element Type { Type },
  attribute Name { text }?,
  attribute Member { text }?
}

Type = {
  {
    element Column { Column }* |
    element Association { Association }*
  }*,
  element Type { Type }*,
  attribute Name { text },
  attribute InheritanceCode { text }?,
  attribute IsInheritanceDefault { boolean }?
}

Column = {
  attribute Name { text }?,
  attribute Member { text },
  attribute Storage { text }?,
  attribute DbType { text }?,
  attribute IsPrimaryKey { boolean }?,
  attribute IsDbGenerated { boolean }?,
  attribute CanBeNull { boolean }?,
  attribute UpdateCheck { UpdateCheck }?,
  attribute IsDiscriminator { boolean }?,
  attribute Expression { text }?,
  attribute IsVersion { boolean }?,
  attribute AutoSync { AutoSync}?
}

Association = {
  attribute Name { text }?,
  attribute Member { text },
  attribute Storage { text }?,
  attribute ThisKey { text }?,
  attribute OtherKey { text }?,
  attribute IsForeignKey { boolean }?,
  attribute IsUnique { boolean }?,
  attribute DeleteRule { text }?,
  attribute DeleteOnNull { boolean }?
}

Function = {
  element Parameter { Parameter }*,
  {
    element ElementType { Type }* |
    element Return { Return }
  },
  attribute Name { text }?,
  attribute Method { text },
  attribute IsComposable { boolean }?
}

Parameter = {
  attribute Name { text }?,
  attribute Parameter { text },
  attribute DbType { text }?,
  attribute Direction { ParameterDirection }?
}

Return = attribute DbType { text}?

UpdateCheck = "Always" | "Never" | "WhenChanged"
ParameterDirection = "In" | "Out" | "InOut"
AutoSync = "Never" | "OnInsert" | "OnUpdate" | "Always" | "Default"

}
---------------- */
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using DbLinq;
using DbLinq.Schema.Dbml;
using DbLinq.Util;

#if MONO_STRICT
namespace System.Data.Linq.Mapping
#else
namespace DbLinq.Data.Linq.Mapping
#endif
{
    public sealed class XmlMappingSource : System.Data.Linq.Mapping.MappingSource
    {
        DbmlDatabase db;

        XmlMappingSource(XmlReader reader)
        {
            db = new DbmlDatabase(reader);
        }

        public static XmlMappingSource FromReader(XmlReader reader)
        {
            return new XmlMappingSource(reader);
        }

        public static XmlMappingSource FromStream(Stream stream)
        {
            return FromReader(XmlReader.Create(stream));
        }

        public static XmlMappingSource FromUrl(string url)
        {
            return FromReader(XmlReader.Create(url));
        }

        public static XmlMappingSource FromXml(string xml)
        {
            return FromReader(XmlReader.Create(new StringReader(xml)));
        }

        protected override System.Data.Linq.Mapping.MetaModel CreateModel(System.Type dataContextType)
        {
            return new XmlMetaModel(this, db, dataContextType);
        }


        abstract class DbmlItem
        {
            public const string DbmlNamespace = "http://schemas.microsoft.com/linqtosql/mapping/2007";

            public void ReadEmptyContent(XmlReader r, string name)
            {
                if (r.IsEmptyElement)
                    r.ReadStartElement(name, DbmlNamespace);
                else
                {
                    r.ReadStartElement(name, DbmlNamespace);
                    for (r.MoveToContent(); r.NodeType != XmlNodeType.EndElement; r.MoveToContent())
                    {
                        if (r.NamespaceURI != DbmlNamespace)
                            r.Skip();
                        throw UnexpectedItemError(r);
                    }
                    r.ReadEndElement();
                }
            }
            public bool GetBooleanAttribute(XmlReader r, string attributeName)
            {
                return r.GetAttribute(attributeName) == "true";
            }
            public UpdateCheck GetUpdateCheckAttribute(XmlReader r, string attributeName)
            {
                var s = r.GetAttribute(attributeName);
                return s != null ? (UpdateCheck)Enum.Parse(typeof(UpdateCheck), s) : default(UpdateCheck);
            }
            public AutoSync GetAutoSyncAttribute(XmlReader r, string attributeName)
            {
                var s = r.GetAttribute(attributeName);
                return s != null ? (AutoSync)Enum.Parse(typeof(AutoSync), s) : default(AutoSync);
            }
            public T GetEnumAttribute<T>(XmlReader r, string attributeName)
            {
                var s = r.GetAttribute(attributeName);
                return s != null ? (T)Enum.Parse(typeof(T), s) : default(T);
            }
            public XmlException UnexpectedItemError(XmlReader r)
            {
                return new XmlException(String.Format("Unexpected dbml element '{0}'", r.LocalName));
            }
        }
        class DbmlDatabase : DbmlItem
        {
            public string Name;
            public string Provider;
            public List<DbmlTable> Tables = new List<DbmlTable>();
            public List<DbmlFunction> Functions = new List<DbmlFunction>();

            public DbmlDatabase(XmlReader r)
            {
                r.MoveToContent();
                Name = r.GetAttribute("Name");
                Provider = r.GetAttribute("Provider");
                if (r.IsEmptyElement)
                    r.ReadStartElement("Database", DbmlNamespace);
                else
                {
                    r.ReadStartElement("Database", DbmlNamespace);
                    for (r.MoveToContent(); r.NodeType != XmlNodeType.EndElement; r.MoveToContent())
                    {
                        if (r.NamespaceURI != DbmlNamespace)
                            r.Skip();
                        else
                        {
                            switch (r.LocalName)
                            {
                                case "Table":
                                    Tables.Add(new DbmlTable(r));
                                    break;
                                case "Function":
                                    Functions.Add(new DbmlFunction(r));
                                    break;
                                default:
                                    throw UnexpectedItemError(r);
                            }
                        }
                    }
                    r.ReadEndElement();
                }
            }
        }
        class DbmlTable : DbmlItem
        {
            public DbmlType Type;
            public string Name;
            public string Member;

            public DbmlTable(XmlReader r)
            {
                Name = r.GetAttribute("Name");
                Member = r.GetAttribute("Member");
                if (r.IsEmptyElement)
                    r.ReadStartElement("Table", DbmlNamespace);
                else
                {
                    r.ReadStartElement("Table", DbmlNamespace);
                    for (r.MoveToContent(); r.NodeType != XmlNodeType.EndElement; r.MoveToContent())
                    {
                        if (r.NamespaceURI != DbmlNamespace)
                            r.Skip();
                        else
                        {
                            switch (r.LocalName)
                            {
                                case "Type":
                                    Type = new DbmlType(r);
                                    break;
                                default:
                                    throw UnexpectedItemError(r);
                            }
                        }
                    }
                    r.ReadEndElement();
                }
            }
        }
        class DbmlType : DbmlItem
        {
            public List<DbmlColumn> Columns = new List<DbmlColumn>();
            public List<DbmlAssociation> Associations = new List<DbmlAssociation>();
            public List<DbmlType> Types = new List<DbmlType>();
            public string Name;
            public string InheritanceCode;
            public bool IsInheritanceDefault;

            public DbmlType(XmlReader r)
            {
                Name = r.GetAttribute("Name");
                InheritanceCode = r.GetAttribute("InheritanceCode");
                IsInheritanceDefault = GetBooleanAttribute(r, "IsInheritanceDefault");
                if (r.IsEmptyElement)
                    r.ReadStartElement("Type", DbmlNamespace);
                else
                {
                    r.ReadStartElement("Type", DbmlNamespace);
                    for (r.MoveToContent(); r.NodeType != XmlNodeType.EndElement; r.MoveToContent())
                    {
                        if (r.NamespaceURI != DbmlNamespace)
                            r.Skip();
                        else
                        {
                            switch (r.LocalName)
                            {
                                case "Column":
                                    Columns.Add(new DbmlColumn(r));
                                    break;
                                case "Association":
                                    Associations.Add(new DbmlAssociation(r));
                                    break;
                                case "Type":
                                    Types.Add(new DbmlType(r));
                                    break;
                                default:
                                    throw UnexpectedItemError(r);
                            }
                        }
                    }
                    r.ReadEndElement();
                }
            }
        }

        class DbmlMemberBase : DbmlItem
        {
            public string Name;
            public string Member;
            public string Storage;
        }

        class DbmlColumn : DbmlMemberBase
        {
            public string DbType;
            public bool IsPrimaryKey;
            public bool IsDbGenerated;
            public bool CanBeNull;
            public System.Data.Linq.Mapping.UpdateCheck UpdateCheck;
            public bool IsDiscriminator;
            public string Expression;
            public bool IsVersion;
            public System.Data.Linq.Mapping.AutoSync AutoSync;

            public DbmlColumn(XmlReader r)
            {
                Member = r.GetAttribute("Member");
                Name = r.GetAttribute("Name") ?? Member;
                Storage = r.GetAttribute("Storage");
                DbType = r.GetAttribute("DbType");
                IsPrimaryKey = GetBooleanAttribute(r, "IsPrimaryKey");
                IsDbGenerated = GetBooleanAttribute(r, "IsDbGenerated");
                CanBeNull = GetBooleanAttribute(r, "CanBeNull");
                UpdateCheck = GetEnumAttribute<System.Data.Linq.Mapping.UpdateCheck>(r, "UpdateCheck");
                IsDiscriminator = GetBooleanAttribute(r, "IsDiscriminator");
                Expression = r.GetAttribute("Expression");
                IsVersion = GetBooleanAttribute(r, "IsVersion");
                AutoSync = GetEnumAttribute<System.Data.Linq.Mapping.AutoSync>(r, "AutoSync");
                ReadEmptyContent(r, "Column");
            }
        }
        class DbmlAssociation : DbmlMemberBase
        {
            public string ThisKey;
            public string OtherKey;
            public bool IsForeignKey;
            public bool IsUnique;
            public string DeleteRule;
            public bool DeleteOnNull;

            public DbmlAssociation(XmlReader r)
            {
                Name = r.GetAttribute("Name");
                Member = r.GetAttribute("Member");
                Storage = r.GetAttribute("Storage");
                ThisKey = r.GetAttribute("ThisKey");
                OtherKey = r.GetAttribute("OtherKey");
                IsForeignKey = GetBooleanAttribute(r, "IsForeignKey");
                IsUnique = GetBooleanAttribute(r, "IsUnique");
                DeleteRule = r.GetAttribute("DeleteRule");
                DeleteOnNull = GetBooleanAttribute(r, "DeleteOnNull");
                ReadEmptyContent(r, "Association");
            }
        }
        class DbmlFunction : DbmlItem
        {
            public string Name;
            public string Method;
            public bool IsComposable;
            public List<DbmlParameter> Parameters = new List<DbmlParameter>();
            public List<DbmlType> ElementTypes = new List<DbmlType>();
            public DbmlReturn Return;

            public DbmlFunction(XmlReader r)
            {
                Name = r.GetAttribute("Name");
                Method = r.GetAttribute("Method");
                IsComposable = GetBooleanAttribute(r, "IsComposable");
                if (r.IsEmptyElement)
                    r.ReadStartElement("Function", DbmlNamespace);
                else
                {
                    r.ReadStartElement("Function", DbmlNamespace);
                    for (r.MoveToContent(); r.NodeType != XmlNodeType.EndElement; r.MoveToContent())
                    {
                        if (r.NamespaceURI != DbmlNamespace)
                            r.Skip();
                        else
                        {
                            switch (r.LocalName)
                            {
                                case "Parameter":
                                    Parameters.Add(new DbmlParameter(r));
                                    break;
                                case "ElementType":
                                    ElementTypes.Add(new DbmlType(r));
                                    break;
                                case "Return":
                                    Return = new DbmlReturn(r);
                                    break;
                                default:
                                    throw UnexpectedItemError(r);
                            }
                        }
                    }
                    r.ReadEndElement();
                }
            }
        }
        class DbmlParameter : DbmlItem
        {
            public string Name;
            public string Parameter;
            public string DbType;
            public ParameterDirection Direction;

            public DbmlParameter(XmlReader r)
            {
                Name = r.GetAttribute("Name");
                Parameter = r.GetAttribute("Parameter");
                DbType = r.GetAttribute("DbType");
                Direction = GetEnumAttribute<ParameterDirection>(r, "Direction");
                ReadEmptyContent(r, "Parameter");
            }
        }
        class DbmlReturn : DbmlItem
        {
            public string DbType;

            public DbmlReturn(XmlReader r)
            {
                DbType = r.GetAttribute("DbType");
                ReadEmptyContent(r, "Return");
            }
        }

        class XmlMetaModel : System.Data.Linq.Mapping.MetaModel
        {
            System.Data.Linq.Mapping.MappingSource source;
            DbmlDatabase d;
            System.Type context_type;
            System.Data.Linq.Mapping.MetaFunction[] functions;
            System.Data.Linq.Mapping.MetaTable[] tables;
            Dictionary<System.Type, XmlMetaType> types;

            public XmlMetaModel(System.Data.Linq.Mapping.MappingSource source, DbmlDatabase database, System.Type contextType)
            {
                this.source = source;
                this.d = database;
                this.context_type = contextType;
                RegisterTypes();
            }

            void RegisterTypes()
            {
                types = new Dictionary<System.Type, XmlMetaType>();
                foreach (var t in d.Tables)
                    RegisterTypeAndDescendants(t.Type);
            }

            void RegisterTypeAndDescendants(DbmlType dt)
            {

                System.Type t = GetTypeFromName(dt.Name);
                if (t == null)
                    throw new ArgumentException(String.Format("type '{0}' not found", dt.Name));
                if (types.ContainsKey(t))
                    return;
                types.Add(t, new XmlMetaType(this, dt));
                foreach (var cdt in dt.Types)
                    RegisterTypeAndDescendants(cdt);
            }

            public override System.Type ContextType
            {
                get { return context_type; }
            }

            public override string DatabaseName
            {
                get { return d.Name; }
            }

            public override System.Data.Linq.Mapping.MappingSource MappingSource
            {
                get { return source; }
            }

            public override System.Type ProviderType
            {
                get { return GetTypeFromName(d.Provider); }
            }

            public override System.Data.Linq.Mapping.MetaFunction GetFunction(MethodInfo method)
            {
                foreach (var f in GetFunctions())
                    if (f.Method == method)
                        return f;
                throw new ArgumentException(String.Format("Corresponding MetaFunction for method '{0}' was not found", method));
            }

            public override IEnumerable<System.Data.Linq.Mapping.MetaFunction> GetFunctions()
            {
                if (functions == null)
                {
                    var l = new List<System.Data.Linq.Mapping.MetaFunction>();
                    foreach (var f in d.Functions)
                        l.Add(new XmlMetaFunction(this, f));
                    functions = l.ToArray();
                }
                return functions;
            }

            public System.Type GetTypeFromName(string name)
            {
                string ns = context_type.Namespace;
                string full = !name.Contains('.') && !String.IsNullOrEmpty(ns) ? String.Concat(ns, ".", name) : name;
                var t = this.context_type.Assembly.GetType(full) ?? System.Type.GetType(full);
                if (t == null)
                    throw new ArgumentException(String.Format("Type '{0}' was not found", full));
                return t;
            }

            public override System.Data.Linq.Mapping.MetaType GetMetaType(System.Type type)
            {
                if (!types.ContainsKey(type))
                    throw new ArgumentException(String.Format("Type '{0}' is not found in the mapping", type));
                return types[type];
            }

            public override System.Data.Linq.Mapping.MetaTable GetTable(System.Type rowType)
            {
                foreach (var t in GetTables())
                    if (t.RowType.Type == rowType)
                        return t;
                //throw new ArgumentException(String.Format("Corresponding MetaTable for row type '{0}' was not found", rowType));
                return null;
            }

            public override IEnumerable<System.Data.Linq.Mapping.MetaTable> GetTables()
            {
                if (tables == null)
                {
                    var l = new List<System.Data.Linq.Mapping.MetaTable>();
                    foreach (var t in d.Tables)
                        l.Add(new XmlMetaTable(this, t));
                    tables = l.ToArray();
                }
                return tables;
            }
        }

        class XmlMetaParameter : System.Data.Linq.Mapping.MetaParameter
        {
            string dbtype, mapped_name;
            ParameterInfo pi;

            public XmlMetaParameter(DbmlParameter p, ParameterInfo parameterInfo)
                : this(p.DbType, p.Parameter, parameterInfo)
            {
            }

            public XmlMetaParameter(string dbType, string mappedName, ParameterInfo parameterInfo)
            {
                this.dbtype = dbType;
                this.mapped_name = mappedName;
                this.pi = parameterInfo;
            }

            public override string DbType { get { return dbtype; } }
            public override string MappedName { get { return mapped_name; } }
            public override string Name { get { return Parameter.Name; } }
            public override ParameterInfo Parameter { get { return pi; } }
            public override System.Type ParameterType { get { return pi.ParameterType; } }
        }

        class XmlMetaTable : System.Data.Linq.Mapping.MetaTable
        {
            public XmlMetaTable(XmlMetaModel model, DbmlTable table)
            {
                this.model = model;
                this.t = table;
                foreach (var member in model.ContextType.GetMember(t.Member))
                {
                    if (table_member != null)
                        throw new ArgumentException(String.Format("The context type '{0}' contains non-identical member '{1}'", model.ContextType, t.Member));
                    table_member = member;
                }
                if (table_member == null)
                    table_member = GetFieldsAndProperties(model.ContextType).First(pi => pi.GetMemberType().IsGenericType &&
                        pi.GetMemberType().GetGenericTypeDefinition() == typeof(Table<>) &&
                        pi.GetMemberType().GetGenericArguments()[0] == model.GetTypeFromName(t.Type.Name));
                if (table_member == null)
                    throw new ArgumentException(String.Format("The context type '{0}' does not contain member '{1}' which is specified in dbml", model.ContextType, t.Member));
                member_type = table_member.GetMemberType();
                if (member_type.GetGenericTypeDefinition() != typeof(Table<>))
                    throw new ArgumentException(String.Format("The table member type was unexpected: '{0}'", member_type));
                var rt = member_type.GetGenericArguments()[0];
                row_type = model.GetMetaType(rt);
                if (row_type == null)
                    throw new ArgumentException(String.Format("MetaType for '{0}' was not found", rt));
            }
            static IEnumerable<MemberInfo> GetFieldsAndProperties(System.Type type)
            {
                foreach (var f in type.GetFields())
                    yield return f;
                foreach (var p in type.GetProperties())
                    yield return p;
            }

            XmlMetaModel model;
            DbmlTable t;
            MemberInfo table_member;
            System.Type member_type;
            System.Data.Linq.Mapping.MetaType row_type;

            [DbLinqToDo]
            public override MethodInfo DeleteMethod
            {
                get { throw new NotImplementedException(); }
            }
            [DbLinqToDo]
            public override MethodInfo InsertMethod
            {
                get { throw new NotImplementedException(); }
            }
            public override System.Data.Linq.Mapping.MetaModel Model { get { return model; } }
            public override System.Data.Linq.Mapping.MetaType RowType { get { return row_type; } }
            System.Type MemberType { get { return member_type; } }
            public override string TableName { get { return t.Name; } }
            [DbLinqToDo]
            public override MethodInfo UpdateMethod
            {
                get { throw new NotImplementedException(); }
            }

            // not used yet
            MethodInfo GetMethod(TableFunction f)
            {
                if (f == null)
                    return null;
                foreach (var mf in model.GetFunctions())
                    if (mf.Name == f.FunctionId)
                        return mf.Method;
                return null;
            }
        }

        class XmlMetaType : System.Data.Linq.Mapping.MetaType
        {
            XmlMetaModel model;
            DbmlType t;
            ReadOnlyCollection<System.Data.Linq.Mapping.MetaAssociation> associations;
            System.Type runtime_type;
            ReadOnlyCollection<System.Data.Linq.Mapping.MetaDataMember> members, identity_members, persistent_members;

            public XmlMetaType(XmlMetaModel model, DbmlType type)
            {
                this.model = model;
                this.t = type;
                runtime_type = model.GetTypeFromName(t.Name);
                int i = 0;
                var l = new List<System.Data.Linq.Mapping.MetaDataMember>();
                l.AddRange(Array.ConvertAll<DbmlColumn, System.Data.Linq.Mapping.MetaDataMember>(
                    t.Columns.ToArray(), c => new XmlColumnMetaDataMember(model, this, c, i++)));
                members = new ReadOnlyCollection<System.Data.Linq.Mapping.MetaDataMember>(l);
            }

            public override ReadOnlyCollection<System.Data.Linq.Mapping.MetaAssociation> Associations
            {
                get
                {
                    if (associations == null)
                    {
                        var l = new List<System.Data.Linq.Mapping.MetaAssociation>();
                        // FIXME: Ordinal?
                        foreach (var a in t.Associations)
                            l.Add(new XmlMetaAssociation(this, new XmlAssociationMetaDataMember(model, this, a, -1), a));
                        associations = new ReadOnlyCollection<System.Data.Linq.Mapping.MetaAssociation>(l.ToArray());
                    }
                    return associations;
                }
            }
            public override bool CanInstantiate { get { return !runtime_type.IsAbstract; } }
            public override ReadOnlyCollection<System.Data.Linq.Mapping.MetaDataMember> DataMembers { get { return members; } }
            public override System.Data.Linq.Mapping.MetaDataMember DBGeneratedIdentityMember
            {
                get { return members.First(m => m.IsDbGenerated && m.IsPrimaryKey); }
            }
            [DbLinqToDo]
            public override ReadOnlyCollection<System.Data.Linq.Mapping.MetaType> DerivedTypes
            {
                get { throw new NotImplementedException(); }
            }
            public override System.Data.Linq.Mapping.MetaDataMember Discriminator
            {
                get { return members.First(m => m.IsDiscriminator); }
            }
            public override bool HasAnyLoadMethod
            {
                get { return members.Any(m => m.LoadMethod != null); }
            }
            [DbLinqToDo]
            public override bool HasAnyValidateMethod
            {
                get { throw new NotImplementedException(); }
            }
            [DbLinqToDo]
            public override bool HasInheritance
            {
                get { throw new NotImplementedException(); }
            }
            public override bool HasInheritanceCode
            {
                get { return t.InheritanceCode != null; }
            }
            public override bool HasUpdateCheck { get { return members.Any(m => m.UpdateCheck != System.Data.Linq.Mapping.UpdateCheck.Never); } }
            public override ReadOnlyCollection<System.Data.Linq.Mapping.MetaDataMember> IdentityMembers
            {
                get
                {
                    if (identity_members == null)
                    {
                        identity_members = new ReadOnlyCollection<System.Data.Linq.Mapping.MetaDataMember>(
                            members.TakeWhile(m => m.IsPrimaryKey).ToArray());
                    }
                    return identity_members;
                }
            }
            [DbLinqToDo]
            public override System.Data.Linq.Mapping.MetaType InheritanceBase
            {
                get { throw new NotImplementedException(); }
            }
            public override Object InheritanceCode { get { return t.InheritanceCode; } }
            [DbLinqToDo]
            public override System.Data.Linq.Mapping.MetaType InheritanceDefault
            {
                get { throw new NotImplementedException(); }
            }
            [DbLinqToDo]
            public override System.Data.Linq.Mapping.MetaType InheritanceRoot
            {
                get { throw new NotImplementedException(); }
            }
            [DbLinqToDo]
            public override ReadOnlyCollection<System.Data.Linq.Mapping.MetaType> InheritanceTypes
            {
                get { throw new NotImplementedException(); }
            }
            [DbLinqToDo]
            public override bool IsEntity { get { return true; } }
            public override bool IsInheritanceDefault { get { return t.IsInheritanceDefault; } }
            public override System.Data.Linq.Mapping.MetaModel Model { get { return model; } }
            public override string Name { get { return t.Name; } }
            [DbLinqToDo]
            public override MethodInfo OnLoadedMethod
            {
                get { throw new NotImplementedException(); }
            }
            [DbLinqToDo]
            public override MethodInfo OnValidateMethod
            {
                get { throw new NotImplementedException(); }
            }
            public override ReadOnlyCollection<System.Data.Linq.Mapping.MetaDataMember> PersistentDataMembers
            {
                get
                {
                    if (persistent_members == null)
                    {
                        persistent_members = new ReadOnlyCollection<System.Data.Linq.Mapping.MetaDataMember>(
                            members.TakeWhile(m => m.IsPersistent).ToArray());
                    }
                    return persistent_members;
                }
            }
            public override System.Data.Linq.Mapping.MetaTable Table { get { return model.GetTable(runtime_type); } }
            public override System.Type Type { get { return runtime_type; } }
            public override System.Data.Linq.Mapping.MetaDataMember VersionMember { get { return members.First(m => m.IsVersion); } }

            public override System.Data.Linq.Mapping.MetaDataMember GetDataMember(MemberInfo member)
            {
                //return members.First(m => m.Member == member);
                foreach (var m in members) if (m.Member == member) return m;
                throw new ArgumentException(String.Format("No corresponding metadata member for '{0}'", member));
            }

            public override System.Data.Linq.Mapping.MetaType GetInheritanceType(System.Type type)
            {
                return InheritanceTypes.First(t => t.Type == type);
            }

            [DbLinqToDo]
            public override System.Data.Linq.Mapping.MetaType GetTypeForInheritanceCode(object code)
            {
                throw new NotImplementedException();
            }
        }

        class XmlMetaAssociation : System.Data.Linq.Mapping.MetaAssociation
        {
            //XmlMetaType owner;
            DbmlAssociation a;
            ReadOnlyCollection<System.Data.Linq.Mapping.MetaDataMember> these_keys, other_keys;
            System.Data.Linq.Mapping.MetaDataMember member;

            public XmlMetaAssociation(XmlMetaType owner, XmlMetaDataMember member, DbmlAssociation a)
            {
                //this.owner = owner;
                this.member = member;
                this.a = a;
                SetupRelationship();
            }

            /// <summary>
            /// This function sets up the relationship information based on the attribute <see cref="XmlMetaModel"/>.
            /// </summary>
            private void SetupRelationship()
            {
                //Get the association target type
                System.Type targetType = member.Member.GetFirstInnerReturnType();

                var metaModel = ThisMember.DeclaringType.Model as XmlMetaModel;
                if (metaModel == null)
                {
                    throw new InvalidOperationException("Internal Error: MetaModel is not a XmlMetaModel");
                }

                System.Data.Linq.Mapping.MetaTable otherTable = metaModel.GetTable(targetType);

                //Setup "this key"
                these_keys = GetKeys(a.ThisKey ?? String.Empty, ThisMember.DeclaringType);

                //Setup other key
                other_keys = GetKeys(a.OtherKey ?? String.Empty, otherTable.RowType);
            }

            //Seperator used for key lists
            private static readonly char[] STRING_SEPERATOR = new[] { ',' };

            /// <summary>
            /// Returns a list of keys from the given meta type based on the key list string.
            /// </summary>
            /// <param name="keyListString">The key list string.</param>
            /// <param name="parentType">Type of the parent.</param>
            /// <returns></returns>
            private static ReadOnlyCollection<System.Data.Linq.Mapping.MetaDataMember> GetKeys(string keyListString, System.Data.Linq.Mapping.MetaType parentType)
            {
                if (keyListString != null)
                {
                    var thisKeyList = new List<System.Data.Linq.Mapping.MetaDataMember>();

                    string[] keyNames = keyListString.Split(STRING_SEPERATOR, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string rawKeyName in keyNames)
                    {
                        string keyName = rawKeyName.Trim();

                        //TODO: maybe speed the lookup up
                        System.Data.Linq.Mapping.MetaDataMember key = (from dataMember in parentType.PersistentDataMembers
                                              where dataMember.Name == keyName
                                              select dataMember).SingleOrDefault();

                        if (key == null)
                        {
                            string errorMessage = string.Format("Could not find key member '{0}' of key '{1}' on type '{2}'. The key may be wrong or the field or property on '{2}' has changed names.",
                                keyName, keyListString, parentType.Type.Name);

                            throw new InvalidOperationException(errorMessage);
                        }

                        thisKeyList.Add(key);
                    }

                    return new ReadOnlyCollection<System.Data.Linq.Mapping.MetaDataMember>(thisKeyList);
                }
                else //Key is the primary key of this table
                {
                    return parentType.IdentityMembers;
                }
            }

            public override bool DeleteOnNull { get { return a.DeleteOnNull; } }
            public override string DeleteRule { get { return a.DeleteRule; } }
            public override bool IsForeignKey { get { return a.IsForeignKey; } }
            [DbLinqToDo]
            public override bool IsMany
            {
                get { throw new NotImplementedException(); }
            }
            public override bool IsNullable { get { return member.Member.GetMemberType().IsNullable(); } }
            public override bool IsUnique { get { return a.IsUnique; } }
            public override ReadOnlyCollection<System.Data.Linq.Mapping.MetaDataMember> OtherKey { get { return other_keys; } }
            public override bool OtherKeyIsPrimaryKey { get { return OtherKey.All(m => m.IsPrimaryKey); } }
            [DbLinqToDo]
            public override System.Data.Linq.Mapping.MetaDataMember OtherMember
            {
                get { throw new NotImplementedException(); }
            }
            public override System.Data.Linq.Mapping.MetaType OtherType { get { return OtherMember.DeclaringType; } }
            public override ReadOnlyCollection<System.Data.Linq.Mapping.MetaDataMember> ThisKey { get { return these_keys; } }
            public override bool ThisKeyIsPrimaryKey { get { return ThisKey.All(m => m.IsPrimaryKey); } }
            public override System.Data.Linq.Mapping.MetaDataMember ThisMember { get { return member; } }
        }

        abstract class XmlMetaDataMember : System.Data.Linq.Mapping.MetaDataMember
        {
            internal XmlMetaModel model;
            internal XmlMetaType type;
            internal MemberInfo member, storage;
            System.Data.Linq.Mapping.MetaAccessor member_accessor, storage_accessor;
            int ordinal;

            protected XmlMetaDataMember(XmlMetaModel model, XmlMetaType type, string memberName, string storageName, int ordinal)
            {
                this.model = model;
                this.type = type;
                BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                if (type.Type.GetMember(memberName, bf).Length == 0)
                    throw new ArgumentException(String.Format("Specified member '{0}' was not found in type '{1}'", memberName, type.Name));
                if (type.Type.GetMember(storageName, bf).Length == 0)
                    throw new ArgumentException(String.Format("Specified member '{0}' was not found in type '{1}'", storageName, type.Name));
                this.member = type.Type.GetMember(memberName, bf)[0];
                this.storage = type.Type.GetMember(storageName, bf)[0];
                this.ordinal = ordinal;
            }

            public override bool CanBeNull { get { return member.GetMemberType().IsNullable(); } }
            public override System.Data.Linq.Mapping.MetaType DeclaringType { get { return type; } }
            public override bool IsDeferred { get { return false; } }
            public override bool IsPersistent { get { return true; } }
            public override MemberInfo Member { get { return member; } }
            public override System.Data.Linq.Mapping.MetaAccessor MemberAccessor
            {
                get
                {
                    if (member_accessor == null)
                        member_accessor = new XmlMetaAccessor(this, Member);
                    return member_accessor;
                }
            }
            public override int Ordinal { get { return ordinal; } }
            public override System.Data.Linq.Mapping.MetaAccessor StorageAccessor
            {
                get
                {
                    if (storage_accessor == null)
                        storage_accessor = new XmlMetaAccessor(this, StorageMember);
                    return storage_accessor;
                }
            }
            public override MemberInfo StorageMember { get { return storage; } }
            public override System.Type Type { get { return member.GetMemberType(); } }

            public override bool IsDeclaredBy(System.Data.Linq.Mapping.MetaType type)
            {
                return this.type == type;
            }
        }

        class XmlColumnMetaDataMember : XmlMetaDataMember
        {
            DbmlColumn c;

            public XmlColumnMetaDataMember(XmlMetaModel model, XmlMetaType type, DbmlColumn column, int ordinal)
                : base(model, type, column.Member, column.Storage, ordinal)
            {
                this.c = column;
            }

            public override System.Data.Linq.Mapping.MetaAssociation Association { get { return null; } }
            public override System.Data.Linq.Mapping.AutoSync AutoSync { get { return (System.Data.Linq.Mapping.AutoSync)c.AutoSync; } }
            public override string DbType { get { return c.DbType; } }
            [DbLinqToDo]
            public override System.Data.Linq.Mapping.MetaAccessor DeferredSourceAccessor
            {
                get { throw new NotImplementedException(); }
            }
            [DbLinqToDo]
            public override System.Data.Linq.Mapping.MetaAccessor DeferredValueAccessor
            {
                get { throw new NotImplementedException(); }
            }

            public override string Expression { get { return c.Expression; } }
            public override bool IsAssociation { get { return false; } }
            public override bool IsDbGenerated { get { return c.IsDbGenerated; } }
            public override bool IsDiscriminator { get { return c.IsDiscriminator; } }
            public override bool IsPrimaryKey { get { return c.IsPrimaryKey; } }
            public override bool IsVersion { get { return c.IsVersion; } }
            [DbLinqToDo]
            public override MethodInfo LoadMethod
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public override string MappedName { get { return c.Name; } }
            public override string Name { get { return c.Name ?? c.Member; } }
            public override System.Data.Linq.Mapping.UpdateCheck UpdateCheck { get { return c.UpdateCheck; } }
        }

        class XmlAssociationMetaDataMember : XmlMetaDataMember
        {
            DbmlAssociation a;
            XmlMetaAssociation ma;

            public XmlAssociationMetaDataMember(XmlMetaModel model, XmlMetaType type, DbmlAssociation association, int ordinal)
                : base(model, type, association.Member, association.Storage, ordinal)
            {
                this.a = association;
            }

            public override System.Data.Linq.Mapping.MetaAssociation Association
            {
                get
                {
                    if (ma == null)
                        this.ma = new XmlMetaAssociation(type, this, a);
                    return ma;
                }
            }
            public override System.Data.Linq.Mapping.AutoSync AutoSync { get { return System.Data.Linq.Mapping.AutoSync.Never; } }
            public override string DbType { get { return String.Empty; } }
            [DbLinqToDo]
            public override System.Data.Linq.Mapping.MetaAccessor DeferredSourceAccessor
            {
                get { throw new NotImplementedException(); }
            }
            [DbLinqToDo]
            public override System.Data.Linq.Mapping.MetaAccessor DeferredValueAccessor
            {
                get { throw new NotImplementedException(); }
            }

            public override string Expression { get { return String.Empty; } }
            public override bool IsAssociation { get { return true; } }
            public override bool IsDbGenerated { get { return false; } }
            public override bool IsDiscriminator { get { return false; } }
            [DbLinqToDo]
            public override bool IsPrimaryKey
            {
                get { throw new NotImplementedException(); }
            }
            [DbLinqToDo]
            public override bool IsVersion
            {
                get { throw new NotImplementedException(); }
            }
            [DbLinqToDo]
            public override MethodInfo LoadMethod
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
            public override string MappedName { get { return a.Member; } }
            public override string Name { get { return a.Name; } }
            public override System.Data.Linq.Mapping.UpdateCheck UpdateCheck
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        class XmlMetaAccessor : System.Data.Linq.Mapping.MetaAccessor
        {
            XmlMetaDataMember member;
            MemberInfo member_info;

            public XmlMetaAccessor(XmlMetaDataMember member, MemberInfo memberInfo)
            {
                this.member = member;
                this.member_info = memberInfo;
            }

            public override System.Type Type
            {
                get { return member_info is FieldInfo ? ((FieldInfo)member_info).FieldType : ((PropertyInfo)member_info).PropertyType; }
            }

            [DbLinqToDo]
            public override object GetBoxedValue(object instance)
            {
                throw new NotImplementedException();
            }

            [DbLinqToDo]
            public override void SetBoxedValue(ref object instance, object value)
            {
                throw new NotImplementedException();
            }
        }

        class XmlMetaFunction : System.Data.Linq.Mapping.MetaFunction
        {
            XmlMetaModel model;
            DbmlFunction f;
            MethodInfo method;
            ReadOnlyCollection<System.Data.Linq.Mapping.MetaParameter> parameters;
            ReadOnlyCollection<System.Data.Linq.Mapping.MetaType> result_types;
            System.Data.Linq.Mapping.MetaParameter return_param;

            public XmlMetaFunction(XmlMetaModel model, DbmlFunction function)
            {
                this.model = model;
                this.f = function;
                method = model.ContextType.GetMethod(function.Method);
                return_param = new XmlMetaParameter(function.Return.DbType, String.Empty, method.ReturnParameter);
            }

            public override bool HasMultipleResults { get { return f.ElementTypes.Count > 0; } }
            public override bool IsComposable { get { return f.IsComposable; } }
            public override string MappedName { get { return f.Name; } }
            public override MethodInfo Method { get { return method; } }
            public override System.Data.Linq.Mapping.MetaModel Model { get { return model; } }
            public override string Name { get { return f.Name; } }
            public override ReadOnlyCollection<System.Data.Linq.Mapping.MetaParameter> Parameters
            {
                get
                {
                    if (parameters == null)
                    {
                        var l = new List<System.Data.Linq.Mapping.MetaParameter>();
                        int i = 0;
                        ParameterInfo[] mparams = method.GetParameters();
                        foreach (var p in f.Parameters)
                            l.Add(new XmlMetaParameter(p, mparams[i++]));
                        parameters = new ReadOnlyCollection<System.Data.Linq.Mapping.MetaParameter>(l);
                    }
                    return parameters;
                }
            }
            public override ReadOnlyCollection<System.Data.Linq.Mapping.MetaType> ResultRowTypes
            {
                get
                {
                    if (result_types == null)
                    {
                        var l = new List<System.Data.Linq.Mapping.MetaType>();
                        foreach (var p in f.ElementTypes)
                            l.Add(model.GetMetaType(model.GetTypeFromName(p.Name)));
                        result_types = new ReadOnlyCollection<System.Data.Linq.Mapping.MetaType>(l.ToArray());
                    }
                    return result_types;
                }
            }
            public override System.Data.Linq.Mapping.MetaParameter ReturnParameter { get { return return_param; } }
        }
    }
}
