//---------------------------------------------------------------------
// <copyright file="MetadataPropertyValue.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Class representing a metadata property on an item. Supports
    /// redirection from MetadataProperty instance to item property value.
    /// </summary>
    internal sealed class MetadataPropertyValue
    {
        internal MetadataPropertyValue(PropertyInfo propertyInfo, MetadataItem item)
        {
            Debug.Assert(null != propertyInfo);
            Debug.Assert(null != item);
            _propertyInfo = propertyInfo;
            _item = item;
        }

        private PropertyInfo _propertyInfo;
        private MetadataItem _item;

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal object GetValue()
        {
            return _propertyInfo.GetValue(_item, new object[] { });
        }
    }
}
