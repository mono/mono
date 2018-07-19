//------------------------------------------------------------------------------
// <copyright file="CodeDomUtility.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Compilation {

using System.Text;
using System.Runtime.Serialization.Formatters;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Globalization;
using System.Web.Util;
using System.Web.UI;
using System.Web.Configuration;
using System.Diagnostics;
using Debug = System.Diagnostics.Debug;
using System.CodeDom;
using System.CodeDom.Compiler;
using Util = System.Web.UI.Util;

internal static class CodeDomUtility {

    internal static BooleanSwitch WebFormsCompilation = new BooleanSwitch("WebFormsCompilation", "Outputs information about the WebForms compilation of ASPX templates");

    internal /*public*/ static CodeExpression GenerateExpressionForValue(PropertyInfo propertyInfo, object value, Type valueType) {
#if DEBUG
        if (WebFormsCompilation.Enabled) {
            Debug.WriteLine("GenerateExpressionForValue() {");
            Debug.Indent();
        }
#endif // DEBUG
        CodeExpression rightExpr = null;

        if (valueType == null) {
            throw new ArgumentNullException("valueType");
        }

        PropertyDescriptor pd = null;
        if (propertyInfo != null) {
            pd = TypeDescriptor.GetProperties(propertyInfo.ReflectedType)[propertyInfo.Name];
        }

        if (valueType == typeof(string) && value is string) {
            if (WebFormsCompilation.Enabled) Debug.WriteLine("simple string");
            rightExpr = new CodePrimitiveExpression((string)value);
        }
        else if (valueType.IsPrimitive) {
            if (WebFormsCompilation.Enabled) Debug.WriteLine("primitive");
            rightExpr = new CodePrimitiveExpression(value);
        }
        else if (propertyInfo == null && valueType == typeof(object) &&
            (value == null || value.GetType().IsPrimitive)) {

            // If the type is object, and the value is a primitive, simply use a
            // CodePrimitiveExpression instead of trying to use a TypeConverter (VSWhidbey 518773)
            if (WebFormsCompilation.Enabled) Debug.WriteLine("primitive to object");
            rightExpr = new CodePrimitiveExpression(value);
        }
        else if (valueType.IsArray) {
            if (WebFormsCompilation.Enabled) Debug.WriteLine("array");
            Array array = (Array)value;
            CodeArrayCreateExpression exp = new CodeArrayCreateExpression();
            exp.CreateType = new CodeTypeReference(valueType.GetElementType());
            if (array != null) {
                foreach (object o in array) {
                    exp.Initializers.Add(GenerateExpressionForValue(null, o, valueType.GetElementType()));
                }
            }
            rightExpr = exp;
        }
        else if (valueType == typeof(Type)) {
            rightExpr = new CodeTypeOfExpression((Type) value);
        }
        else {
            if (WebFormsCompilation.Enabled) Debug.WriteLine("other");
            TypeConverter converter = null;
            if (pd != null) {
                converter = pd.Converter;
            }
            else {
                converter = TypeDescriptor.GetConverter(valueType);
            }

            bool added = false;

            if (converter != null) {
                InstanceDescriptor desc = null;
                
                if (converter.CanConvertTo(typeof(InstanceDescriptor))) {
                    desc = (InstanceDescriptor)converter.ConvertTo(value, typeof(InstanceDescriptor));
                }
                if (desc != null) {
                    if (WebFormsCompilation.Enabled) Debug.WriteLine("has converter with instance descriptor");

                    // static field ref...
                    //
                    if (desc.MemberInfo is FieldInfo) {
                        if (WebFormsCompilation.Enabled) Debug.WriteLine("persistinfo is a field ref");
                        CodeFieldReferenceExpression fieldRef = new CodeFieldReferenceExpression(BuildGlobalCodeTypeReferenceExpression(desc.MemberInfo.DeclaringType.FullName), desc.MemberInfo.Name);
                        rightExpr = fieldRef;
                        added = true;
                    }
                    // static property ref
                    else if (desc.MemberInfo is PropertyInfo) {
                        if (WebFormsCompilation.Enabled) Debug.WriteLine("persistinfo is a property ref");
                        CodePropertyReferenceExpression propRef = new CodePropertyReferenceExpression(BuildGlobalCodeTypeReferenceExpression(desc.MemberInfo.DeclaringType.FullName), desc.MemberInfo.Name);
                        rightExpr = propRef;
                        added = true;
                    }

                    // static method invoke
                    //
                    else {
                        object[] args = new object[desc.Arguments.Count];
                        desc.Arguments.CopyTo(args, 0);
                        CodeExpression[] expressions = new CodeExpression[args.Length];
                        
                        if (desc.MemberInfo is MethodInfo) {
                            MethodInfo mi = (MethodInfo)desc.MemberInfo;
                            ParameterInfo[] parameters = mi.GetParameters();
                            
                            for(int i = 0; i < args.Length; i++) {
                                expressions[i] = GenerateExpressionForValue(null, args[i], parameters[i].ParameterType);
                            }
                            
                            if (WebFormsCompilation.Enabled) Debug.WriteLine("persistinfo is a method invoke");
                            CodeMethodInvokeExpression methCall = new CodeMethodInvokeExpression(BuildGlobalCodeTypeReferenceExpression(desc.MemberInfo.DeclaringType.FullName), desc.MemberInfo.Name);
                            foreach (CodeExpression e in expressions) {
                                methCall.Parameters.Add(e);
                            }
                            rightExpr = new CodeCastExpression(valueType, methCall);
                            added = true;
                        }
                        else if (desc.MemberInfo is ConstructorInfo) {
                            ConstructorInfo ci = (ConstructorInfo)desc.MemberInfo;
                            ParameterInfo[] parameters = ci.GetParameters();
                            
                            for(int i = 0; i < args.Length; i++) {
                                expressions[i] = GenerateExpressionForValue(null, args[i], parameters[i].ParameterType);
                            }
                        
                            if (WebFormsCompilation.Enabled) Debug.WriteLine("persistinfo is a constructor call");
                            CodeObjectCreateExpression objectCreate = new CodeObjectCreateExpression(desc.MemberInfo.DeclaringType.FullName);
                            foreach (CodeExpression e in expressions) {
                                objectCreate.Parameters.Add(e);
                            }
                            rightExpr = objectCreate;
                            added = true;
                        }
                    }
                }
            }

            if (!added) {
#if DEBUG
                if (WebFormsCompilation.Enabled) {
                    Debug.WriteLine("unabled to determine type, attempting Parse");
                    Debug.Indent();
                    Debug.WriteLine("value.GetType  == " + value.GetType().FullName);
                    Debug.WriteLine("value.ToString == " + value.ToString());
                    Debug.WriteLine("valueType      == " + valueType.FullName);
                    if (propertyInfo != null) {
                        Debug.WriteLine("propertyInfo   == " + propertyInfo.ReflectedType.FullName + "." + propertyInfo.Name + " : " + propertyInfo.PropertyType.FullName);
                    }
                    else {
                        Debug.WriteLine("propertyInfo   == (null)");
                    }

                    Debug.Unindent();
                }
#endif // DEBUG


                // Not a known type: try calling Parse

                // If possible, pass it an InvariantCulture (ASURT 79412)
                if (valueType.GetMethod("Parse", new Type[] {typeof(string), typeof(CultureInfo)}) != null) {
                    CodeMethodInvokeExpression methCall = new CodeMethodInvokeExpression(BuildGlobalCodeTypeReferenceExpression(valueType.FullName), "Parse");

                    // Convert the object to a string.
                    // If we have a type converter, use it to convert to a string in a culture
                    // invariant way (ASURT 87094)
                    string s;
                    if (converter != null) {
                        s = converter.ConvertToInvariantString(value);
                    }
                    else {
                        s = value.ToString();
                    }

                    methCall.Parameters.Add(new CodePrimitiveExpression(s));
                    methCall.Parameters.Add(new CodePropertyReferenceExpression(BuildGlobalCodeTypeReferenceExpression(typeof(CultureInfo)), "InvariantCulture"));
                    rightExpr = methCall;

                }
                else if (valueType.GetMethod("Parse", new Type[] {typeof(string)}) != null) {
                    // Otherwise, settle for passing just the string
                    CodeMethodInvokeExpression methCall = new CodeMethodInvokeExpression(BuildGlobalCodeTypeReferenceExpression(valueType.FullName), "Parse");
                    methCall.Parameters.Add(new CodePrimitiveExpression(value.ToString()));
                    rightExpr = methCall;

                }
                else {
                    throw new HttpException(SR.GetString(SR.CantGenPropertySet, propertyInfo.Name, valueType.FullName));
                }
            }
        }

#if DEBUG
        if (WebFormsCompilation.Enabled) {
            Debug.Unindent();
            Debug.WriteLine("}");
        }
#endif // DEBUG
        return rightExpr;
    }

    // Adds a property assignment statement to "statements". This takes into account
    // checking for nulls when the destination property type is a value type. This can
    // also generate expressions that use IAttributeAccessor when desinationType is null.
    internal static void CreatePropertySetStatements(CodeStatementCollection methodStatements, CodeStatementCollection statements,
        CodeExpression target, string targetPropertyName, Type destinationType,
        CodeExpression value,
        CodeLinePragma linePragma) {
        // Generates:
        // If destination is property:
        //     If destination type is string:
        //         {{target}}.{{targetPropertyName}} = System.Convert.ToString( {{value}} );
        //     Else If destination type is reference type:
        //         {{target}}.{{targetPropertyName}} = ( {{destinationType}} ) {{value}};
        //     Else destination type is value type:
        //         {{target}}.{{targetPropertyName}} = ( {{destinationType}} ) ({value});
        // Else use SetAttribute (expandos):
        //     ((IAttributeAccessor) {{target}} ).SetAttribute( {{targetPropertyName}} , System.Convert.ToString( {{value}} ));

        bool useSetAttribute = false;
        // This signifies it's using SetAttribute
        if (destinationType == null) {
            useSetAttribute = true;
        }

        if (useSetAttribute) {
            // ((IAttributeAccessor) {{target}} ).SetAttribute( {{targetPropertyName}} , System.Convert.ToString( {{value}} ));
            CodeMethodInvokeExpression methodInvoke = new CodeMethodInvokeExpression();
            CodeExpressionStatement setAttributeCall = new CodeExpressionStatement(methodInvoke);
            setAttributeCall.LinePragma = linePragma;

            // Dev11 128332: Ensure style attribute on html control is lowercase as required for xhtml validation
            if (targetPropertyName.Equals("Style", StringComparison.Ordinal)) {
                targetPropertyName = "style";
            }

            methodInvoke.Method.TargetObject = new CodeCastExpression(typeof(IAttributeAccessor), target);
            methodInvoke.Method.MethodName = "SetAttribute";
            methodInvoke.Parameters.Add(new CodePrimitiveExpression(targetPropertyName));

            methodInvoke.Parameters.Add(GenerateConvertToString(value));

            statements.Add(setAttributeCall);
        }
        else {
            // Use the property setter. Must take into account that null cannot be
            // cast to a value type, so we have to explicitly check for that in
            // the code we generate.

            if (destinationType.IsValueType) {
                //         {{target}}.{{targetPropertyName}} = ( {{destinationType}} ) ({value});
                CodeAssignStatement assignStmt = new CodeAssignStatement(
                        BuildPropertyReferenceExpression(target, targetPropertyName),
                        new CodeCastExpression(destinationType, value));
                assignStmt.LinePragma = linePragma;
                statements.Add(assignStmt);
            }
            else {
                CodeExpression rightSide;

                if (destinationType == typeof(string)) {
                    // {{target}}.{{targetPropertyName}} = System.Convert.ToString( {{value}} );
                    rightSide = GenerateConvertToString(value);
                }
                else {
                    // {{target}}.{{targetPropertyName}} = ( {{destinationType}} ) {{value}};
                    rightSide = new CodeCastExpression(destinationType, value);
                }

                CodeAssignStatement assignStmt = new CodeAssignStatement(
                    BuildPropertyReferenceExpression(target, targetPropertyName),
                    rightSide);
                assignStmt.LinePragma = linePragma;
                statements.Add(assignStmt);
            }
        }
    }

    // Generate a call to System.Convert.ToString(value, CultureInfo.CurrentCulture)
    internal static CodeExpression GenerateConvertToString(CodeExpression value) {

        CodeMethodInvokeExpression invokeExpr = new CodeMethodInvokeExpression();
        invokeExpr.Method.TargetObject = BuildGlobalCodeTypeReferenceExpression(typeof(System.Convert));
        invokeExpr.Method.MethodName = "ToString";
        invokeExpr.Parameters.Add(value);
        invokeExpr.Parameters.Add(new CodePropertyReferenceExpression(
            BuildGlobalCodeTypeReferenceExpression(typeof(CultureInfo)), "CurrentCulture"));

        return invokeExpr;
    }

    // Prepend a string TO the CompilerOptions string
    internal static void PrependCompilerOption(
        CompilerParameters compilParams, string compilerOptions) {

        if (compilParams.CompilerOptions == null)
            compilParams.CompilerOptions = compilerOptions;
        else
            compilParams.CompilerOptions = compilerOptions + " " + compilParams.CompilerOptions;
    }

    // Append a string to the CompilerOptions string
    internal static void AppendCompilerOption(
        CompilerParameters compilParams, string compilerOptions) {

        if (compilParams.CompilerOptions == null)
            compilParams.CompilerOptions = compilerOptions;
        else
            compilParams.CompilerOptions = compilParams.CompilerOptions + " " + compilerOptions;
    }

    internal static CodeExpression BuildPropertyReferenceExpression(
        CodeExpression objRefExpr, string propName) {

        // The name may contain several '.' separated properties, so we
        // need to make sure we build the CodeDom accordingly (ASURT 91875, VSWhidbey 313018)
        string[] parts = propName.Split('.');
        CodeExpression ret = objRefExpr;
        foreach (string part in parts)
            ret = new CodePropertyReferenceExpression(ret, part);

        return ret;
    }

    internal static CodeCastExpression BuildJSharpCastExpression(Type castType, CodeExpression expression) {
        
        // VJ# does not support automatic boxing of value types, so passing a simple type, say a bool, into a method
        // expecting an object will give a compiler error.  We are working around this issue by adding special code for
        // VJ# that will cast the expression for boxing.  When the VJ# team adds implicit boxing of value types, we
        // should remove this code.  VSWhidbey 269028
        CodeCastExpression castedExpression = new CodeCastExpression(castType, expression); 
        castedExpression.UserData.Add("CastIsBoxing", true);
        return castedExpression;
    }

    internal static CodeTypeReference BuildGlobalCodeTypeReference(string typeName) {
        return new CodeTypeReference(typeName, CodeTypeReferenceOptions.GlobalReference);
    }

    internal static CodeTypeReference BuildGlobalCodeTypeReference(Type type) {
        return new CodeTypeReference(type, CodeTypeReferenceOptions.GlobalReference);
    }

    private static CodeTypeReferenceExpression BuildGlobalCodeTypeReferenceExpression(string typeName) {
        // Returns an expression that resolves the type name from the root namespace, 
        // eg global::Namespace.TypeName
        CodeTypeReference codeTypeReference = BuildGlobalCodeTypeReference(typeName);
        CodeTypeReferenceExpression codeTypeReferenceExpression = new CodeTypeReferenceExpression(codeTypeReference);
        return codeTypeReferenceExpression;
    }

    private static CodeTypeReferenceExpression BuildGlobalCodeTypeReferenceExpression(Type type) {
        // Returns an expression that resolves the type name from the root namespace, 
        // eg global::Namespace.TypeName
        CodeTypeReference codeTypeReference = BuildGlobalCodeTypeReference(type);
        CodeTypeReferenceExpression codeTypeReferenceExpression = new CodeTypeReferenceExpression(codeTypeReference);
        return codeTypeReferenceExpression;
    }

}
}

