//---------------------------------------------------------------------
// <copyright file="EntityContainerRelationshipSetEnd.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.EntityModel.SchemaObjectModel
{
    using System.Data.Metadata.Edm;
    using System.Xml;

    /// <summary>
    /// Represents an RelationshipSetEnd element.
    /// </summary>
    internal class EntityContainerRelationshipSetEnd : SchemaElement
    {
        private IRelationshipEnd _relationshipEnd;
        private string _unresolvedEntitySetName;
        private EntityContainerEntitySet _entitySet;

        /// <summary>
        /// Constructs an EntityContainerRelationshipSetEnd
        /// </summary>
        /// <param name="parentElement">Reference to the schema element.</param>
        public EntityContainerRelationshipSetEnd( EntityContainerRelationshipSet parentElement )
            : base( parentElement )
        {
        }


        /// <summary>
        /// the End in the parent’s Association that this element refers to
        /// </summary>
        public IRelationshipEnd RelationshipEnd
        {
            get { return _relationshipEnd; }
            internal set { _relationshipEnd = value; }
        }

        public EntityContainerEntitySet EntitySet
        {
            get { return _entitySet; }
            internal set { _entitySet = value; }
        }

        protected override bool ProhibitAttribute(string namespaceUri, string localName)
        {
            if (base.ProhibitAttribute(namespaceUri, localName))
            {
                return true;
            }

            if (namespaceUri == null && localName == XmlConstants.Name)
            {
                return false;
            }
            return false;
        }

        protected override bool HandleAttribute(XmlReader reader)
        {
            if (base.HandleAttribute(reader))
            {
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.EntitySet))
            {
                HandleEntitySetAttribute(reader);
                return true;
            }

            return false;
        }

        /// <summary>
        /// This is the method that is called when an EntitySet Attribute is encountered.
        /// </summary>
        /// <param name="reader">The XmlRead positned at the extent attribute.</param>
        private void HandleEntitySetAttribute( XmlReader reader )
        {
            if (Schema.DataModel == SchemaDataModelOption.ProviderDataModel)
            {
                // ssdl will take anything, because this is the table name, and we
                // can't predict what the vendor will need in a table name
                _unresolvedEntitySetName = reader.Value;
            }
            else
            {
                _unresolvedEntitySetName = HandleUndottedNameAttribute(reader, _unresolvedEntitySetName);
            }
        }

        /// <summary>
        /// Used during the resolve phase to resolve the type name to the object that represents that type
        /// </summary>
        internal override void ResolveTopLevelNames()
        {
            base.ResolveTopLevelNames();

            if ( _entitySet == null )
            {
                _entitySet = this.ParentElement.ParentElement.FindEntitySet( _unresolvedEntitySetName );
                if ( _entitySet == null )
                {
                    AddError( ErrorCode.InvalidEndEntitySet, EdmSchemaErrorSeverity.Error,
                        System.Data.Entity.Strings.InvalidEntitySetNameReference(_unresolvedEntitySetName, Name ) );
                }
            }
        }

        /// <summary>
        /// Do all validation for this element here, and delegate to all sub elements
        /// </summary>
        internal override void Validate()
        {
            base.Validate();

            if ( _relationshipEnd == null || _entitySet == null )
            {
                return;
            }

            // We need to allow 2 kind of scenarios:
            // 1> If you have a relationship type defined between Customer and Order, then you can have a association set in 
            //    which the Customer end refers to a Entity Set of type GoodCustomer where GoodCustomer type derives from Customer
            // 2> If you have a relationship type defined between GoodCustomer and Order, then you can have a relationship
            //    set which GoodCustomer end refers to an entity set whose entity type is Customer (where GoodCustomer derives
            //    from Customer). This scenario enables us to support scenarios where you want specific types in an entity set
            //    to take part in a relationship.
            if ( !_relationshipEnd.Type.IsOfType( _entitySet.EntityType ) &&
                 !_entitySet.EntityType.IsOfType( _relationshipEnd.Type ))
            {
                AddError( ErrorCode.InvalidEndEntitySet, EdmSchemaErrorSeverity.Error,
                    System.Data.Entity.Strings.InvalidEndEntitySetTypeMismatch(_relationshipEnd.Name ) );
            }
        }

        /// <summary>
        /// The parent element as an EntityContainerProperty
        /// </summary>
        internal new EntityContainerRelationshipSet ParentElement
        {
            get
            {
                return (EntityContainerRelationshipSet)( base.ParentElement );
            }
        }
    }
}
