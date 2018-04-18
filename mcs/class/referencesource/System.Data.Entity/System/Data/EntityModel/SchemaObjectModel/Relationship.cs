//---------------------------------------------------------------------
// <copyright file="Relationship.cs" company="Microsoft">
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
    using System.Data.Objects.DataClasses;
    using System.Diagnostics;
    using System.Xml;

    /// <summary>
    /// Represents an Association element
    /// </summary>
    internal sealed class Relationship : SchemaType, IRelationship
    {
        private RelationshipKind _relationshipKind;
        private RelationshipEndCollection _ends;
        private List<ReferentialConstraint> _constraints;
        private bool _isForeignKey;

        /// <summary>
        /// Construct a Relationship object
        /// </summary>
        /// <param name="parent">the parent</param>
        /// <param name="kind">the kind of relationship</param>
        public Relationship(Schema parent, RelationshipKind kind)
        : base(parent)
        {
            RelationshipKind = kind;

            if (Schema.DataModel == SchemaDataModelOption.EntityDataModel)
            {
                _isForeignKey = false;
                OtherContent.Add(Schema.SchemaSource);
            }
            else if (Schema.DataModel == SchemaDataModelOption.ProviderDataModel)
            {
                _isForeignKey = true;
            }
        }


        /// <summary>
        /// List of Ends defined for this Association
        /// </summary>
        public IList<IRelationshipEnd> Ends
        {
            get
            {
                if ( _ends == null )
                    _ends = new RelationshipEndCollection();
                return _ends;
            }
        }

        /// <summary>
        /// Returns the list of constraints on this relation
        /// </summary>
        public IList<ReferentialConstraint> Constraints
        {
            get
            {
                if (_constraints == null)
                {
                    _constraints = new List<ReferentialConstraint>();
                }
                return _constraints;
            }
        }

        public bool TryGetEnd( string roleName, out IRelationshipEnd end )
        {
            return _ends.TryGetEnd( roleName, out end );
        }


        /// <summary>
        /// Is this an Association
        /// </summary>
        public RelationshipKind RelationshipKind
        {
            get
            {
                return _relationshipKind;
            }
            private set
            {
                _relationshipKind = value;
            }
        }

        /// <summary>
        /// Is this a foreign key (aka foreign key) relationship?
        /// </summary>
        public bool IsForeignKey
        {
            get { return _isForeignKey; }
        }

        /// <summary>
        /// do whole element validation
        /// </summary>
        /// <returns></returns>
        internal override void Validate()
        {
            base.Validate();

            bool foundOperations = false;
            foreach ( RelationshipEnd end in Ends )
            {
                end.Validate();
                if ( RelationshipKind == RelationshipKind.Association )
                {
                    if ( end.Operations.Count > 0 )
                    {
                        if ( foundOperations )
                            end.AddError( ErrorCode.InvalidOperation, EdmSchemaErrorSeverity.Error, System.Data.Entity.Strings.InvalidOperationMultipleEndsInAssociation);
                        foundOperations = true;
                    }
                }
            }

            if (Constraints.Count == 0)
            {
                if (this.Schema.DataModel == SchemaDataModelOption.ProviderDataModel)
                {
                    AddError(ErrorCode.MissingConstraintOnRelationshipType,
                             EdmSchemaErrorSeverity.Error,
                             System.Data.Entity.Strings.MissingConstraintOnRelationshipType(FQName));
                }
            }
            else
            {
                foreach (ReferentialConstraint constraint in Constraints)
                {
                    constraint.Validate();
                }
            }
        }

        /// <summary>
        /// do whole element resolution
        /// </summary>
        internal override void ResolveTopLevelNames()
        {
            base.ResolveTopLevelNames();

            foreach ( RelationshipEnd end in Ends )
                end.ResolveTopLevelNames();

            foreach (ReferentialConstraint referentialConstraint in Constraints)
            {
                referentialConstraint.ResolveTopLevelNames();
            }
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
            else if (CanHandleElement(reader, XmlConstants.ReferentialConstraint))
            {
                HandleConstraintElement(reader);
                return true;
            }
            return false;
        }

        /// <summary>
        /// handle the End child element
        /// </summary>
        /// <param name="reader">XmlReader positioned at the end element</param>
        private void HandleEndElement(XmlReader reader)
        {
            Debug.Assert(reader != null);
            RelationshipEnd end = new RelationshipEnd(this);
            end.Parse(reader);

            if (Ends.Count == 2)
            {
                AddError( ErrorCode.InvalidAssociation, EdmSchemaErrorSeverity.Error, System.Data.Entity.Strings.TooManyAssociationEnds(FQName ) );
                return;
            }

            Ends.Add(end);
        }

        /// <summary>
        /// handle the constraint element
        /// </summary>
        /// <param name="reader">XmlReader positioned at the constraint element</param>
        private void HandleConstraintElement(XmlReader reader)
        {
            Debug.Assert(reader != null);

            ReferentialConstraint constraint = new ReferentialConstraint(this);
            constraint.Parse(reader);
            this.Constraints.Add(constraint);

            if (this.Schema.DataModel == SchemaDataModelOption.EntityDataModel && this.Schema.SchemaVersion >= XmlConstants.EdmVersionForV2)
            {
                // in V2, referential constraint implies foreign key
                _isForeignKey = true;
            }
        }

    }
}
