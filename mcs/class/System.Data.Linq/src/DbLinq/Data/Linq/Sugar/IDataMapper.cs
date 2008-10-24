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
using System.Data.Linq.Mapping;
using System.Reflection;
#if MONO_STRICT
using System.Data.Linq.Sugar.Expressions;

#else
using DbLinq.Data.Linq.Sugar.Expressions;
#endif

#if MONO_STRICT
namespace System.Data.Linq.Sugar
#else
namespace DbLinq.Data.Linq.Sugar
#endif
{
    internal interface IDataMapper
    {
        /// <summary>
        /// Returns a table given a type, or null if the type is not mapped
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        string GetTableName(Type tableType, DataContext dataContext);

        /// <summary>
        /// Returns a column name, provided its table and memberInfo
        /// </summary>
        /// <param name="tableExpression"></param>
        /// <param name="memberInfo"></param>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        string GetColumnName(TableExpression tableExpression, MemberInfo memberInfo, DataContext dataContext);

        /// <summary>
        /// Returns a column name, provided its table type and memberInfo
        /// </summary>
        /// <param name="tableType"></param>
        /// <param name="memberInfo"></param>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        string GetColumnName(Type tableType, MemberInfo memberInfo, DataContext dataContext);

        /// <summary>
        /// Enumerates PKs
        /// </summary>
        /// <param name="tableExpression"></param>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        IList<MemberInfo> GetPrimaryKeys(TableExpression tableExpression, DataContext dataContext);

        /// <summary>
        /// Enumerates PKs
        /// </summary>
        /// <param name="tableDescription"></param>
        /// <returns></returns>
        IList<MemberInfo> GetPrimaryKeys(MetaTable tableDescription);

        /// <summary>
        /// Lists table mapped columns
        /// </summary>
        /// <param name="tableDescription"></param>
        /// <returns></returns>
        IList<MemberInfo> GetColumns(MetaTable tableDescription);

        /// <summary>
        /// Returns child associations (EntitySets)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IList<MemberInfo> GetEntitySetAssociations(Type type);

        /// <summary>
        /// Returns parent associations (EntityRef)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IList<MemberInfo> GetEntityRefAssociations(Type type);

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
        IList<MemberInfo> GetAssociation(TableExpression thisTableExpression, MemberInfo memberInfo, Type otherType, out IList<MemberInfo> otherKey, out TableJoinType joinType, out string joinID, DataContext dataContext);
    }
}
