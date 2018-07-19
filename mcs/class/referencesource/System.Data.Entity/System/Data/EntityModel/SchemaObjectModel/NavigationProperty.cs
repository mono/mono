//---------------------------------------------------------------------
// <copyright file="NavigationProperty.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.EntityModel.SchemaObjectModel
{
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;
    using System.Xml;

    /// <summary>
    /// Summary description for Association.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Name={Name}, Relationship={_unresolvedRelationshipName}, FromRole={_unresolvedFromEndRole}, ToRole={_unresolvedToEndRole}")]
    internal sealed class NavigationProperty : Property
    {
        private string _unresolvedFromEndRole = null;
        private string _unresolvedToEndRole = null;
        private string _unresolvedRelationshipName = null;
        private IRelationshipEnd _fromEnd = null;
        private IRelationshipEnd _toEnd = null;
        private IRelationship _relationship = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        public NavigationProperty(SchemaEntityType parent)
            : base(parent)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public new SchemaEntityType ParentElement
        {
            get
            {
                return base.ParentElement as SchemaEntityType;
            }
        }

        internal IRelationship Relationship
        {
            get { return _relationship; }
        }

        internal IRelationshipEnd ToEnd
        {
            get { return _toEnd; }
        }

        internal IRelationshipEnd FromEnd
        {
            get { return _fromEnd; }
        }

        /// <summary>
        /// Gets the Type of the property
        /// </summary>
        public override SchemaType Type
        {
            get
            {
                if (_toEnd == null || _toEnd.Type == null)
                {
                    return null;
                }

                return _toEnd.Type;
            }
        }

        protected override bool HandleAttribute(XmlReader reader)
        {
            if (base.HandleAttribute(reader))
            {
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.Relationship))
            {
                HandleAssociationAttribute(reader);
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.FromRole))
            {
                HandleFromRoleAttribute(reader);
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.ToRole))
            {
                HandleToRoleAttribute(reader);
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.ContainsTarget))
            {
                // EF does not support this EDM 3.0 attribute, so ignore it.
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        internal override void ResolveTopLevelNames()
        {
            base.ResolveTopLevelNames();

            SchemaType element;
            if (!Schema.ResolveTypeName(this, _unresolvedRelationshipName, out element))
                return;

            _relationship = element as IRelationship;
            if (_relationship == null)
            {
                AddError(ErrorCode.BadNavigationProperty, EdmSchemaErrorSeverity.Error,
                    System.Data.Entity.Strings.BadNavigationPropertyRelationshipNotRelationship(_unresolvedRelationshipName));
                return;
            }

            bool foundBothEnds = true;
            if (!_relationship.TryGetEnd(_unresolvedFromEndRole, out _fromEnd))
            {
                AddError(ErrorCode.BadNavigationProperty, EdmSchemaErrorSeverity.Error,
                    System.Data.Entity.Strings.BadNavigationPropertyUndefinedRole(_unresolvedFromEndRole, _relationship.FQName));
                foundBothEnds = false;
            }

            if (!_relationship.TryGetEnd(_unresolvedToEndRole, out _toEnd))
            {
                AddError(ErrorCode.BadNavigationProperty, EdmSchemaErrorSeverity.Error,
                    System.Data.Entity.Strings.BadNavigationPropertyUndefinedRole(_unresolvedToEndRole, _relationship.FQName));

                foundBothEnds = false;
            }

            if (foundBothEnds && _fromEnd == _toEnd)
            {
                AddError(ErrorCode.BadNavigationProperty, EdmSchemaErrorSeverity.Error,
                    System.Data.Entity.Strings.BadNavigationPropertyRolesCannotBeTheSame);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        internal override void Validate()
        {
            base.Validate();

            System.Diagnostics.Debug.Assert(_fromEnd != null && _toEnd != null, 
                "FromEnd and ToEnd must not be null in Validate. ResolveNames must have resolved it or added error");

            if (_fromEnd.Type != ParentElement)
            {
                AddError(ErrorCode.BadNavigationProperty, EdmSchemaErrorSeverity.Error,
                    System.Data.Entity.Strings.BadNavigationPropertyBadFromRoleType(this.Name,
                    _fromEnd.Type.FQName, _fromEnd.Name, _relationship.FQName, ParentElement.FQName));
            }

            StructuredType type = _toEnd.Type;
        }

        #region Private Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        private void HandleToRoleAttribute(XmlReader reader)
        {
            _unresolvedToEndRole = HandleUndottedNameAttribute(reader, _unresolvedToEndRole);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        private void HandleFromRoleAttribute(XmlReader reader)
        {
            _unresolvedFromEndRole = HandleUndottedNameAttribute(reader, _unresolvedFromEndRole);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        private void HandleAssociationAttribute(XmlReader reader)
        {
            Debug.Assert(_unresolvedRelationshipName == null, string.Format(CultureInfo.CurrentCulture, "{0} is already defined", reader.Name));

            string association;
            if (!Utils.GetDottedName(this.Schema, reader, out association))
                return;

            _unresolvedRelationshipName = association;
        }

        #endregion
    }
}
