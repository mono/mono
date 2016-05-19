//---------------------------------------------------------------------
// <copyright file="EntityContainerEntitySet.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.EntityModel.SchemaObjectModel
{
    using System.Data.Entity;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Xml;
    
    /// <summary>
    /// Represents an EntitySet element.
    /// </summary>
    internal sealed class EntityContainerEntitySet : SchemaElement
    {
        private SchemaEntityType _entityType = null;
        private string _unresolvedEntityTypeName = null;
        private string _schema = null;
        private string _table = null;
        private EntityContainerEntitySetDefiningQuery _definingQueryElement = null;

      
        /// <summary>
        /// Constructs an EntityContainerEntitySet
        /// </summary>
        /// <param name="parentElement">Reference to the schema element.</param>
        public EntityContainerEntitySet( EntityContainer parentElement )
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

        public SchemaEntityType EntityType
        {
            get
            {
                return _entityType;
            }
        }

        public string DbSchema
        {
            get
            {
                return _schema;
            }
        }

        public string Table
        {
            get
            {
                return _table;
            }
        }

        public string DefiningQuery
        {
            get 
            {
                if (_definingQueryElement != null)
                {
                    return _definingQueryElement.Query;
                }
                return null;
            }
        }

        protected override bool HandleElement(XmlReader reader)
        {
            if (base.HandleElement(reader))
            {
                return true;
            }
            else if (Schema.DataModel == SchemaDataModelOption.ProviderDataModel)
            {
                if (CanHandleElement(reader, XmlConstants.DefiningQuery))
                {
                    HandleDefiningQueryElement(reader);
                    return true;
                }
            }
            else if (Schema.DataModel == SchemaDataModelOption.EntityDataModel)
            {
                if (CanHandleElement(reader, XmlConstants.ValueAnnotation))
                {
                    // EF does not support this EDM 3.0 element, so ignore it.
                    SkipElement(reader);
                    return true;
                }
                else if (CanHandleElement(reader, XmlConstants.TypeAnnotation))
                {
                    // EF does not support this EDM 3.0 element, so ignore it.
                    SkipElement(reader);
                    return true;
                }
            }
            return false;
        }

        protected override bool HandleAttribute(XmlReader reader)
        {
            if (base.HandleAttribute(reader))
            {
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.EntityType))
            {
                HandleEntityTypeAttribute(reader);
                return true;
            }
            if (Schema.DataModel == SchemaDataModelOption.ProviderDataModel)
            {
                if (CanHandleAttribute(reader, XmlConstants.Schema))
                {
                    HandleDbSchemaAttribute(reader);
                    return true;
                }
                else if (CanHandleAttribute(reader, XmlConstants.Table))
                {
                    HandleTableAttribute(reader);
                    return true;
                }
            }
            return false;
        }

        private void HandleDefiningQueryElement(XmlReader reader)
        {
            Debug.Assert(reader != null);

            EntityContainerEntitySetDefiningQuery query = new EntityContainerEntitySetDefiningQuery(this);
            query.Parse(reader);
            _definingQueryElement = query;
        }

        protected override void HandleNameAttribute(XmlReader reader)
        {
            if (Schema.DataModel == SchemaDataModelOption.ProviderDataModel)
            {
                // ssdl will take anything, because this is the table name, and we
                // can't predict what the vendor will need in a table name
                Name = reader.Value;
            }
            else
            {
                base.HandleNameAttribute(reader);
            }
        }
        /// <summary>
        /// The method that is called when a Type attribute is encountered.
        /// </summary>
        /// <param name="reader">An XmlReader positioned at the Type attribute.</param>
        private void HandleEntityTypeAttribute( XmlReader reader )
        {
            Debug.Assert( reader != null );

            ReturnValue<string> value = HandleDottedNameAttribute( reader, _unresolvedEntityTypeName, Strings.PropertyTypeAlreadyDefined );
            if ( value.Succeeded )
            {
                _unresolvedEntityTypeName = value.Value;
            }
        }

        /// <summary>
        /// The method that is called when a DbSchema attribute is encountered.
        /// </summary>
        /// <param name="reader">An XmlReader positioned at the Type attribute.</param>
        private void HandleDbSchemaAttribute( XmlReader reader )
        {
            Debug.Assert(Schema.DataModel == SchemaDataModelOption.ProviderDataModel, "We shouldn't see this attribute unless we are parsing ssdl");
            Debug.Assert( reader != null );

             _schema = reader.Value;
        }

        /// <summary>
        /// The method that is called when a DbTable attribute is encountered.
        /// </summary>
        /// <param name="reader">An XmlReader positioned at the Type attribute.</param>
        private void HandleTableAttribute( XmlReader reader )
        {
            Debug.Assert(Schema.DataModel == SchemaDataModelOption.ProviderDataModel, "We shouldn't see this attribute unless we are parsing ssdl");
            Debug.Assert( reader != null );

             _table = reader.Value;
        }
        
        
        /// <summary>
        /// Used during the resolve phase to resolve the type name to the object that represents that type
        /// </summary>
        internal override void ResolveTopLevelNames()
        {
            base.ResolveTopLevelNames();

            if ( _entityType == null )
            {
                SchemaType type = null;
                if ( ! Schema.ResolveTypeName( this, _unresolvedEntityTypeName, out type) )
                {
                    return;
                }

                _entityType = type as SchemaEntityType;
                if ( _entityType == null )
                {
                    AddError( ErrorCode.InvalidPropertyType, EdmSchemaErrorSeverity.Error,
                        System.Data.Entity.Strings.InvalidEntitySetType(_unresolvedEntityTypeName ) );
                    return;
                }
            }
        }

        internal override void Validate()
        {
            base.Validate();

            if (_entityType.KeyProperties.Count == 0)
            {
                AddError(ErrorCode.EntitySetTypeHasNoKeys, EdmSchemaErrorSeverity.Error,
                    System.Data.Entity.Strings.EntitySetTypeHasNoKeys(Name, _entityType.FQName));
            }

            if (_definingQueryElement != null)
            {
                _definingQueryElement.Validate();

                if (DbSchema != null || Table != null)
                {
                    AddError(ErrorCode.TableAndSchemaAreMutuallyExclusiveWithDefiningQuery, EdmSchemaErrorSeverity.Error,
                        System.Data.Entity.Strings.TableAndSchemaAreMutuallyExclusiveWithDefiningQuery(FQName));
                }
            }
        }

        internal override SchemaElement Clone(SchemaElement parentElement)
        {
            EntityContainerEntitySet entitySet = new EntityContainerEntitySet((EntityContainer)parentElement);
            entitySet._definingQueryElement = this._definingQueryElement;
            entitySet._entityType = this._entityType;
            entitySet._schema = this._schema;
            entitySet._table = this._table;
            entitySet.Name = this.Name;

            return entitySet;
        }
   }
}
