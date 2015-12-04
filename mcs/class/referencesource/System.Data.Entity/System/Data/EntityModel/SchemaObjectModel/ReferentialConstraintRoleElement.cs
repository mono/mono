//---------------------------------------------------------------------
// <copyright file="ReferentialConstraintRoleElement.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.EntityModel.SchemaObjectModel
{
    using System;
    using System.Collections.Generic;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Xml;

    /// <summary>
    /// Represents an role element in referential constraint element.
    /// </summary>
    internal sealed class ReferentialConstraintRoleElement : SchemaElement
    {
        private List<PropertyRefElement> _roleProperties;
        private IRelationshipEnd _end;

        /// <summary>
        /// Constructs an EntityContainerAssociationSetEnd
        /// </summary>
        /// <param name="parentElement">Reference to the schema element.</param>
        public ReferentialConstraintRoleElement(ReferentialConstraint parentElement)
            : base( parentElement )
        {
        }

        public IList<PropertyRefElement> RoleProperties
        {
            get
            {
                if (_roleProperties == null)
                {
                    _roleProperties = new List<PropertyRefElement>();
                }
                return _roleProperties;
            }
        }

        public IRelationshipEnd End
        {
            get
            {
                return _end;
            }
        }

        protected override bool HandleElement(XmlReader reader)
        {
            if (base.HandleElement(reader))
            {
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.PropertyRef))
            {
                HandlePropertyRefElement(reader);
                return true;
            }

            return false;
        }

        protected override bool HandleAttribute(XmlReader reader)
        {
            if (CanHandleAttribute(reader, XmlConstants.Role))
            {
                HandleRoleAttribute(reader);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        private void HandlePropertyRefElement(XmlReader reader)
        {
            PropertyRefElement property = new PropertyRefElement(ParentElement);
            property.Parse(reader);
            this.RoleProperties.Add(property);
        }

        private void HandleRoleAttribute(XmlReader reader)
        {
            string roleName;
            Utils.GetString(Schema, reader, out roleName);
            this.Name = roleName;
        }

        /// <summary>
        /// Used during the resolve phase to resolve the type name to the object that represents that type
        /// </summary>
        internal override void ResolveTopLevelNames()
        {
            Debug.Assert(!String.IsNullOrEmpty(this.Name), "RoleName should never be empty");
            IRelationship relationship = (IRelationship)this.ParentElement.ParentElement;

            if (!relationship.TryGetEnd(this.Name, out _end))
            {
                AddError(ErrorCode.InvalidRoleInRelationshipConstraint,
                         EdmSchemaErrorSeverity.Error,
                         System.Data.Entity.Strings.InvalidEndRoleInRelationshipConstraint(this.Name, relationship.Name));

                return;
            }

            // we are gauranteed that the _end has gone through ResolveNames, but 
            // we are not gauranteed that it was successful
            if (_end.Type == null)
            {
                // an error has already been added for this
                return;
            }

        }

        internal override void Validate()
        {
            base.Validate();
            // we can't resolve these names until validate because they will reference properties and types
            // that may not be resolved when this objects ResolveNames gets called
            Debug.Assert(_roleProperties != null, "xsd should have verified that there should be atleast one property ref element in referential role element");
            foreach (PropertyRefElement property in _roleProperties)
            {
                if (!property.ResolveNames((SchemaEntityType)_end.Type))
                {
                    AddError(ErrorCode.InvalidPropertyInRelationshipConstraint,
                            EdmSchemaErrorSeverity.Error,
                            System.Data.Entity.Strings.InvalidPropertyInRelationshipConstraint(
                                          property.Name,
                                          this.Name));
                }
            }
        }
    }
}
