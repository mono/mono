//---------------------------------------------------------------------
// <copyright file="MetadataItemEmitter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.CodeDom;
using System.Reflection;

namespace System.Data.EntityModel.Emitters
{
    internal abstract class MetadataItemEmitter : Emitter
    {
        private MetadataItem _item;

        protected MetadataItemEmitter(ClientApiGenerator generator, MetadataItem item)
            :base(generator)
        {
            Debug.Assert(item != null, "item is null");
            _item = item;
        }

        protected MetadataItem Item
        {
            get { return _item; }
        }

        /// <summary>
        /// Emitter-specific validation here
        /// </summary>
        protected abstract void Validate();

        #region Operations for Getting Accessibility 

        private const string CodeGenerationValueAccessibilityInternal = "Internal";
        private const string CodeGenerationValueAccessibilityProtected = "Protected";
        private const string CodeGenerationValueAccessibilityPublic = "Public";
        private const string CodeGenerationValueAccessibilityPrivate = "Private";

        #region Protected Block
        protected static MemberAttributes AccessibilityFromGettersAndSetters(EdmMember property)
        {
            MemberAttributes scope = MemberAttributes.Private;

            MemberAttributes getter = GetGetterAccessibility(property);
            if (IsLeftMoreAccessableThanRight(getter, scope))
            {
                scope = getter;
            }

            MemberAttributes setter = GetSetterAccessibility(property);
            if (IsLeftMoreAccessableThanRight(setter, scope))
            {
                scope = setter;
            }
            return scope;
        }

        protected static MemberAttributes GetGetterAccessibility(EdmMember item)
        {
            return GetAccessibilityValue(item, XmlConstants.GetterAccess);
        }

        protected static MemberAttributes GetSetterAccessibility(EdmMember item)
        {
            return GetAccessibilityValue(item, XmlConstants.SetterAccess);
        }

        protected static MemberAttributes GetFunctionImportAccessibility(EdmFunction item)
        {
            return GetAccessibilityValue(item, XmlConstants.MethodAccess);
        }

        protected static MemberAttributes GetEntitySetPropertyAccessibility(EntitySet item)
        {
            return GetAccessibilityValue(item, XmlConstants.GetterAccess);
        }
        protected static MemberAttributes GetEntityTypeAccessibility(EntityType item)
        {
            return GetAccessibilityValue(item, XmlConstants.TypeAccess);
        }

        protected static int GetAccessibilityRank(MemberAttributes accessibility)
        {
            Debug.Assert(accessibility == MemberAttributes.Private ||
                         accessibility == MemberAttributes.Public ||
                         accessibility == MemberAttributes.Assembly ||
                         accessibility == MemberAttributes.Family,
                         "this method is intended to deal with only single access attributes");
            switch (accessibility)
            {
                case MemberAttributes.Public:
                    return 0;
                case MemberAttributes.Assembly:
                    return 1;
                case MemberAttributes.Family:
                    return 2;
                default:
                    Debug.Assert(accessibility == MemberAttributes.Private, "did a new type get added?");
                    return 3;
            }
        }

        protected static TypeAttributes GetTypeAccessibilityValue(MetadataItem item)
        {
            TypeAttributes accessibilty = TypeAttributes.Public;
            MetadataProperty metadataProperty;
            if (item.MetadataProperties.TryGetValue(Utils.GetFullyQualifiedCodeGenerationAttributeName(XmlConstants.TypeAccess), false, out metadataProperty))
            {
                accessibilty = GetCodeAccessibilityTypeAttribute(metadataProperty.Value.ToString());
            }
            return accessibilty;
        }
        #endregion Protected

        #region Private Block
        private static MemberAttributes GetAccessibilityValue(MetadataItem item, string attribute)
        {
            MemberAttributes accessibilty = MemberAttributes.Public;
            MetadataProperty metadataProperty;
            if (item.MetadataProperties.TryGetValue(Utils.GetFullyQualifiedCodeGenerationAttributeName(attribute), false, out metadataProperty))
            {
                accessibilty = GetCodeAccessibilityMemberAttribute(metadataProperty.Value.ToString());
            }
            return accessibilty;
        }

        private static MemberAttributes GetCodeAccessibilityMemberAttribute(string accessibility)
        {
            Debug.Assert(accessibility != null, "why does accessibility == null?");

            switch (accessibility)
            {
                case CodeGenerationValueAccessibilityInternal:
                    return MemberAttributes.Assembly;
                case CodeGenerationValueAccessibilityPrivate:
                    return MemberAttributes.Private;
                case CodeGenerationValueAccessibilityProtected:
                    return MemberAttributes.Family;

                default:
                    Debug.Assert(accessibility == CodeGenerationValueAccessibilityPublic, "found an accessibility other than " + CodeGenerationValueAccessibilityPublic);
                    return MemberAttributes.Public;
            }
        }

        /// <summary>
        /// Given a MemberAttribute, returns a string representation used in CSDL. 
        /// For e.g: MemebrAttribtue.Family is Protected in our csdl, (protected in C#, Family in VB)
        /// Inverse of the method above (GetCodeAccessibilityMemberAttribute)
        /// </summary>
        protected static string GetAccessibilityCsdlStringFromMemberAttribute(MemberAttributes attribute)
        {
            switch (attribute)
            {
                case MemberAttributes.Assembly:
                    return CodeGenerationValueAccessibilityInternal;
                case MemberAttributes.Private:
                    return CodeGenerationValueAccessibilityPrivate;
                case MemberAttributes.Family:
                    return CodeGenerationValueAccessibilityProtected;

                default:
                    Debug.Assert(attribute == MemberAttributes.Public, "found MemberAttribute other than " + CodeGenerationValueAccessibilityPublic);
                    return CodeGenerationValueAccessibilityPublic;
            }
        }

        private static bool IsLeftMoreAccessableThanRight(MemberAttributes left, MemberAttributes right)
        {
            return GetAccessibilityRank(left) < GetAccessibilityRank(right);
        }

        private static TypeAttributes GetCodeAccessibilityTypeAttribute(string accessibility)
        {
            Debug.Assert(accessibility != null, "why does accessibility == null?");
            if (accessibility == CodeGenerationValueAccessibilityInternal || accessibility == CodeGenerationValueAccessibilityProtected)
            {
                return TypeAttributes.NotPublic;
            }
            else
            {
                Debug.Assert(accessibility == CodeGenerationValueAccessibilityPublic, "found an accessibility other than " + CodeGenerationValueAccessibilityPublic);
                return TypeAttributes.Public;
            }
        }
        #endregion Private

        #endregion Operations for getting Accessibility
    }
}
