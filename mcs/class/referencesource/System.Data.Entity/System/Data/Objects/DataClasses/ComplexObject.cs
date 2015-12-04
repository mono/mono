//---------------------------------------------------------------------
// <copyright file="ComplexObject.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace System.Data.Objects.DataClasses
{
    /// <summary>
    /// This is the interface that represent the minimum interface required
    /// to be an entity in ADO.NET.
    /// </summary>
    [DataContract(IsReference = true)]
    [Serializable]
    public abstract class ComplexObject : StructuralObject
    {
        // The following fields are serialized.  Adding or removing a serialized field is considered
        // a breaking change.  This includes changing the field type or field name of existing
        // serialized fields. If you need to make this kind of change, it may be possible, but it
        // will require some custom serialization/deserialization code.
        private StructuralObject _parent;     // Object that contains this ComplexObject (can be Entity or ComplexObject)
        private string _parentPropertyName;   // Property name for this type on the containing object

        /// <summary>
        /// Associate the ComplexType with an Entity or another ComplexObject
        /// Parent may be an Entity or ComplexObject
        /// </summary>
        /// <param name="parent">Object to be added to.</param>
        /// <param name="parentPropertyName">The property on the parent that reference the complex type.</param>
        internal void AttachToParent(
            StructuralObject parent,
            string parentPropertyName)
        {
            Debug.Assert(null != parent, "Attempt to attach to a null parent");
            Debug.Assert(null != parentPropertyName, "Must provide parentPropertyName in order to attach");

            if (_parent != null)
            {
                throw EntityUtil.ComplexObjectAlreadyAttachedToParent();
            }

            Debug.Assert(_parentPropertyName == null);

            _parent = parent;
            _parentPropertyName = parentPropertyName;
        }

        /// <summary>
        /// Removes this instance from the parent it was attached to. 
        /// Parent may be an Entity or ComplexObject
        /// </summary>
        internal void DetachFromParent()
        {
            // We will null out _parent and _parentPropertyName anyway, so if they are already null
            // it is an unexpected condition, but should not cause a failure in released code
            Debug.Assert(_parent != null, "Attempt to detach from a null _parent");
            Debug.Assert(_parentPropertyName != null, "Null _parentPropertyName on a non-null _parent");

            _parent = null;
            _parentPropertyName = null;
        }

        /// <summary>
        /// Reports that a change is about to occur to one of the properties of this instance
        /// to the containing object and then continues default change
        /// reporting behavior.
        /// </summary>
        protected sealed override void ReportPropertyChanging(
            string property)
        {
            EntityUtil.CheckStringArgument(property, "property");

            base.ReportPropertyChanging(property);

            // Since we are a ComplexObject, all changes (scalar or complex) are considered complex property changes            
            ReportComplexPropertyChanging(null, this, property);            
        }

        /// <summary>
        /// Reports a change to one of the properties of this instance
        /// to the containing object and then continues default change
        /// reporting behavior.
        /// </summary>
        protected sealed override void ReportPropertyChanged(
            string property)
        {
            EntityUtil.CheckStringArgument(property, "property");

            // Since we are a ComplexObject, all changes (scalar or complex) are considered complex property changes
            ReportComplexPropertyChanged(null, this, property);
            
            base.ReportPropertyChanged(property);
        }


        internal sealed override bool IsChangeTracked
        {
            get
            {
                return _parent == null ? false : _parent.IsChangeTracked;                
            }
        }

        /// <summary>
        /// This method is used to report all changes on this ComplexObject to its parent entity or ComplexObject
        /// </summary>
        /// <param name="entityMemberName">
        /// Should be null in this method override.
        /// This is only relevant in Entity's implementation of this method, so it is unused here
        /// Instead of passing the most-derived property name up the hierarchy, we will always pass the current _parentPropertyName
        /// Once this gets up to the Entity, it will actually use the value that was passed in
        /// </param>
        /// <param name="complexObject">
        /// The instance of the object on which the property is changing.
        /// </param>
        /// <param name="complexMemberName">
        /// The name of the changing property on complexObject.
        /// </param>
        internal sealed override void ReportComplexPropertyChanging(
            string entityMemberName, ComplexObject complexObject, string complexMemberName)
        {
            // entityMemberName is unused here because we just keep passing the current parent name up the hierarchy
            // This value is only used in the EntityObject override of this method

            Debug.Assert(complexObject != null, "invalid complexObject");
            Debug.Assert(!String.IsNullOrEmpty(complexMemberName), "invalid complexMemberName");
            
            if (null != _parent)
            {
                _parent.ReportComplexPropertyChanging(_parentPropertyName, complexObject, complexMemberName);
            }
        }

        /// <summary>
        /// This method is used to report all changes on this ComplexObject to its parent entity or ComplexObject
        /// </summary>
        /// <param name="entityMemberName">
        /// Should be null in this method override.
        /// This is only relevant in Entity's implementation of this method, so it is unused here
        /// Instead of passing the most-derived property name up the hierarchy, we will always pass the current _parentPropertyName
        /// Once this gets up to the Entity, it will actually use the value that was passed in.
        /// </param>
        /// <param name="complexObject">
        /// The instance of the object on which the property is changing.
        /// </param>
        /// <param name="complexMemberName">
        /// The name of the changing property on complexObject.
        /// </param>
        internal sealed override void ReportComplexPropertyChanged(
            string entityMemberName, ComplexObject complexObject, string complexMemberName)
        {
            // entityMemberName is unused here because we just keep passing the current parent name up the hierarchy
            // This value is only used in the EntityObject override of this method

            Debug.Assert(complexObject != null, "invalid complexObject");
            Debug.Assert(!String.IsNullOrEmpty(complexMemberName), "invalid complexMemberName");

            if (null != _parent)
            {
                _parent.ReportComplexPropertyChanged(_parentPropertyName, complexObject, complexMemberName);
            }
        }
    }
}
