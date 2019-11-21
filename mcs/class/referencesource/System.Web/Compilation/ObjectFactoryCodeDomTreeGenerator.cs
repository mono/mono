//------------------------------------------------------------------------------
// <copyright file="ObjectFactoryCodeDomTreeGenerator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

// This code is used to optimize the instantiation of generated types.


namespace System.Web.Compilation {

using System;
using System.Collections;
using System.Reflection;
using System.Security.Permissions;
using System.Globalization;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Web.Util;
using Util = System.Web.UI.Util;
using System.Linq;
using System.Runtime.InteropServices;

internal delegate object InstantiateObject();

internal class ObjectFactoryCodeDomTreeGenerator {

    private CodeCompileUnit _codeCompileUnit;
    private CodeTypeDeclaration _factoryClass;

    private const string factoryClassNameBase = "FastObjectFactory_";
    private const string factoryFullClassNameBase = BaseCodeDomTreeGenerator.internalAspNamespace +
        "." + factoryClassNameBase;
    private static readonly string optionalAttributeTypeName = typeof(OptionalAttribute).FullName;

    internal ObjectFactoryCodeDomTreeGenerator(string outputAssemblyName) {

        _codeCompileUnit = new CodeCompileUnit();

        CodeNamespace sourceDataNamespace = new CodeNamespace(
            BaseCodeDomTreeGenerator.internalAspNamespace);
        _codeCompileUnit.Namespaces.Add(sourceDataNamespace);

        // Make the class name vary based on the assembly (VSWhidbey 363214)
        string factoryClassName = factoryClassNameBase +
            Util.MakeValidTypeNameFromString(outputAssemblyName).ToLower(CultureInfo.InvariantCulture);

        // Create a single class, in which a method will be added for each
        // type that needs to be fast created in this assembly
        _factoryClass = new CodeTypeDeclaration(factoryClassName);

        // Make the class internal (VSWhidbey 363214)
        _factoryClass.TypeAttributes &= ~TypeAttributes.Public;

        // We generate a dummy line pragma, just so it will end with a '#line hidden'
        // and prevent the following generated code from ever being treated as user
        // code.  We need to use this hack because CodeDOM doesn't allow simply generating
        // a '#line hidden'. (VSWhidbey 199384)
        CodeSnippetTypeMember dummySnippet = new CodeSnippetTypeMember(String.Empty);
#if !PLATFORM_UNIX /// Unix file system
        // CORIOLISTODO: Unix file system
        dummySnippet.LinePragma = new CodeLinePragma(@"c:\\dummy.txt", 1);
#else // !PLATFORM_UNIX
        dummySnippet.LinePragma = new CodeLinePragma(@"/dummy.txt", 1);
#endif // !PLATFORM_UNIX
        _factoryClass.Members.Add(dummySnippet);

        // Add a private default ctor to make the class non-instantiatable (VSWhidbey 340829)
        CodeConstructor ctor = new CodeConstructor();
        ctor.Attributes |= MemberAttributes.Private;
        _factoryClass.Members.Add(ctor);

        sourceDataNamespace.Types.Add(_factoryClass);
    }

    internal void AddFactoryMethod(string typeToCreate, CodeCompileUnit ccu = null) {

        // Generate a simple factory method for this type.  e.g.
        // static object Create_Acme_Foo() {
        //     return new ASP.foo_aspx();
        // }

        CodeMemberMethod method = new CodeMemberMethod();
        method.Name = GetCreateMethodNameForType(typeToCreate);
        method.ReturnType = new CodeTypeReference(typeof(object));
        method.Attributes = MemberAttributes.Static;

        AddCreateTypeInstanceStatement(typeToCreate, ccu, method.Statements);

        _factoryClass.Members.Add(method);
    }

    private static void AddCreateTypeInstanceStatement(string typeToCreate, CodeCompileUnit ccu, CodeStatementCollection statements) {
        if (BinaryCompatibility.Current.TargetsAtLeastFramework472 && ccu != null) {
            /* Generate code like below
             
             IServiceProvider __activator = HttpRuntime.WebObjectActivator;
             
             //-- Generate code like this if default c-tor exists
            if (activator != null) {
                return activator.GetService(ctrlType);
            }

             // if default c-tor exists
            else {
                _ctrl = new ....
            }
            // if no default c-tor
            else {
                throw new InvalidOperationException(SR.GetString(SR.Could_not_create_type_instance, ctrlType))
            }
            
             //-- if default c-tor doesn't exist, assume dev wants to use DI.
            // if there is no default c-tor, you will get compilation error on framework 4.7.1 and below.
            */
            var webObjectActivatorExpr = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("System.Web.HttpRuntime"), "WebObjectActivator");
            statements.Add(new CodeVariableDeclarationStatement(typeof(IServiceProvider), "__activator"));
            var activatorRefExpr = new CodeVariableReferenceExpression("__activator");
            statements.Add(new CodeAssignStatement(activatorRefExpr, webObjectActivatorExpr));
            var getServiceExpr = new CodeMethodInvokeExpression(activatorRefExpr, "GetService", new CodeTypeOfExpression(typeToCreate));

             var createObjectStatement = new CodeConditionStatement() {
                Condition = new CodeBinaryOperatorExpression(activatorRefExpr,
                CodeBinaryOperatorType.IdentityInequality,
                new CodePrimitiveExpression(null))
            };
            createObjectStatement.TrueStatements.Add(new CodeMethodReturnStatement(getServiceExpr));

             // If default c-tor exists
            if (DoesGeneratedCodeHaveDefaultCtor(typeToCreate, ccu)) {
                var newObjectExpr = new CodeObjectCreateExpression(typeToCreate);
                createObjectStatement.FalseStatements.Add(new CodeMethodReturnStatement(newObjectExpr));
            }
            else {
                var throwExceptionStatement = new CodeThrowExceptionStatement(new CodeObjectCreateExpression(
                    new CodeTypeReference(typeof(System.InvalidOperationException)),
                    new CodeExpression[] { new CodePrimitiveExpression(SR.GetString(SR.Could_not_create_type_instance, typeToCreate)) }));
                createObjectStatement.FalseStatements.Add(throwExceptionStatement);
            }
            statements.Add(createObjectStatement);
        }
        else {
            // Generate new typeToCreate()
            var newObjectExpression = new CodeObjectCreateExpression(typeToCreate);
            statements.Add(new CodeMethodReturnStatement(newObjectExpression));
        }
    }

     private static bool DoesGeneratedCodeHaveDefaultCtor(string typeToCreate, CodeCompileUnit ccu) {
        for(var i = 0; i < ccu.Namespaces.Count; i++) {
            var ns = ccu.Namespaces[i];
            for(var n = 0; n < ns.Types.Count; n++) {
                var type = ns.Types[n];
                if(StringUtil.Equals(typeToCreate, ns.Name + "." + type.Name)) {
                    foreach (var ctm in type.Members) {
                        var ctor = ctm as CodeConstructor;
                        if (ctor != null && (ctor.Attributes & MemberAttributes.Public) == MemberAttributes.Public
                                && DoesAllConstructorParametersHaveDefaultValue(ctor)) {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

     private static bool DoesAllConstructorParametersHaveDefaultValue(CodeConstructor ctor) {
        foreach(CodeParameterDeclarationExpression paramExpr in ctor.Parameters) {
            var hasOptionalAttribute = false;
            foreach (CodeAttributeDeclaration attr in paramExpr.CustomAttributes) {
                if(attr.AttributeType?.BaseType == optionalAttributeTypeName) {
                    hasOptionalAttribute = true;
                    break;
                }
            }
            if(!hasOptionalAttribute) {
                return false;
            }
        }
        return true;
    }

    private static string GetCreateMethodNameForType(string typeToCreate) {
        return "Create_" + Util.MakeValidTypeNameFromString(typeToCreate);
    }

    internal CodeCompileUnit CodeCompileUnit {
        get { return _codeCompileUnit; }
    }

    // Get the factory delegate for this type
    // Could be called with user code on the stack, so need to assert here 
    // e.g. This can happen during a Server.Transfer, or a LoadControl.
    [ReflectionPermission(SecurityAction.Assert, Flags = ReflectionPermissionFlag.MemberAccess)]
    internal static InstantiateObject GetFastObjectCreationDelegate(Type t) {
        // Look for the factory class in the same assembly
        Assembly a = t.Assembly;

        string shortAssemblyName = Util.GetAssemblyShortName(t.Assembly);
        shortAssemblyName = shortAssemblyName.ToLower(CultureInfo.InvariantCulture);
        Type factoryType = a.GetType(factoryFullClassNameBase + Util.MakeValidTypeNameFromString(shortAssemblyName));
        if (factoryType == null)
            return null;

        string methodName = GetCreateMethodNameForType(t.FullName);

        try {
            return (InstantiateObject) Delegate.CreateDelegate(
                typeof(InstantiateObject), factoryType, methodName);
        }
        catch {
            return null;
        }
    }
}

}

