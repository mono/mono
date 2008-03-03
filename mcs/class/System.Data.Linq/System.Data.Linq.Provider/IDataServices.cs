// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//
// Authors:
//        Antonello Provenzano  <antonello@deveel.com>
//

using System.Linq;
using System.Linq.Expressions;


namespace System.Data.Linq.Provider
{
    public interface IDataServices
    {/*
        #region Properties
        DataContext Context { get; }

        MetaModel Model { get; }
        #endregion

        #region Methods
        IQueryable CreateQuery(Type type, Expression expression);

        object GetCachedObject(Expression query);

        object GetCachedObject(MetaType type, object[] keyValues);

        IQueryable GetDataMemberQuery(MetaDataMember member, Expression[] keyValues);

        IQueryable GetDataMemberQuery(MetaDataMember member, object[] keyValues);

        IDeferredSourceFactory GetDeferredSourceFactory(MetaDataMember member);

        object[] GetForeignKeyValues(MetaAssociation association, object instance);

        object[] GetKeyValues(MetaType type, LambdaExpression predicate);

        object[] GetKeyValues(MetaType type, object instance);

        IQueryable GetObjectQuery(MetaType type, Expression[] keyValues);

        IQueryable GetObjectQuery(MetaType type, object[] keyValues);

        object InsertLookupCachedObject(MetaType type, object instance);

        bool IsCachedObject(MetaType type, object instance);

        void OnEntityMaterialized(MetaType type, object instance);
        #endregion
        */
    }
}
