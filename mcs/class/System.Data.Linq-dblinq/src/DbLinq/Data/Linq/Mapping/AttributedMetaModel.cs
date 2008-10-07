#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry
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

using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

#if MONO_STRICT
using System.Data.Linq;
#else
using DbLinq.Data.Linq;
using DbLinq.Data.Linq.Mapping;
#endif

using DbLinq.Util;

#if MONO_STRICT
namespace System.Data.Linq.Mapping
#else
namespace DbLinq.Data.Linq.Mapping
#endif
{
    /// <summary>
    /// This class is a stateless attribute meta model (it does not depend on any provider)
    /// So the MappingSource can use singletons
    /// </summary>
    [DebuggerDisplay("MetaModel for {DatabaseName}")]
    internal class AttributedMetaModel : MetaModel
    {
        public AttributedMetaModel(Type dataContextType, MappingSource mappingSource)
        {
            contextType = dataContextType;
            this.mappingSource = mappingSource;
            Load();
        }

        protected virtual void Load()
        {
            // global attributes
            var database = GetDatabaseAttribute();
            databaseName = database != null ? database.Name : null;

            // stored procedures
            metaFunctions = new Dictionary<MethodInfo, MetaFunction>();
            var functionAttributes = GetFunctionsAttributes();
            foreach (var functionPair in functionAttributes)
            {
                metaFunctions[functionPair.Key] = new AttributedMetaFunction(functionPair.Key, functionPair.Value);
            }

            // tables
            tables = new Dictionary<Type, MetaTable>();
            var tableAttributes = GetTablesAttributes();
            foreach (var tablePair in tableAttributes)
            {
                var type = new AttributedMetaType(tablePair.Key);
                var table = new AttributedMetaTable(tablePair.Value, type);
                tables[tablePair.Key] = table;
                type.SetMetaTable(table);
            }

            // reverse associations
            foreach (var table in GetTables())
            {
                foreach (var association in table.RowType.Associations)
                {
                    // we cast to call the SetOtherKey method
                    var attributedAssociation = association as AttributedMetaAssociation;
                    if (attributedAssociation != null)
                    {
                        var memberInfo = attributedAssociation.ThisMember.Member;
                        var associationAttribute = memberInfo.GetAttribute<AssociationAttribute>();
                        var memberType = memberInfo.GetMemberType();
                        Type otherTableType;
                        if (memberType.IsGenericType)
                            otherTableType = memberType.GetGenericArguments()[0];
                        else
                            otherTableType = memberType;
                        var otherTable = GetTable(otherTableType);
                        // then we lookup by the attribute if we have a match
                        MetaDataMember otherAssociationMember = null;
                        foreach (var member in otherTableType.GetMembers())
                        {
                            var otherAssociationAttribute = member.GetAttribute<AssociationAttribute>();
                            if (otherAssociationAttribute != null && otherAssociationAttribute.Name == associationAttribute.Name)
                            {
                                otherAssociationMember =
                                    (from a in otherTable.RowType.Associations
                                     where a.ThisMember.Member == member
                                     select a.ThisMember).SingleOrDefault();
                                if (otherAssociationMember == attributedAssociation.ThisMember)
                                {
                                    otherAssociationMember = null;
                                    continue;
                                }
                                break;
                            }
                        }
                        attributedAssociation.SetOtherKey(associationAttribute.OtherKey, table, otherTable, otherAssociationMember);
                    }
                }
            }
        }

        protected virtual DatabaseAttribute GetDatabaseAttribute()
        {
            return contextType.GetAttribute<DatabaseAttribute>();
        }

        protected virtual IDictionary<MethodInfo, FunctionAttribute> GetFunctionsAttributes()
        {
            var functionAttributes = new Dictionary<MethodInfo, FunctionAttribute>();
            foreach (var methodInfo in contextType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                var function = methodInfo.GetAttribute<FunctionAttribute>();
                if (function != null)
                    functionAttributes[methodInfo] = function;
            }
            return functionAttributes;
        }

        protected virtual IDictionary<Type, TableAttribute> GetTablesAttributes()
        {
            var tableAttributes = new Dictionary<Type, TableAttribute>();
            // to find the tables, we list all properties/fields contained in the DataContext inheritor
            // if the return type has a TableAttribute, then it is ours (muhahahah!)
            foreach (var memberInfo in contextType.GetMembers(BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                var memberType = memberInfo.GetMemberType();
                if (memberType == null)
                    continue;
                var classType = GetClassType(memberType);
                if (classType == null)
                    continue;
                // if somebody someday can explain why the GetCustomAttributes(true) does not return inherited attributes, I'd be very glad to hear him
                TableAttribute tableAttribute = null;
                for (var testType = classType; testType != null; testType = testType.BaseType)
                {
                    tableAttribute = testType.GetAttribute<TableAttribute>();
                    if (tableAttribute != null)
                        break;
                }
                // finally, we have something here, keep it
                if (tableAttribute != null)
                    tableAttributes[classType] = tableAttribute;
            }
            return tableAttributes;
        }

        protected virtual Type GetClassType(Type t)
        {
            // for property get, it is a IQueryable<T>, so we want T
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Table<>))
                return t.GetGenericArguments()[0];
            // for non property get, we may also have a direct inner type, so return it directly
            return t;
        }

        private Type contextType;
        public override Type ContextType
        {
            get { return contextType; }
        }

        private string databaseName;
        public override string DatabaseName
        {
            get { return databaseName; }
        }

        private IDictionary<MethodInfo, MetaFunction> metaFunctions;
        public override MetaFunction GetFunction(MethodInfo method)
        {
            MetaFunction metaFunction;
            metaFunctions.TryGetValue(method, out metaFunction);
            return metaFunction;
        }

        public override IEnumerable<MetaFunction> GetFunctions()
        {
            return metaFunctions.Values;
        }

        public override MetaType GetMetaType(Type type)
        {
            var metaTable = GetTable(type);
            if (metaTable == null)
                return null;
            return metaTable.RowType;
        }

        private IDictionary<Type, MetaTable> tables;
        public override MetaTable GetTable(Type rowType)
        {
            MetaTable metaTable;
            tables.TryGetValue(rowType, out metaTable);
            return metaTable;
        }

        public override IEnumerable<MetaTable> GetTables()
        {
            return tables.Values;
        }

        //private Type providerType;
        public override Type ProviderType
        {
            get { throw new NotImplementedException(); }
        }

        // just because of this, the whole model can not be cached efficiently, since we can not guarantee
        // that another mapping source instance will not use the same model
        private MappingSource mappingSource;
        public override MappingSource MappingSource
        {
            get { return mappingSource; }
        }

    }
}