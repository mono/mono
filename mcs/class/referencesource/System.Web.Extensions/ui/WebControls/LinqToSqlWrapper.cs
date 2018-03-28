//------------------------------------------------------------------------------
// <copyright file="LinqToSqlWrapper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System.Reflection;
    using System.Data.Linq;

    internal class LinqToSqlWrapper : ILinqToSql {

        public void Add(ITable table, object row) {
            table.InsertOnSubmit(row);
        }

        public void Attach(ITable table, object row) {
            table.Attach(row);
        }

        public object GetOriginalEntityState(ITable table, object row) {
            return table.GetOriginalEntityState(row);
        }

        public void Refresh(DataContext dataContext, RefreshMode mode, object entity) {
            dataContext.Refresh(mode, entity);
        }

        public void Remove(ITable table, object row) {
            table.DeleteOnSubmit(row);
        }

        public void SubmitChanges(DataContext dataContext) {
            dataContext.SubmitChanges();
        }

    }

}
