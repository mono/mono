//---------------------------------------------------------------------
// <copyright file="PropertyEmitterBase.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Data.Entity.Design.SsdlGenerator;
using System.Data.Entity.Design.Common;

namespace System.Data.EntityModel.Emitters
{
    internal abstract class PropertyEmitterBase : MetadataItemEmitter
    {
        private bool _declaringTypeUsesStandardBaseType;
        protected PropertyEmitterBase(ClientApiGenerator generator, MetadataItem item, bool declaringTypeUsesStandardBaseType)
            :base(generator, item)
        {
            Debug.Assert(item != null, "item is null");
            _declaringTypeUsesStandardBaseType = declaringTypeUsesStandardBaseType;
        }

        /// <summary>
        /// This is where the derived classes supply their emit logic.
        /// </summary>
        /// <param name="typeDecl">The CodeDom representation of the type that the property is being added to.</param>
        protected abstract void EmitProperty(CodeTypeDeclaration typeDecl);

        /// <summary>
        /// Validation logic specific to property emitters
        /// </summary>
        protected override void Validate()
        {
            VerifyGetterAndSetterAccessibilityCompatability();
            Generator.VerifyLanguageCaseSensitiveCompatibilityForProperty(Item as EdmMember);
        }

        /// <summary>
        /// The compiler ensures accessibility on a Setter/Getter is more restrictive than on the Property.
        /// However accessibility modifiers are not well ordered. Internal and Protected don't go well together 
        /// because neither is more restrictive than others.
        /// </summary>
        private void VerifyGetterAndSetterAccessibilityCompatability()
        {
            if (PropertyEmitter.GetGetterAccessibility(Item) == MemberAttributes.Assembly
                        && PropertyEmitter.GetSetterAccessibility(Item) == MemberAttributes.Family)
            {
                Generator.AddError(System.Data.Entity.Design.Strings.GeneratedPropertyAccessibilityConflict(Item.Name, "Internal", "Protected"),
                        ModelBuilderErrorCode.GeneratedPropertyAccessibilityConflict,
                        EdmSchemaErrorSeverity.Error, Item.DeclaringType.FullName, Item.Name);
            }
            else if (PropertyEmitter.GetGetterAccessibility(Item) == MemberAttributes.Family
                        && PropertyEmitter.GetSetterAccessibility(Item) == MemberAttributes.Assembly)
            {
                Generator.AddError(System.Data.Entity.Design.Strings.GeneratedPropertyAccessibilityConflict(Item.Name, "Protected", "Internal"),
                        ModelBuilderErrorCode.GeneratedPropertyAccessibilityConflict,
                        EdmSchemaErrorSeverity.Error, Item.DeclaringType.FullName, Item.Name);
            }
        }



        /// <summary>
        /// Main method for Emitting property code.
        /// </summary>
        /// <param name="typeDecl">The CodeDom representation of the type that the property is being added to.</param>
        public void Emit(CodeTypeDeclaration typeDecl)
        {
            Validate();
            EmitProperty(typeDecl);
        }

        protected bool AncestorClassDefinesName(string name)
        {
            if (_declaringTypeUsesStandardBaseType && Utils.DoesTypeReserveMemberName(Item.DeclaringType, name, Generator.LanguageAppropriateStringComparer))
            {
                return true;
            }
            
            StructuralType baseType = Item.DeclaringType.BaseType as StructuralType;
            if (baseType != null && baseType.Members.Contains(name))
            {
                return true;
            }

            return false;
        }

        public new EdmMember Item
        {
            get
            {
                return base.Item as EdmMember;
            }
        }

    }
}
