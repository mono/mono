//------------------------------------------------------------------------------
// <copyright file="DynamicQueryableWrapper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System.Web.Query.Dynamic;   
    using System.Linq;     

    internal class DynamicQueryableWrapper : IDynamicQueryable {

        public IQueryable Where(IQueryable source, string predicate, params object[] values) {
            return DynamicQueryable.Where(source, predicate, values);
        }

        public IQueryable Select(IQueryable source, string selector, params object[] values) {
            return DynamicQueryable.Select(source, selector, values);
        }

        public IQueryable OrderBy(IQueryable source, string ordering, params object[] values) {
            return DynamicQueryable.OrderBy(source, ordering, values);
        }

        public IQueryable Take(IQueryable source, int count) {
            return DynamicQueryable.Take(source, count);
        }

        public IQueryable Skip(IQueryable source, int count) {
            return DynamicQueryable.Skip(source, count);
        }

        public IQueryable GroupBy(IQueryable source, string keySelector, string elementSelector, params object[] values) {
            return DynamicQueryable.GroupBy(source, keySelector, elementSelector, values );
        }

        public int Count(IQueryable source) {
            return DynamicQueryable.Count(source);
        }

    }

}
