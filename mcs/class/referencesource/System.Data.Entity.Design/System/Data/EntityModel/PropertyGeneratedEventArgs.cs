//---------------------------------------------------------------------
// <copyright file="PropertyGeneratedEventArgs.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System;
using System.Data;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Entity.Design
{
    /// <summary>
    /// This class encapsulates the EventArgs dispatched as part of the event
    /// raised when a property is generated.
    /// </summary>
    public sealed class PropertyGeneratedEventArgs : EventArgs
    {
        #region Private Data

        private MetadataItem _propertySource;
        private string _backingFieldName;
        private CodeTypeReference _returnType;
        private List<CodeStatement> _additionalGetStatements = new List<CodeStatement>();
        private List<CodeStatement> _additionalSetStatements = new List<CodeStatement>();
        private List<CodeAttributeDeclaration> _additionalAttributes = new List<CodeAttributeDeclaration>();

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public PropertyGeneratedEventArgs()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="propertySource">The event source</param>
        /// <param name="backingFieldName">The name of the field corresponding to the property</param>
        /// <param name="returnType">The property return type</param>
        public PropertyGeneratedEventArgs(MetadataItem propertySource, 
                                          string backingFieldName,
                                          CodeTypeReference returnType)
        {
            this._propertySource = propertySource;
            this._backingFieldName = backingFieldName;
            this._returnType = returnType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The Metadata object that is the source of the property
        /// </summary>
        public MetadataItem PropertySource
        {
            get
            {
                return this._propertySource;
            }
        }

        /// <summary>
        /// The name of the field that backs the property; can be null in the case of
        /// navigation property
        /// </summary>
        public string BackingFieldName
        {
            get
            {
                return this._backingFieldName;
            }
        }

        /// <summary>
        /// The type of the property by default; if changed by the user, the new value
        /// will be used by the code generator
        /// </summary>
        public CodeTypeReference ReturnType
        {
            get
            {
                return this._returnType;
            }
            set
            {
                this._returnType = value;
            }
        }

        /// <summary>
        /// Statements to be included in the property's getter
        /// </summary>
        public List<CodeStatement> AdditionalGetStatements
        {
            get
            {
                return this._additionalGetStatements;
            }
        }

        /// <summary>
        /// Statements to be included in the property's setter
        /// </summary>
        public List<CodeStatement> AdditionalSetStatements
        {
            get
            {
                return _additionalSetStatements;
            }
        }

        /// <summary>
        /// Attributes to be added to the property's CustomAttributes collection
        /// </summary>
        public List<CodeAttributeDeclaration> AdditionalAttributes
        {
            get
            {
                return this._additionalAttributes;
            }
        }

        #endregion
    }
}
