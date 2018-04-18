//---------------------------------------------------------------------
// <copyright file="ObjectContext.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Data.Objects
{
    /// <summary>
    /// EventArgs for the ObjectMaterialized event.
    /// </summary>
    public class ObjectMaterializedEventArgs : EventArgs
    {
        /// <summary>
        /// The object that was materialized.
        /// </summary>
        private readonly object _entity;

        /// <summary>
        /// Constructs new arguments for the ObjectMaterialized event.
        /// </summary>
        /// <param name="entity">The object that has been materialized.</param>
        internal ObjectMaterializedEventArgs(object entity)
        {
            _entity = entity;
        }

        /// <summary>
        /// The object that was materialized.
        /// </summary>
        public object Entity
        {
            get { return _entity; }
        }
    }

    /// <summary>
    /// Delegate for the ObjectMaterialized event.
    /// </summary>
    /// <param name="sender">The ObjectContext responsable for materializing the object.</param>
    /// <param name="e">EventArgs containing a reference to the materialized object.</param>
    public delegate void ObjectMaterializedEventHandler(object sender, ObjectMaterializedEventArgs e);
}
