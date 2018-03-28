//---------------------------------------------------------------------
// <copyright file="NavigationProperty.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Linq;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Represent the edm navigation property class
    /// </summary>
    public sealed class NavigationProperty : EdmMember
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the navigation property class
        /// </summary>
        /// <param name="name">name of the navigation property</param>
        /// <param name="typeUsage">TypeUsage object containing the navigation property type and its facets</param>
        /// <exception cref="System.ArgumentNullException">Thrown if name or typeUsage arguments are null</exception>
        /// <exception cref="System.ArgumentException">Thrown if name argument is empty string</exception>
        internal NavigationProperty(string name, TypeUsage typeUsage)
            : base(name, typeUsage)
        {
            EntityUtil.CheckStringArgument(name, "name");
            EntityUtil.GenericCheckArgumentNull(typeUsage, "typeUsage");
            _accessor = new NavigationPropertyAccessor(name);
        }

        /// <summary>
        /// Initializes a new OSpace instance of the property class
        /// </summary>
        /// <param name="name">name of the property</param>
        /// <param name="typeUsage">TypeUsage object containing the property type and its facets</param>
        /// <param name="propertyInfo">for the property</param>
        internal NavigationProperty(string name, TypeUsage typeUsage, System.Reflection.PropertyInfo propertyInfo)
            : this(name, typeUsage)
        {
            System.Diagnostics.Debug.Assert(name == propertyInfo.Name, "different PropertyName?");
            if (null != propertyInfo)
            {
                System.Reflection.MethodInfo method;
                
                method = propertyInfo.GetGetMethod();
                PropertyGetterHandle = ((null != method) ? method.MethodHandle : default(System.RuntimeMethodHandle));
            }
        }
        #endregion

        /// <summary>
        /// Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind { get { return BuiltInTypeKind.NavigationProperty; } }

        #region Fields
        internal const string RelationshipTypeNamePropertyName = "RelationshipType";
        internal const string ToEndMemberNamePropertyName = "ToEndMember";
        private RelationshipType _relationshipType;
        private RelationshipEndMember _toEndMember;
        private RelationshipEndMember _fromEndMember;

        /// <summary>Store the handle, allowing the PropertyInfo/MethodInfo/Type references to be GC'd</summary>
        internal readonly System.RuntimeMethodHandle PropertyGetterHandle;

        /// <summary>cached dynamic methods to access the property values from a CLR instance</summary> 
        private readonly NavigationPropertyAccessor _accessor;
        #endregion

        /// <summary>
        /// Gets/Sets the relationship type that this navigation property operates on
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the NavigationProperty instance is in ReadOnly state</exception>
        [MetadataProperty(BuiltInTypeKind.RelationshipType, false)]
        public RelationshipType RelationshipType
        {
            get
            {
                return _relationshipType;
            }
            internal set
            {
                _relationshipType = value;
            }
        }

        /// <summary>
        /// Gets/Sets the to relationship end member in the navigation
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the NavigationProperty instance is in ReadOnly state</exception>
        [MetadataProperty(BuiltInTypeKind.RelationshipEndMember, false)]
        public RelationshipEndMember ToEndMember
        {
            get
            {
                return _toEndMember;
            }
            internal set
            {
                _toEndMember = value;
            }
        }

        /// <summary>
        /// Gets/Sets the from relationship end member in the navigation
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the NavigationProperty instance is in ReadOnly state</exception>
        [MetadataProperty(BuiltInTypeKind.RelationshipEndMember, false)]
        public RelationshipEndMember FromEndMember
        {
            get
            {
                return _fromEndMember;
            }
            internal set
            {
                _fromEndMember = value;
            }
        }

        internal NavigationPropertyAccessor Accessor
        {
            get { return _accessor; }
        }

        /// <summary>
        /// Where the given navigation property is on the dependent end of a referential constraint,
        /// returns the foreign key properties. Otherwise, returns an empty set. We will return the members in the order
        /// of the principal end key properties.
        /// </summary>
        /// <returns>Foreign key properties</returns>
        public IEnumerable<EdmProperty> GetDependentProperties()
        {
            // Get the declared type
            AssociationType associationType = (AssociationType)this.RelationshipType;
            Debug.Assert(
                         associationType.ReferentialConstraints != null,
                         "ReferenceConstraints cannot be null");

            if (associationType.ReferentialConstraints.Count > 0)
            {
                ReferentialConstraint rc = associationType.ReferentialConstraints[0];
                RelationshipEndMember dependentEndMember = rc.ToRole;

                if (dependentEndMember.EdmEquals(this.FromEndMember))
                {
                    //Order the dependant properties in the order of principal end's key members.
                    var keyMembers = rc.FromRole.GetEntityType().KeyMembers;
                    var dependantProperties = new List<EdmProperty>(keyMembers.Count);
                    for (int i = 0; i < keyMembers.Count; i++)
                    {
                        dependantProperties.Add(rc.ToProperties[rc.FromProperties.IndexOf(((EdmProperty)keyMembers[i]))]);
                    }
                    return dependantProperties.AsReadOnly();
                }
            }
            
            return Enumerable.Empty<EdmProperty>();
        }
    }
}
