//---------------------------------------------------------------------
// <copyright file="Documentation.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.EntityModel.SchemaObjectModel
{
    using System.Data.Common.Utils;
    using System.Data.Metadata.Edm;
    using System.Xml;

    /// <summary>
    /// Summary description for Documentation.
    /// </summary>
    internal sealed class DocumentationElement: SchemaElement
    {
        #region Instance Fields
        Documentation _metdataDocumentation = new Documentation();
        #endregion


        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentElement"></param>
        public DocumentationElement(SchemaElement parentElement)
        :   base(parentElement)
        {
        }
        #endregion

        #region Public Properties

        /// <summary>
        /// Returns the wrapped metaDocumentation instance
        /// </summary>
        public Documentation MetadataDocumentation
        {
            get
            {
                _metdataDocumentation.SetReadOnly();
                return _metdataDocumentation;
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
            else if (CanHandleElement(reader, XmlConstants.Summary))
            {
                HandleSummaryElement(reader);
                return true;
            }
            else if (CanHandleElement(reader, XmlConstants.LongDescription))
            {
                HandleLongDescriptionElement(reader);
                return true;
            }
            return false;
        }
        #endregion

        #region Private Methods

        protected override bool HandleText(XmlReader reader)
        {
            string text = reader.Value;
            if (!StringUtil.IsNullOrEmptyOrWhiteSpace(text))
            {
                AddError(ErrorCode.UnexpectedXmlElement, EdmSchemaErrorSeverity.Error, System.Data.Entity.Strings.InvalidDocumentationBothTextAndStructure);
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        private void HandleSummaryElement(XmlReader reader)
        {
            TextElement text = new TextElement(this);

            text.Parse(reader);

            _metdataDocumentation.Summary = text.Value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        private void HandleLongDescriptionElement(XmlReader reader)
        {

            TextElement text = new TextElement(this);

            text.Parse(reader);

            _metdataDocumentation.LongDescription = text.Value;
        }
        #endregion
    }
}
