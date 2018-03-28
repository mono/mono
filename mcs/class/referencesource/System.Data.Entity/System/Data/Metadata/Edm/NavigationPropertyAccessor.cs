//---------------------------------------------------------------------
// <copyright file="RelationshipNavigation.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Text;
using System.Threading;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Cached dynamic method to get the property value from a CLR instance
    /// </summary> 
    internal class NavigationPropertyAccessor
    {
        #region Constructors

        public NavigationPropertyAccessor(string propertyName)
        {
            _propertyName = propertyName;
        }

        #endregion

        #region Fields

        private Func<object, object> _memberGetter;
        private Action<object, object> _memberSetter;
        private Action<object, object> _collectionAdd;
        private Func<object, object, bool> _collectionRemove;
        private Func<object> _collectionCreate;
        private readonly string _propertyName;

        #endregion

        #region Properties

        public bool HasProperty
        {
            get { return (_propertyName != null); }
        }

        public string PropertyName
        {
            get { return _propertyName; }
        }

        /// <summary>cached dynamic method to get the property value from a CLR instance</summary> 
        public Func<object, object> ValueGetter
        {
            get { return _memberGetter; }
            set
            {
                System.Diagnostics.Debug.Assert(null != value, "clearing ValueGetter");
                // It doesn't matter which delegate wins, but only one should be jitted
                Interlocked.CompareExchange(ref _memberGetter, value, null);
            }
        }

        /// <summary>cached dynamic method to set the property value from a CLR instance</summary> 
        public Action<object, object> ValueSetter
        {
            get { return _memberSetter; }
            set
            {
                System.Diagnostics.Debug.Assert(null != value, "clearing ValueSetter");
                // It doesn't matter which delegate wins, but only one should be jitted
                Interlocked.CompareExchange(ref _memberSetter, value, null);
            }
        }

        public Action<object, object> CollectionAdd
        {
            get { return _collectionAdd; }
            set
            {
                System.Diagnostics.Debug.Assert(null != value, "clearing CollectionAdd");
                // It doesn't matter which delegate wins, but only one should be jitted
                Interlocked.CompareExchange(ref _collectionAdd, value, null);
            }
        }

        public Func<object, object, bool> CollectionRemove
        {
            get { return _collectionRemove; }
            set
            {
                System.Diagnostics.Debug.Assert(null != value, "clearing CollectionRemove");
                // It doesn't matter which delegate wins, but only one should be jitted
                Interlocked.CompareExchange(ref _collectionRemove, value, null);
            }
        }

        public Func<object> CollectionCreate
        {
            get { return _collectionCreate; }
            set
            {
                System.Diagnostics.Debug.Assert(null != value, "clearing CollectionCreate");
                // It doesn't matter which delegate wins, but only one should be jitted
                Interlocked.CompareExchange(ref _collectionCreate, value, null);
            }
        }

        #endregion

        #region Static Properties

        public static NavigationPropertyAccessor NoNavigationProperty
        {
            get { return new NavigationPropertyAccessor(null); }
        }

        #endregion
    }
}
