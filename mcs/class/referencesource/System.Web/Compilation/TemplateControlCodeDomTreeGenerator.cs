//------------------------------------------------------------------------------
// <copyright file="TemplateControlCodeDomTreeGenerator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Compilation {

using System;
using System.Collections;
using System.Reflection;
using System.ComponentModel;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Globalization;
using System.Web.UI;
using System.Web.Util;
using Debug=System.Web.Util.Debug;

internal abstract class TemplateControlCodeDomTreeGenerator : BaseTemplateCodeDomTreeGenerator {

    private const string stringResourcePointerName = "__stringResource";

    private TemplateControlParser _tcParser;
    private TemplateControlParser Parser { get { return _tcParser; } }
    private const string literalMemoryBlockName = "__literals";

    // This is used to detect incorrect base class in code beside scenarios.  See usage for details.
    internal const int badBaseClassLineMarker = 912304;

    internal TemplateControlCodeDomTreeGenerator(TemplateControlParser tcParser) : base(tcParser) {
        _tcParser = tcParser;
    }

    /*
     * Build the default constructor
     */
    protected override void BuildInitStatements(CodeStatementCollection trueStatements, CodeStatementCollection topLevelStatements) {

        base.BuildInitStatements(trueStatements, topLevelStatements);

        if (_stringResourceBuilder.HasStrings) {
            // e.g. private static object __stringResource;
            CodeMemberField stringResourcePointer = new CodeMemberField(typeof(Object), stringResourcePointerName);
            stringResourcePointer.Attributes |= MemberAttributes.Static;
            _sourceDataClass.Members.Add(stringResourcePointer);

            // e.g. __stringResource = TemplateControl.ReadStringResource(typeof(__GeneratedType));
            CodeAssignStatement readResource = new CodeAssignStatement();
            readResource.Left = new CodeFieldReferenceExpression(_classTypeExpr,
                                                             stringResourcePointerName);
            CodeMethodInvokeExpression methCallExpression = new CodeMethodInvokeExpression();
            methCallExpression.Method.TargetObject = new CodeThisReferenceExpression();
            methCallExpression.Method.MethodName = "ReadStringResource";
            readResource.Right = methCallExpression;
            trueStatements.Add(readResource);
        }
        //
        // Set the AppRelativeVirtualPath
        // e.g. ((System.Web.UI.Page)(this)).AppRelativeVirtualPath = "~/foo.aspx";
        // Note that we generate an artificial cast to cause a compile error if the base class
        // is incorrect (see below).
        //
        // Make sure the BuildAppRelativeVirtualPathProperty property is app independent, since
        // in precompilation scenarios, we can't make an assumption on the app name.

        // Use global:: to resolve types to avoid naming conflicts when user uses a class name
        // in the global namespace that already exists, such as Login or ReportViewer (DevDiv 79336)
        CodeTypeReference classTypeRef = CodeDomUtility.BuildGlobalCodeTypeReference(Parser.BaseType);

        CodeAssignStatement setProp = new CodeAssignStatement(
            new CodePropertyReferenceExpression(new CodeCastExpression(classTypeRef, new CodeThisReferenceExpression()), "AppRelativeVirtualPath"),
            new CodePrimitiveExpression(Parser.CurrentVirtualPath.AppRelativeVirtualPathString));

        // This line will fail to compile if the base class in the code beside is missing.  Set
        // a special line number on it to improve error handling (VSWhidbey 376977/468830)
        if (!_designerMode && Parser.CodeFileVirtualPath != null) {
            setProp.LinePragma = CreateCodeLinePragmaHelper(
                Parser.CodeFileVirtualPath.VirtualPathString, badBaseClassLineMarker);
        }
        topLevelStatements.Add(setProp);
    }

    /*
     * Build various properties, fields, methods
     */
    protected override void BuildMiscClassMembers() {
        base.BuildMiscClassMembers();

        // Build the automatic event hookup code
        if (!_designerMode)
            BuildAutomaticEventHookup();

        // Build the ApplicationInstance property
        BuildApplicationInstanceProperty();

        if (_designerMode) {
            GenerateDummyBindMethodsAtDesignTime();
        }

        BuildSourceDataTreeFromBuilder(Parser.RootBuilder,
            false /*fInTemplate*/, false /*topLevelTemplate*/, null /*pse*/);

        if (!_designerMode)
            BuildFrameworkInitializeMethod();
    }

    /*
     * Build the strongly typed new property
     */
    // e.g. public new {propertyType} Master { get { return ({propertyType})base.Master; } }
    internal void BuildStronglyTypedProperty(string propertyName, Type propertyType) {
        // VSWhidbey 321818.
        // overriding method with same name is not allowed using J#.
        if (_usingVJSCompiler) {
            return;
        }

        CodeMemberProperty prop = new CodeMemberProperty();
        prop.Attributes &= ~MemberAttributes.AccessMask;
        prop.Attributes &= ~MemberAttributes.ScopeMask;
        prop.Attributes |= MemberAttributes.Final | MemberAttributes.New | MemberAttributes.Public;
        prop.Name = propertyName;
        prop.Type = new CodeTypeReference(propertyType);

        CodePropertyReferenceExpression propRef = new CodePropertyReferenceExpression(
            new CodeBaseReferenceExpression(), propertyName);

        prop.GetStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(propertyType, propRef)));
        _intermediateClass.Members.Add(prop);
    }

    private void GenerateDummyBindMethodsAtDesignTime() {
        // public string Bind(string expression,string format) {return String.Empty;}
        GenerateBindMethod(addFormatParameter: true);
        
        // public string Bind(string expression) {return String.Empty;}
        GenerateBindMethod(addFormatParameter: false);
    }

    private void GenerateBindMethod(bool addFormatParameter) {
        if (_sourceDataClass == null) {
            return;
        }

        CodeMemberMethod bindMethod = new CodeMemberMethod();
        bindMethod.Name = "Bind";
        bindMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "expression"));
        if (addFormatParameter) {
            bindMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "format"));
        }
        bindMethod.ReturnType = new CodeTypeReference(typeof(string));
        bindMethod.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(String.Empty)));
        _sourceDataClass.Members.Add(bindMethod);
    }

    /*
     * Build the data tree for the FrameworkInitialize method
     */
    private void BuildFrameworkInitializeMethod() {

        // Skip if we're only generating the intermediate class
        if (_sourceDataClass == null)
            return;

        CodeMemberMethod method = new CodeMemberMethod();
        AddDebuggerNonUserCodeAttribute(method);
        method.Attributes &= ~MemberAttributes.AccessMask;
        method.Attributes &= ~MemberAttributes.ScopeMask;
        method.Attributes |= MemberAttributes.Override | MemberAttributes.Family;
        method.Name = "FrameworkInitialize";

        BuildFrameworkInitializeMethodContents(method);

        // This line will fail to compile if the base class in the code beside is incorrect.  Set
        // a special line number on it to improve error handling (VSWhidbey 376977/468830)
        if (!_designerMode && Parser.CodeFileVirtualPath != null) {
            method.LinePragma = CreateCodeLinePragmaHelper(
                Parser.CodeFileVirtualPath.VirtualPathString, badBaseClassLineMarker);
        }

        _sourceDataClass.Members.Add(method);
    }

    /*
     * Build the contents of the FrameworkInitialize method
     */
    protected virtual void BuildFrameworkInitializeMethodContents(CodeMemberMethod method) {

        // Call the base FrameworkInitialize
        CodeMethodInvokeExpression baseCallExpression = new CodeMethodInvokeExpression(
            new CodeBaseReferenceExpression(), method.Name);
        method.Statements.Add(new CodeExpressionStatement(baseCallExpression));

        // No strings: don't do anything
        if (_stringResourceBuilder.HasStrings) {

            // e.g. SetStringResourcePointer(__stringResource, 0);
            CodeMethodInvokeExpression methCallExpression = new CodeMethodInvokeExpression(
                new CodeThisReferenceExpression(), "SetStringResourcePointer");
            methCallExpression.Parameters.Add(new CodeFieldReferenceExpression(
                _classTypeExpr, stringResourcePointerName));
            // Pass 0 for the maxResourceOffset, since it's being ignored
            methCallExpression.Parameters.Add(new CodePrimitiveExpression(0));
            method.Statements.Add(new CodeExpressionStatement(methCallExpression));
        }

        CodeMethodInvokeExpression call = new CodeMethodInvokeExpression();
        call.Method.TargetObject = new CodeThisReferenceExpression();
        call.Method.MethodName = "__BuildControlTree";
        call.Parameters.Add(new CodeThisReferenceExpression());
        method.Statements.Add(new CodeExpressionStatement(call));
    }

    /*
     * Build the automatic event hookup code
     */
    private void BuildAutomaticEventHookup() {

        // Skip if we're only generating the intermediate class
        if (_sourceDataClass == null)
            return;

        CodeMemberProperty prop;

        // If FAutoEventWireup is turned off, generate a SupportAutoEvents prop that
        // returns false.
        if (!Parser.FAutoEventWireup) {
            prop = new CodeMemberProperty();
            prop.Attributes &= ~MemberAttributes.AccessMask;
            prop.Attributes &= ~MemberAttributes.ScopeMask;
            prop.Attributes |= MemberAttributes.Override | MemberAttributes.Family;
            prop.Name = "SupportAutoEvents";
            prop.Type = new CodeTypeReference(typeof(bool));
            prop.GetStatements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(false)));
            _sourceDataClass.Members.Add(prop);
            return;
        }
    }

    /*
     * Build the ApplicationInstance property
     */
    private void BuildApplicationInstanceProperty() {

        CodeMemberProperty prop;

        Type appType = BuildManager.GetGlobalAsaxType();

        prop = new CodeMemberProperty();
        prop.Attributes &= ~MemberAttributes.AccessMask;
        prop.Attributes &= ~MemberAttributes.ScopeMask;
        prop.Attributes |= MemberAttributes.Final | MemberAttributes.Family;

        if (_designerMode) {
            ApplyEditorBrowsableCustomAttribute(prop);
        }

        prop.Name = "ApplicationInstance";
        prop.Type = new CodeTypeReference(appType);

        CodePropertyReferenceExpression propRef = new CodePropertyReferenceExpression(
            new CodeThisReferenceExpression(), "Context");
        propRef = new CodePropertyReferenceExpression(propRef, "ApplicationInstance");

        prop.GetStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(
            appType, propRef)));
        _intermediateClass.Members.Add(prop);
    }
}
}
