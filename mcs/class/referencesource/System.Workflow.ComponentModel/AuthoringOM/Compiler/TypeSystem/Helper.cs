namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Collections;
    using System.Collections.Generic;

    internal static class Helper
    {
        internal static ParameterAttributes ConvertToParameterAttributes(FieldDirection direction)
        {
            ParameterAttributes paramAttributes = ParameterAttributes.None;
            // Only few param attributes are supported
            switch (direction)
            {
                case FieldDirection.In:
                    paramAttributes = ParameterAttributes.In;
                    break;
                case FieldDirection.Out:
                    paramAttributes = ParameterAttributes.Out;
                    break;
                default:
                    paramAttributes = default(ParameterAttributes);
                    break;
            }
            return paramAttributes;
        }

        internal static MethodAttributes ConvertToMethodAttributes(MemberAttributes memberAttributes)
        {
            MethodAttributes methodAttributes = MethodAttributes.ReuseSlot;
            // convert access attributes
            if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.Assembly)
                methodAttributes |= MethodAttributes.Assembly;
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.Family)
                methodAttributes |= MethodAttributes.Family;
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.FamilyAndAssembly)
                methodAttributes |= MethodAttributes.FamANDAssem;
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.FamilyOrAssembly)
                methodAttributes |= MethodAttributes.FamORAssem;
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.Private)
                methodAttributes |= MethodAttributes.Private;
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.Public)
                methodAttributes |= MethodAttributes.Public;

            // covert scope attributes 
            if ((memberAttributes & MemberAttributes.ScopeMask) == MemberAttributes.Abstract)
                methodAttributes |= MethodAttributes.Abstract;
            else if ((memberAttributes & MemberAttributes.ScopeMask) == MemberAttributes.Final)
                methodAttributes |= MethodAttributes.Final;
            else if ((memberAttributes & MemberAttributes.ScopeMask) == MemberAttributes.Static)
                methodAttributes |= MethodAttributes.Static;
            //else if ((memberAttributes & MemberAttributes.ScopeMask) == MemberAttributes.Override)
            // methodAttributes |= MethodAttributes.ReuseSlot;// 

            // convert vtable slot
            if ((memberAttributes & MemberAttributes.VTableMask) == MemberAttributes.New)
                methodAttributes |= MethodAttributes.NewSlot;
            //if ((memberAttributes & MemberAttributes.VTableMask) == MemberAttributes.Overloaded)
            // methodAttributes |= MethodAttributes.HideBySig; // 

            return methodAttributes;
        }

        internal static FieldAttributes ConvertToFieldAttributes(MemberAttributes memberAttributes)
        {
            FieldAttributes fieldAttributes = default(FieldAttributes);

            if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.Assembly)
                fieldAttributes |= FieldAttributes.Assembly;
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.Family)
                fieldAttributes |= FieldAttributes.Family;
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.FamilyAndAssembly)
                fieldAttributes |= FieldAttributes.FamANDAssem;
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.FamilyOrAssembly)
                fieldAttributes |= FieldAttributes.FamORAssem;
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.Private)
                fieldAttributes |= FieldAttributes.Private;
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.Public)
                fieldAttributes |= FieldAttributes.Public;

            if ((memberAttributes & MemberAttributes.ScopeMask) == MemberAttributes.Const)
                fieldAttributes |= (FieldAttributes.Static | FieldAttributes.Literal);
            else if ((memberAttributes & MemberAttributes.ScopeMask) == MemberAttributes.Static)
                fieldAttributes |= FieldAttributes.Static;

            return fieldAttributes;
        }

        internal static TypeAttributes ConvertToTypeAttributes(MemberAttributes memberAttributes, Type declaringType)
        {
            TypeAttributes typeAttributes = default(TypeAttributes);

            // convert access attributes
            if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.Assembly)
                typeAttributes |= ((declaringType != null) ? TypeAttributes.NestedAssembly : TypeAttributes.NotPublic);
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.Family)
                typeAttributes |= ((declaringType != null) ? TypeAttributes.NestedFamily : TypeAttributes.NotPublic);
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.FamilyAndAssembly)
                typeAttributes |= ((declaringType != null) ? TypeAttributes.NestedFamANDAssem : TypeAttributes.NotPublic);
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.FamilyOrAssembly)
                typeAttributes |= ((declaringType != null) ? TypeAttributes.NestedFamORAssem : TypeAttributes.NotPublic);
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.Private)
                typeAttributes |= ((declaringType != null) ? TypeAttributes.NestedPrivate : TypeAttributes.NotPublic);
            else if ((memberAttributes & MemberAttributes.AccessMask) == MemberAttributes.Public)
                typeAttributes |= ((declaringType != null) ? TypeAttributes.NestedPublic : TypeAttributes.Public);

            // covert scope attributes 
            if ((memberAttributes & MemberAttributes.ScopeMask) == MemberAttributes.Abstract)
                typeAttributes |= TypeAttributes.Abstract;
            else if ((memberAttributes & MemberAttributes.ScopeMask) == MemberAttributes.Final)
                typeAttributes |= TypeAttributes.Sealed;
            else if ((memberAttributes & MemberAttributes.Static) == MemberAttributes.Static)
                typeAttributes |= (TypeAttributes.Abstract | TypeAttributes.Sealed);

            return typeAttributes;
        }

        internal static bool IncludeAccessor(MethodInfo accessor, bool nonPublic)
        {
            if (accessor == null)
                return false;

            if (nonPublic)
                return true;

            if (accessor.IsPublic)
                return true;

            return false;
        }

        internal static Attribute[] LoadCustomAttributes(CodeAttributeDeclarationCollection codeAttributeCollection, DesignTimeType declaringType)
        {
            if (declaringType == null)
                throw new ArgumentNullException("declaringType");

            if (codeAttributeCollection == null)
                return new Attribute[0];

            List<Attribute> attributes = new List<Attribute>();

            // walk through the attributes 
            foreach (CodeAttributeDeclaration codeAttribute in codeAttributeCollection)
            {
                String[] argumentNames = new String[codeAttribute.Arguments.Count];
                object[] argumentValues = new object[codeAttribute.Arguments.Count];

                Type attributeType = declaringType.ResolveType(codeAttribute.Name);

                if (attributeType != null)
                {
                    int index = 0;

                    // walk through tha arguments
                    foreach (CodeAttributeArgument codeArgument in codeAttribute.Arguments)
                    {
                        argumentNames[index] = codeArgument.Name;

                        if (codeArgument.Value is CodePrimitiveExpression)
                            argumentValues[index] = (codeArgument.Value as CodePrimitiveExpression).Value;
                        else if (codeArgument.Value is CodeTypeOfExpression)
                            argumentValues[index] = codeArgument.Value;
                        else if (codeArgument.Value is CodeSnippetExpression)
                            argumentValues[index] = (codeArgument.Value as CodeSnippetExpression).Value;
                        else
                            argumentValues[index] = new ArgumentException(SR.GetString(SR.Error_TypeSystemAttributeArgument));

                        index++;
                    }
                    bool alreadyExists = false;
                    foreach (AttributeInfoAttribute attribInfo in attributes)
                    {
                        if (attribInfo.AttributeInfo.AttributeType.FullName.Equals(attributeType.FullName))
                        {
                            alreadyExists = true;
                            break;
                        }
                    }
                    // 

                    bool allowMultiple = false;
                    if (alreadyExists && attributeType.Assembly != null)
                    {
                        object[] usageAttribs = attributeType.GetCustomAttributes(typeof(System.AttributeUsageAttribute), true);
                        if (usageAttribs != null && usageAttribs.Length > 0)
                        {
                            AttributeUsageAttribute usage = usageAttribs[0] as AttributeUsageAttribute;
                            allowMultiple = usage.AllowMultiple;
                        }
                    }
                    // now create and add the placeholder attribute
                    if (!alreadyExists || allowMultiple)
                        attributes.Add(AttributeInfoAttribute.CreateAttributeInfoAttribute(attributeType, argumentNames, argumentValues));
                }
            }
            return attributes.ToArray();
        }

        internal static object[] GetCustomAttributes(Type attributeType, bool inherit, Attribute[] attributes, MemberInfo memberInfo)
        {

            if (attributeType == null)
                throw new ArgumentNullException("attributeType");

            ArrayList attributeList = new ArrayList();
            ArrayList attributeTypes = new ArrayList();

            if (attributeType == typeof(object))
            {
                attributeList.AddRange(attributes);
            }
            else
            {
                foreach (AttributeInfoAttribute attribute in attributes)
                {
                    if (attribute.AttributeInfo.AttributeType == attributeType)
                    {
                        attributeList.Add(attribute);
                        attributeTypes.Add(attributeType);
                    }
                }
            }

            if (inherit)
            {
                MemberInfo baseMemberInfo = null;

                // we need to get a base type or the overriden member that might declare additional attributes
                if (memberInfo is Type)
                    baseMemberInfo = ((Type)memberInfo).BaseType;
                else
                    baseMemberInfo = ((DesignTimeType)memberInfo.DeclaringType).GetBaseMember(memberInfo.GetType(), memberInfo.DeclaringType.BaseType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance, new DesignTimeType.MemberSignature(memberInfo));

                // Add base attributes
                if (baseMemberInfo != null)
                {
                    object[] baseAttributes = baseMemberInfo.GetCustomAttributes(attributeType, inherit);

                    // check that attributes are not repeated 
                    foreach (Attribute baseAttribute in baseAttributes)
                    {
                        if (!(baseAttribute is AttributeInfoAttribute) || (!attributeTypes.Contains(((AttributeInfoAttribute)baseAttribute).AttributeInfo.AttributeType)))
                            attributeList.Add(baseAttribute);
                    }
                }
            }

            return attributeList.ToArray();
        }

        internal static bool IsDefined(Type attributeType, bool inherit, Attribute[] attributes, MemberInfo memberInfo)
        {
            if (attributeType == null)
                throw new ArgumentNullException("attributeType");

            foreach (Attribute attribute in attributes)
            {
                // check to see if a type is wrapped in an AttributeInfoAttribute
                if ((attribute is AttributeInfoAttribute) && ((attribute as AttributeInfoAttribute).AttributeInfo.AttributeType == attributeType))
                    return true;
            }

            MemberInfo baseMemberInfo = null;

            // we need to get a base type or the overriden member that might declare additional attributes
            if (memberInfo is Type)
                baseMemberInfo = ((Type)memberInfo).BaseType;
            else
                baseMemberInfo = ((DesignTimeType)memberInfo.DeclaringType).GetBaseMember(memberInfo.GetType(), memberInfo.DeclaringType.BaseType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance, new DesignTimeType.MemberSignature(memberInfo));

            // Add base attributes
            if (baseMemberInfo != null)
                return baseMemberInfo.IsDefined(attributeType, inherit);

            return false;
        }

        internal static string EnsureTypeName(string typeName)
        {
            if (typeName == null || typeName.Length == 0)
                return typeName;

            if (typeName.IndexOf('.') == -1)
            {
                if (typeName.StartsWith("@", StringComparison.Ordinal))
                    typeName = typeName.Substring(1);
                else if (typeName.StartsWith("[", StringComparison.Ordinal) && typeName.EndsWith("]", StringComparison.Ordinal))
                    typeName = typeName.Substring(1, typeName.Length - 1);
            }
            else
            {
                string[] tokens = typeName.Split(new char[] { '.' });
                typeName = string.Empty;
                int i;
                for (i = 0; i < tokens.Length - 1; i++)
                {
                    typeName += EnsureTypeName(tokens[i]);
                    typeName += ".";
                }

                typeName += EnsureTypeName(tokens[i]);
            }

            return typeName;
        }
    }
}
