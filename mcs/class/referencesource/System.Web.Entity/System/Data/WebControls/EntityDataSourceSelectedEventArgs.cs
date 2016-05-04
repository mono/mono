//---------------------------------------------------------------------
// <copyright file="EntityDataSourceSelectedEventArgs.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner objsdev
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;
using System.Collections;

namespace System.Web.UI.WebControls
{
    public class EntityDataSourceSelectedEventArgs : EventArgs
    {
        private readonly ObjectContext _context;
        private readonly Exception _exception = null;
        private bool _exceptionHandled = false;
        private readonly IEnumerable _results = null;
        private readonly int _totalRowCount = 0;
        private readonly DataSourceSelectArguments _selectArguments;

        internal EntityDataSourceSelectedEventArgs(ObjectContext context, 
                                                   IEnumerable results, 
                                                   int totalRowCount, 
                                                   DataSourceSelectArguments selectArgs)
        {
            _context = context;
            _results = results;
            _totalRowCount = totalRowCount;
            _selectArguments = selectArgs;
        }

        internal EntityDataSourceSelectedEventArgs(Exception exception)
        {
            _exception = exception;
        }

        public Exception Exception
        {
            get { return _exception; }
        }

        public bool ExceptionHandled
        {
            get { return _exceptionHandled; }
            set { _exceptionHandled = value; }
        }

        public IEnumerable Results
        {
            get { return _results; }
        }

        public ObjectContext Context
        {
            get { return _context; }
        }

        public int TotalRowCount
        {
            get { return _totalRowCount; }
        }

        public DataSourceSelectArguments SelectArguments
        {
            get { return _selectArguments; }
        }
    }
}
