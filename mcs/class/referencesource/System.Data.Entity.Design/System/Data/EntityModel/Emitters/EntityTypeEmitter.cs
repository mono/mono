//---------------------------------------------------------------------
// <copyright file="EntityTypeEmitter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.CodeDom;
using System.Data;
using Som=System.Data.EntityModel.SchemaObjectModel;
using System.Data.Metadata.Edm;


namespace System.Data.EntityModel.Emitters
{
    /// <summary>
    /// Summary description for ItemTypeEmitter.
    /// </summary>
    internal sealed class EntityTypeEmitter : StructuredTypeEmitter
    {
        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="itemType"></param>
        public EntityTypeEmitter(ClientApiGenerator generator, EntityType entity)
        : base(generator, entity)
        {
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeDecl"></param>
        protected override void EmitProperties(CodeTypeDeclaration typeDecl)
        {
            base.EmitProperties(typeDecl);
            foreach ( NavigationProperty navigationProperty in Item.GetDeclaredOnlyMembers<NavigationProperty>() )
            {
                NavigationPropertyEmitter navigationPropertyEmitter = new NavigationPropertyEmitter(Generator, navigationProperty, UsingStandardBaseClass);
                navigationPropertyEmitter.Emit(typeDecl);
            }
        }

        public override CodeTypeDeclarationCollection EmitApiClass()
        {
            CodeTypeDeclarationCollection typeDecls = base.EmitApiClass();

            if ( Item.KeyMembers.Count > 0 && typeDecls.Count == 1 )
            {
                // generate xml comments for the key properties
                CodeTypeDeclaration typeDecl = typeDecls[0];
                typeDecl.Comments.Add( new CodeCommentStatement( "<KeyProperties>", true ) );
                foreach ( EdmMember keyProperty in Item.KeyMembers)
                {
                    string name = keyProperty.Name;
                    typeDecl.Comments.Add( new CodeCommentStatement( name, true ) );
                }
                typeDecl.Comments.Add( new CodeCommentStatement( "</KeyProperties>", true ) );
            }

            return typeDecls;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeDecl"></param>
        protected override void EmitTypeAttributes(CodeTypeDeclaration typeDecl)
        {
            Generator.AttributeEmitter.EmitTypeAttributes( this, typeDecl );
            base.EmitTypeAttributes( typeDecl );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override CodeTypeReference GetBaseType()
        {
            CodeTypeReference baseType = base.GetBaseType();
            if (baseType == null)
            {
                baseType = TypeReference.EntityTypeBaseClass;
            }
            return baseType;
        }

        protected override ReadOnlyMetadataCollection<EdmProperty> GetProperties()
        {
            return Item.Properties;
        }
        #endregion



        #region Public Properties
        #endregion

        #region Protected Properties
        #endregion

        #region Private Properties
        /// <summary>
        /// Gets the SchemaElement that this class is generating code for.
        /// </summary>
        /// <value></value>
        public new EntityType Item
        {
            get
            {
                return base.Item as EntityType;
            }
        }

        #endregion



    }
}
