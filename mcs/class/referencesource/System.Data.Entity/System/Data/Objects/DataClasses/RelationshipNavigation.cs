//---------------------------------------------------------------------
// <copyright file="RelationshipNavigation.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Text;
using System.Data.Metadata.Edm;

namespace System.Data.Objects.DataClasses
{
    /// <summary>
    /// This class describes a relationship navigation from the
    /// navigation property on one entity to another entity.  It is
    /// used throughout the collections and refs system to describe a
    /// relationship and to connect from the navigation property on
    /// one end of a relationship to the navigation property on the
    /// other end.
    /// </summary>
    [Serializable]
    internal class RelationshipNavigation
    {
        // ------------
        // Constructors
        // ------------

        /// <summary>
        /// Creates a navigation object with the given relationship
        /// name, role name for the source and role name for the
        /// destination.
        /// </summary>
        /// <param name="relationshipName">Canonical-space name of the relationship.</param>
        /// <param name="from">Name of the role which is the source of the navigation.</param>
        /// <param name="to">Name of the role which is the destination of the navigation.</param>
        /// <param name="fromAccessor">The navigation property which is the source of the navigation.</param>
        /// <param name="toAccessor">The navigation property which is the destination of the navigation.</param>
        internal RelationshipNavigation(string relationshipName, string from, string to, NavigationPropertyAccessor fromAccessor, NavigationPropertyAccessor toAccessor)
        {
            EntityUtil.CheckStringArgument(relationshipName, "relationshipName");
            EntityUtil.CheckStringArgument(from, "from");
            EntityUtil.CheckStringArgument(to, "to");
            
            _relationshipName = relationshipName;
            _from = from;
            _to = to;

            _fromAccessor = fromAccessor;
            _toAccessor = toAccessor;
        }
    
        // ------
        // Fields
        // ------

        // The following fields are serialized.  Adding or removing a serialized field is considered
        // a breaking change.  This includes changing the field type or field name of existing
        // serialized fields. If you need to make this kind of change, it may be possible, but it
        // will require some custom serialization/deserialization code.
        private readonly string _relationshipName;
        private readonly string _from;
        private readonly string _to;

        [NonSerialized]
        private RelationshipNavigation _reverse;

        [NonSerialized]
        private NavigationPropertyAccessor _fromAccessor;

        [NonSerialized]
        private NavigationPropertyAccessor _toAccessor;

        // ----------
        // Properties
        // ----------

        /// <summary>
        /// Canonical-space relationship name.
        /// </summary>        
        internal string RelationshipName
        {
            get
            {
                return _relationshipName;
            }
        }

        /// <summary>
        /// Role name for the source of this navigation.
        /// </summary>        
        internal string From
        {
            get
            {
                return _from;
            }
        }

        /// <summary>
        /// Role name for the destination of this navigation.
        /// </summary>        
        internal string To
        {
            get
            {
                return _to;
            }
        }

        /// <summary>
        /// Navigation property name for the destination of this navigation.
        /// NOTE: There is not a FromPropertyAccessor property on RelationshipNavigation because it is not currently accessed anywhere
        ///       It is only used to calculate the "reverse" RelationshipNavigation.
        /// </summary>        
        internal NavigationPropertyAccessor ToPropertyAccessor
        {
            get { return _toAccessor; }
        }

        internal bool IsInitialized
        {
            get { return _toAccessor != null && _fromAccessor != null; }
        }

        internal void InitializeAccessors(NavigationPropertyAccessor fromAccessor, NavigationPropertyAccessor toAccessor)
        {
            _fromAccessor = fromAccessor;
            _toAccessor = toAccessor;
        }
        
        /// <summary>
        /// The "reverse" version of this navigation.
        /// </summary>        
        internal RelationshipNavigation Reverse
        {
            get
            {
                if (_reverse == null || !_reverse.IsInitialized)
                {
                    // the reverse relationship is exactly like this
                    // one but from & to are switched
                    _reverse = new RelationshipNavigation(_relationshipName, _to, _from, _toAccessor, _fromAccessor);
                }
                
                return _reverse;
            }
        }

        /// <summary>
        /// Compares this instance to a given Navigation by their values.
        /// </summary>        
        public override bool Equals(object obj)
        {
                RelationshipNavigation compareTo = obj as RelationshipNavigation;
                return ((this == compareTo)
                        || ((null != this) && (null != compareTo)
                            && (this.RelationshipName == compareTo.RelationshipName)
                            && (this.From == compareTo.From)
                            && (this.To == compareTo.To)));
        }
        
        /// <summary>
        /// Returns a value-based hash code.
        /// </summary>
        /// <returns>the hash value of this Navigation</returns>
        public override int GetHashCode()
        {
                return this.RelationshipName.GetHashCode();
        }
        
        // -------
        // Methods
        // -------

        /// <summary>
        /// ToString is provided to simplify debugging, etc.
        /// </summary>
        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture,
                                 "RelationshipNavigation: ({0},{1},{2})",
                                 _relationshipName,
                                 _from,
                                 _to);
        }        
    }
}
