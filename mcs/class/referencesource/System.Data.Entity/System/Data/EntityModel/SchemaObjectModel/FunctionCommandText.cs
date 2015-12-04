//---------------------------------------------------------------------
// <copyright file="FunctionCommandText.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.EntityModel.SchemaObjectModel
{
    using System;
    using System.Data.Metadata.Edm;
    using System.Xml;

    /// <summary>
    /// Represents an CommandText element.
    /// </summary>
    internal sealed class FunctionCommandText : SchemaElement
    {
        private string _commandText;

        /// <summary>
        /// Constructs an FunctionCommandText
        /// </summary>
        /// <param name="parentElement">Reference to the schema element.</param>
        public FunctionCommandText(Function parentElement)
            : base(parentElement)
        {
        }

        public string CommandText
        {
            get { return _commandText; }
        }

        protected override bool HandleText(XmlReader reader)
        {
            _commandText = reader.Value;
            return true;
        }

        internal override void Validate()
        {
            base.Validate();

            if (String.IsNullOrEmpty(_commandText))
            {
                AddError(ErrorCode.EmptyCommandText, EdmSchemaErrorSeverity.Error,
                    System.Data.Entity.Strings.EmptyCommandText);
            }
        }
    }
}
