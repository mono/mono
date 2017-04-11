//---------------------------------------------------------------------
// <copyright file="PropertyRef.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
//using System.Diagnostics; // Please use PlanCompiler.Assert instead of Debug.Assert in this class...

// It is fine to use Debug.Assert in cases where you assert an obvious thing that is supposed
// to prevent from simple mistakes during development (e.g. method argument validation 
// in cases where it was you who created the variables or the variables had already been validated or 
// in "else" clauses where due to code changes (e.g. adding a new value to an enum type) the default 
// "else" block is chosen why the new condition should be treated separately). This kind of asserts are 
// (can be) helpful when developing new code to avoid simple mistakes but have no or little value in 
// the shipped product. 
// PlanCompiler.Assert *MUST* be used to verify conditions in the trees. These would be assumptions 
// about how the tree was built etc. - in these cases we probably want to throw an exception (this is
// what PlanCompiler.Assert does when the condition is not met) if either the assumption is not correct 
// or the tree was built/rewritten not the way we thought it was.
// Use your judgment - if you rather remove an assert than ship it use Debug.Assert otherwise use
// PlanCompiler.Assert.

using System.Globalization;

using System.Data.Common;
using md = System.Data.Metadata.Edm;

//
// The PropertyRef class (and its subclasses) represent references to a property
// of a type.
// The PropertyRefList class represents a list of expected properties
// where each property from the type is described as a PropertyRef
//
// These classes are used by the StructuredTypeEliminator module as part of
// eliminating all structured types. The basic idea of this module is that all
// structured types are flattened out into a single level. To avoid a large amount
// of potentially unnecessary information, we try to identify what pieces of information
// are really necessary at each node of the tree. This is where PropertyRef comes in.
// A PropertyRef (and more generally, a PropertyRefList) identifies a list of
// properties, and can be attached to a node/var to indicate that these were the
// only desired properties.
//
namespace System.Data.Query.PlanCompiler
{
    /// <summary>
    /// A PropertyRef class encapsulates a reference to one or more properties of
    /// a complex instance - a record type, a complex type or an entity type.
    /// A PropertyRef may be of the following kinds.
    ///   - a simple property reference (just a reference to a simple property)
    ///   - a typeid reference - applies only to entitytype and complextypes
    ///   - an entitysetid reference - applies only to ref and entity types
    ///   - a nested property reference (a reference to a nested property - a.b)
    ///   - an "all" property reference (all properties)
    /// </summary>
    internal abstract class PropertyRef
    {
        /// <summary>
        /// trivial constructor
        /// </summary>
        internal PropertyRef() { }

        /// <summary>
        /// Create a nested property ref, with "p" as the prefix.
        /// The best way to think of this function as follows.
        /// Consider a type T where "this" describes a property X on T. Now
        /// consider a new type S, where "p" is a property of S and is of type T.
        /// This function creates a PropertyRef that describes the same property X
        /// from S.p instead
        /// </summary>
        /// <param name="p">the property to prefix with</param>
        /// <returns>the nested property reference</returns>
        internal virtual PropertyRef CreateNestedPropertyRef(PropertyRef p)
        {
            return new NestedPropertyRef(p, this);
        }

        /// <summary>
        /// Create a nested property ref for a simple property. Delegates to the function
        /// above
        /// </summary>
        /// <param name="p">the simple property</param>
        /// <returns>a nestedPropertyRef</returns>
        internal PropertyRef CreateNestedPropertyRef(md.EdmMember p)
        {
            return CreateNestedPropertyRef(new SimplePropertyRef(p));
        }

        /// <summary>
        /// Creates a nested property ref for a rel-property. Delegates to the function above
        /// </summary>
        /// <param name="p">the rel-property</param>
        /// <returns>a nested property ref</returns>
        internal PropertyRef CreateNestedPropertyRef(InternalTrees.RelProperty p)
        {
            return CreateNestedPropertyRef(new RelPropertyRef(p));
        }
       
        /// <summary>
        /// The tostring method for easy debuggability
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "";
        }
    }

    /// <summary>
    /// A "simple" property ref - represents a simple property of the type
    /// </summary>
    internal class SimplePropertyRef : PropertyRef
    {
        private md.EdmMember m_property;

        /// <summary>
        /// Simple constructor
        /// </summary>
        /// <param name="property">the property metadata</param>
        internal SimplePropertyRef(md.EdmMember property)
        {
            m_property = property;
        }

        /// <summary>
        /// Gets the property metadata
        /// </summary>
        internal md.EdmMember Property { get { return m_property; } }

        /// <summary>
        /// Overrides the default equality function. Two SimplePropertyRefs are
        /// equal, if they describe the same property
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            SimplePropertyRef other = obj as SimplePropertyRef;
            return (other != null &&
                InternalTrees.Command.EqualTypes(m_property.DeclaringType, other.m_property.DeclaringType) &&
                other.m_property.Name.Equals(this.m_property.Name));
        }

        /// <summary>
        /// Overrides the default hashcode function.
        /// Simply returns the hashcode for the property instead
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return m_property.Name.GetHashCode();
        }
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return m_property.Name;
        }
    }

    /// <summary>
    /// A TypeId propertyref represents a reference to the TypeId property
    /// of a type (complex type, entity type etc.)
    /// </summary>
    internal class TypeIdPropertyRef : PropertyRef
    {
        private TypeIdPropertyRef() : base() { }

        /// <summary>
        /// Gets the default instance of this type
        /// </summary>
        internal static TypeIdPropertyRef Instance = new TypeIdPropertyRef();

        /// <summary>
        /// Friendly string for debugging.
        /// </summary>
        public override string ToString()
        {
            return "TYPEID";
        }
        
    }

    /// <summary>
    /// An NullSentinel propertyref represents the NullSentinel property for
    /// a row type.
    /// As with TypeId, this class is a singleton instance
    /// </summary>
    internal class NullSentinelPropertyRef : PropertyRef
    {
        private static NullSentinelPropertyRef s_singleton = new NullSentinelPropertyRef();
        private NullSentinelPropertyRef() : base() { }

        /// <summary>
        /// Gets the singleton instance
        /// </summary>
        internal static NullSentinelPropertyRef Instance
        {
            get { return s_singleton; }
        }
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "NULLSENTINEL";
        }

    }

    /// <summary>
    /// An EntitySetId propertyref represents the EntitySetId property for
    /// an entity type or a ref type.
    /// As with TypeId, this class is a singleton instance
    /// </summary>
    internal class EntitySetIdPropertyRef : PropertyRef
    {
        private EntitySetIdPropertyRef() : base() { }

        /// <summary>
        /// Gets the singleton instance
        /// </summary>
        internal static EntitySetIdPropertyRef Instance = new EntitySetIdPropertyRef();

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "ENTITYSETID";
        }

    }

    /// <summary>
    /// A nested propertyref describes a nested property access - think "a.b.c"
    /// </summary>
    internal class NestedPropertyRef : PropertyRef
    {
        private readonly PropertyRef m_inner;
        private readonly PropertyRef m_outer;

        /// <summary>
        /// Basic constructor.
        /// Represents the access of property "propertyRef" within property "property"
        /// </summary>
        /// <param name="innerProperty">the inner property</param>
        /// <param name="outerProperty">the outer property</param>
        internal NestedPropertyRef(PropertyRef innerProperty, PropertyRef outerProperty)
        {
            PlanCompiler.Assert(!(innerProperty is NestedPropertyRef), "innerProperty cannot be a NestedPropertyRef"); 
            m_inner = innerProperty;
            m_outer = outerProperty;
        }

        /// <summary>
        /// the nested property
        /// </summary>
        internal PropertyRef OuterProperty { get { return m_outer; } }

        /// <summary>
        /// the parent property
        /// </summary>
        internal PropertyRef InnerProperty { get { return m_inner; } }

        /// <summary>
        /// Overrides the default equality function. Two NestedPropertyRefs are
        /// equal if the have the same property name, and the types are the same
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            NestedPropertyRef other = obj as NestedPropertyRef;
            return (other != null &&
                m_inner.Equals(other.m_inner) &&
                m_outer.Equals(other.m_outer));
        }

        /// <summary>
        /// Overrides the default hashcode function. Simply adds the hashcodes
        /// of the "property" and "propertyRef" fields
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return m_inner.GetHashCode() ^ m_outer.GetHashCode();
        }
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return m_inner + "." + m_outer;
        }
    }

    /// <summary>
    /// A reference to "all" properties of a type
    /// </summary>
    internal class AllPropertyRef : PropertyRef
    {
        private AllPropertyRef() : base() { }

        /// <summary>
        /// Get the singleton instance
        /// </summary>
        internal static AllPropertyRef Instance = new AllPropertyRef();

        /// <summary>
        /// Create a nested property ref, with "p" as the prefix
        /// </summary>
        /// <param name="p">the property to prefix with</param>
        /// <returns>the nested property reference</returns>
        internal override PropertyRef CreateNestedPropertyRef(PropertyRef p)
        {
            return p;
        }
        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "ALL";
        }
    }

    /// <summary>
    /// A rel-property ref - represents a rel property of the type
    /// </summary>
    internal class RelPropertyRef : PropertyRef
    {
#region private state
        private InternalTrees.RelProperty m_property;
#endregion

        #region constructor
        /// <summary>
        /// Simple constructor
        /// </summary>
        /// <param name="property">the property metadata</param>
        internal RelPropertyRef(InternalTrees.RelProperty property)
        {
            m_property = property;
        }
        #endregion

        #region public apis
        /// <summary>
        /// Gets the property metadata
        /// </summary>
        internal InternalTrees.RelProperty Property { get { return m_property; } }

        /// <summary>
        /// Overrides the default equality function. Two RelPropertyRefs are
        /// equal, if they describe the same property
        /// </summary>
        /// <param name="obj">the other object to compare to</param>
        /// <returns>true, if the objects are equal</returns>
        public override bool Equals(object obj)
        {
            RelPropertyRef other = obj as RelPropertyRef;
            return (other != null &&
                m_property.Equals(other.m_property));
        }

        /// <summary>
        /// Overrides the default hashcode function.
        /// Simply returns the hashcode for the property instead
        /// </summary>
        /// <returns>hashcode for the relpropertyref</returns>
        public override int GetHashCode()
        {
            return m_property.GetHashCode();
        }

        /// <summary>
        /// debugging support
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return m_property.ToString();
        }
        #endregion
    }

    /// <summary>
    /// Represents a collection of property references
    /// </summary>
    internal class PropertyRefList
    {
        private Dictionary<PropertyRef, PropertyRef> m_propertyReferences;
        private bool m_allProperties;

        /// <summary>
        /// Get something that represents "all" property references
        /// </summary>
        internal static PropertyRefList All = new PropertyRefList(true);

        /// <summary>
        /// Trivial constructor
        /// </summary>
        internal PropertyRefList() : this(false) {}

        private PropertyRefList(bool allProps)
        {
            this.m_propertyReferences = new Dictionary<PropertyRef, PropertyRef>();

            if (allProps)
            {
                MakeAllProperties();
            }
        }
        private void MakeAllProperties()
        {
            m_allProperties = true;
            m_propertyReferences.Clear();
            m_propertyReferences.Add(AllPropertyRef.Instance, AllPropertyRef.Instance);
        }

        /// <summary>
        /// Add a new property reference to this list
        /// </summary>
        /// <param name="property">new property reference</param>
        internal void Add(PropertyRef property)
        {
            if (m_allProperties)
                return;
            else if (property is AllPropertyRef)
                MakeAllProperties();
            else
                m_propertyReferences[property] = property;
        }
        /// <summary>
        /// Append an existing list of property references to myself
        /// </summary>
        /// <param name="propertyRefs">list of property references</param>
        internal void Append(PropertyRefList propertyRefs)
        {
            if (m_allProperties)
                return;
            foreach (PropertyRef p in propertyRefs.m_propertyReferences.Keys)
            {
                this.Add(p);
            }
        }

        /// <summary>
        /// Do I contain "all" properties?
        /// </summary>
        internal bool AllProperties { get { return m_allProperties; } }

        /// <summary>
        /// Create a clone of myself
        /// </summary>
        /// <returns>a clone of myself</returns>
        internal PropertyRefList Clone()
        {
            PropertyRefList newProps = new PropertyRefList(m_allProperties);
            foreach (PropertyRef p in this.m_propertyReferences.Keys)
                newProps.Add(p);
            return newProps;
        }

        /// <summary>
        /// Do I contain the specifed property?
        /// </summary>
        /// <param name="p">The property</param>
        /// <returns>true, if I do</returns>
        internal bool Contains(PropertyRef p)
        {
            return m_allProperties || m_propertyReferences.ContainsKey(p);
        }

        /// <summary>
        /// Get the list of all properties
        /// </summary>
        internal IEnumerable<PropertyRef> Properties
        {
            get { return m_propertyReferences.Keys; }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string x = "{";
            foreach (PropertyRef p in m_propertyReferences.Keys)
                x += p.ToString() + ",";
            x += "}";
            return x;
        }
    }
}
