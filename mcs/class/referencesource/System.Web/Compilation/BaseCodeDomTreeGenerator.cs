//------------------------------------------------------------------------------
// <copyright file="BaseCodeDomTreeGenerator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/**********************************************

Class hierarchy:

BaseCodeDomTreeGenerator
    BaseTemplateCodeDomTreeGenerator
        TemplateControlCodeDomTreeGenerator
            PageCodeDomTreeGenerator
            UserControlCodeDomTreeGenerator
        PageThemeCodeDomTreeGenerator
    ApplicationFileCodeDomTreeGenerator
***********************************************/



namespace System.Web.Compilation {

using System.Text;
using System.Runtime.Serialization.Formatters;
using System.ComponentModel;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.IO;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.Web.Caching;
using System.Web.Util;
using System.Web.UI;
using System.Web.SessionState;
using System.CodeDom;
using System.CodeDom.Compiler;
using Util = System.Web.UI.Util;
using System.Web.Hosting;
using System.Web.Profile;
using System.Web.Configuration;
using System.Globalization;


internal abstract class BaseCodeDomTreeGenerator {

    protected CodeDomProvider _codeDomProvider;
    protected CodeCompileUnit _codeCompileUnit;
    private CodeNamespace _sourceDataNamespace;
    protected CodeTypeDeclaration _sourceDataClass;
    protected CodeTypeDeclaration _intermediateClass;
    private CompilerParameters _compilParams;
    protected StringResourceBuilder _stringResourceBuilder;
    protected bool _usingVJSCompiler;
    private static IDictionary _generatedColumnOffsetDictionary;

    private VirtualPath _virtualPath;

    // The constructors
    protected CodeConstructor _ctor;

    protected CodeTypeReferenceExpression _classTypeExpr;

    internal const string defaultNamespace = "ASP";

    // Used for things that we don't want the user to see
    internal const string internalAspNamespace = "__ASP";

    private const string initializedFieldName = "__initialized";

    private const string _dummyVariable = "__dummyVar";
    private const int _defaultColumnOffset = 4;

    private TemplateParser _parser;
    TemplateParser Parser { get { return _parser; } }

    // We generate different code for the designer
    protected bool _designerMode;
    internal void SetDesignerMode() { _designerMode = true; }

    private IDictionary _linePragmasTable;
    internal IDictionary LinePragmasTable { get { return _linePragmasTable; } }

    // Used to generate indexed into the LinePragmasTable
    private int _pragmaIdGenerator=1;

    private static bool _urlLinePragmas;

    static BaseCodeDomTreeGenerator() {
        CompilationSection config = MTConfigUtil.GetCompilationAppConfig();

        _urlLinePragmas = config.UrlLinePragmas;
    }

#if DBG
    private bool _addedDebugComment;
#endif

#if DBG
    protected void AppendDebugComment(CodeStatementCollection statements) {
        if (!_addedDebugComment) {
            _addedDebugComment = true;
            StringBuilder debugComment = new StringBuilder();

            debugComment.Append("\r\n");
            debugComment.Append("** DEBUG INFORMATION **");
            debugComment.Append("\r\n");

            statements.Add(new CodeCommentStatement(debugComment.ToString()));
        }
    }
#endif

    internal /*public*/ CodeCompileUnit GetCodeDomTree(CodeDomProvider codeDomProvider,
        StringResourceBuilder stringResourceBuilder, VirtualPath virtualPath) {

        Debug.Assert(_codeDomProvider == null && _stringResourceBuilder == null);

        _codeDomProvider = codeDomProvider;
        _stringResourceBuilder = stringResourceBuilder;
        _virtualPath = virtualPath;

        // Build the data tree that needs to be compiled
        if (!BuildSourceDataTree())
            return null;

        // Tell the root builder that the CodeCompileUnit is now fully complete
        if (Parser.RootBuilder != null) {
            Parser.RootBuilder.OnCodeGenerationComplete();
        }

        return _codeCompileUnit;
    }

    protected /*public*/ CompilerParameters CompilParams { get { return _compilParams; } }

    internal string GetInstantiatableFullTypeName() {

        // In updatable mode, we never build the final type, so return null
        if (PrecompilingForUpdatableDeployment)
            return null;

        return Util.MakeFullTypeName(_sourceDataNamespace.Name, _sourceDataClass.Name);
    }

    internal string GetIntermediateFullTypeName() {

        return Util.MakeFullTypeName(Parser.BaseTypeNamespace, _intermediateClass.Name);
    }

    /*
     * Set some fields that are needed for code generation
     */
    protected BaseCodeDomTreeGenerator(TemplateParser parser) {
        _parser = parser;

        Debug.Assert(Parser.BaseType != null);
    }

    protected void ApplyEditorBrowsableCustomAttribute(CodeTypeMember member) {
        Debug.Assert(_designerMode, "This method should only be used in design mode.");

        // Generate EditorBrowsableAttribute to hide the generated methods from the tool
        // [EditorBrowsable(EditorBrowsableState.Never)]
        CodeAttributeDeclaration editorBrowsableAttribute = new CodeAttributeDeclaration();
        editorBrowsableAttribute.Name = typeof(EditorBrowsableAttribute).FullName;
        editorBrowsableAttribute.Arguments.Add(new CodeAttributeArgument(new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(EditorBrowsableState)), "Never")));
        member.CustomAttributes.Add(editorBrowsableAttribute);
    }


    /// <devdoc>
    ///     Create a name for the generated class
    /// </devdoc>
    protected virtual string GetGeneratedClassName() {
        string className;

        // If the user specified the class name, just use that
        if (Parser.GeneratedClassName != null)
            return Parser.GeneratedClassName;

        // Use the input file name to generate the class name

        className = _virtualPath.FileName;

        // Prepend the class name with the directory path within the app (DevDiv 42063)
        string appRelVirtualDir = _virtualPath.Parent.AppRelativeVirtualPathStringOrNull;
        if (appRelVirtualDir != null) {
            Debug.Assert(UrlPath.IsAppRelativePath(appRelVirtualDir));
            className = appRelVirtualDir.Substring(2) + className;
        }

        // Change invalid chars to underscores
        className = Util.MakeValidTypeNameFromString(className);

        // Make it lower case to make it more predictable (VSWhidbey 503369)
        className = className.ToLowerInvariant();

        // If it's the same as the base type name, prepend it with an underscore to prevent
        // a compile error.
        string baseTypeName = Parser.BaseTypeName != null ? Parser.BaseTypeName : Parser.BaseType.Name;
        if (StringUtil.EqualsIgnoreCase(className, baseTypeName)) {
            className = "_" + className;
        }

        return className;
    }

    internal static bool IsAspNetNamespace(string ns) {
        return (ns == defaultNamespace);
    }

    private bool PrecompilingForUpdatableDeployment {
        get {
            // For global.asax, this never applies
            if (IsGlobalAsaxGenerator)
                return false;

            return BuildManager.PrecompilingForUpdatableDeployment;
        }
    }

    private bool BuildSourceDataTree() {

        _compilParams = Parser.CompilParams;

        _codeCompileUnit = new CodeCompileUnit();
        _codeCompileUnit.UserData["AllowLateBound"] = !Parser.FStrict;
        _codeCompileUnit.UserData["RequireVariableDeclaration"] = Parser.FExplicit;

        // Set a flag indicating if we're using the VJS compiler.  See comment in BuildExtractMethod for more information.
        _usingVJSCompiler = (_codeDomProvider.FileExtension == ".jsl");

        _sourceDataNamespace = new CodeNamespace(Parser.GeneratedNamespace);

        string generatedClassName = GetGeneratedClassName();

        if (Parser.BaseTypeName != null) {

            Debug.Assert(Parser.CodeFileVirtualPath != null);

            // This is the case where the page has a CodeFile attribute

            CodeNamespace intermediateNamespace = new CodeNamespace(Parser.BaseTypeNamespace);
            _codeCompileUnit.Namespaces.Add(intermediateNamespace);
            _intermediateClass = new CodeTypeDeclaration(Parser.BaseTypeName);

            // Specify the base class in the UserData in case the CodeDom provider needs
            // to reflect on it when generating code from the CodeCompileUnit (VSWhidbey 475294)
            // In design mode, use the default base type (e.g. Page or UserControl) to avoid
            // ending up with a type that can't be serialized to the Venus domain (VSWhidbey 545535)
            if (_designerMode)
                _intermediateClass.UserData["BaseClassDefinition"] = Parser.DefaultBaseType;
            else
                _intermediateClass.UserData["BaseClassDefinition"] = Parser.BaseType;

            intermediateNamespace.Types.Add(_intermediateClass);

            // Generate a partial class
            _intermediateClass.IsPartial = true;

            // Unless we're precompiling for updatable deployment, create the derived class
            if (!PrecompilingForUpdatableDeployment) {

                _sourceDataClass = new CodeTypeDeclaration(generatedClassName);
                // VSWhidbey 411701. Always use global type reference for the baseType 
                // when codefile is present.
                _sourceDataClass.BaseTypes.Add(CodeDomUtility.BuildGlobalCodeTypeReference(
                    Util.MakeFullTypeName(Parser.BaseTypeNamespace, Parser.BaseTypeName)));

                _sourceDataNamespace.Types.Add(_sourceDataClass);
            }
        }
        else {

            // The page is not using code besides

            _intermediateClass = new CodeTypeDeclaration(generatedClassName);
            _intermediateClass.BaseTypes.Add(CodeDomUtility.BuildGlobalCodeTypeReference(Parser.BaseType));
            _sourceDataNamespace.Types.Add(_intermediateClass);

            // There is only one class, so make both fields point to the same thing
            _sourceDataClass = _intermediateClass;
        }

        // Add the derived class namespace after the base partial class so C# parser
        // can still parse the code correctly in case the derived class contains error.
        // VSWhidbey 397646
        _codeCompileUnit.Namespaces.Add(_sourceDataNamespace);

        // We don't generate any code during updatable precompilation of a single (inline) page,
        // except for global.asax
        if (PrecompilingForUpdatableDeployment && Parser.CodeFileVirtualPath == null)
            return false;

        // Add metadata attributes to the class
        GenerateClassAttributes();

        // In VB, always import Microsoft.VisualBasic (VSWhidbey 256475)
        if (_codeDomProvider is Microsoft.VisualBasic.VBCodeProvider)
            _sourceDataNamespace.Imports.Add(new CodeNamespaceImport("Microsoft.VisualBasic"));

        // Add all the namespaces
        if (Parser.NamespaceEntries != null) {
            foreach (NamespaceEntry entry in Parser.NamespaceEntries.Values) {
                // Create a line pragma if available
                CodeLinePragma linePragma;
                if (entry.VirtualPath != null) {
                    linePragma = CreateCodeLinePragma(entry.VirtualPath, entry.Line);
                }
                else {
                    linePragma = null;
                }

                CodeNamespaceImport nsi = new CodeNamespaceImport(entry.Namespace);
                nsi.LinePragma = linePragma;

                _sourceDataNamespace.Imports.Add(nsi);
            }
        }

        if (_sourceDataClass != null) {
            // We need to generate a global reference to avoid ambiguities (VSWhidbey 284936)
            string fullClassName = Util.MakeFullTypeName(_sourceDataNamespace.Name, _sourceDataClass.Name);
            CodeTypeReference classTypeRef = CodeDomUtility.BuildGlobalCodeTypeReference(fullClassName);

            // Since this is needed in several places, store it in a member variable
            _classTypeExpr = new CodeTypeReferenceExpression(classTypeRef);
        }

        // Add the implemented interfaces
        GenerateInterfaces();

        // Build various properties, fields, methods
        BuildMiscClassMembers();

        // Build the default constructors
        if (!_designerMode && _sourceDataClass != null) {
            _ctor = new CodeConstructor();
            AddDebuggerNonUserCodeAttribute(_ctor);
            _sourceDataClass.Members.Add(_ctor);
            BuildDefaultConstructor();
        }

        return true;
    }

    /*
     * Add metadata attributes to the class
     */
    protected virtual void GenerateClassAttributes() {
        // If this is a debuggable page, generate a
        // CompilerGlobalScopeAttribute attribute (ASURT 33027)
        if (CompilParams.IncludeDebugInformation && _sourceDataClass != null) {
            CodeAttributeDeclaration attribDecl = new CodeAttributeDeclaration(
                "System.Runtime.CompilerServices.CompilerGlobalScopeAttribute");
            _sourceDataClass.CustomAttributes.Add(attribDecl);
        }
    }

    /*
     * Generate the list of implemented interfaces
     */
    protected virtual void GenerateInterfaces() {
        if (Parser.ImplementedInterfaces != null) {
            foreach (Type t in Parser.ImplementedInterfaces) {
                _intermediateClass.BaseTypes.Add(new CodeTypeReference(t));
            }
        }
    }

    /*
     * Build first-time intialization statements
     */
    protected virtual void BuildInitStatements(CodeStatementCollection trueStatements, CodeStatementCollection topLevelStatements) {
    }


    /*
     * Build the default constructor
     */
    protected virtual void BuildDefaultConstructor() {

        _ctor.Attributes &= ~MemberAttributes.AccessMask;
        _ctor.Attributes |= MemberAttributes.Public;

        // private static bool __initialized;
        CodeMemberField initializedField = new CodeMemberField(typeof(bool), initializedFieldName);
        initializedField.Attributes |= MemberAttributes.Static;
        _sourceDataClass.Members.Add(initializedField);


        // if (__intialized == false)
        CodeConditionStatement initializedCondition = new CodeConditionStatement();
        initializedCondition.Condition = new CodeBinaryOperatorExpression(
                                                new CodeFieldReferenceExpression(
                                                    _classTypeExpr,
                                                    initializedFieldName),
                                                CodeBinaryOperatorType.ValueEquality,
                                                new CodePrimitiveExpression(false));

        this.BuildInitStatements(initializedCondition.TrueStatements, _ctor.Statements);

        initializedCondition.TrueStatements.Add(new CodeAssignStatement(
                                                    new CodeFieldReferenceExpression(
                                                        _classTypeExpr,
                                                        initializedFieldName),
                                                    new CodePrimitiveExpression(true)));

        // i.e. __intialized = true;
        _ctor.Statements.Add(initializedCondition);
    }

    /*
     * Build various properties, fields, methods
     */
    protected virtual void BuildMiscClassMembers() {

        // Build the Profile property
        if (NeedProfileProperty)
            BuildProfileProperty();

        // Skip the rest if we're only generating the intermediate class
        if (_sourceDataClass == null)
            return;

        // Build the injected properties from the global.asax <object> tags
        BuildApplicationObjectProperties();
        BuildSessionObjectProperties();

        // Build the injected properties for objects scoped to the page
        BuildPageObjectProperties();

        // Add all the <script runat=server> code blocks
        foreach (ScriptBlockData script in Parser.ScriptList) {

            // Pad the code block so its generated offset matches the aspx
            string code = script.Script;
            code = code.PadLeft(code.Length + script.Column - 1);

            CodeSnippetTypeMember literal = new CodeSnippetTypeMember(code);
            literal.LinePragma = CreateCodeLinePragma(script.VirtualPath, script.Line,
                script.Column, script.Column, script.Script.Length, false);
            _sourceDataClass.Members.Add(literal);
        }
    }

    /*
     * Build the Profile property
     */
    private void BuildProfileProperty() {

        if (!ProfileManager.Enabled)
            return;

        CodeMemberProperty prop;
        string typeName = ProfileBase.GetProfileClassName();

        prop = new CodeMemberProperty();
        prop.Attributes &= ~MemberAttributes.AccessMask;
        prop.Attributes &= ~MemberAttributes.ScopeMask;
        prop.Attributes |= MemberAttributes.Final | MemberAttributes.Family;
        prop.Name = "Profile";

        if (_designerMode) {
            ApplyEditorBrowsableCustomAttribute(prop);
        }

        //if (ProfileBase.GetPropertiesForCompilation().Count == 0)
        //    typeName = "System.Web.Profile.DefaultProfile";
        //else
        //    typeName = "ASP.Profile";
        prop.Type = new CodeTypeReference(typeName);

        CodePropertyReferenceExpression propRef = new CodePropertyReferenceExpression(
            new CodeThisReferenceExpression(), "Context");
        propRef = new CodePropertyReferenceExpression(propRef, "Profile");

        prop.GetStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(
            typeName, propRef)));
        _intermediateClass.Members.Add(prop);
    }

    // By default, we build the Profile property
    protected virtual bool NeedProfileProperty { get { return true; } }

    protected void BuildAccessorProperty(string propName, CodeFieldReferenceExpression fieldRef,
        Type propType, MemberAttributes attributes, CodeAttributeDeclarationCollection attrDeclarations) {

        // e.g.
        // [attrDeclaration]
        // public SomeType SomeProp {
        //    get {
        //        return this.__SomeProp;
        //    }
        // }
        CodeMemberProperty prop = new CodeMemberProperty();
        prop.Attributes = attributes;
        prop.Name = propName;
        prop.Type = new CodeTypeReference(propType);
        prop.GetStatements.Add(new CodeMethodReturnStatement(fieldRef));
        prop.SetStatements.Add(new CodeAssignStatement(
            fieldRef, new CodePropertySetValueReferenceExpression()));

        if (attrDeclarations != null) {
            prop.CustomAttributes = attrDeclarations;
        }

        _sourceDataClass.Members.Add(prop);
    }

    protected void BuildFieldAndAccessorProperty(string propName, string fieldName,
        Type propType, bool fStatic, CodeAttributeDeclarationCollection attrDeclarations) {

        // e.g. private SomeType __SomeProp;
        CodeMemberField field = new CodeMemberField(propType, fieldName);
        if (fStatic) {
            field.Attributes |= MemberAttributes.Static;
        }
        _sourceDataClass.Members.Add(field);

        CodeFieldReferenceExpression fieldRef = new CodeFieldReferenceExpression(
            new CodeThisReferenceExpression(), fieldName);
        BuildAccessorProperty(propName, fieldRef, propType, MemberAttributes.Public, attrDeclarations);
    }

    /*
     * Helper method used to build the properties of injected
     * global.asax properties.  These look like:
     *   PropType __propName;
     *   protected PropType propName
     *   {
     *       get
     *       {
     *           if (__propName == null)
     *               __propName = [some expression];
     *
     *           return __propName;
     *       }
     *   }
     */
    private void BuildInjectedGetPropertyMethod(string propName,
                                                Type propType,
                                                CodeExpression propertyInitExpression,
                                                bool fPublicProp) {

        string fieldName = "cached" + propName;

        CodeExpression fieldAccess = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName);

        // Add a private field for the object
        _sourceDataClass.Members.Add(new CodeMemberField(propType, fieldName));

        CodeMemberProperty prop = new CodeMemberProperty();
        if (fPublicProp) {
            prop.Attributes &= ~MemberAttributes.AccessMask;
            prop.Attributes |= MemberAttributes.Public;
        }
        prop.Name = propName;
        prop.Type = new CodeTypeReference(propType);


        CodeConditionStatement ifStmt = new CodeConditionStatement();
        ifStmt.Condition = new CodeBinaryOperatorExpression(fieldAccess, CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null));
        ifStmt.TrueStatements.Add(new CodeAssignStatement(fieldAccess, propertyInitExpression));

        prop.GetStatements.Add(ifStmt);
        prop.GetStatements.Add(new CodeMethodReturnStatement(fieldAccess));

        _sourceDataClass.Members.Add(prop);
    }

    /*
     * Helper method for building application and session scope injected
     * properties.  If useApplicationState, build application properties, otherwise
     * build session properties.
     */
    private void BuildObjectPropertiesHelper(IDictionary objects, bool useApplicationState) {

        IDictionaryEnumerator en = objects.GetEnumerator();
        while (en.MoveNext()) {
            HttpStaticObjectsEntry entry = (HttpStaticObjectsEntry)en.Value;

            // e.g. (PropType)Session.StaticObjects["PropName"]

            // Use the appropriate collection
            CodePropertyReferenceExpression stateObj = new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(),
                                                                                                                               useApplicationState ? "Application" : "Session"),
                                                                                           "StaticObjects");

            CodeMethodInvokeExpression getObject = new CodeMethodInvokeExpression(stateObj, "GetObject");
            getObject.Parameters.Add(new CodePrimitiveExpression(entry.Name));


            Type declaredType = entry.DeclaredType;
            Debug.Assert(!Util.IsLateBoundComClassicType(declaredType));

            if (useApplicationState) {
                // for application state use property that does caching in a member
                BuildInjectedGetPropertyMethod(entry.Name, declaredType,
                                               new CodeCastExpression(declaredType, getObject),
                                               false /*fPublicProp*/);
            }
            else {
                // for session state use lookup every time, as one application instance deals with many sessions
                CodeMemberProperty prop = new CodeMemberProperty();
                prop.Name = entry.Name;
                prop.Type = new CodeTypeReference(declaredType);
                prop.GetStatements.Add(new CodeMethodReturnStatement(new CodeCastExpression(declaredType, getObject)));
                _sourceDataClass.Members.Add(prop);
            }
        }
    }

    /*
     * Build the injected properties from the global.asax <object> tags
     * declared with scope=application
     */
    private void BuildApplicationObjectProperties() {
        if (Parser.ApplicationObjects != null)
            BuildObjectPropertiesHelper(Parser.ApplicationObjects.Objects, true);
    }

    /*
     * Build the injected properties from the global.asax <object> tags
     * declared with scope=session
     */
    private void BuildSessionObjectProperties() {
        if (Parser.SessionObjects != null)
            BuildObjectPropertiesHelper(Parser.SessionObjects.Objects, false);
    }

    protected virtual bool IsGlobalAsaxGenerator { get { return false; } }

    /*
     * Build the injected properties from the global.asax <object> tags
     * declared with scope=appinstance, or the aspx/ascx tags with scope=page.
     */
    private void BuildPageObjectProperties() {
        if (Parser.PageObjectList == null) return;

        foreach (ObjectTagBuilder obj in Parser.PageObjectList) {

            CodeExpression propertyInitExpression;

            if (obj.Progid != null) {
                // If we are dealing with a COM classic object that hasn't been tlbreg'ed,
                // we need to call HttpServerUtility.CreateObject(progid) to create it
                CodeMethodInvokeExpression createObjectCall = new CodeMethodInvokeExpression();

                createObjectCall.Method.TargetObject = new CodePropertyReferenceExpression(
                    new CodeThisReferenceExpression(), "Server");
                createObjectCall.Method.MethodName = "CreateObject";
                createObjectCall.Parameters.Add(new CodePrimitiveExpression(obj.Progid));

                propertyInitExpression = createObjectCall;
            }
            else if (obj.Clsid != null) {
                // Same as previous case, but with a clsid instead of a progId
                CodeMethodInvokeExpression createObjectCall = new CodeMethodInvokeExpression();
                createObjectCall.Method.TargetObject = new CodePropertyReferenceExpression(
                    new CodeThisReferenceExpression(), "Server");
                createObjectCall.Method.MethodName = "CreateObjectFromClsid";
                createObjectCall.Parameters.Add(new CodePrimitiveExpression(obj.Clsid));

                propertyInitExpression = createObjectCall;
            }
            else {
                propertyInitExpression = new CodeObjectCreateExpression(obj.ObjectType);
            }

            // Make the appinstance properties public for global.asax (ASURT 63253)
            BuildInjectedGetPropertyMethod(obj.ID, obj.DeclaredType,
                propertyInitExpression, IsGlobalAsaxGenerator /*fPublicProp*/);
        }
    }

    protected CodeLinePragma CreateCodeLinePragma(ControlBuilder builder) {

        string virtualPath = builder.PageVirtualPath;
        int line = builder.Line;
        int column = 1;
        int generatedColumn = 1;
        int codeLength = -1;

        CodeBlockBuilder codeBlockBuilder = builder as CodeBlockBuilder;
        if (codeBlockBuilder != null) {
            column = codeBlockBuilder.Column;
            codeLength = codeBlockBuilder.Content.Length;

            if (codeBlockBuilder.BlockType == CodeBlockType.Code) {
                // If it's a <% ... %> block, the generated column is the same as the source
                generatedColumn = column;
            }
            else {
                // If it's a <%= ... %> block, we always generate '__o = expr' is
                // designer mode, so the column is fixed
                // 
                generatedColumn = BaseTemplateCodeDomTreeGenerator.tempObjectVariable.Length + 
                    GetGeneratedColumnOffset(_codeDomProvider);
            }
        }

        return CreateCodeLinePragma(virtualPath, line, column, generatedColumn, codeLength);
    }

    internal static int GetGeneratedColumnOffset(CodeDomProvider codeDomProvider) {
        object o = null;

        if (_generatedColumnOffsetDictionary == null) {
            _generatedColumnOffsetDictionary = new ListDictionary();
        }
        else {
            o = _generatedColumnOffsetDictionary[codeDomProvider.GetType()];
        }

        if (o == null) {
            CodeCompileUnit ccu = new CodeCompileUnit();

            CodeNamespace cnamespace = new CodeNamespace("ASP");
            ccu.Namespaces.Add(cnamespace);

            CodeTypeDeclaration type = new CodeTypeDeclaration("ColumnOffsetCalculator");
            type.IsClass = true;
            cnamespace.Types.Add(type);

            CodeMemberMethod method = new CodeMemberMethod();
            method.ReturnType = new CodeTypeReference(typeof(void));
            method.Name = "GenerateMethod";
            type.Members.Add(method);

            CodeStatement simpleAssignment = new CodeAssignStatement(
                new CodeVariableReferenceExpression(BaseTemplateCodeDomTreeGenerator.tempObjectVariable),
                new CodeSnippetExpression(_dummyVariable));
            method.Statements.Add(simpleAssignment);

            StringBuilder sb = new StringBuilder();
            StringWriter w = new StringWriter(sb, CultureInfo.InvariantCulture);

            codeDomProvider.GenerateCodeFromCompileUnit(ccu, w, null);

            StringReader reader = new StringReader(sb.ToString());
            String line = null;

            int offset = _defaultColumnOffset;

            while ((line = reader.ReadLine()) != null) {
                int index = 0;
                line = line.TrimStart();
                if ((index = line.IndexOf(_dummyVariable, StringComparison.Ordinal)) != -1) {
                    offset = index - BaseTemplateCodeDomTreeGenerator.tempObjectVariable.Length + 1;
                }
            }

            // Save the offset per type.
            _generatedColumnOffsetDictionary[codeDomProvider.GetType()] = offset;

            return offset;
        }

        return (int)o;
    }

    protected CodeLinePragma CreateCodeLinePragma(string virtualPath, int lineNumber) {
        return CreateCodeLinePragma(virtualPath, lineNumber, 1, 1, -1, true);
    }

    protected CodeLinePragma CreateCodeLinePragma(string virtualPath, int lineNumber,
        int column, int generatedColumn, int codeLength) {
        return CreateCodeLinePragma(virtualPath, lineNumber, column, generatedColumn, codeLength, true);
    }

    protected CodeLinePragma CreateCodeLinePragma(string virtualPath, int lineNumber,
        int column, int generatedColumn, int codeLength, bool isCodeNugget) {

        // Return null if we're not supposed to generate line pragmas
        if (!Parser.FLinePragmas)
            return null;

        // The problem with disabling pragmas in non-debug is that we no longer
        // get line information on compile errors, while in v1 we did. So
        // unless we find a better solution, don't disable pragmas in non-debug
/*
        // Also, don't bother with pragmas unless we're compiling for debugging
        if (!CompilParams.IncludeDebugInformation)
            return null;
*/

        if (String.IsNullOrEmpty(virtualPath))
            return null;

        if (_designerMode) {

            // Only generate pragmas for code blocks in designer mode
            if (codeLength < 0)
                return null;

            LinePragmaCodeInfo codeInfo = new LinePragmaCodeInfo();
            codeInfo._startLine = lineNumber;
            codeInfo._startColumn = column;
            codeInfo._startGeneratedColumn = generatedColumn;
            codeInfo._codeLength = codeLength;
            codeInfo._isCodeNugget = isCodeNugget;
            lineNumber = _pragmaIdGenerator++;

            if (_linePragmasTable == null)
                _linePragmasTable = new Hashtable();

            _linePragmasTable[lineNumber] = codeInfo;
        }

        return CreateCodeLinePragmaHelper(virtualPath, lineNumber);
    }

    internal static CodeLinePragma CreateCodeLinePragmaHelper(string virtualPath, int lineNumber) {

        string pragmaFile = null;

        if (UrlPath.IsAbsolutePhysicalPath(virtualPath)) {

            // Due to config system limitations, we can end up with virtualPath
            // actually being a physical path.  If that's the case, just use it as is.
            // 

            pragmaFile = virtualPath;
        }
        else {

            if (_urlLinePragmas) {

                // If specified in config, generate URL's for the line pragmas
                // instead of physical path.  This is used for VS debugging.
                // We don't know the server name, so we just used a fixed one.  This should be
                // fine, as VS will ignore the server name.

                pragmaFile = ErrorFormatter.MakeHttpLinePragma(virtualPath);
            }
            else {
                try {
                    // Try using the physical path for the line pragma
                    pragmaFile = HostingEnvironment.MapPathInternal(virtualPath);

                    // If the physical path doesn't exist, use the URL instead.
                    // This can happen when using a VirtualPathProvider (VSWhidbey 272259)
                    if (!File.Exists(pragmaFile)) {
                        pragmaFile = ErrorFormatter.MakeHttpLinePragma(virtualPath);
                    }
                }
                catch {
                    // If MapPath failed, use the URL instead
                    pragmaFile = ErrorFormatter.MakeHttpLinePragma(virtualPath);
                }
            }
        }

        return new CodeLinePragma(pragmaFile, lineNumber);
    }

    // Adds [DebuggerNonUserCode] to the method to prevent debugger from stepping into generated code
    protected void AddDebuggerNonUserCodeAttribute(CodeMemberMethod method) {
        if (method == null) return;
        // If LinePragmas is false, the user might want to debug generated code, so we do not add the attribute
        if (!Parser.FLinePragmas) return; 

        CodeAttributeDeclaration attributeDeclaration = new CodeAttributeDeclaration(new CodeTypeReference(typeof(System.Diagnostics.DebuggerNonUserCodeAttribute)));
        method.CustomAttributes.Add(attributeDeclaration);
    }
}

}
