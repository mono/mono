//---------------------------------------------------------------------
// <copyright file="Item.cs" company="Microsoft">
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
    using System.Xml;

    /// <summary>
    /// Summary description for Item.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Name={Name}, BaseType={BaseType.FQName}, HasKeys={HasKeys}")]
    internal sealed class SchemaEntityType : StructuredType
    {
        #region Private Fields
        
        private const char KEY_DELIMITER = ' ';
        private ISchemaElementLookUpTable<NavigationProperty> _navigationProperties = null;
        private EntityKeyElement _keyElement = null;
        private static List<PropertyRefElement> EmptyKeyProperties = new List<PropertyRefElement>(0);
        #endregion
        #region Public Methods
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentElement"></param>
        public SchemaEntityType(Schema parentElement)
            : base(parentElement)
        {
            if (Schema.DataModel == SchemaDataModelOption.EntityDataModel)
                OtherContent.Add(Schema.SchemaSource);
        }
        
        #endregion
        #region Protected Methods

        /// <summary>
        /// 
        /// </summary>
        internal override void ResolveTopLevelNames()
        {
            base.ResolveTopLevelNames();

            if (BaseType != null)
            {
                if (!(BaseType is SchemaEntityType))
                {
                    AddError(ErrorCode.InvalidBaseType, EdmSchemaErrorSeverity.Error,
                        System.Data.Entity.Strings.InvalidBaseTypeForItemType(BaseType.FQName, FQName));
                }
                // Since the base type is not null, key must be defined on the base type
                else if (_keyElement != null && BaseType != null)
                {
                    AddError(ErrorCode.InvalidKey, EdmSchemaErrorSeverity.Error,
                        System.Data.Entity.Strings.InvalidKeyKeyDefinedInBaseClass(FQName, BaseType.FQName));
                }
            }
            // If the base type is not null, then the key must be defined on the base entity type, since
            // we don't allow entity type without keys. 
            else if (_keyElement == null)
            {
                AddError(ErrorCode.KeyMissingOnEntityType, EdmSchemaErrorSeverity.Error,
                    System.Data.Entity.Strings.KeyMissingOnEntityType(this.FQName));
            }
            else if (null == BaseType && null != UnresolvedBaseType)
            {
                // this is already an error situation, we won't do any resolve name further in this type
                return;
            }
            else
            {
                _keyElement.ResolveTopLevelNames();
            }
        }

        #endregion
        #region Protected Properties

        protected override bool HandleAttribute(XmlReader reader)
        {
            if (base.HandleAttribute(reader))
            {
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.OpenType) && Schema.DataModel == SchemaDataModelOption.EntityDataModel)
            {
                // EF does not support this EDM 3.0 attribute, so ignore it.
                return true;
            }

            return false;
        }

        #endregion
        #region Private Methods

        #endregion
        #region Public Properties

        public EntityKeyElement KeyElement
        {
            get
            {
                return _keyElement;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IList<PropertyRefElement> DeclaredKeyProperties
        {
            get
            {
                if (KeyElement == null)
                {
                    return EmptyKeyProperties;
                }
                return KeyElement.KeyProperties;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public IList<PropertyRefElement> KeyProperties
        {
            get
            {
                if (KeyElement == null)
                {
                    if (BaseType != null)
                    {
                        System.Diagnostics.Debug.Assert(BaseType is SchemaEntityType, "ItemType.BaseType is not ItemType");
                        return (BaseType as SchemaEntityType).KeyProperties;
                    }

                    return EmptyKeyProperties;
                }
                return this.KeyElement.KeyProperties;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ISchemaElementLookUpTable<NavigationProperty> NavigationProperties
        {
            get
            {
                if (_navigationProperties == null)
                {
                    _navigationProperties = new FilteredSchemaElementLookUpTable<NavigationProperty, SchemaElement>(NamedMembers);
                }
                return _navigationProperties;
            }
        }
        
        #endregion
        #region Protected Methods

        /// <summary>
        /// 
        /// </summary>
        internal override void Validate()
        {
            // structured type base class will validate all members (properties, nav props, etc)
            base.Validate();

            if (this.KeyElement != null)
            {
                this.KeyElement.Validate();
            }
        }

        #endregion
        #region Protected Properties

        protected override bool HandleElement(XmlReader reader)
        {
            if (base.HandleElement(reader))
            {
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.Key))
            {
                HandleKeyElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.NavigationProperty))
            {
                HandleNavigationPropertyElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.ValueAnnotation) && Schema.DataModel == SchemaDataModelOption.EntityDataModel)
            {
                // EF does not support this EDM 3.0 element, so ignore it.
                SkipElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.TypeAnnotation) && Schema.DataModel == SchemaDataModelOption.EntityDataModel)
            {
                // EF does not support this EDM 3.0 element, so ignore it.
                SkipElement(reader);
                return true;
            }
            return false;
        }

        #endregion
        #region Private Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        private void HandleNavigationPropertyElement(XmlReader reader)
        {
            NavigationProperty navigationProperty = new NavigationProperty(this);
            navigationProperty.Parse(reader);
            AddMember(navigationProperty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        private void HandleKeyElement(XmlReader reader)
        {
            _keyElement = new EntityKeyElement(this);
            _keyElement.Parse(reader);
        }
        #endregion
    }
}
