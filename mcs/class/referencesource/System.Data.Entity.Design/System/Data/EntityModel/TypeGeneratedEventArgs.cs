//---------------------------------------------------------------------
// <copyright file="TypeGeneratedEventArgs.cs" company="Microsoft">
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
    /// raised when a type is generated.
    /// </summary>
    public sealed class TypeGeneratedEventArgs : EventArgs
    {
        #region Private Data

        private GlobalItem _typeSource;
        private CodeTypeReference _baseType;
        private List<Type> _additionalInterfaces = new List<Type>();
        private List<CodeTypeMember> _additionalMembers = new List<CodeTypeMember>();
        private List<CodeAttributeDeclaration> _additionalAttributes = new List<CodeAttributeDeclaration>();

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public TypeGeneratedEventArgs()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="typeSource">The source of the event</param>
        /// <param name="baseType">The base type of the type being generated</param>
        public TypeGeneratedEventArgs(GlobalItem typeSource, CodeTypeReference baseType)
        {
            this._typeSource = typeSource;
            this._baseType = baseType;
        }

        #endregion

        #region Properties

        public GlobalItem TypeSource
        {
            get
            {
                return this._typeSource;
            }
        }

        /// <summary>
        /// The type appropriate for the TypeSource
        /// </summary>
        public CodeTypeReference BaseType
        {
            get
            {
                return this._baseType;
            }
            set
            {
                this._baseType = value;
            }
        }

        /// <summary>
        /// Interfaces to be included in the new type's definition
        /// </summary>
        public List<Type> AdditionalInterfaces
        {
            get
            {
                return this._additionalInterfaces;
            }
        }

        /// <summary>
        /// Members to be included in the new type's definition
        /// </summary>
        public List<CodeTypeMember> AdditionalMembers
        {
            get
            {
                return this._additionalMembers;
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
