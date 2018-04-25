//---------------------------------------------------------------------
// <copyright file="EntityContainerAssociationSetEnd.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.EntityModel.SchemaObjectModel
{
    using System.Collections.Generic;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Xml;

    /// <summary>
    /// Represents an  element.
    /// </summary>
    internal sealed class EntityContainerAssociationSetEnd : EntityContainerRelationshipSetEnd
    {
        private string _unresolvedRelationshipEndRole;

        /// <summary>
        /// Constructs an EntityContainerAssociationSetEnd
        /// </summary>
        /// <param name="parentElement">Reference to the schema element.</param>
        public EntityContainerAssociationSetEnd( EntityContainerAssociationSet parentElement )
            : base( parentElement )
        {
        }

        public string Role
        {
            get
            {
                return _unresolvedRelationshipEndRole;
            }
            set
            {
                _unresolvedRelationshipEndRole = value;
            }
        }

        public override string Name
        {
            get
            {
                return Role;
            }
        }

        protected override bool HandleAttribute(XmlReader reader)
        {
            if (base.HandleAttribute(reader))
            {
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.Role))
            {
                HandleRoleAttribute(reader);
                return true;
            }

            return false;
        }

        /// <summary>
        /// This is the method that is called when an Role Attribute is encountered.
        /// </summary>
        /// <param name="reader">The XmlRead positned at the extent attribute.</param>
        private void HandleRoleAttribute( XmlReader reader )
        {
            _unresolvedRelationshipEndRole = HandleUndottedNameAttribute( reader, _unresolvedRelationshipEndRole );
        }

        /// <summary>
        /// Used during the resolve phase to resolve the type name to the object that represents that type
        /// </summary>
        internal override void  ResolveTopLevelNames()
        {
            base.ResolveTopLevelNames();

            // resolve end name to the corosponding relationship end
            IRelationship relationship = ParentElement.Relationship;
            if ( relationship == null )
            {
                // error already logged for this
                return;
            }

        }

        internal override void ResolveSecondLevelNames()
        {
            base.ResolveSecondLevelNames();

            if (_unresolvedRelationshipEndRole == null && EntitySet != null)
            {
                // no role provided, infer it
                RelationshipEnd = InferRelationshipEnd(EntitySet);
                if (RelationshipEnd != null)
                {
                    _unresolvedRelationshipEndRole = RelationshipEnd.Name;
                }
            }
            else if (_unresolvedRelationshipEndRole != null)
            {
                IRelationship relationship = ParentElement.Relationship;
                IRelationshipEnd end;
                if (relationship.TryGetEnd(_unresolvedRelationshipEndRole, out end))
                {
                    RelationshipEnd = end;
                }
                else
                {
                    // couldn't find a matching relationship end for this RelationshipSet end
                    AddError(ErrorCode.InvalidContainerTypeForEnd, EdmSchemaErrorSeverity.Error,
                        System.Data.Entity.Strings.InvalidEntityEndName(Role, relationship.FQName));

                }
            }
        }

        /// <summary>
        /// If the role name is missing but an entity set is given, figure out what the
        /// relationship end should be
        /// </summary>
        /// <param name="set">The given EntitySet</param>
        /// <returns>The appropriate relationship end</returns>
        private IRelationshipEnd InferRelationshipEnd( EntityContainerEntitySet set )
        {
            Debug.Assert(set != null, "set parameter is null");

            if ( ParentElement.Relationship == null )
            {
                return null;
            }

            List<IRelationshipEnd> possibleEnds = new List<IRelationshipEnd>();
            foreach ( IRelationshipEnd end in ParentElement.Relationship.Ends )
            {
                if ( set.EntityType.IsOfType( end.Type ) )
                {
                    possibleEnds.Add( end );
                }
            }

            if ( possibleEnds.Count == 1 )
            {
                return possibleEnds[0];
            }
            else if ( possibleEnds.Count == 0 )
            {
                // no matchs
                AddError( ErrorCode.FailedInference, EdmSchemaErrorSeverity.Error,
                    System.Data.Entity.Strings.InferRelationshipEndFailedNoEntitySetMatch(
                    set.Name, this.ParentElement.Name, ParentElement.Relationship.FQName, set.EntityType.FQName, this.ParentElement.ParentElement.FQName ) );
            }
            else
            {
                // ambiguous
                AddError( ErrorCode.FailedInference, EdmSchemaErrorSeverity.Error,
                    System.Data.Entity.Strings.InferRelationshipEndAmbiguous(
                    set.Name, this.ParentElement.Name, ParentElement.Relationship.FQName, set.EntityType.FQName, this.ParentElement.ParentElement.FQName));

            }

            return null;
        }

        internal override SchemaElement Clone(SchemaElement parentElement)
        {
            EntityContainerAssociationSetEnd setEnd = new EntityContainerAssociationSetEnd((EntityContainerAssociationSet)parentElement);
            setEnd._unresolvedRelationshipEndRole = _unresolvedRelationshipEndRole;
            setEnd.EntitySet = this.EntitySet;

            return setEnd;
        }

    }
}
