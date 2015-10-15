//---------------------------------------------------------------------
// <copyright file="EntityContainerRelationshipSet.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.EntityModel.SchemaObjectModel
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Xml;

    /// <summary>
    /// Represents an RelationshipSet element.
    /// </summary>
    internal abstract class EntityContainerRelationshipSet : SchemaElement
    {

        private IRelationship _relationship;
        string _unresolvedRelationshipTypeName;

        
        /// <summary>
        /// Constructs an EntityContainerRelationshipSet
        /// </summary>
        /// <param name="parentElement">Reference to the schema element.</param>
        public EntityContainerRelationshipSet( EntityContainer parentElement )
            : base( parentElement )
        {
        }

        public override string FQName
        {
            get
            {
                return this.ParentElement.Name + "." + this.Name;
            }
        }

        internal IRelationship Relationship
        {
            get
            {
                return _relationship;
            }
            set
            {
                Debug.Assert(value != null, "relationship can never be set to null");
                _relationship = value;
            }
        }

        protected abstract bool HasEnd( string role );
        protected abstract void AddEnd( IRelationshipEnd relationshipEnd, EntityContainerEntitySet entitySet );
        internal abstract IEnumerable<EntityContainerRelationshipSetEnd> Ends { get; }

        /// <summary>
        /// The method that is called when an Association attribute is encountered.
        /// </summary>
        /// <param name="reader">An XmlReader positioned at the Association attribute.</param>
        protected void HandleRelationshipTypeNameAttribute( XmlReader reader )
        {
            Debug.Assert( reader != null );
            ReturnValue<string> value = HandleDottedNameAttribute( reader, _unresolvedRelationshipTypeName, Strings.PropertyTypeAlreadyDefined );
            if ( value.Succeeded )
            {
                _unresolvedRelationshipTypeName = value.Value;
            }
        }

        
        /// <summary>
        /// Used during the resolve phase to resolve the type name to the object that represents that type
        /// </summary>
        internal override void ResolveTopLevelNames()
        {
            base.ResolveTopLevelNames();

            if ( _relationship == null )
            {
                SchemaType element;
                if ( !Schema.ResolveTypeName( this, _unresolvedRelationshipTypeName, out element ) )
                {
                    return;
                }

                _relationship = element as IRelationship;
                if ( _relationship == null )
                {
                    AddError( ErrorCode.InvalidPropertyType, EdmSchemaErrorSeverity.Error,
                        System.Data.Entity.Strings.InvalidRelationshipSetType(element.Name ) );
                    return;
                }
            }

            foreach ( EntityContainerRelationshipSetEnd end in Ends )
            {
                end.ResolveTopLevelNames();
            }
        }

        internal override void ResolveSecondLevelNames()
        {
            base.ResolveSecondLevelNames();
            foreach (EntityContainerRelationshipSetEnd end in Ends)
            {
                end.ResolveSecondLevelNames();
            }
        }

        /// <summary>
        /// Do all validation for this element here, and delegate to all sub elements
        /// </summary>
        internal override void Validate()
        {
            base.Validate();

            InferEnds();

            // check out the ends
            foreach ( EntityContainerRelationshipSetEnd end in Ends )
            {
                end.Validate();
            }


            // Enabling Association between subtypes in case of Referential Constraints, since 
            // CSD is blocked on this. We need to make a long term call about whether we should
            // really allow this. 















            // Validate Number of ends is correct
            //    What we know:
            //      No ends are missing, becuase we infered all missing ends
            //      No extra ends are there because the names have been matched, and an extra name will have caused an error
            //
            //    looks like no count validation needs to be done

        }

        /// <summary>
        /// Adds any ends that need to be infered
        /// </summary>
        private void InferEnds()
        {
            Debug.Assert( Relationship != null );

            foreach ( IRelationshipEnd relationshipEnd in Relationship.Ends )
            {
                if ( ! HasEnd( relationshipEnd.Name ) )
                {
                    EntityContainerEntitySet entitySet = InferEntitySet(relationshipEnd);
                    if (entitySet != null)
                    {
                        // we don't have this end, we need to add it
                        AddEnd(relationshipEnd, entitySet);
                    }
                }
            }
        }


        /// <summary>
        /// For the given relationship end, find the EntityContainer Property that will work for the extent
        /// </summary>
        /// <param name="relationshipEnd">The relationship end of the RelationshipSet that needs and extent</param>
        /// <returns>Null is none could be found, or the EntityContainerProperty that is the valid extent</returns>
        private EntityContainerEntitySet InferEntitySet( IRelationshipEnd relationshipEnd )
        {
            Debug.Assert(relationshipEnd != null, "relationshipEnd parameter is null");

            List<EntityContainerEntitySet> possibleExtents = new List<EntityContainerEntitySet>();
            foreach ( EntityContainerEntitySet set in ParentElement.EntitySets )
            {
                if ( relationshipEnd.Type.IsOfType( set.EntityType ) )
                {
                    possibleExtents.Add( set );
                }
            }

            if ( possibleExtents.Count == 1 )
            {
                return possibleExtents[0];
            }
            else if ( possibleExtents.Count == 0 )
            {
                // no matchs
                AddError( ErrorCode.MissingExtentEntityContainerEnd, EdmSchemaErrorSeverity.Error,
                    System.Data.Entity.Strings.MissingEntityContainerEnd(relationshipEnd.Name, FQName ) );
            }
            else
            {
                // abmigous
                AddError( ErrorCode.AmbiguousEntityContainerEnd, EdmSchemaErrorSeverity.Error,
                    System.Data.Entity.Strings.AmbiguousEntityContainerEnd(relationshipEnd.Name, FQName ) );
            }

            return null;
        }

        
        /// <summary>
        /// The parent element as an EntityContainer
        /// </summary>
        internal new EntityContainer ParentElement
        {
            get
            {
                return (EntityContainer)( base.ParentElement );
            }
        }
    }
}
