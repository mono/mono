//---------------------------------------------------------------------
// <copyright file="EntityDataSourceSelectingEventArgs.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner objsdev
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.Objects;
using System.ComponentModel;
using System.Data.Metadata.Edm;

namespace System.Web.UI.WebControls
{
    public class EntityDataSourceSelectingEventArgs : System.ComponentModel.CancelEventArgs
    {
        private readonly EntityDataSource _dataSource;
        private readonly DataSourceSelectArguments _selectArguments;

        internal EntityDataSourceSelectingEventArgs(EntityDataSource dataSource, DataSourceSelectArguments selectArgs)
        {
            _dataSource = dataSource;
            _selectArguments = selectArgs;
        }

        public EntityDataSource DataSource
        {
            get { return _dataSource; }
        }

        public DataSourceSelectArguments SelectArguments
        {
            get { return _selectArguments; }
        }

     }
}
