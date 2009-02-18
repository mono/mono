#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
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
using System.Collections.ObjectModel;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
#if MONO_STRICT
using System.Data.Linq.Sugar;
using System.Data.Linq.Sugar.Expressions;
#else
using DbLinq.Data.Linq.Sugar.Expressions;
#endif

#if MONO_STRICT
namespace System.Data.Linq.Sugar.Implementation
#else
namespace DbLinq.Data.Linq.Sugar.Implementation
#endif
{
    internal class DataMapper : IDataMapper
    {
        /// <summary>
        /// Returns a table given a type, or null if the type is not mapped
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        public virtual string GetTableName(Type tableType, DataContext dataContext)
        {
            var tableDescription = dataContext.Mapping.GetTable(tableType);
            if (tableDescription != null)
                return tableDescription.TableName;
            return null;
        }

        public virtual string GetColumnName(TableExpression tableExpression, MemberInfo memberInfo, DataContext dataContext)
        {
            return GetColumnName(tableExpression.Type, memberInfo, dataContext);
        }

        public virtual string GetColumnName(Type tableType, MemberInfo memberInfo, DataContext dataContext)
        {
            var tableDescription = dataContext.Mapping.GetTable(tableType);
            var columnDescription = tableDescription.RowType.GetDataMember(memberInfo);
            if (columnDescription != null)
                return columnDescription.MappedName;
            return null;
        }

        public virtual IList<MemberInfo> GetPrimaryKeys(TableExpression tableExpression, DataContext dataContext)
        {
            var tableDescription = dataContext.Mapping.GetTable(tableExpression.Type);
            if (tableDescription != null)
                return GetPrimaryKeys(tableDescription);
            return null;
        }

        public virtual IList<MemberInfo> GetPrimaryKeys(MetaTable tableDescription)
        {
            return (from column in tableDescription.RowType.IdentityMembers select column.Member).ToList();
        }

        /// <summary>
        /// Lists table mapped columns
        /// </summary>
        /// <param name="tableDescription"></param>
        /// <returns></returns>
        public IList<MemberInfo> GetColumns(MetaTable tableDescription)
        {
            return (from column in tableDescription.RowType.PersistentDataMembers select column.Member).ToList();
        }

        /// <summary>
        /// Returns association definition, if any
        /// </summary>
        /// <param name="thisTableExpression">The table referenced by the assocation (the type holding the member)</param>
        /// <param name="memberInfo">The memberInfo related to association</param>
        /// <param name="otherType"></param>
        /// <param name="otherKey">The keys in the associated table</param>
        /// <param name="joinType"></param>
        /// <param name="joinID"></param>
        /// <param name="dataContext"></param>
        /// <returns>ThisKey</returns>
        public virtual IList<MemberInfo> GetAssociation(TableExpression thisTableExpression, MemberInfo memberInfo, Type otherType, out IList<MemberInfo> otherKey, out TableJoinType joinType, out string joinID, DataContext dataContext)
        {
            var thisTableDescription = dataContext.Mapping.GetTable(thisTableExpression.Type);
            var thisAssociation =
                (from association in thisTableDescription.RowType.Associations
                 where association.ThisMember.Member == memberInfo
                 select association).SingleOrDefault();
            if (thisAssociation != null)
            {
                // by default, join is inner
                joinType = TableJoinType.Inner;
                joinID = thisAssociation.ThisMember.MappedName;
                if (string.IsNullOrEmpty(joinID))
                    throw Error.BadArgument("S0108: Association name is required to ensure join uniqueness");

                var otherTableDescription = dataContext.Mapping.GetTable(otherType);
                bool thisKeyHasNullables, otherKeyHasNullables;
                var thisKey = GetAssociationKeys(thisTableDescription, thisAssociation.ThisKey, dataContext,
                                                 out thisKeyHasNullables);
                otherKey = GetAssociationKeys(otherTableDescription, thisAssociation.OtherKey, dataContext,
                                              out otherKeyHasNullables);

                // we just test here the left join (since associations are symmetric,
                //        we can only find left joins here, and the otherKeyHasNullables is
                //        always equal to thisKeyHasNullables)
                if (thisKeyHasNullables)
                    joinType |= TableJoinType.LeftOuter;

                return thisKey;
            }
            otherKey = null;
            joinType = TableJoinType.Default;
            joinID = null;
            return null;
        }

        /// <summary>
        /// Enumerates Keys for a given table.
        /// Keys can be provided as input. If none provided, PKs are taken from table
        /// </summary>
        /// <param name="description"></param>
        /// <param name="keys">Keys to be used, leave empty to use PKs instead</param>
        /// <param name="dataContext"></param>
        /// <param name="hasNullableKeys">returned as true if some keys can be null (we then have an outer join)</param>
        /// <returns></returns>
        protected virtual IList<MemberInfo> GetAssociationKeys(MetaTable description, ReadOnlyCollection<MetaDataMember> keys,
                                                               DataContext dataContext, out bool hasNullableKeys)
        {
            var sourceKeys = keys;
            if (sourceKeys.Count == 0)
                sourceKeys = description.RowType.IdentityMembers;

            hasNullableKeys = false;
            var members = new List<MemberInfo>();
            foreach (var sourceKey in sourceKeys)
            {
                members.Add(sourceKey.Member);
                if (sourceKey.CanBeNull)
                    hasNullableKeys = true;
            }
            return members;
        }

        public IList<MemberInfo> GetEntitySetAssociations(Type type)
        {
            return type.GetProperties()
                .Where(p => p.PropertyType.IsGenericType 
                    && (p.PropertyType.GetGenericTypeDefinition() == typeof(System.Data.Linq.EntitySet<>) 
#if !MONO_STRICT
                    || p.PropertyType.GetGenericTypeDefinition() == typeof(DbLinq.Data.Linq.EntitySet<>)
#endif
                    )
                    && p.IsDefined(typeof(AssociationAttribute), true))
                .Cast<MemberInfo>().ToList();
        }

        public IList<MemberInfo> GetEntityRefAssociations(Type type)
        {
            return (from p in type.GetProperties()
                    let associationAttribute = p.GetCustomAttributes(typeof(AssociationAttribute), true).FirstOrDefault() as AssociationAttribute
                    let field = type.GetField(associationAttribute != null ? (associationAttribute.Storage ?? string.Empty) : string.Empty, BindingFlags.NonPublic | BindingFlags.Instance)
                    where associationAttribute != null &&
                             field != null &&
                            field.FieldType.IsGenericType &&
                            field.FieldType.GetGenericTypeDefinition() == typeof(System.Data.Linq.EntityRef<>)
                    select p)
                .Cast<MemberInfo>().ToList();
        }
    }
}
