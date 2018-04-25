//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.CodeDom;
    using System.Xml;
    
    static class SecurityAttributeGenerationHelper
    {
        public static CodeAttributeDeclaration FindOrCreateAttributeDeclaration<T>(CodeAttributeDeclarationCollection attributes)
            where T : Attribute
        {
            if (attributes == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("attributes");

            CodeTypeReference refT = new CodeTypeReference(typeof(T));

            foreach (CodeAttributeDeclaration attribute in attributes)
                if (attribute.AttributeType.BaseType == refT.BaseType)
                    return attribute;

            CodeAttributeDeclaration result = new CodeAttributeDeclaration(refT);
            attributes.Add(result);

            return result;
        }

        public static void CreateOrOverridePropertyDeclaration<V>(CodeAttributeDeclaration attribute, string propertyName, V value)
        {
            if (attribute == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("attribute");
            if (propertyName == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("propertyName");

            CodeExpression newValue;

            if (value is TimeSpan)
                newValue = new CodeObjectCreateExpression(
                    typeof(TimeSpan), 
                    new CodePrimitiveExpression(((TimeSpan)(object)value).Ticks));
            else if (value is Enum)
                newValue = new CodeFieldReferenceExpression(
                    new CodeTypeReferenceExpression(typeof(V)),
                    ((object)value).ToString());
            else
                newValue = new CodePrimitiveExpression((object)value);

            CodeAttributeArgument attributeProperty = TryGetAttributeProperty(attribute, propertyName);

            if (attributeProperty == null)
            {
                attributeProperty = new CodeAttributeArgument(propertyName, newValue);
                attribute.Arguments.Add(attributeProperty);
            }
            else
                attributeProperty.Value = newValue;
        }

        public static CodeAttributeArgument TryGetAttributeProperty(CodeAttributeDeclaration attribute, string propertyName)
        {
            if (attribute == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("attribute");
            if (propertyName == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("propertyName");

            foreach (CodeAttributeArgument argument in attribute.Arguments)
                if (argument.Name == propertyName)
                    return argument;

            return null;
        }
    }
}
