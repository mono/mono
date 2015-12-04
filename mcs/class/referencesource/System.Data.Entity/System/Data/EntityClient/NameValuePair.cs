//------------------------------------------------------------------------------
// <copyright file="NameValuePair.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

// @owner  [....]
// @backupOwner  [....]
//------------------------------------------------------------------------------

namespace System.Data.EntityClient
{
    /// <summary>
    /// Copied from System.Data.dll
    /// </summary>
    sealed internal class NameValuePair {
        readonly private string _name;
        readonly private string _value;
        readonly private int _length;
        private NameValuePair _next;

        internal NameValuePair(string name, string value, int length) {
            System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(name), "empty keyname");
            _name = name;
            _value = value;
            _length = length;
        }

        internal NameValuePair Next {
            get {
                return _next;
            }
            set {
                if ((null != _next) || (null == value)) {
                    throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.NameValuePairNext);
                }
                _next = value;
            }
        } 
    }
}
