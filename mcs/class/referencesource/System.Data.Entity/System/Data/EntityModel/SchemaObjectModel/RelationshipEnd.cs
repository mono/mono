//---------------------------------------------------------------------
// <copyright file="RelationshipEnd.cs" company="Microsoft">
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
    using System.Diagnostics;
    using System.Xml;

    /// <summary>
    /// Represents an End element in a relationship
    /// </summary>
    internal sealed class RelationshipEnd : SchemaElement, IRelationshipEnd
    {
        private string _unresolvedType;
        private RelationshipMultiplicity? _multiplicity;
        private SchemaEntityType _type;
        private List<OnOperation> _operations;

        /// <summary>
        /// construct a Relationship End
        /// </summary>
        /// <param name="relationship"></param>
        public RelationshipEnd(Relationship relationship)
            : base(relationship)
        {
        }

        /// <summary>
        /// Type of the End
        /// </summary>
        public SchemaEntityType Type
        {
            get
            {
                return _type;
            }
            private set
            {
                _type = value;
            }
        }

        /// <summary>
        /// Multiplicity of the End
        /// </summary>
        public RelationshipMultiplicity? Multiplicity
        {
            get
            {
                return _multiplicity;
            }
            set
            {
                _multiplicity = value;
            }
        }

        /// <summary>
        /// The On&lt;Operation&gt;s defined for the End
        /// </summary>
        public ICollection<OnOperation> Operations
        {
            get
            {
                if (_operations == null)
                    _operations = new List<OnOperation>();
                return _operations;
            }
        }

        /// <summary>
        /// do whole element resolution
        /// </summary>
        internal override void ResolveTopLevelNames()
        {
            base.ResolveTopLevelNames();

            if (Type == null && _unresolvedType != null)
            {
                SchemaType element;
                if (!Schema.ResolveTypeName(this, _unresolvedType, out element))
                {
                    return;
                }

                Type = element as SchemaEntityType;
                if (Type == null)
                {
                    AddError(ErrorCode.InvalidRelationshipEndType, EdmSchemaErrorSeverity.Error,
                        System.Data.Entity.Strings.InvalidRelationshipEndType(ParentElement.Name, element.FQName));
                }
            }
        }

        internal override void Validate()
        {
            base.Validate();

            // Check if the end has multiplicity as many, it cannot have any operation behaviour
            if (Multiplicity == RelationshipMultiplicity.Many && Operations.Count != 0)
            {
                AddError(ErrorCode.EndWithManyMultiplicityCannotHaveOperationsSpecified,
                         EdmSchemaErrorSeverity.Error,
                         System.Data.Entity.Strings.EndWithManyMultiplicityCannotHaveOperationsSpecified(this.Name, ParentElement.FQName));

                
                
            }
            
            // if there is no RefConstraint in Association and multiplicity is null
            if (this.ParentElement.Constraints.Count == 0 && Multiplicity == null)
            {
                AddError(ErrorCode.EndWithoutMultiplicity,
                         EdmSchemaErrorSeverity.Error,
                         System.Data.Entity.Strings.EndWithoutMultiplicity(this.Name, ParentElement.FQName));
            }
        }

        /// <summary>
        /// Do simple validation across attributes
        /// </summary>
        protected override void HandleAttributesComplete()
        {
            // set up the default name in before validating anythig that might want to display it in an error message;
            if (Name == null && _unresolvedType != null)
                Name = Utils.ExtractTypeName(Schema.DataModel, _unresolvedType);

            base.HandleAttributesComplete();
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
            else if (CanHandleAttribute(reader, XmlConstants.Multiplicity))
            {
                HandleMultiplicityAttribute(reader);
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.Role))
            {
                HandleNameAttribute(reader);
                return true;
            }
            else if (CanHandleAttribute(reader, XmlConstants.TypeElement))
            {
                HandleTypeAttribute(reader);
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
            else if (CanHandleElement(reader, XmlConstants.OnDelete))
            {
                HandleOnDeleteElement(reader);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handle the Type attribute
        /// </summary>
        /// <param name="reader">reader positioned at Type attribute</param>
        private void HandleTypeAttribute(XmlReader reader)
        {
            Debug.Assert(reader != null);

            string type;
            if (!Utils.GetDottedName(this.Schema, reader, out type))
                return;

            _unresolvedType = type;
        }

        /// <summary>
        /// Handle the Multiplicity attribute
        /// </summary>
        /// <param name="reader">reader positioned at Type attribute</param>
        private void HandleMultiplicityAttribute(XmlReader reader)
        {
            Debug.Assert(reader != null);
            RelationshipMultiplicity multiplicity;
            if (!TryParseMultiplicity(reader.Value, out multiplicity))
            {
                AddError(ErrorCode.InvalidMultiplicity, EdmSchemaErrorSeverity.Error, reader, System.Data.Entity.Strings.InvalidRelationshipEndMultiplicity(ParentElement.Name, reader.Value));
            }
            _multiplicity = multiplicity;
        }

        /// <summary>
        /// Handle an OnDelete element
        /// </summary>
        /// <param name="reader">reader positioned at the element</param>
        private void HandleOnDeleteElement(XmlReader reader)
        {
            HandleOnOperationElement(reader, Operation.Delete);
        }

        /// <summary>
        /// Handle an On&lt;Operation&gt; element
        /// </summary>
        /// <param name="reader">reader positioned at the element</param>
        /// <param name="operation">the kind of operation being handled</param>
        private void HandleOnOperationElement(XmlReader reader, Operation operation)
        {
            Debug.Assert(reader != null);

            foreach (OnOperation other in Operations)
            {
                if (other.Operation == operation)
                    AddError(ErrorCode.InvalidOperation, EdmSchemaErrorSeverity.Error, reader, System.Data.Entity.Strings.DuplicationOperation(reader.Name));
            }

            OnOperation onOperation = new OnOperation(this, operation);
            onOperation.Parse(reader);
            _operations.Add(onOperation);
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

        /// <summary>
        /// Create a new Multiplicity object from a string
        /// </summary>
        /// <param name="value">string containing Multiplicity definition</param>
        /// <param name="multiplicity">new multiplicity object (null if there were errors)</param>
        /// <returns>try if the string was parsable, false otherwise</returns>
        private static bool TryParseMultiplicity(string value, out RelationshipMultiplicity multiplicity)
        {
            switch (value)
            {
                case "0..1":
                    multiplicity = RelationshipMultiplicity.ZeroOrOne;
                    return true;
                case "1":
                    multiplicity = RelationshipMultiplicity.One;
                    return true;
                case "*":
                    multiplicity = RelationshipMultiplicity.Many;
                    return true;
                default:
                    multiplicity = (RelationshipMultiplicity)(- 1);
                    return false;
            }
        }
    }
}
