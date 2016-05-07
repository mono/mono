//------------------------------------------------------------------------------
// <copyright file="ILinqToSql.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System.Data.Linq;

    internal interface ILinqToSql {

        void Add(ITable table, object row);

        void Attach(ITable table, object row);

        object GetOriginalEntityState(ITable table, object row);

        void Refresh(DataContext dataContext, RefreshMode mode, object entity);

        void Remove(ITable table, object row);

        void SubmitChanges(DataContext dataContext);

    }

}
