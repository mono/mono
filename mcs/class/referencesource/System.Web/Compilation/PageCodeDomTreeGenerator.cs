//------------------------------------------------------------------------------
// <copyright file="PageCodeDomTreeGenerator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Compilation {

using System;
using System.CodeDom;
using System.Reflection;
using System.Web.Configuration;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.Util;
using Debug = System.Web.Util.Debug;

internal class PageCodeDomTreeGenerator : TemplateControlCodeDomTreeGenerator {

    private PageParser _pageParser;
    PageParser Parser { get { return _pageParser; } }

    private const string fileDependenciesName = "__fileDependencies";
    private const string dependenciesLocalName = "dependencies";
    private const string outputCacheSettingsLocalName = "outputCacheSettings";
    private const string _previousPagePropertyName = "PreviousPage";
    private const string _masterPropertyName = "Master";
    private const string _styleSheetThemePropertyName = "StyleSheetTheme";
    private const string outputCacheSettingsFieldName = "__outputCacheSettings";

    internal const int  DebugScriptTimeout = 30000000;

    internal PageCodeDomTreeGenerator(PageParser pageParser) : base(pageParser) {
        _pageParser = pageParser;
    }

    /*
     * Generate the list of implemented interfaces
     */
    protected override void GenerateInterfaces() {

        base.GenerateInterfaces();

        if (Parser.FRequiresSessionState) {
            _intermediateClass.BaseTypes.Add(new CodeTypeReference(typeof(IRequiresSessionState)));
        }
        if (Parser.FReadOnlySessionState) {
            _intermediateClass.BaseTypes.Add(new CodeTypeReference(typeof(IReadOnlySessionState)));
        }

        // Skip if we're only generating the intermediate class
        if (!_designerMode && _sourceDataClass != null && (Parser.AspCompatMode || Parser.AsyncMode)) {
            _sourceDataClass.BaseTypes.Add(new CodeTypeReference(typeof(IHttpAsyncHandler)));
        }
    }

    /*
     * Build first-time intialization statements
     */
    protected override void BuildInitStatements(CodeStatementCollection trueStatements, CodeStatementCollection topLevelStatements) {

        base.BuildInitStatements(trueStatements, topLevelStatements);

        // 



        CodeMemberField fileDependencies = new CodeMemberField(typeof(object), fileDependenciesName);
        fileDependencies.Attributes |= MemberAttributes.Static;
        _sourceDataClass.Members.Add(fileDependencies);

        // Note: it may look like this local variable declaration is redundant. However it is necessary
        // to make this init code re-entrant safe. This way, even if two threads enter the contructor
        // at the same time, they will not add multiple dependencies.

        // e.g. string[] dependencies;
        CodeVariableDeclarationStatement dependencies = new CodeVariableDeclarationStatement();
        dependencies.Type = new CodeTypeReference(typeof(string[]));
        dependencies.Name = dependenciesLocalName;
        // Note: it is important to add all local variables at the top level for CodeDom Subset compliance.
        topLevelStatements.Insert(0, dependencies);

        Debug.Assert(Parser.SourceDependencies != null);

        StringSet virtualDependencies = new StringSet();
        virtualDependencies.AddCollection(Parser.SourceDependencies);

        // e.g. dependencies = new string[{{virtualDependencies.Count}}];;
        CodeAssignStatement assignDependencies = new CodeAssignStatement();
        assignDependencies.Left =
            new CodeVariableReferenceExpression(dependenciesLocalName);
        assignDependencies.Right =
            new CodeArrayCreateExpression(typeof(String), virtualDependencies.Count);
        trueStatements.Add(assignDependencies);

        int i = 0;
        foreach (string virtualDependency in virtualDependencies) {
            // e.g. dependencies[i] = "~/sub/foo.aspx";
            CodeAssignStatement addFileDep = new CodeAssignStatement();
            addFileDep.Left =
                new CodeArrayIndexerExpression(
                    new CodeVariableReferenceExpression(dependenciesLocalName),
                    new CodeExpression[] {new CodePrimitiveExpression(i++)});
            string src = UrlPath.MakeVirtualPathAppRelative(virtualDependency);
            addFileDep.Right = new CodePrimitiveExpression(src);
            trueStatements.Add(addFileDep);
        }

        // e.g. __fileDependencies = this.GetWrappedFileDependencies(dependencies);
        CodeAssignStatement initFile = new CodeAssignStatement();
        initFile.Left = new CodeFieldReferenceExpression(_classTypeExpr, fileDependenciesName);
        CodeMethodInvokeExpression createWrap = new CodeMethodInvokeExpression();
        createWrap.Method.TargetObject = new CodeThisReferenceExpression();
        createWrap.Method.MethodName = "GetWrappedFileDependencies";
        createWrap.Parameters.Add(new CodeVariableReferenceExpression(dependenciesLocalName));
        initFile.Right = createWrap;

#if DBG
        AppendDebugComment(trueStatements);
#endif
        trueStatements.Add(initFile);
    }

    /*
     * Build the default constructor
     */
    protected override void BuildDefaultConstructor() {

        base.BuildDefaultConstructor();

        if (CompilParams.IncludeDebugInformation) {
            // If in debug mode, set the timeout to some huge value (ASURT 49427)
            //      Server.ScriptTimeout = 30000000;
            CodeAssignStatement setScriptTimeout = new CodeAssignStatement();
            setScriptTimeout.Left = new CodePropertyReferenceExpression(
                new CodePropertyReferenceExpression(
                    new CodeThisReferenceExpression(), "Server"),
                "ScriptTimeout");
            setScriptTimeout.Right = new CodePrimitiveExpression(DebugScriptTimeout);
            _ctor.Statements.Add(setScriptTimeout);

        }

        if (Parser.TransactionMode != 0 /*TransactionOption.Disabled*/) {
            _ctor.Statements.Add(new CodeAssignStatement(
                new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "TransactionMode"),
                new CodePrimitiveExpression(Parser.TransactionMode)));
        }

        if (Parser.AspCompatMode) {
            _ctor.Statements.Add(new CodeAssignStatement(
                new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "AspCompatMode"),
                new CodePrimitiveExpression(Parser.AspCompatMode)));
        }

        if (Parser.AsyncMode) {
            _ctor.Statements.Add(new CodeAssignStatement(
                new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "AsyncMode"),
                new CodePrimitiveExpression(Parser.AsyncMode)));
        }

        if (Parser.OutputCacheParameters != null) {
            OutputCacheParameters cacheSettings = Parser.OutputCacheParameters;
            if ((cacheSettings.CacheProfile != null && cacheSettings.CacheProfile.Length != 0) ||
                cacheSettings.Duration != 0 ||
                cacheSettings.Location == OutputCacheLocation.None) {

                // Add the following code snippet as a static on the class:
                //
                // private static OutputCacheParameters __outputCacheSettings = null;
                //
                CodeMemberField outputCacheSettingsField = new CodeMemberField(typeof(OutputCacheParameters), outputCacheSettingsFieldName);
                outputCacheSettingsField.Attributes |= MemberAttributes.Static;
                outputCacheSettingsField.InitExpression = new CodePrimitiveExpression(null);
                _sourceDataClass.Members.Add(outputCacheSettingsField);

                // Then, add the following code to the default constructor:
                //
                // if (__outputCacheSettings == null)
                //     __outputCacheSettings = new OutputCacheParameters(.....)
                //

                // This is the "if (__outputCacheSettings == null)" part
                CodeConditionStatement outputCacheSettingsCondition = new CodeConditionStatement();
                outputCacheSettingsCondition.Condition = new CodeBinaryOperatorExpression(
                                                        new CodeFieldReferenceExpression(
                                                            _classTypeExpr,
                                                            outputCacheSettingsFieldName),
                                                        CodeBinaryOperatorType.IdentityEquality,
                                                        new CodePrimitiveExpression(null));

                // This is the "__outputCacheSettings = new OutputCacheParameters()" part


                // e.g. declare local variable: OutputCacheParameters outputCacheSettings;
                CodeVariableDeclarationStatement outputCacheSettingsDeclaration = new CodeVariableDeclarationStatement();
                outputCacheSettingsDeclaration.Type = new CodeTypeReference(typeof(OutputCacheParameters));
                outputCacheSettingsDeclaration.Name = outputCacheSettingsLocalName;
                outputCacheSettingsCondition.TrueStatements.Insert(0, outputCacheSettingsDeclaration);

                // e.g. outputCacheSettings = new outputCacheParameters;
                CodeObjectCreateExpression cacheSettingsObject = new CodeObjectCreateExpression();
                cacheSettingsObject.CreateType = new CodeTypeReference(typeof(OutputCacheParameters));

                CodeVariableReferenceExpression outputCacheSettings = 
                    new CodeVariableReferenceExpression(outputCacheSettingsLocalName);

                CodeAssignStatement setOutputCacheObject = 
                    new CodeAssignStatement(outputCacheSettings, cacheSettingsObject);

                // Add the statement to the "true" clause
                outputCacheSettingsCondition.TrueStatements.Add(setOutputCacheObject);

                if (cacheSettings.IsParameterSet(OutputCacheParameter.CacheProfile)) {
                    CodeAssignStatement setPropertyStatement = new CodeAssignStatement(
                                                                   new CodePropertyReferenceExpression(outputCacheSettings, "CacheProfile"),
                                                                   new CodePrimitiveExpression(cacheSettings.CacheProfile));
                    
                    outputCacheSettingsCondition.TrueStatements.Add(setPropertyStatement);
                }

                if (cacheSettings.IsParameterSet(OutputCacheParameter.Duration)) {
                    CodeAssignStatement setPropertyStatement = new CodeAssignStatement(
                                                                   new CodePropertyReferenceExpression(outputCacheSettings, "Duration"),
                                                                   new CodePrimitiveExpression(cacheSettings.Duration));
                    
                    outputCacheSettingsCondition.TrueStatements.Add(setPropertyStatement);
                }

                if (cacheSettings.IsParameterSet(OutputCacheParameter.Enabled)) {
                    CodeAssignStatement setPropertyStatement = new CodeAssignStatement(
                                                                   new CodePropertyReferenceExpression(outputCacheSettings, "Enabled"),
                                                                   new CodePrimitiveExpression(cacheSettings.Enabled));
                    
                    outputCacheSettingsCondition.TrueStatements.Add(setPropertyStatement);
                }

                if (cacheSettings.IsParameterSet(OutputCacheParameter.Location)) {
                    CodeAssignStatement setPropertyStatement = new CodeAssignStatement(
                                                                   new CodePropertyReferenceExpression(outputCacheSettings, "Location"),
                                                                   new CodeFieldReferenceExpression(
                                                                        new CodeTypeReferenceExpression(typeof(OutputCacheLocation)), 
                                                                        cacheSettings.Location.ToString()));
                    
                    outputCacheSettingsCondition.TrueStatements.Add(setPropertyStatement);
                }

                if (cacheSettings.IsParameterSet(OutputCacheParameter.NoStore)) {
                    CodeAssignStatement setPropertyStatement = new CodeAssignStatement(
                                                                   new CodePropertyReferenceExpression(outputCacheSettings, "NoStore"),
                                                                   new CodePrimitiveExpression(cacheSettings.NoStore));
                    
                    outputCacheSettingsCondition.TrueStatements.Add(setPropertyStatement);
                }

                if (cacheSettings.IsParameterSet(OutputCacheParameter.SqlDependency)) {
                    CodeAssignStatement setPropertyStatement = new CodeAssignStatement(
                                                                   new CodePropertyReferenceExpression(outputCacheSettings, "SqlDependency"),
                                                                   new CodePrimitiveExpression(cacheSettings.SqlDependency));
                    
                    outputCacheSettingsCondition.TrueStatements.Add(setPropertyStatement);
                }

                if (cacheSettings.IsParameterSet(OutputCacheParameter.VaryByControl)) {
                    CodeAssignStatement setPropertyStatement = new CodeAssignStatement(
                                                                   new CodePropertyReferenceExpression(outputCacheSettings, "VaryByControl"),
                                                                   new CodePrimitiveExpression(cacheSettings.VaryByControl));
                    
                    outputCacheSettingsCondition.TrueStatements.Add(setPropertyStatement);
                }

                if (cacheSettings.IsParameterSet(OutputCacheParameter.VaryByCustom)) {
                    CodeAssignStatement setPropertyStatement = new CodeAssignStatement(
                                                                   new CodePropertyReferenceExpression(outputCacheSettings, "VaryByCustom"),
                                                                   new CodePrimitiveExpression(cacheSettings.VaryByCustom));
                    
                    outputCacheSettingsCondition.TrueStatements.Add(setPropertyStatement);
                }

                if (cacheSettings.IsParameterSet(OutputCacheParameter.VaryByContentEncoding)) {
                    CodeAssignStatement setPropertyStatement = new CodeAssignStatement(
                                                                   new CodePropertyReferenceExpression(outputCacheSettings, "VaryByContentEncoding"),
                                                                   new CodePrimitiveExpression(cacheSettings.VaryByContentEncoding));
                    
                    outputCacheSettingsCondition.TrueStatements.Add(setPropertyStatement);
                }
                if (cacheSettings.IsParameterSet(OutputCacheParameter.VaryByHeader)) {
                    CodeAssignStatement setPropertyStatement = new CodeAssignStatement(
                                                                   new CodePropertyReferenceExpression(outputCacheSettings, "VaryByHeader"),
                                                                   new CodePrimitiveExpression(cacheSettings.VaryByHeader));
                    
                    outputCacheSettingsCondition.TrueStatements.Add(setPropertyStatement);
                }

                if (cacheSettings.IsParameterSet(OutputCacheParameter.VaryByParam)) {
                    CodeAssignStatement setPropertyStatement = new CodeAssignStatement(
                                                                   new CodePropertyReferenceExpression(outputCacheSettings, "VaryByParam"),
                                                                   new CodePrimitiveExpression(cacheSettings.VaryByParam));
                    
                    outputCacheSettingsCondition.TrueStatements.Add(setPropertyStatement);
                }

                // e.g. __outputCacheSettings = outputCacheSettings;
                CodeFieldReferenceExpression staticOutputCacheSettings = 
                    new CodeFieldReferenceExpression(_classTypeExpr, outputCacheSettingsFieldName);
                CodeAssignStatement assignOutputCacheSettings = 
                    new CodeAssignStatement(staticOutputCacheSettings, outputCacheSettings);
                // Add the statement to the "true" clause
                outputCacheSettingsCondition.TrueStatements.Add(assignOutputCacheSettings);

                _ctor.Statements.Add(outputCacheSettingsCondition);
            }
        }
    }

    /*
     * Build various properties, fields, methods
     */
    protected override void BuildMiscClassMembers() {
        base.BuildMiscClassMembers();

        // The following method should not be built in designer mode, and should only be built
        // when we're generating the full class (as opposed to the partial stub)
        if (!_designerMode && _sourceDataClass != null) {
            BuildGetTypeHashCodeMethod();

            if (Parser.AspCompatMode)
                BuildAspCompatMethods();

            if (Parser.AsyncMode)
                BuildAsyncPageMethods();

            BuildProcessRequestOverride();
        }

        if (Parser.PreviousPageType != null)
            BuildStronglyTypedProperty(_previousPagePropertyName, Parser.PreviousPageType);

        if (Parser.MasterPageType != null)
            BuildStronglyTypedProperty(_masterPropertyName, Parser.MasterPageType);


    }

    /*
     * Build the data tree for the GetTypeHashCode method
     */
    private void BuildGetTypeHashCodeMethod() {

        CodeMemberMethod method = new CodeMemberMethod();
        AddDebuggerNonUserCodeAttribute(method);
        method.Name = "GetTypeHashCode";
        method.ReturnType = new CodeTypeReference(typeof(int));
        method.Attributes &= ~MemberAttributes.AccessMask;
        method.Attributes &= ~MemberAttributes.ScopeMask;
        method.Attributes |= MemberAttributes.Override | MemberAttributes.Public;

        _sourceDataClass.Members.Add(method);

#if DBG
        AppendDebugComment(method.Statements);
#endif
        method.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(Parser.TypeHashCode)));
    }

    internal override CodeExpression BuildPagePropertyReferenceExpression() {
        return new CodeThisReferenceExpression();
    }

    /*
     * Build the contents of the FrameworkInitialize method
     */
    protected override void BuildFrameworkInitializeMethodContents(CodeMemberMethod method) {

        // Generate code to apply stylesheet before calling base.FrameworkInitialize();
        if (Parser.StyleSheetTheme != null) {
            CodeExpression rightExpr = new CodePrimitiveExpression(Parser.StyleSheetTheme);
            CodeExpression leftExpr = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), _styleSheetThemePropertyName);
            CodeAssignStatement setStatment = new CodeAssignStatement(leftExpr, rightExpr);
            method.Statements.Add(setStatment);
        }

        base.BuildFrameworkInitializeMethodContents(method);

        CodeMethodInvokeExpression addDeps = new CodeMethodInvokeExpression();
        addDeps.Method.TargetObject = new CodeThisReferenceExpression();
        addDeps.Method.MethodName = "AddWrappedFileDependencies";
        addDeps.Parameters.Add(new CodeFieldReferenceExpression(_classTypeExpr, fileDependenciesName));
        method.Statements.Add(addDeps);

        if (Parser.OutputCacheParameters != null) {
            OutputCacheParameters cacheSettings = Parser.OutputCacheParameters;
            if ((cacheSettings.CacheProfile != null && cacheSettings.CacheProfile.Length != 0) ||
                cacheSettings.Duration != 0 ||
                cacheSettings.Location == OutputCacheLocation.None) {

                CodeMethodInvokeExpression call = new CodeMethodInvokeExpression();
                call.Method.TargetObject = new CodeThisReferenceExpression();
                call.Method.MethodName = "InitOutputCache";
                call.Parameters.Add(new CodeFieldReferenceExpression(
                                    _classTypeExpr,
                                    outputCacheSettingsFieldName));

                method.Statements.Add(call);
            }
        }

        if (Parser.TraceEnabled != TraceEnable.Default) {
            method.Statements.Add(new CodeAssignStatement(
                new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "TraceEnabled"),
                new CodePrimitiveExpression(Parser.TraceEnabled == TraceEnable.Enable)));
        }

        if (Parser.TraceMode != TraceMode.Default) {
            method.Statements.Add(new CodeAssignStatement(
                new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "TraceModeValue"),
                new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(TraceMode)), Parser.TraceMode.ToString())));
        }

        if (Parser.ValidateRequest) {
            // e.g. Request.ValidateInput();
            CodeMethodInvokeExpression invokeExpr = new CodeMethodInvokeExpression();
            invokeExpr.Method.TargetObject = new CodePropertyReferenceExpression(
                new CodeThisReferenceExpression(), "Request");
            invokeExpr.Method.MethodName = "ValidateInput";
            method.Statements.Add(new CodeExpressionStatement(invokeExpr));
        }
        else if (MultiTargetingUtil.TargetFrameworkVersion >= VersionUtil.Framework45) {
            // Only emit the ValidateRequestMode setter if the target framework is 4.5 or higher.
            // On frameworks 4.0 and earlier that property did not exist.
            CodePropertyReferenceExpression left = new CodePropertyReferenceExpression(
                new CodeThisReferenceExpression(), "ValidateRequestMode");
            CodeFieldReferenceExpression right = new CodeFieldReferenceExpression(
                new CodeTypeReferenceExpression(typeof(ValidateRequestMode)), "Disabled");
            CodeAssignStatement statement = new CodeAssignStatement(left, right);
            method.Statements.Add(statement);
        }
    }

    /*
     * Build the data tree for the AspCompat implementation for IHttpAsyncHandler:
     */
    private void BuildAspCompatMethods() {
        CodeMemberMethod method;
        CodeMethodInvokeExpression call;

        //  public IAsyncResult BeginProcessRequest(HttpContext context, Async'back cb, Object extraData) {
        //      IAsyncResult ar;
        //      ar = this.AspCompatBeginProcessRequest(context, cb, extraData);
        //      return ar;
        //  }

        method = new CodeMemberMethod();
        AddDebuggerNonUserCodeAttribute(method);
        method.Name = "BeginProcessRequest";
        method.Attributes &= ~MemberAttributes.AccessMask;
        method.Attributes &= ~MemberAttributes.ScopeMask;
        method.Attributes |= MemberAttributes.Public;
        method.ImplementationTypes.Add(new CodeTypeReference(typeof(IHttpAsyncHandler)));
        method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(HttpContext),   "context"));
        method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(AsyncCallback), "cb"));
        method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(Object),        "data"));
        method.ReturnType = new CodeTypeReference(typeof(IAsyncResult));

        CodeMethodInvokeExpression invokeExpr = new CodeMethodInvokeExpression();
        invokeExpr.Method.TargetObject = new CodeThisReferenceExpression();
        invokeExpr.Method.MethodName = "AspCompatBeginProcessRequest";
        invokeExpr.Parameters.Add(new CodeArgumentReferenceExpression("context"));
        invokeExpr.Parameters.Add(new CodeArgumentReferenceExpression("cb"));
        invokeExpr.Parameters.Add(new CodeArgumentReferenceExpression("data"));

        method.Statements.Add(new CodeMethodReturnStatement(invokeExpr));

        _sourceDataClass.Members.Add(method);

        //  public void EndProcessRequest(IAsyncResult ar) {
        //      this.AspCompatEndProcessRequest(ar);
        //  }

        method = new CodeMemberMethod();
        AddDebuggerNonUserCodeAttribute(method);
        method.Name = "EndProcessRequest";
        method.Attributes &= ~MemberAttributes.AccessMask;
        method.Attributes &= ~MemberAttributes.ScopeMask;
        method.Attributes |= MemberAttributes.Public;
        method.ImplementationTypes.Add(typeof(IHttpAsyncHandler));
        method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(IAsyncResult), "ar"));

        call = new CodeMethodInvokeExpression();
        call.Method.TargetObject = new CodeThisReferenceExpression();
        call.Method.MethodName = "AspCompatEndProcessRequest";
        call.Parameters.Add(new CodeArgumentReferenceExpression("ar"));
        method.Statements.Add(call);

        _sourceDataClass.Members.Add(method);
    }

    /*
     * Build the data tree for the Async page implementation for IHttpAsyncHandler:
     */
    private void BuildAsyncPageMethods() {
        CodeMemberMethod method;
        CodeMethodInvokeExpression call;

        //  public IAsyncResult BeginProcessRequest(HttpContext context, Async'back cb, Object extraData) {
        //      IAsyncResult ar;
        //      ar = this.AsyncPageBeginProcessRequest(context, cb, extraData);
        //      return ar;
        //  }

        method = new CodeMemberMethod();
        AddDebuggerNonUserCodeAttribute(method);
        method.Name = "BeginProcessRequest";
        method.Attributes &= ~MemberAttributes.AccessMask;
        method.Attributes &= ~MemberAttributes.ScopeMask;
        method.Attributes |= MemberAttributes.Public;
        method.ImplementationTypes.Add(new CodeTypeReference(typeof(IHttpAsyncHandler)));
        method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(HttpContext),   "context"));
        method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(AsyncCallback), "cb"));
        method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(Object),        "data"));
        method.ReturnType = new CodeTypeReference(typeof(IAsyncResult));

        CodeMethodInvokeExpression invokeExpr = new CodeMethodInvokeExpression();
        invokeExpr.Method.TargetObject = new CodeThisReferenceExpression();
        invokeExpr.Method.MethodName = "AsyncPageBeginProcessRequest";
        invokeExpr.Parameters.Add(new CodeArgumentReferenceExpression("context"));
        invokeExpr.Parameters.Add(new CodeArgumentReferenceExpression("cb"));
        invokeExpr.Parameters.Add(new CodeArgumentReferenceExpression("data"));

        method.Statements.Add(new CodeMethodReturnStatement(invokeExpr));

        _sourceDataClass.Members.Add(method);

        //  public void EndProcessRequest(IAsyncResult ar) {
        //      this.AsyncPageEndProcessRequest(ar);
        //  }

        method = new CodeMemberMethod();
        AddDebuggerNonUserCodeAttribute(method);
        method.Name = "EndProcessRequest";
        method.Attributes &= ~MemberAttributes.AccessMask;
        method.Attributes &= ~MemberAttributes.ScopeMask;
        method.Attributes |= MemberAttributes.Public;
        method.ImplementationTypes.Add(typeof(IHttpAsyncHandler));
        method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(IAsyncResult), "ar"));

        call = new CodeMethodInvokeExpression();
        call.Method.TargetObject = new CodeThisReferenceExpression();
        call.Method.MethodName = "AsyncPageEndProcessRequest";
        call.Parameters.Add(new CodeArgumentReferenceExpression("ar"));
        method.Statements.Add(call);

        _sourceDataClass.Members.Add(method);
    }

    /*
     * Build a ProcessRequest override which just calls the base.  This is used to make sure
     * there is user code on the stack when requests are executed (VSWhidbey 499386)
     */
    private void BuildProcessRequestOverride() {

        //  public override void ProcessRequest(HttpContext context) {
        //      base.ProcessRequest(context);
        //  }

        CodeMemberMethod method = new CodeMemberMethod();
        AddDebuggerNonUserCodeAttribute(method);
        method.Name = "ProcessRequest";
        method.Attributes &= ~MemberAttributes.AccessMask;
        method.Attributes &= ~MemberAttributes.ScopeMask;

        // If the base type is non-default (i.e. not Page) we have to be careful overriding
        // ProcessRequest, because if the base has its own IHttpHandler.ProcessRequest implementation
        // and it's not overridable, we would fail to compile.  So when we detect this situation,
        // we instead generate it as a new IHttpHandler.ProcessRequest implementation instead of an
        // override.  In theory, we could *always* do this, but it's safer to limit it to this
        // constrained scenario (VSWhidbey 517240)
        MethodInfo methodInfo = null;
        if (Parser.BaseType != typeof(Page)) {
            methodInfo = Parser.BaseType.GetMethod("ProcessRequest",
                BindingFlags.Public | BindingFlags.Instance,
                null, new Type[] { typeof(HttpContext) }, null);
            Debug.Assert(methodInfo != null);
        }

        _sourceDataClass.BaseTypes.Add(new CodeTypeReference(typeof(IHttpHandler)));

        if (methodInfo != null && methodInfo.DeclaringType != typeof(Page)) {
            method.Attributes |= MemberAttributes.New | MemberAttributes.Public;
        }
        else {
            method.Attributes |= MemberAttributes.Override | MemberAttributes.Public;
        }

        method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(HttpContext), "context"));

        CodeMethodInvokeExpression invokeExpr = new CodeMethodInvokeExpression();
        invokeExpr.Method.TargetObject = new CodeBaseReferenceExpression();
        invokeExpr.Method.MethodName = "ProcessRequest";
        invokeExpr.Parameters.Add(new CodeArgumentReferenceExpression("context"));

        method.Statements.Add(invokeExpr);

        _sourceDataClass.Members.Add(method);
    }

}

}
