//---------------------------------------------------------------------
// <copyright file="OnOperation.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.EntityModel.SchemaObjectModel
{
    using System.Data.Metadata.Edm;
    using System.Data.Objects.DataClasses;
    using System.Diagnostics;
    using System.Xml;

    /// <summary>
    /// Represents an OnDelete, OnCopy, OnSecure, OnLock or OnSerialize element
    /// </summary>
    internal sealed class OnOperation : SchemaElement
    {
        private Operation _Operation;
        private Action _Action;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentElement"></param>
        /// <param name="operation"></param>
        public OnOperation(RelationshipEnd parentElement, Operation operation)
        : base(parentElement)
        {
            Operation = operation;
        }

        /// <summary>
        /// The operation
        /// </summary>
        public Operation Operation
        {
            get
            {
                return _Operation;
            }
            private set
            {
                _Operation = value;
            }
        }

        /// <summary>
        /// The action
        /// </summary>
        public Action Action
        {
            get
            {
                return _Action;
            }
            private set
            {
                _Action = value;
            }
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
            else if (CanHandleAttribute(reader, XmlConstants.Action))
            {
                HandleActionAttribute(reader);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handle the Action attribute
        /// </summary>
        /// <param name="reader">reader positioned at Action attribute</param>
        private void HandleActionAttribute(XmlReader reader)
        {
            Debug.Assert(reader != null);

            RelationshipKind relationshipKind = ParentElement.ParentElement.RelationshipKind;

            switch ( reader.Value.Trim() )
            {
                case "None":
                    Action = Action.None;
                    break;
                case "Cascade":
                    Action = Action.Cascade;
                    break;
                default:
                    AddError( ErrorCode.InvalidAction, EdmSchemaErrorSeverity.Error, reader, System.Data.Entity.Strings.InvalidAction(reader.Value, ParentElement.FQName ) );
                    break;
            }
        }

        /// <summary>
        /// the parent element.
            /// </summary>
        private new RelationshipEnd ParentElement
        {
            get
            {
                return (RelationshipEnd)base.ParentElement;
            }
        }
    }
}
