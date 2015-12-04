//---------------------------------------------------------------------
// <copyright file="EntityContainerAssociationSet.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.EntityModel.SchemaObjectModel
{
    using System.Collections.Generic;
    using System.Data.Metadata.Edm;
    using System.Data.Objects.DataClasses;
    using System.Diagnostics;
    using System.Globalization;
    using System.Xml;

    /// <summary>
    /// Represents an AssociationSet element.
    /// </summary>
    internal sealed class EntityContainerAssociationSet : EntityContainerRelationshipSet
    {
        // Note: If you add more fields, please make sure you handle that in the clone method
        private Dictionary<string, EntityContainerAssociationSetEnd> _relationshipEnds = new Dictionary<string, EntityContainerAssociationSetEnd>();
        private List<EntityContainerAssociationSetEnd> _rolelessEnds = new List<EntityContainerAssociationSetEnd>();

        /// <summary>
        /// Constructs an EntityContainerAssociationSet
        /// </summary>
        /// <param name="parentElement">Reference to the schema element.</param>
        public EntityContainerAssociationSet( EntityContainer parentElement )
            : base( parentElement )
        {
        }


        /// <summary>
        /// The ends defined and infered for this AssociationSet
        /// </summary>
        internal override IEnumerable<EntityContainerRelationshipSetEnd> Ends
        {
            get 
            {
                foreach ( EntityContainerAssociationSetEnd end in _relationshipEnds.Values )
                {
                    yield return end;
                }

                foreach ( EntityContainerAssociationSetEnd end in _rolelessEnds )
                {
                    yield return end;
                }
            }
        }

        protected override bool HandleAttribute(XmlReader reader)
        {
            if (base.HandleAttribute(reader))
            {
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.Association))
            {
                HandleRelationshipTypeNameAttribute(reader);
                return true;
            }

            return false;
        }

        protected override bool HandleElement(XmlReader reader)
        {
            if (base.HandleElement(reader))
            {
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.End))
            {
                HandleEndElement(reader);
                return true;
            }
            return false;
        }

        /// <summary>
        /// The method that is called when an End element is encountered.
        /// </summary>
        /// <param name="reader">The XmlReader positioned at the EndElement.</param>
        private void HandleEndElement( XmlReader reader )
        {
            Debug.Assert( reader != null );

            EntityContainerAssociationSetEnd end = new EntityContainerAssociationSetEnd( this );
            end.Parse( reader );

            if ( end.Role == null  )
            {
                // we will resolve the role name later and put it in the 
                // normal _relationshipEnds dictionary
                _rolelessEnds.Add( end );
                return;
            }

            if ( HasEnd( end.Role ) )
            {
                end.AddError( ErrorCode.InvalidName, EdmSchemaErrorSeverity.Error, reader,
                    System.Data.Entity.Strings.DuplicateEndName(end.Name ) );
                return;
            }

            _relationshipEnds.Add( end.Role, end );
        }

        internal override void ResolveTopLevelNames()
        {
            base.ResolveTopLevelNames();

            // this just got resolved
            Debug.Assert(
                Relationship == null || Relationship.RelationshipKind == RelationshipKind.Association,
                string.Format(CultureInfo.InvariantCulture, "The relationship referenced by the Association attribute of {0} is not an Association relationship.", FQName));
        }

        internal override void ResolveSecondLevelNames()
        {
            base.ResolveSecondLevelNames();
            // the base class should have fixed up the role names on my ends
            foreach (EntityContainerAssociationSetEnd end in _rolelessEnds)
            {
                if (end.Role != null)
                {
                    if (HasEnd(end.Role))
                    {
                        end.AddError(ErrorCode.InvalidName, EdmSchemaErrorSeverity.Error,
                            System.Data.Entity.Strings.InferRelationshipEndGivesAlreadyDefinedEnd(end.EntitySet.FQName, Name));
                    }
                    else
                    {
                        _relationshipEnds.Add(end.Role, end);
                    }
                }

                // any that didn't get resolved will already have errors entered
            }

            _rolelessEnds.Clear();
        }

        /// <summary>
        /// Create and add a EntityContainerEnd from the IRelationshipEnd provided
        /// </summary>
        /// <param name="relationshipEnd">The relationship end of the end to add.</param>
        /// <param name="entitySet">The entitySet to associate with the relationship end.</param>
        protected override void AddEnd( IRelationshipEnd relationshipEnd, EntityContainerEntitySet entitySet )
        {
            Debug.Assert( relationshipEnd != null );
            Debug.Assert( !_relationshipEnds.ContainsKey( relationshipEnd.Name ) );
            // we expect set to be null sometimes

            EntityContainerAssociationSetEnd end = new EntityContainerAssociationSetEnd( this );
            end.Role = relationshipEnd.Name;
            end.RelationshipEnd = relationshipEnd;

            end.EntitySet = entitySet;
            if ( end.EntitySet != null )
            {
                _relationshipEnds.Add( end.Role, end );
            }
        }

        protected override bool HasEnd( string role )
        {
            return _relationshipEnds.ContainsKey( role );
        }

        internal override SchemaElement Clone(SchemaElement parentElement)
        {
            EntityContainerAssociationSet associationSet = new EntityContainerAssociationSet((EntityContainer)parentElement);

            associationSet.Name = this.Name;
            associationSet.Relationship = this.Relationship;

            foreach (EntityContainerAssociationSetEnd end in this.Ends)
            {
                EntityContainerAssociationSetEnd clonedEnd = (EntityContainerAssociationSetEnd)end.Clone(associationSet);
                associationSet._relationshipEnds.Add(clonedEnd.Role, clonedEnd);
            }

            return associationSet;
        }
    }
}
