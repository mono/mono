//---------------------------------------------------------------------
// <copyright file="ReturnValue.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;

namespace System.Data.EntityModel.SchemaObjectModel
{
    /// <summary>
    /// Summary description for ReturnValue.
    /// </summary>
    internal sealed class ReturnValue<T>
    {
        #region Instance Fields
        private bool _succeeded = false;
        private T _value = default(T);
        #endregion
        /// <summary>
        /// 
        /// </summary>
        internal  ReturnValue()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        internal  bool Succeeded
        {
            get
            {
                return _succeeded;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal  T Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                _succeeded = true;
            }
        }
    }
}
