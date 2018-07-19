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


internal delegate object InstantiateObject();

internal class ObjectFactoryCodeDomTreeGenerator {

    private CodeCompileUnit _codeCompileUnit;
    private CodeTypeDeclaration _factoryClass;

    private const string factoryClassNameBase = "FastObjectFactory_";
    private const string factoryFullClassNameBase = BaseCodeDomTreeGenerator.internalAspNamespace +
        "." + factoryClassNameBase;

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

    internal void AddFactoryMethod(string typeToCreate) {

        // Generate a simple factory method for this type.  e.g.
        // static object Create_Acme_Foo() {
        //     return new ASP.foo_aspx();
        // }

        CodeMemberMethod method = new CodeMemberMethod();
        method.Name = GetCreateMethodNameForType(typeToCreate);
        method.ReturnType = new CodeTypeReference(typeof(object));
        method.Attributes = MemberAttributes.Static;

        CodeMethodReturnStatement cmrs = new CodeMethodReturnStatement(
            new CodeObjectCreateExpression(typeToCreate));
        method.Statements.Add(cmrs);

        _factoryClass.Members.Add(method);
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

