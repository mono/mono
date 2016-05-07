//---------------------------------------------------------------------
// <copyright file="ComplexTypeEmitter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.CodeDom;
using System.Data;
using System.Data.Metadata.Edm;
using System.Data.EntityModel.SchemaObjectModel;


namespace System.Data.EntityModel.Emitters
{
    /// <summary>
    /// Summary description for NestedTypeEmitter.
    /// </summary>
    internal sealed class ComplexTypeEmitter : StructuredTypeEmitter
    {
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="nestedType"></param>
        public ComplexTypeEmitter(ClientApiGenerator generator, ComplexType complexType)
            : base(generator, complexType)
        {
        }


        /// <summary>
        /// Apply the attributes to this type.
        /// </summary>
        /// <param name="typeDecl">The declaration of the type that should have attributes added to it.</param>
        protected override void EmitTypeAttributes( CodeTypeDeclaration typeDecl )
        {
            Generator.AttributeEmitter.EmitTypeAttributes( this, typeDecl );
            base.EmitTypeAttributes( typeDecl );
        }
        #endregion
        #region Protected Properties

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override CodeTypeReference GetBaseType()
        {
            CodeTypeReference baseType = base.GetBaseType();
            if (baseType == null)
            {
                baseType = TypeReference.ComplexTypeBaseClass;
            }
            return baseType;
        }

        protected override ReadOnlyMetadataCollection<EdmProperty> GetProperties()
        {
            return Item.Properties;
        }

        internal new ComplexType Item
        {
            get
            {
                return base.Item as ComplexType;
            }
        }

        #endregion

    }
}
