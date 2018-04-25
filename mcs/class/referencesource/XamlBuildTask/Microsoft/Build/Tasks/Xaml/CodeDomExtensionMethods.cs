//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{
    using System;
    using System.CodeDom;

    static class CodeDomExtensionMethods
    {
        internal static CodeVariableReferenceExpression DeclareVar(this CodeStatementCollection block, Type type,
            string name, CodeExpression initExpression)
        {
            block.Add(new CodeVariableDeclarationStatement()
                {
                    Name = name,
                    Type = new CodeTypeReference(type),
                    InitExpression = initExpression
                });
            return new CodeVariableReferenceExpression(name);
        }

        internal static CodeFieldReferenceExpression Field(this CodeExpression targetObject, string fieldName)
        {
            return new CodeFieldReferenceExpression(targetObject, fieldName);
        }

        internal static CodeMethodInvokeExpression Invoke(this CodeExpression targetObject, string methodName,
            params CodeExpression[] parameters)
        {
            return new CodeMethodInvokeExpression(targetObject, methodName, parameters);
        }

        internal static CodeObjectCreateExpression New(this Type type, params CodeExpression[] parameters)
        {
            return new CodeObjectCreateExpression(type, parameters);
        }

        internal static CodePropertyReferenceExpression Property(this CodeExpression targetObject, string propertyName)
        {
            return new CodePropertyReferenceExpression(targetObject, propertyName);
        }
    }
}
