//---------------------------------------------------------------------
// <copyright file="SchemaTypeEmitter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.CodeDom;
using System.Data.EntityModel.SchemaObjectModel;
using Som = System.Data.EntityModel.SchemaObjectModel;
using System.Data.Metadata.Edm;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Data.Entity.Design;
using System.Data.Entity.Design.SsdlGenerator;


namespace System.Data.EntityModel.Emitters
{
    /// <summary>
    /// 
    /// </summary>
    internal abstract class SchemaTypeEmitter : MetadataItemEmitter
    {
        #region Public Methods
        public abstract CodeTypeDeclarationCollection EmitApiClass();
        #endregion

        #region Protected Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="generator"></param>
        /// <param name="schemaType"></param>
        protected SchemaTypeEmitter(ClientApiGenerator generator, MetadataItem item)
        : base(generator, item)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeDecl"></param>
        protected virtual void EmitTypeAttributes( CodeTypeDeclaration typeDecl )
        {
            Generator.AttributeEmitter.EmitTypeAttributes( this, typeDecl );
        }

        /// <summary>
        /// Emitter-specific validation: for SchemaTypeEmitter-derived classes, we
        /// check the EdmItemCollection for other entities that have the same name
        /// but differ in case
        /// </summary>
        protected override void Validate()
        {
            Generator.VerifyLanguageCaseSensitiveCompatibilityForType(Item);
        }

        /// <summary>
        /// Add attributes to a type's CustomAttributes collection
        /// </summary>
        /// <param name="itemName">The name of the type</param>
        /// <param name="typeDecl">The type to annotate</param>
        /// <param name="additionalAttributes">The additional attributes</param>
        protected void EmitTypeAttributes(string itemName, CodeTypeDeclaration typeDecl,
                                          List<CodeAttributeDeclaration> additionalAttributes)
        {
            if (additionalAttributes != null && additionalAttributes.Count > 0)
            {
                try
                {
                    typeDecl.CustomAttributes.AddRange(additionalAttributes.ToArray());
                }
                catch (ArgumentNullException e)
                {
                    Generator.AddError(Strings.InvalidAttributeSuppliedForType(itemName),
                                       ModelBuilderErrorCode.InvalidAttributeSuppliedForType,
                                       EdmSchemaErrorSeverity.Error,
                                       e);
                }
            }

            EmitTypeAttributes(typeDecl);
        }

        /// <summary>
        /// Add interfaces to the type's list of BaseTypes
        /// </summary>
        /// <param name="itemName">The name of the type</param>
        /// <param name="typeDecl">The type whose list of base types needs to be extended</param>
        /// <param name="additionalInterfaces">The interfaces to add to the list of base types</param>
        protected void AddInterfaces(string itemName, 
                                     CodeTypeDeclaration typeDecl, 
                                     List<Type> additionalInterfaces)
        {
            if (additionalInterfaces != null)
            {
                try
                {
                    foreach (Type interfaceType in additionalInterfaces)
                    {
                        typeDecl.BaseTypes.Add(new CodeTypeReference(interfaceType));
                    }
                }
                catch (ArgumentNullException e)
                {
                    Generator.AddError(Strings.InvalidInterfaceSuppliedForType(itemName),
                                      ModelBuilderErrorCode.InvalidInterfaceSuppliedForType,
                                      EdmSchemaErrorSeverity.Error,
                                      e);
                }
            }
        }

        /// <summary>
        /// Add interfaces to the type's list of BaseTypes
        /// </summary>
        /// <param name="itemName">The name of the type</param>
        /// <param name="typeDecl">The type to which members need to be added</param>
        /// <param name="additionalMembers">The members to add</param>
        protected void AddMembers(string itemName, CodeTypeDeclaration typeDecl, 
                                  List<CodeTypeMember> additionalMembers)
        {
            if (additionalMembers != null && additionalMembers.Count > 0)
            {
                try
                {
                    typeDecl.Members.AddRange(additionalMembers.ToArray());
                }
                catch (ArgumentNullException e)
                {
                    Generator.AddError(Strings.InvalidMemberSuppliedForType(itemName),
                                       ModelBuilderErrorCode.InvalidMemberSuppliedForType,
                                       EdmSchemaErrorSeverity.Error,
                                       e);
                }
            }
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the element that code is being emitted for.
        /// </summary>
        internal new GlobalItem Item
        {
            get
            {
                return base.Item as GlobalItem;
            }
        }

        internal void SetTypeVisibility(CodeTypeDeclaration typeDecl)
        {
            typeDecl.TypeAttributes &= ~System.Reflection.TypeAttributes.VisibilityMask;
            typeDecl.TypeAttributes |= GetTypeAccessibilityValue(Item);
        }


        #endregion
    }
}
