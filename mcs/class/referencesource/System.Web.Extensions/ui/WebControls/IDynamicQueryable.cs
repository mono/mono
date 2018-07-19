//------------------------------------------------------------------------------
// <copyright file="IDynamicQueryable.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System.Linq;

    internal interface IDynamicQueryable {

        IQueryable Where(IQueryable source, string predicate, params object[] values);

        IQueryable Select(IQueryable source, string selector, params object[] values);

        IQueryable OrderBy(IQueryable source, string ordering, params object[] values);

        IQueryable Take(IQueryable source, int count);

        IQueryable Skip(IQueryable source, int count);

        IQueryable GroupBy(IQueryable source, string keySelector, string elementSelector, params object[] values);

        int Count(IQueryable source);

    }

}
