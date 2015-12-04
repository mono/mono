//---------------------------------------------------------------------
// <copyright file="ReferentialConstraint.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.EntityModel.SchemaObjectModel
{
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Xml;

    /// <summary>
    /// Represents an referential constraint on a relationship
    /// </summary>
    internal sealed class ReferentialConstraint : SchemaElement
    {
        private const char KEY_DELIMITER = ' ';
        private ReferentialConstraintRoleElement _principalRole;
        private ReferentialConstraintRoleElement _dependentRole;

        /// <summary>
        /// construct a Referential constraint
        /// </summary>
        /// <param name="relationship"></param>
        public ReferentialConstraint(Relationship relationship)
            : base(relationship)
        {
        }

        /// <summary>
        /// Validate this referential constraint
        /// </summary>
        internal override void Validate()
        {
            base.Validate();
            _principalRole.Validate();
            _dependentRole.Validate();

            if (ReadyForFurtherValidation(_principalRole) && ReadyForFurtherValidation(_dependentRole))
            {
                // Validate the to end and from end of the referential constraint
                IRelationshipEnd principalRoleEnd = _principalRole.End;
                IRelationshipEnd dependentRoleEnd = _dependentRole.End;

                bool isPrinicipalRoleKeyProperty, isDependentRoleKeyProperty;
                bool areAllPrinicipalRolePropertiesNullable, areAllDependentRolePropertiesNullable;
                bool isDependentRolePropertiesSubsetofKeyProperties, isPrinicipalRolePropertiesSubsetofKeyProperties;
                bool isAnyPrinicipalRolePropertyNullable, isAnyDependentRolePropertyNullable;

                // Validate the role name to be different
                if (_principalRole.Name == _dependentRole.Name)
                {
                    AddError(ErrorCode.SameRoleReferredInReferentialConstraint,
                             EdmSchemaErrorSeverity.Error,
                             System.Data.Entity.Strings.SameRoleReferredInReferentialConstraint(this.ParentElement.Name));
                }

                // Resolve all the property in the ToProperty attribute. Also checks whether this is nullable or not and 
                // whether the properties are the keys for the type in the ToRole
                IsKeyProperty(_dependentRole, dependentRoleEnd.Type, 
                    out isPrinicipalRoleKeyProperty, 
                    out areAllDependentRolePropertiesNullable, 
                    out isAnyDependentRolePropertyNullable,
                    out isDependentRolePropertiesSubsetofKeyProperties);

                // Resolve all the property in the ToProperty attribute. Also checks whether this is nullable or not and 
                // whether the properties are the keys for the type in the ToRole
                IsKeyProperty(_principalRole, principalRoleEnd.Type, 
                    out isDependentRoleKeyProperty, 
                    out areAllPrinicipalRolePropertiesNullable,
                    out isAnyPrinicipalRolePropertyNullable,
                    out isPrinicipalRolePropertiesSubsetofKeyProperties);

                Debug.Assert(_principalRole.RoleProperties.Count != 0, "There should be some ref properties in Principal Role");
                Debug.Assert(_dependentRole.RoleProperties.Count != 0, "There should be some ref properties in Dependent Role");

                // The properties in the PrincipalRole must be the key of the Entity type referred to by the principal role
                if (!isDependentRoleKeyProperty)
                {
                    AddError(ErrorCode.InvalidPropertyInRelationshipConstraint,
                             EdmSchemaErrorSeverity.Error,
                             System.Data.Entity.Strings.InvalidFromPropertyInRelationshipConstraint(
                             PrincipalRole.Name, principalRoleEnd.Type.FQName, this.ParentElement.FQName));
                }
                else
                {
                    bool v1Behavior = Schema.SchemaVersion <= XmlConstants.EdmVersionForV1_1;

                    // Determine expected multiplicities
                    RelationshipMultiplicity expectedPrincipalMultiplicity = (v1Behavior 
                        ? areAllPrinicipalRolePropertiesNullable
                        : isAnyPrinicipalRolePropertyNullable)
                        ? RelationshipMultiplicity.ZeroOrOne
                        : RelationshipMultiplicity.One;
                    RelationshipMultiplicity expectedDependentMultiplicity = (v1Behavior
                        ? areAllDependentRolePropertiesNullable
                        : isAnyDependentRolePropertyNullable)
                        ? RelationshipMultiplicity.ZeroOrOne
                        : RelationshipMultiplicity.Many;
                    principalRoleEnd.Multiplicity = principalRoleEnd.Multiplicity ?? expectedPrincipalMultiplicity;
                    dependentRoleEnd.Multiplicity = dependentRoleEnd.Multiplicity ?? expectedDependentMultiplicity;

                    // Since the FromProperty must be the key of the FromRole, the FromRole cannot be '*' as multiplicity
                    // Also the lower bound of multiplicity of FromRole can be zero if and only if all the properties in 
                    // ToProperties are nullable
                    // for v2+
                    if (principalRoleEnd.Multiplicity == RelationshipMultiplicity.Many)
                    {
                        AddError(ErrorCode.InvalidMultiplicityInRoleInRelationshipConstraint,
                                 EdmSchemaErrorSeverity.Error,
                                 System.Data.Entity.Strings.InvalidMultiplicityFromRoleUpperBoundMustBeOne(_principalRole.Name, this.ParentElement.Name));
                    }
                    else if (areAllDependentRolePropertiesNullable
                            && principalRoleEnd.Multiplicity == RelationshipMultiplicity.One)
                    {
                        string message = System.Data.Entity.Strings.InvalidMultiplicityFromRoleToPropertyNullableV1(_principalRole.Name, this.ParentElement.Name);
                        AddError(ErrorCode.InvalidMultiplicityInRoleInRelationshipConstraint,
                                 EdmSchemaErrorSeverity.Error,
                                 message);
                    }
                    else if ((
                                (v1Behavior && !areAllDependentRolePropertiesNullable) ||
                                (!v1Behavior && !isAnyDependentRolePropertyNullable)
                             )
                            && principalRoleEnd.Multiplicity != RelationshipMultiplicity.One)
                    {
                        string message;
                        if (v1Behavior)
                        {
                            message = System.Data.Entity.Strings.InvalidMultiplicityFromRoleToPropertyNonNullableV1(_principalRole.Name, this.ParentElement.Name);
                        }
                        else
                        {
                            message = System.Data.Entity.Strings.InvalidMultiplicityFromRoleToPropertyNonNullableV2(_principalRole.Name, this.ParentElement.Name);
                        }
                        AddError(ErrorCode.InvalidMultiplicityInRoleInRelationshipConstraint,
                                 EdmSchemaErrorSeverity.Error,
                                 message);
                    }

                    // If the ToProperties form the key of the type in ToRole, then the upper bound of the multiplicity 
                    // of the ToRole must be '1'. The lower bound must always be zero since there can be entries in the from
                    // column which are not related to child columns.
                    if (dependentRoleEnd.Multiplicity == RelationshipMultiplicity.One && Schema.DataModel == SchemaDataModelOption.ProviderDataModel)
                    {
                        AddError(ErrorCode.InvalidMultiplicityInRoleInRelationshipConstraint,
                                 EdmSchemaErrorSeverity.Error,
                                 System.Data.Entity.Strings.InvalidMultiplicityToRoleLowerBoundMustBeZero(_dependentRole.Name, this.ParentElement.Name));
                    }

                    // Need to constrain the dependent role in CSDL to Key properties if this is not a IsForeignKey
                    // relationship.
                    if ((!isDependentRolePropertiesSubsetofKeyProperties) &&
                        (!this.ParentElement.IsForeignKey) &&
                        (Schema.DataModel == SchemaDataModelOption.EntityDataModel))
                    {
                        AddError(ErrorCode.InvalidPropertyInRelationshipConstraint,
                                 EdmSchemaErrorSeverity.Error,
                                 System.Data.Entity.Strings.InvalidToPropertyInRelationshipConstraint(
                                 DependentRole.Name, dependentRoleEnd.Type.FQName, this.ParentElement.FQName));

                    }

                    // If the ToProperty is a key property, then the upper bound must be 1 i.e. every parent (from property) can 
                    // have exactly one child
                    if (isPrinicipalRoleKeyProperty)
                    {
                        if (dependentRoleEnd.Multiplicity == RelationshipMultiplicity.Many)
                        {
                            AddError(ErrorCode.InvalidMultiplicityInRoleInRelationshipConstraint,
                                     EdmSchemaErrorSeverity.Error,
                                     System.Data.Entity.Strings.InvalidMultiplicityToRoleUpperBoundMustBeOne(dependentRoleEnd.Name, this.ParentElement.Name));
                        }
                    }
                    // if the ToProperty is not the key, then the upper bound must be many i.e every parent (from property) can
                    // be related to many childs
                    else if (dependentRoleEnd.Multiplicity != RelationshipMultiplicity.Many)
                    {
                        AddError(ErrorCode.InvalidMultiplicityInRoleInRelationshipConstraint,
                                     EdmSchemaErrorSeverity.Error,
                                     System.Data.Entity.Strings.InvalidMultiplicityToRoleUpperBoundMustBeMany(dependentRoleEnd.Name, this.ParentElement.Name));
                    }

                    if (_dependentRole.RoleProperties.Count != _principalRole.RoleProperties.Count)
                    {
                        AddError(ErrorCode.MismatchNumberOfPropertiesInRelationshipConstraint,
                                 EdmSchemaErrorSeverity.Error,
                                 System.Data.Entity.Strings.MismatchNumberOfPropertiesinRelationshipConstraint);
                    }
                    else
                    {
                        for (int i = 0; i < _dependentRole.RoleProperties.Count; i++)
                        {
                            if (_dependentRole.RoleProperties[i].Property.Type != _principalRole.RoleProperties[i].Property.Type)
                            {
                                AddError(ErrorCode.TypeMismatchRelationshipConstaint,
                                         EdmSchemaErrorSeverity.Error,
                                         System.Data.Entity.Strings.TypeMismatchRelationshipConstaint(
                                                       _dependentRole.RoleProperties[i].Name,
                                                       _dependentRole.End.Type.Identity,
                                                       _principalRole.RoleProperties[i].Name,
                                                       _principalRole.End.Type.Identity,
                                                       this.ParentElement.Name
                                                       ));
                            }
                        }
                    }
                }
            }
        }

        private static bool ReadyForFurtherValidation(ReferentialConstraintRoleElement role)
        {
            if (role == null)
                return false;

            if(role.End == null)
                return false;

            if(role.RoleProperties.Count == 0)
                return false;

            foreach(PropertyRefElement propRef in role.RoleProperties)
            {
                if(propRef.Property == null)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Resolves the given property names to the property in the item
        /// Also checks whether the properties form the key for the given type and whether all the properties are nullable or not
        /// </summary>
        /// <param name="roleElement"></param>
        /// <param name="itemType"></param>
        /// <param name="isKeyProperty"></param>
        /// <param name="areAllPropertiesNullable"></param>
        /// <param name="isSubsetOfKeyProperties"></param>
        private static void IsKeyProperty(ReferentialConstraintRoleElement roleElement, SchemaEntityType itemType,
            out bool isKeyProperty,
            out bool areAllPropertiesNullable,
            out bool isAnyPropertyNullable,
            out bool isSubsetOfKeyProperties)
        {
            isKeyProperty = true;
            areAllPropertiesNullable = true;
            isAnyPropertyNullable = false;
            isSubsetOfKeyProperties = true;

            if (itemType.KeyProperties.Count != roleElement.RoleProperties.Count)
            {
                isKeyProperty = false;
            }

            // Checking that ToProperties must be the key properties in the entity type referred by the ToRole
            for (int i = 0; i < roleElement.RoleProperties.Count; i++)
            {
                // Once we find that the properties in the constraint are not a subset of the
                // Key, one need not search for it every time
                if (isSubsetOfKeyProperties)
                {

                    bool foundKeyProperty = false;

                    // All properties that are defined in ToProperties must be the key property on the entity type
                    for (int j = 0; j < itemType.KeyProperties.Count; j++)
                    {
                        if (itemType.KeyProperties[j].Property == roleElement.RoleProperties[i].Property)
                        {
                            foundKeyProperty = true;
                            break;
                        }
                    }

                    if (!foundKeyProperty)
                    {
                        isKeyProperty = false;
                        isSubsetOfKeyProperties = false;
                    }
                }

                areAllPropertiesNullable &= roleElement.RoleProperties[i].Property.Nullable;
                isAnyPropertyNullable |= roleElement.RoleProperties[i].Property.Nullable;
            }
        }

        protected override bool HandleAttribute(XmlReader reader)
        {
            return false;
        }

        protected override bool HandleElement(XmlReader reader)
        {
            if (base.HandleElement(reader))
            {
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.PrincipalRole))
            {
                HandleReferentialConstraintPrincipalRoleElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.DependentRole))
            {
                HandleReferentialConstraintDependentRoleElement(reader);
                return true;
            }

            return false;
        }

        internal void HandleReferentialConstraintPrincipalRoleElement(XmlReader reader)
        {
            _principalRole = new ReferentialConstraintRoleElement(this);
            _principalRole.Parse(reader);
        }

        internal void HandleReferentialConstraintDependentRoleElement(XmlReader reader)
        {
            _dependentRole = new ReferentialConstraintRoleElement(this);
            _dependentRole.Parse(reader);
        }

        internal override void ResolveTopLevelNames()
        {
            _dependentRole.ResolveTopLevelNames();

            _principalRole.ResolveTopLevelNames();
        }

        /// <summary>
        /// The parent element as an IRelationship
        /// </summary>
        internal new IRelationship ParentElement
        {
            get
            {
                return (IRelationship)(base.ParentElement);
            }
        }

        internal ReferentialConstraintRoleElement PrincipalRole
        {
            get
            {
                return _principalRole;
            }
        }

        internal ReferentialConstraintRoleElement DependentRole
        {
            get
            {
                return _dependentRole;
            }
        }
    }
}
