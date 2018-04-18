//---------------------------------------------------------------------
// <copyright file="TextElement.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace System.Data.EntityModel.SchemaObjectModel
{
    /// <summary>
    /// Summary description for Documentation.
    /// </summary>
    internal sealed class TextElement : SchemaElement
    {
        #region Instance Fields
        private string _value = null;
        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentElement"></param>
        public TextElement(SchemaElement parentElement)
        : base(parentElement)
        {
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// 
        /// </summary>
        public string Value
        {
            get
            {
                return _value;
            }
            private set
            {
                _value = value;
            }
        }
        #endregion

        #region Protected Properties
        protected override bool HandleText(XmlReader reader)
        {
            TextElementTextHandler(reader);
            return true;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        private void TextElementTextHandler(XmlReader reader)
        {
            string text = reader.Value;
            if ( string.IsNullOrEmpty(text) )
                return;

            if ( string.IsNullOrEmpty(Value) )
                Value = text;
            else
                Value += text;
        }
        #endregion
    }
}
