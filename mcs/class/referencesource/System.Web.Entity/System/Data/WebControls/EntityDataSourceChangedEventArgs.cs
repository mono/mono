//---------------------------------------------------------------------
// <copyright file="EntityDataSourceChangedEventArgs.cs" company="Microsoft">
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
using System.Data.Objects;

namespace System.Web.UI.WebControls
{
    public class EntityDataSourceChangedEventArgs : EventArgs
    {
        private readonly ObjectContext _context;
        private readonly Exception _exception = null;
        private bool _exceptionHandled = false;
        private readonly object _entity = null;

        internal EntityDataSourceChangedEventArgs(ObjectContext context, object entity)
        {
            _context = context;
            _entity = entity;
        }

        internal EntityDataSourceChangedEventArgs(Exception exception) 
        {
            _exception = exception;
        }

        public Exception Exception 
        {
            get { return _exception; }
        }

        public bool ExceptionHandled 
        {
            get {  return _exceptionHandled; }
            set { _exceptionHandled = value; }
        }

        public object Entity 
        {
            get {  return _entity; }
        }

        public ObjectContext Context
        {
            get { return _context; }
        }
    }
}
