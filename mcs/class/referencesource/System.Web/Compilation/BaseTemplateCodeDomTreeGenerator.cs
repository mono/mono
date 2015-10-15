//------------------------------------------------------------------------------
// <copyright file="BaseTemplateCodeDomTreeGenerator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Compilation {

    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Web.Caching;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Web.Util;
    using Debug = System.Web.Util.Debug;

    internal abstract class BaseTemplateCodeDomTreeGenerator : BaseCodeDomTreeGenerator {

        protected static readonly string buildMethodPrefix = "__BuildControl";
        protected static readonly string extractTemplateValuesMethodPrefix = "__ExtractValues";
        protected static readonly string templateSourceDirectoryName = "AppRelativeTemplateSourceDirectory";
        protected static readonly string applyStyleSheetMethodName = "ApplyStyleSheetSkin";
        protected static readonly string pagePropertyName = "Page";
        internal const string skinIDPropertyName = "SkinID";
        private const string _localVariableRef = "__ctrl";

        private TemplateParser _parser;
        private int _controlCount;

        // Minimum literal string length for it to be placed in the resource
        private const int minLongLiteralStringLength = 256;
        private const string renderMethodParameterName = "__w";

        // Used in designer mode
        internal const string tempObjectVariable = "__o";


        /*
         * Set some fields that are needed for code generation
         */
        internal BaseTemplateCodeDomTreeGenerator(TemplateParser parser) : base(parser) {
            _parser = parser;
        }

        private TemplateParser Parser {
            get {
                return _parser;
            }
        }

        private CodeStatement GetOutputWriteStatement(CodeExpression expr, bool encode) {

            // Call HttpUtility.HtmlEncode on the expression if needed
            if (encode) {
                expr = new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(
                        new CodeTypeReferenceExpression(typeof(HttpUtility)),
                       "HtmlEncode"),
                    expr);
            }

            CodeMethodInvokeExpression methodInvoke = new CodeMethodInvokeExpression();
            CodeExpressionStatement call = new CodeExpressionStatement(methodInvoke);
            methodInvoke.Method.TargetObject = new CodeArgumentReferenceExpression(renderMethodParameterName);
            methodInvoke.Method.MethodName = "Write";

            methodInvoke.Parameters.Add(expr);

            return call;
        }


        /// <devdoc>
        ///     Append an output.Write() statement to a Render method
        /// </devdoc>
        private void AddOutputWriteStatement(CodeStatementCollection methodStatements,
                                     CodeExpression expr,
                                     CodeLinePragma linePragma) {

            CodeStatement outputWriteStmt = GetOutputWriteStatement(expr, false /*encode*/);
            if (linePragma != null)
                outputWriteStmt.LinePragma = linePragma;

            methodStatements.Add(outputWriteStmt);
        }

        private void AddOutputWriteStringStatement(CodeStatementCollection methodStatements,
                                     String s) {

            if (!UseResourceLiteralString(s)) {
                AddOutputWriteStatement(methodStatements, new CodePrimitiveExpression(s), null);
                return;
            }

            // Add the string to the resource builder, and get back its offset/size
            int offset, size;
            bool fAsciiOnly;
            _stringResourceBuilder.AddString(s, out offset, out size, out fAsciiOnly);

            // e.g. WriteUTF8ResourceString(output, 314, 20);
            CodeMethodInvokeExpression methodInvoke = new CodeMethodInvokeExpression();
            CodeExpressionStatement call = new CodeExpressionStatement(methodInvoke);
            methodInvoke.Method.TargetObject = new CodeThisReferenceExpression();
            methodInvoke.Method.MethodName = "WriteUTF8ResourceString";
            methodInvoke.Parameters.Add(new CodeArgumentReferenceExpression(renderMethodParameterName));
            methodInvoke.Parameters.Add(new CodePrimitiveExpression(offset));
            methodInvoke.Parameters.Add(new CodePrimitiveExpression(size));
            methodInvoke.Parameters.Add(new CodePrimitiveExpression(fAsciiOnly));
            methodStatements.Add(call);
        }

        private static void BuildAddParsedSubObjectStatement(
                    CodeStatementCollection statements, CodeExpression ctrlToAdd, CodeLinePragma linePragma, CodeExpression ctrlRefExpr, ref bool gotParserVariable) {

            if (!gotParserVariable) {
                // e.g. IParserAccessor __parser = ((IParserAccessor)__ctrl);
                CodeVariableDeclarationStatement parserDeclaration = new CodeVariableDeclarationStatement();
                parserDeclaration.Name = "__parser";
                parserDeclaration.Type = new CodeTypeReference(typeof(IParserAccessor));
                parserDeclaration.InitExpression = new CodeCastExpression(
                                                        typeof(IParserAccessor),
                                                        ctrlRefExpr);
                statements.Add(parserDeclaration);
                gotParserVariable = true;
            }

            // e.g. __parser.AddParsedSubObject({{controlName}});
            CodeMethodInvokeExpression methCallExpression = new CodeMethodInvokeExpression(
                    new CodeVariableReferenceExpression("__parser"), "AddParsedSubObject");
            methCallExpression.Parameters.Add(ctrlToAdd);
            CodeExpressionStatement methCallStatement = new CodeExpressionStatement(methCallExpression);
            methCallStatement.LinePragma = linePragma;

            statements.Add(methCallStatement);
        }

        internal virtual CodeExpression BuildPagePropertyReferenceExpression() {
            return new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), pagePropertyName);
        }

        /*
         * Build the data tree for a control's build method
         */
        protected CodeMemberMethod BuildBuildMethod(ControlBuilder builder, bool fTemplate,
            bool fInTemplate, bool topLevelControlInTemplate, PropertyEntry pse, bool fControlSkin) {

            Debug.Assert(builder.ServiceProvider == null);

            ServiceContainer container = new ServiceContainer();
            container.AddService(typeof(IFilterResolutionService), HttpCapabilitiesBase.EmptyHttpCapabilitiesBase);

            try {
                builder.SetServiceProvider(container);
                builder.EnsureEntriesSorted();
            }
            finally {
                builder.SetServiceProvider(null);
            }

            string methodName = GetMethodNameForBuilder(buildMethodPrefix, builder);
            Type ctrlType = GetCtrlTypeForBuilder(builder, fTemplate);
            bool fStandardControl = false;
            bool fControlFieldDeclared = false;

            CodeMemberMethod method = new CodeMemberMethod();
            AddDebuggerNonUserCodeAttribute(method);
            method.Name = methodName;
            method.Attributes = MemberAttributes.Private | MemberAttributes.Final;

            _sourceDataClass.Members.Add(method);

            // If it's for a template or a r/o complex prop, pass a parameter of the control's type
            ComplexPropertyEntry cpse = pse as ComplexPropertyEntry;
            if (fTemplate || (cpse != null && cpse.ReadOnly)) {
                if (builder is RootBuilder)
                    method.Parameters.Add(new CodeParameterDeclarationExpression(_sourceDataClass.Name, "__ctrl"));
                else
                    method.Parameters.Add(new CodeParameterDeclarationExpression(ctrlType, "__ctrl"));
            }
            else {
                // If it's a standard control, return it from the method
                if (typeof(Control).IsAssignableFrom(builder.ControlType)) {
                    fStandardControl = true;
                }

                Debug.Assert(builder.ControlType != null);
                if (builder.ControlType != null) {
                    if (fControlSkin) {
                        // ReturnType needs to be of type Control in a skin file to match
                        // the controlskin delegate.
                        if (fStandardControl) {
                            method.ReturnType = new CodeTypeReference(typeof(Control));
                        }
                    }
                    else {
                        PartialCachingAttribute cacheAttrib = (PartialCachingAttribute)
                            TypeDescriptor.GetAttributes(builder.ControlType)[typeof(PartialCachingAttribute)];
                        if (cacheAttrib != null) {
                            method.ReturnType = new CodeTypeReference(typeof(Control));
                        }
                        else {
                            // Otherwise the return type is always the actual component type.
                            method.ReturnType = CodeDomUtility.BuildGlobalCodeTypeReference(builder.ControlType);
                        }
                    }
                }

                // A control field declaration is required, this field will be returned
                // in the method.
                fControlFieldDeclared = true;
            }

            // Add a control parameter if it's a ControlSkin
            if (fControlSkin) {
                method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(Control).FullName, "ctrl"));
            }

            BuildBuildMethodInternal(builder, builder.ControlType, fInTemplate, topLevelControlInTemplate, pse,
                method.Statements, fStandardControl, fControlFieldDeclared, null, fControlSkin);

            return method;
        }

        /* Helper method to generate the content of the control's build method
         *  Type _ctrl;
         *  _ctrl = new Type();
         *      ...
         *  return _ctrl;
         */
        [SuppressMessage("Microsoft.Usage", "CA2303:FlagTypeGetHashCode", Justification = "This is used for caching - it's ok to supress.")]
        private void BuildBuildMethodInternal(ControlBuilder builder, Type ctrlType, bool fInTemplate,
            bool topLevelControlInTemplate, PropertyEntry pse, CodeStatementCollection statements,
            bool fStandardControl, bool fControlFieldDeclared, string deviceFilter, bool fControlSkin) {

            // Same linePragma in the entire build method
            CodeLinePragma linePragma = CreateCodeLinePragma(builder);

            CodeObjectCreateExpression newExpr;
            CodeExpressionStatement methCallStatement;
            CodeMethodInvokeExpression methCallExpression;

            CodeExpression ctrlRefExpr;

            if (fControlSkin) {
                CodeCastExpression cast = new CodeCastExpression(builder.ControlType.FullName,
                                           new CodeArgumentReferenceExpression("ctrl"));
                statements.Add(new CodeVariableDeclarationStatement(builder.ControlType.FullName, "__ctrl", cast));
                ctrlRefExpr = new CodeVariableReferenceExpression("__ctrl");
            }
            // Not a control. ie. it's for a template or a r/o complex prop,
            else if (!fControlFieldDeclared) {
                ctrlRefExpr = new CodeArgumentReferenceExpression("__ctrl");
            }
            else {
                CodeTypeReference ctrlTypeRef = CodeDomUtility.BuildGlobalCodeTypeReference(ctrlType);
                    
                newExpr = new CodeObjectCreateExpression(ctrlTypeRef);

                // If it has a ConstructorNeedsTagAttribute, it needs a tag name
                ConstructorNeedsTagAttribute cnta = (ConstructorNeedsTagAttribute)
                    TypeDescriptor.GetAttributes(ctrlType)[typeof(ConstructorNeedsTagAttribute)];

                if (cnta != null && cnta.NeedsTag) {
                    newExpr.Parameters.Add(new CodePrimitiveExpression(builder.TagName));
                }

                // If it's for a DataBoundLiteralControl, pass it the number of
                // entries in the constructor
                DataBoundLiteralControlBuilder dataBoundBuilder = builder as DataBoundLiteralControlBuilder;
                if (dataBoundBuilder != null) {
                    newExpr.Parameters.Add(new CodePrimitiveExpression(
                        dataBoundBuilder.GetStaticLiteralsCount()));
                    newExpr.Parameters.Add(new CodePrimitiveExpression(
                        dataBoundBuilder.GetDataBoundLiteralCount()));
                }

                // e.g. {{controlTypeName}} __ctrl;
                statements.Add(new CodeVariableDeclarationStatement(ctrlTypeRef, "__ctrl"));
                ctrlRefExpr = new CodeVariableReferenceExpression("__ctrl");

                // e.g. __ctrl = new {{controlTypeName}}();
                CodeAssignStatement setCtl = new CodeAssignStatement(ctrlRefExpr, newExpr);
                setCtl.LinePragma = linePragma;
                statements.Add(setCtl);

                if (!builder.IsGeneratedID) {
                    // Assign the local control reference to the global control variable
                    CodeFieldReferenceExpression ctrlNameExpr = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), builder.ID);

                    // e.g. {{controlName}} = __ctrl;
                    CodeAssignStatement underscoreCtlSet = new CodeAssignStatement(ctrlNameExpr, ctrlRefExpr);
                    statements.Add(underscoreCtlSet);
                }

                // Don't do this if the control is itself a TemplateControl, in which case it
                // will point its TemplateControl property to itself (instead of its parent
                // TemplateControl).  VSWhidbey 214356.
                if (topLevelControlInTemplate && !typeof(TemplateControl).IsAssignableFrom(ctrlType)) {
                    statements.Add(BuildTemplatePropertyStatement(ctrlRefExpr));
                }

                if (fStandardControl) {
                    // e.g. __ctrl.SkinID = {{skinID}};
                    if (builder.SkinID != null) {
                        CodeAssignStatement set = new CodeAssignStatement();
                        set.Left = new CodePropertyReferenceExpression(ctrlRefExpr, skinIDPropertyName);
                        set.Right = new CodePrimitiveExpression(builder.SkinID);
                        statements.Add(set);
                    }

                    // e.g. __ctrl.ApplyStyleSheetSkin(this);
                    if (ThemeableAttribute.IsTypeThemeable(ctrlType)) {
                        // e.g. __ctrl.ApplyStyleSheetSkin(this.Page);
                        CodeMethodInvokeExpression applyStyleSheetExpr = new CodeMethodInvokeExpression(ctrlRefExpr, applyStyleSheetMethodName);
                        applyStyleSheetExpr.Parameters.Add(BuildPagePropertyReferenceExpression());

                        statements.Add(applyStyleSheetExpr);
                    }
                }
            }

            // Process the templates
            if (builder.TemplatePropertyEntries.Count > 0) {

                // Used to deal with the device filter conditionals
                CodeStatementCollection currentStmts;
                CodeStatementCollection nextStmts = statements;
                PropertyEntry previous = null;

                foreach (TemplatePropertyEntry pseSub in builder.TemplatePropertyEntries) {
                    currentStmts = nextStmts;

                    HandleDeviceFilterConditional(ref previous, pseSub, statements, ref currentStmts, out nextStmts);

                    string controlName = pseSub.Builder.ID;
                    CodeDelegateCreateExpression newDelegate = new CodeDelegateCreateExpression();
                    newDelegate.DelegateType = new CodeTypeReference(typeof(BuildTemplateMethod));
                    newDelegate.TargetObject = new CodeThisReferenceExpression();
                    newDelegate.MethodName = buildMethodPrefix + controlName;

                    CodeAssignStatement set = new CodeAssignStatement();
                    if (pseSub.PropertyInfo != null) {
                        set.Left = new CodePropertyReferenceExpression(ctrlRefExpr, pseSub.Name);
                    }
                    else {
                        set.Left = new CodeFieldReferenceExpression(ctrlRefExpr, pseSub.Name);
                    }

                    if (pseSub.BindableTemplate) {
                        // e.g. __ctrl.{{templateName}} = new CompiledBindableTemplateBuilder(
                        // e.g.     new BuildTemplateMethod(this.__BuildControl {{controlName}}),
                        // e.g.     new ExtractTemplateValuesMethod(this.__ExtractValues {{controlName}}));
                        CodeExpression newExtractValuesDelegate;
                        if (pseSub.Builder.HasTwoWayBoundProperties) {
                            newExtractValuesDelegate = new CodeDelegateCreateExpression();
                            ((CodeDelegateCreateExpression)newExtractValuesDelegate).DelegateType = new CodeTypeReference(typeof(ExtractTemplateValuesMethod));
                            ((CodeDelegateCreateExpression)newExtractValuesDelegate).TargetObject = new CodeThisReferenceExpression();
                            ((CodeDelegateCreateExpression)newExtractValuesDelegate).MethodName = extractTemplateValuesMethodPrefix + controlName;
                        }
                        else {
                            newExtractValuesDelegate = new CodePrimitiveExpression(null);
                        }

                        newExpr = new CodeObjectCreateExpression(typeof(CompiledBindableTemplateBuilder));
                        newExpr.Parameters.Add(newDelegate);
                        newExpr.Parameters.Add(newExtractValuesDelegate);

                    }
                    else {
                        // e.g. __ctrl.{{templateName}} = new CompiledTemplateBuilder(
                        // e.g.     new BuildTemplateMethod(this.__BuildControl {{controlName}}));
                        newExpr = new CodeObjectCreateExpression(typeof(CompiledTemplateBuilder));
                        newExpr.Parameters.Add(newDelegate);

                    }
                    set.Right = newExpr;
                    set.LinePragma = CreateCodeLinePragma(pseSub.Builder);
                    currentStmts.Add(set);
                }
            }

            // Is this BuilderData for a declarative control?  If so initialize it (75330)
            // Only do this is the control field has been declared (i.e. not with templates)
            if (typeof(UserControl).IsAssignableFrom(ctrlType) && fControlFieldDeclared && !fControlSkin) {
                // e.g. _ctrl.InitializeAsUserControl(Context, Page);
                methCallExpression = new CodeMethodInvokeExpression(ctrlRefExpr, "InitializeAsUserControl");
                methCallExpression.Parameters.Add(new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), pagePropertyName));
                methCallStatement = new CodeExpressionStatement(methCallExpression);
                methCallStatement.LinePragma = linePragma;
                statements.Add(methCallStatement);
            }

            // Process the simple attributes
            if (builder.SimplePropertyEntries.Count > 0) {
                // Used to deal with the device filter conditionals
                CodeStatementCollection currentStmts;
                CodeStatementCollection nextStmts = statements;
                PropertyEntry previous = null;

                foreach (SimplePropertyEntry pseSub in builder.SimplePropertyEntries) {
                    currentStmts = nextStmts;

                    HandleDeviceFilterConditional(ref previous, pseSub, statements, ref currentStmts, out nextStmts);

                    CodeStatement statement = pseSub.GetCodeStatement(this, ctrlRefExpr);
                    statement.LinePragma = linePragma;
                    currentStmts.Add(statement);
                }
            }

            // Call the helper method for allowing page developers to customize culture settings
            if (typeof(Page).IsAssignableFrom(ctrlType) && !fControlSkin) {
                // e.g. this.InitializeCulture();
                methCallExpression = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "InitializeCulture");
                methCallStatement = new CodeExpressionStatement(methCallExpression);
                methCallStatement.LinePragma = linePragma;
                statements.Add(methCallStatement);
            }

            // Automatic template support (i.e. <asp:template name=SomeTemplate/>)
            CodeMethodInvokeExpression instantiateTemplateExpr = null;
            CodeConditionStatement templateIfStmt = null;
            CodeStatementCollection buildSubControlBlock = statements;
            string autoTemplateName = null;
            if (builder is System.Web.UI.WebControls.ContentPlaceHolderBuilder) {

                string templateName = ((System.Web.UI.WebControls.ContentPlaceHolderBuilder)builder).Name;
                autoTemplateName = MasterPageControlBuilder.AutoTemplatePrefix + templateName;

                Debug.Assert(autoTemplateName != null && autoTemplateName.Length > 0, "Template Name is empty.");

                // Generate a private field and public property for the ITemplate
                string fieldName = "__"+ autoTemplateName;

                Type containerType = builder.BindingContainerType;
                // Use the base class or template type if INamingContainer cannot be found.
                if (!typeof(INamingContainer).IsAssignableFrom(containerType)) {
                    if (typeof(INamingContainer).IsAssignableFrom(Parser.BaseType)) {
                        containerType = Parser.BaseType;
                    }
                    else {
                        // This should not occur as all base classes are namingcontainers.
                        Debug.Assert(false, "baseClassType is not an INamingContainer");
                        containerType = typeof(System.Web.UI.Control);
                    }
                }

                CodeAttributeDeclarationCollection attrDeclarations = new CodeAttributeDeclarationCollection();

                CodeAttributeDeclaration templateContainerAttrDeclaration = new CodeAttributeDeclaration(
                    "TemplateContainer",
                    new CodeAttributeArgument[] {
                        new CodeAttributeArgument(new CodeTypeOfExpression(containerType))});

                attrDeclarations.Add(templateContainerAttrDeclaration);

                // If the template control is in a template, assume its container allows multiple instances,
                // otherwise set the TemplateInstanceAttribute
                if (fInTemplate == false) {
                    CodeAttributeDeclaration templateInstanceAttrDeclaration = new CodeAttributeDeclaration(
                        "TemplateInstanceAttribute",
                        new CodeAttributeArgument[] {
                            new CodeAttributeArgument(
                                new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(TemplateInstance)),
                                                                 "Single"))});
                    attrDeclarations.Add(templateInstanceAttrDeclaration);
                }

                BuildFieldAndAccessorProperty(autoTemplateName, fieldName, typeof(ITemplate), false /*fStatic*/, attrDeclarations);
                CodeExpression templateFieldRef = new CodeFieldReferenceExpression(
                    new CodeThisReferenceExpression(), fieldName);

                if (builder is System.Web.UI.WebControls.ContentPlaceHolderBuilder) {
                    // We generate something like this:
                    // if (this.ContentTemplates != null) {
                    //     this.__Template_TestTemplate = (ITemplate)this.ContentTemplates[{templateName}];
                    // }
                    CodePropertyReferenceExpression contentTemplatesFieldRef = 
                        new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "ContentTemplates");

                    CodeAssignStatement setStatement = new CodeAssignStatement();
                    setStatement.Left = templateFieldRef;
                    setStatement.Right = new CodeCastExpression(typeof(ITemplate), new CodeIndexerExpression(contentTemplatesFieldRef, 
                                                                                       new CodePrimitiveExpression(templateName)));

                    CodeConditionStatement contentTemplateIfStmt = new CodeConditionStatement();

                    CodeBinaryOperatorExpression contentNullCheckExpr = new CodeBinaryOperatorExpression(contentTemplatesFieldRef, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));

                    CodeMethodInvokeExpression removeExpr = new CodeMethodInvokeExpression(contentTemplatesFieldRef, "Remove");
                    removeExpr.Parameters.Add(new CodePrimitiveExpression(templateName));

                    contentTemplateIfStmt.Condition = contentNullCheckExpr;
                    contentTemplateIfStmt.TrueStatements.Add(setStatement);
                    
                    statements.Add(contentTemplateIfStmt);
                }

                // We generate something like this:
                // if ((this.__Template_TestTemplate != null)) {
                //     // For 2.0:
                //     this.__Template_TestTemplate.InstantiateIn(__ctrl);
                //     // For 4.0, use a new method. This is for fixing Dev10 





                if (MultiTargetingUtil.IsTargetFramework40OrAbove) {
                    instantiateTemplateExpr = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "InstantiateInContentPlaceHolder");
                    instantiateTemplateExpr.Parameters.Add(ctrlRefExpr);
                    instantiateTemplateExpr.Parameters.Add(templateFieldRef);
                }
                else {
                    instantiateTemplateExpr = new CodeMethodInvokeExpression(templateFieldRef, "InstantiateIn");
                    instantiateTemplateExpr.Parameters.Add(ctrlRefExpr);
                }

                templateIfStmt = new CodeConditionStatement();
                templateIfStmt.Condition = new CodeBinaryOperatorExpression(templateFieldRef, CodeBinaryOperatorType.IdentityInequality, new CodePrimitiveExpression(null));
                templateIfStmt.TrueStatements.Add(new CodeExpressionStatement(instantiateTemplateExpr));
                buildSubControlBlock = templateIfStmt.FalseStatements;
                statements.Add(templateIfStmt);
            }

            ICollection contentBuilderEntries = null;
            if (builder is FileLevelPageControlBuilder) {
                contentBuilderEntries = ((FileLevelPageControlBuilder)builder).ContentBuilderEntries;

                if (contentBuilderEntries != null) {
                    CodeStatementCollection currentStmts;
                    CodeStatementCollection nextStmts = statements;
                    PropertyEntry previous = null;

                    foreach (TemplatePropertyEntry entry in contentBuilderEntries) {
                        System.Web.UI.WebControls.ContentBuilderInternal child = 
                            (System.Web.UI.WebControls.ContentBuilderInternal)entry.Builder;

                        currentStmts = nextStmts;

                        HandleDeviceFilterConditional(ref previous, entry, statements, ref currentStmts, out nextStmts);

                        string controlName = child.ID;
                        string contentPlaceHolderID = child.ContentPlaceHolder;

                        CodeDelegateCreateExpression newDelegate = new CodeDelegateCreateExpression();
                        newDelegate.DelegateType = new CodeTypeReference(typeof(BuildTemplateMethod));
                        newDelegate.TargetObject = new CodeThisReferenceExpression();
                        newDelegate.MethodName = buildMethodPrefix + controlName;

                        // e.g. this.AddContentTemplate(contentPlaceHolderID, new CompiledTemplateBuilder(
                        // e.g.     new BuildTemplateMethod(this.__BuildControl {{controlName}}));
                        CodeObjectCreateExpression cocExpr = new CodeObjectCreateExpression(typeof(CompiledTemplateBuilder));
                        cocExpr.Parameters.Add(newDelegate);

                        CodeMethodInvokeExpression cmiExpression = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "AddContentTemplate");
                        cmiExpression.Parameters.Add(new CodePrimitiveExpression(contentPlaceHolderID));
                        cmiExpression.Parameters.Add(cocExpr);

                        CodeExpressionStatement ceStatement = new CodeExpressionStatement(cmiExpression);
                        ceStatement.LinePragma = CreateCodeLinePragma((ControlBuilder)child);

                        currentStmts.Add(ceStatement);
                    }
                }
            }

            if (builder is DataBoundLiteralControlBuilder) {

                // If it's a DataBoundLiteralControl, build it by calling SetStaticString
                // on all the static literal strings.
                int i = -1;
                foreach (object child in builder.SubBuilders) {
                    i++;

                    // Ignore it if it's null
                    if (child == null)
                        continue;

                    // Only deal with the strings here, which have even index
                    if (i % 2 == 1) {
                        Debug.Assert(child is CodeBlockBuilder, "child is CodeBlockBuilder");
                        continue;
                    }

                    string s = (string) child;

                    // e.g. __ctrl.SetStaticString(3, "literal string");
                    methCallExpression = new CodeMethodInvokeExpression(ctrlRefExpr, "SetStaticString");
                    methCallExpression.Parameters.Add(new CodePrimitiveExpression(i/2));
                    methCallExpression.Parameters.Add(new CodePrimitiveExpression(s));
                    statements.Add(new CodeExpressionStatement(methCallExpression));
                }
            }
            // Process the children
            else if (builder.SubBuilders != null) {

                bool gotParserVariable = false;
                int localVarIndex = 1;

                foreach (object child in builder.SubBuilders) {
                    if (child is ControlBuilder && !(child is CodeBlockBuilder) && !(child is CodeStatementBuilder) && !(child is System.Web.UI.WebControls.ContentBuilderInternal)) {
                        ControlBuilder ctrlBuilder = (ControlBuilder) child;

                        if (fControlSkin) {
                            throw new HttpParseException(SR.GetString(SR.ControlSkin_cannot_contain_controls),
                                null,
                                builder.VirtualPath, null, builder.Line);
                        }

                        PartialCachingAttribute cacheAttrib = (PartialCachingAttribute)
                            TypeDescriptor.GetAttributes(ctrlBuilder.ControlType)[typeof(PartialCachingAttribute)];

                        methCallExpression = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(),
                                                                          buildMethodPrefix + ctrlBuilder.ID);
                        methCallStatement = new CodeExpressionStatement(methCallExpression);

                        if (cacheAttrib == null) {
                            string localVariableRef = _localVariableRef + (localVarIndex++).ToString(CultureInfo.InvariantCulture);

                            // Variable reference to the local control variable
                            CodeVariableReferenceExpression childCtrlRefExpr = new CodeVariableReferenceExpression(localVariableRef);

                            // e.g. {{controlTypeName}} ctrl5;
                            CodeTypeReference ctrlTypeReference =
                                CodeDomUtility.BuildGlobalCodeTypeReference(ctrlBuilder.ControlType);
                            buildSubControlBlock.Add(new CodeVariableDeclarationStatement(ctrlTypeReference, localVariableRef));

                            // e.g. ctrl5 = __BuildControl__control6();
                            CodeAssignStatement setCtl = new CodeAssignStatement(childCtrlRefExpr, methCallExpression);
                            setCtl.LinePragma = linePragma;
                            buildSubControlBlock.Add(setCtl);

                            // If there is no caching on the control, just create it and add it
                            // e.g. __parser.AddParsedSubObject(ctrl5);
                            BuildAddParsedSubObjectStatement(
                                buildSubControlBlock,
                                childCtrlRefExpr,
                                linePragma,
                                ctrlRefExpr,
                                ref gotParserVariable);
                        }
                        else {
                            string providerName = null;
                            // Only use the providerName parameter when targeting 4.0 and above
                            bool useProviderName = MultiTargetingUtil.IsTargetFramework40OrAbove;
                            if (useProviderName) {
                                providerName = cacheAttrib.ProviderName;
                                if (providerName == OutputCache.ASPNET_INTERNAL_PROVIDER_NAME) {
                                    providerName = null;
                                }
                            }
                            // The control's output is getting cached.  Call
                            // StaticPartialCachingControl.BuildCachedControl to do the work.

                            // e.g. StaticPartialCachingControl.BuildCachedControl(__ctrl, Request, "e4192e6d-cbe0-4df5-b516-682c10415590", __pca, new System.Web.UI.BuildMethod(this.__BuildControlt1));
                            CodeMethodInvokeExpression call = new CodeMethodInvokeExpression();
                            call.Method.TargetObject = new CodeTypeReferenceExpression(typeof(System.Web.UI.StaticPartialCachingControl));
                            call.Method.MethodName = "BuildCachedControl";
                            call.Parameters.Add(ctrlRefExpr);
                            call.Parameters.Add(new CodePrimitiveExpression(ctrlBuilder.ID));

                            // If the caching is shared, use the type of the control as the key
                            // otherwise, generate a guid
                            if (cacheAttrib.Shared) {
                                call.Parameters.Add(new CodePrimitiveExpression(
                                    ctrlBuilder.ControlType.GetHashCode().ToString(CultureInfo.InvariantCulture)));
                            }
                            else
                                call.Parameters.Add(new CodePrimitiveExpression(Guid.NewGuid().ToString()));
                            call.Parameters.Add(new CodePrimitiveExpression(cacheAttrib.Duration));
                            call.Parameters.Add(new CodePrimitiveExpression(cacheAttrib.VaryByParams));
                            call.Parameters.Add(new CodePrimitiveExpression(cacheAttrib.VaryByControls));
                            call.Parameters.Add(new CodePrimitiveExpression(cacheAttrib.VaryByCustom));
                            call.Parameters.Add(new CodePrimitiveExpression(cacheAttrib.SqlDependency));
                            CodeDelegateCreateExpression newDelegate = new CodeDelegateCreateExpression();
                            newDelegate.DelegateType = new CodeTypeReference(typeof(BuildMethod));
                            newDelegate.TargetObject = new CodeThisReferenceExpression();
                            newDelegate.MethodName = buildMethodPrefix + ctrlBuilder.ID;
                            call.Parameters.Add(newDelegate);
                            if (useProviderName) {
                                call.Parameters.Add(new CodePrimitiveExpression(providerName));
                            }
                            buildSubControlBlock.Add(new CodeExpressionStatement(call));
                        }

                    }
                    else if (child is string && !builder.HasAspCode) {

                        // VSWhidbey 276806: if the control cares about the inner text (builder does not allow whitespace literals)
                        // the inner literals should be added to the control.
                        if (!fControlSkin || !builder.AllowWhitespaceLiterals()) {
                            string s = (string) child;
                            CodeExpression expr;

                            if (!UseResourceLiteralString(s)) {
                                // e.g. ((IParserAccessor)__ctrl).AddParsedSubObject(new LiteralControl({{@QuoteCString(text)}}));
                                newExpr = new CodeObjectCreateExpression(typeof(LiteralControl));
                                newExpr.Parameters.Add(new CodePrimitiveExpression(s));
                                expr = newExpr;
                            }
                            else {
                                // Add the string to the resource builder, and get back its offset/size
                                int offset, size;
                                bool fAsciiOnly;
                                _stringResourceBuilder.AddString(s, out offset, out size, out fAsciiOnly);

                                methCallExpression = new CodeMethodInvokeExpression();
                                methCallExpression.Method.TargetObject = new CodeThisReferenceExpression();
                                methCallExpression.Method.MethodName = "CreateResourceBasedLiteralControl";
                                methCallExpression.Parameters.Add(new CodePrimitiveExpression(offset));
                                methCallExpression.Parameters.Add(new CodePrimitiveExpression(size));
                                methCallExpression.Parameters.Add(new CodePrimitiveExpression(fAsciiOnly));
                                expr = methCallExpression;
                            }

                            BuildAddParsedSubObjectStatement(buildSubControlBlock, expr, linePragma, ctrlRefExpr, ref gotParserVariable);
                        }
                    }
                }
            }

            // Process the complex attributes
            if (builder.ComplexPropertyEntries.Count > 0) {

                // Used to deal with the device filter conditionals
                CodeStatementCollection currentStmts;
                CodeStatementCollection nextStmts = statements;
                PropertyEntry previous = null;
                int localVarIndex = 1;
                String localVariableRef = null;

                foreach (ComplexPropertyEntry pseSub in builder.ComplexPropertyEntries) {

                    currentStmts = nextStmts;

                    HandleDeviceFilterConditional(ref previous, pseSub, statements, ref currentStmts, out nextStmts);

                    if (pseSub.Builder is StringPropertyBuilder) {
                        // If it's a string inner property, treat it like a simple property
                        CodeExpression leftExpr, rightExpr = null;

                        // __ctrl.{{_name}}
                        // In case of a string property, there should only be one property name (unlike complex properties)
                        Debug.Assert(pseSub.Name.IndexOf('.') < 0, "pseSub._name.IndexOf('.') < 0");
                        leftExpr = new CodePropertyReferenceExpression(ctrlRefExpr, pseSub.Name);

                        // We need to call BuildStringPropertyExpression so any additional processing can be done
                        rightExpr = BuildStringPropertyExpression(pseSub);

                        // Now that we have both side, add the assignment
                        CodeAssignStatement setStatment = new CodeAssignStatement(leftExpr, rightExpr);
                        setStatment.LinePragma = linePragma;
                        currentStmts.Add(setStatment);

                        continue;
                    }

                    if (pseSub.ReadOnly) {

                        if (fControlSkin && pseSub.Builder != null && pseSub.Builder is CollectionBuilder &&
                        pseSub.Builder.ComplexPropertyEntries.Count > 0) {

                            // If it's a collection on a control theme and the themed collection is not empty, clear it first.
                            // e.g. __ctrl.{{pse_name}}.Clear();

                            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
                            // Look for the "Clear" method on the collection.
                            if (pseSub.Type.GetMethod("Clear", bindingFlags) != null) {
                                CodeMethodReferenceExpression refExpr = new CodeMethodReferenceExpression();
                                refExpr.MethodName = "Clear";
                                refExpr.TargetObject = new CodePropertyReferenceExpression(ctrlRefExpr, pseSub.Name);
                                CodeMethodInvokeExpression invokeClearExpr = new CodeMethodInvokeExpression();
                                invokeClearExpr.Method = refExpr;
                                currentStmts.Add(invokeClearExpr);
                            }
                        }

                        // If it's a readonly prop, pass it as a parameter to the
                        // build method.
                        // e.g. __BuildControl {{controlName}}(__ctrl.{{pse._name}});
                        methCallExpression = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(),
                                                                          buildMethodPrefix + pseSub.Builder.ID);
                        methCallExpression.Parameters.Add(new CodePropertyReferenceExpression(ctrlRefExpr, pseSub.Name));
                        methCallStatement = new CodeExpressionStatement(methCallExpression);
                        methCallStatement.LinePragma = linePragma;
                        currentStmts.Add(methCallStatement);
                    }
                    else {
                        localVariableRef = _localVariableRef + (localVarIndex++).ToString(CultureInfo.InvariantCulture);

                        // e.g. {{controlTypeName}} ctrl4;
                        CodeTypeReference ctrlTypeReference =
                            CodeDomUtility.BuildGlobalCodeTypeReference(pseSub.Builder.ControlType);
                        currentStmts.Add(new CodeVariableDeclarationStatement(ctrlTypeReference, localVariableRef));

                        // Variable reference to the local control variable.
                        CodeVariableReferenceExpression childCtrlRefExpr = new CodeVariableReferenceExpression(localVariableRef);

                        methCallExpression = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(),
                                                                          buildMethodPrefix + pseSub.Builder.ID);
                        methCallStatement = new CodeExpressionStatement(methCallExpression);

                        // e.g. ctrl4 = __BuildControl {{controlName}}();
                        CodeAssignStatement setCtl = new CodeAssignStatement(childCtrlRefExpr, methCallExpression);
                        setCtl.LinePragma = linePragma;
                        currentStmts.Add(setCtl);

                        if (pseSub.IsCollectionItem) {
                            // e.g. __ctrl.Add(ctrl4);
                            methCallExpression = new CodeMethodInvokeExpression(ctrlRefExpr, "Add");
                            methCallStatement = new CodeExpressionStatement(methCallExpression);
                            methCallStatement.LinePragma = linePragma;
                            currentStmts.Add(methCallStatement);
                            methCallExpression.Parameters.Add(childCtrlRefExpr);
                        }
                        else {
                            // e.g. __ctrl.{{pse._name}} = {{controlName}};
                            CodeAssignStatement set = new CodeAssignStatement();
                            set.Left = new CodePropertyReferenceExpression(ctrlRefExpr, pseSub.Name);
                            set.Right = childCtrlRefExpr;
                            set.LinePragma = linePragma;
                            currentStmts.Add(set);
                        }
                    }
                }
            }

            // If there are bound properties, hook up the binding method
            if (builder.BoundPropertyEntries.Count > 0) {

                bool isBindableTemplateBuilder = builder is BindableTemplateBuilder;
                bool hasDataBindingEntry = false;

                // Used to deal with the device filter conditionals
                CodeStatementCollection currentStmts;
                CodeStatementCollection methodStatements = statements;
                CodeStatementCollection nextStmts = statements;
                PropertyEntry previous = null;

                bool hasTempObject = false;

                foreach (BoundPropertyEntry entry in builder.BoundPropertyEntries) {

                    // Skip two-way entries if it's a BindableTemplateBuilder or the two-way entry has no setter
                    if (entry.TwoWayBound && (isBindableTemplateBuilder || entry.ReadOnlyProperty))
                        continue;

                    if (entry.IsDataBindingEntry) {
                        hasDataBindingEntry = true;
                        continue;
                    }

                    currentStmts = nextStmts;

                    HandleDeviceFilterConditional(ref previous, entry, statements, ref currentStmts, out nextStmts);

                    ExpressionBuilder eb = entry.ExpressionBuilder;
                    Debug.Assert(eb != null, "Did not expect null expression builder");
                    eb.BuildExpression(entry, builder, ctrlRefExpr, methodStatements, currentStmts, null, ref hasTempObject);
                }

                if (hasDataBindingEntry) {

                    EventInfo eventInfo = DataBindingExpressionBuilder.Event;

                    // __ctrl.{EventName} += new EventHandler(this.{{bindingMethod}})
                    CodeDelegateCreateExpression newDelegate = new CodeDelegateCreateExpression();
                    CodeAttachEventStatement attachEvent = new CodeAttachEventStatement(ctrlRefExpr, eventInfo.Name, newDelegate);
                    attachEvent.LinePragma = linePragma;
                    newDelegate.DelegateType = new CodeTypeReference(typeof(EventHandler));
                    newDelegate.TargetObject = new CodeThisReferenceExpression();
                    newDelegate.MethodName = GetExpressionBuilderMethodName(eventInfo.Name, builder);
                    statements.Add(attachEvent);
                }
            }

            if (builder is DataBoundLiteralControlBuilder) {

                // __ctrl.DataBinding += new EventHandler(this.{{bindingMethod}})
                CodeDelegateCreateExpression newDelegate = new CodeDelegateCreateExpression();
                CodeAttachEventStatement attachEvent = new CodeAttachEventStatement(ctrlRefExpr, "DataBinding", newDelegate);
                attachEvent.LinePragma = linePragma;
                newDelegate.DelegateType = new CodeTypeReference(typeof(EventHandler));
                newDelegate.TargetObject = new CodeThisReferenceExpression();
                newDelegate.MethodName = BindingMethodName(builder);
                statements.Add(attachEvent);
            }

            // If there is any ASP code, set the render method delegate
            if (builder.HasAspCode && !fControlSkin) {
                // e.g. __ctrl.SetRenderMethodDelegate(new RenderMethod(this.__Render {{controlName}}));
                CodeDelegateCreateExpression newDelegate = new CodeDelegateCreateExpression();
                newDelegate.DelegateType = new CodeTypeReference(typeof(RenderMethod));
                newDelegate.TargetObject = new CodeThisReferenceExpression();
                newDelegate.MethodName = "__Render" + builder.ID;

                methCallExpression = new CodeMethodInvokeExpression(ctrlRefExpr, "SetRenderMethodDelegate");
                methCallExpression.Parameters.Add(newDelegate);
                methCallStatement = new CodeExpressionStatement(methCallExpression);

                // VSWhidbey 579101
                // If this is a contentPlaceHolder, we need to check if there is any content defined.
                // We set the render method only when there is no contentTemplate defined.
                // if ((this.__Template_TestTemplate == null)) {
                //     __ctrl.SetRenderMethodDelegate(new RenderMethod(this.__Render {{controlName}}));
                // }
                if (builder is System.Web.UI.WebControls.ContentPlaceHolderBuilder) {
                    string templateName = ((System.Web.UI.WebControls.ContentPlaceHolderBuilder)builder).Name;
                    autoTemplateName = MasterPageControlBuilder.AutoTemplatePrefix + templateName;
                    string fieldName = "__" + autoTemplateName;
                    CodeExpression templateFieldRef = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName);
                    templateIfStmt = new CodeConditionStatement();
                    templateIfStmt.Condition = new CodeBinaryOperatorExpression(templateFieldRef, CodeBinaryOperatorType.IdentityEquality, new CodePrimitiveExpression(null));
                    templateIfStmt.TrueStatements.Add(methCallStatement);
                    statements.Add(templateIfStmt);
                }
                else {
                    statements.Add(methCallStatement);
                }
            }

            // Process the events
            if (builder.EventEntries.Count > 0) {

                foreach (EventEntry eventEntry in builder.EventEntries) {

                    // Attach the event.  Detach it first to avoid duplicates (see ASURT 42603),
                    // but only if there is codebehind
                    // 


                    // e.g. __ctrl.ServerClick -= new System.EventHandler(this.buttonClicked);
                    // e.g. __ctrl.ServerClick += new System.EventHandler(this.buttonClicked);
                    CodeDelegateCreateExpression newDelegate = new CodeDelegateCreateExpression();
                    newDelegate.DelegateType = new CodeTypeReference(eventEntry.HandlerType);
                    newDelegate.TargetObject = new CodeThisReferenceExpression();
                    newDelegate.MethodName = eventEntry.HandlerMethodName;

                    if (Parser.HasCodeBehind) {
                        CodeRemoveEventStatement detachEvent = new CodeRemoveEventStatement(ctrlRefExpr, eventEntry.Name, newDelegate);
                        detachEvent.LinePragma = linePragma;
                        statements.Add(detachEvent);
                    }

                    CodeAttachEventStatement attachEvent = new CodeAttachEventStatement(ctrlRefExpr, eventEntry.Name, newDelegate);
                    attachEvent.LinePragma = linePragma;
                    statements.Add(attachEvent);
                }
            }

            // If a control field is declared, we need to return it at the end of the method.
            if (fControlFieldDeclared)
                statements.Add(new CodeMethodReturnStatement(ctrlRefExpr));
        }

        /*
         * Build the template's method to extract values from contained controls
         */
        protected void BuildExtractMethod(ControlBuilder builder) {
            BindableTemplateBuilder bindableTemplateBuilder = builder as BindableTemplateBuilder;
            // This will get called if Bind is in a non-bindable template.  We should just skip the Extract method.
            if (bindableTemplateBuilder != null && bindableTemplateBuilder.HasTwoWayBoundProperties) {
                // Get the name of the databinding method
                string methodName = ExtractMethodName(builder);
                const string tableVarName = "__table";
                const string containerVarName = "__container";

                // Same linePragma in the entire method
                CodeLinePragma linePragma = CreateCodeLinePragma(builder);

                CodeMemberMethod method = new CodeMemberMethod();
                AddDebuggerNonUserCodeAttribute(method);
                method.Name = methodName;
                method.Attributes &= ~MemberAttributes.AccessMask;
                method.Attributes |= MemberAttributes.Public;
                method.ReturnType = new CodeTypeReference(typeof(IOrderedDictionary));
                _sourceDataClass.Members.Add(method);

                /// Variable declarations need to go at the top for CodeDom compliance.
                CodeStatementCollection topLevelStatements = method.Statements;
                CodeStatementCollection statements = new CodeStatementCollection();

                // add a container control parameter
                method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(Control), containerVarName));

                // OrderedDictionary table;
                CodeVariableDeclarationStatement tableDecl = new CodeVariableDeclarationStatement(typeof(OrderedDictionary), tableVarName);
                topLevelStatements.Add(tableDecl);

                // table = new OrderedDictionary();
                CodeObjectCreateExpression newTableExpression = new CodeObjectCreateExpression(typeof(OrderedDictionary));
                CodeAssignStatement newTableAssign = new CodeAssignStatement(new CodeVariableReferenceExpression(tableVarName),
                                                                             newTableExpression);
                newTableAssign.LinePragma = linePragma;
                statements.Add(newTableAssign);

                BuildExtractStatementsRecursive(bindableTemplateBuilder.SubBuilders, statements, topLevelStatements, linePragma, tableVarName, containerVarName);

                // return table;
                CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement(new CodeVariableReferenceExpression(tableVarName));
                statements.Add(returnStatement);

                // add all the non-variable declaration statements to the bottom of the method
                method.Statements.AddRange(statements);
            }
        }

        private void BuildExtractStatementsRecursive(ArrayList subBuilders, CodeStatementCollection statements, CodeStatementCollection topLevelStatements, CodeLinePragma linePragma, string tableVarName, string containerVarName) {
            foreach (object subBuilderObject in subBuilders) {
                ControlBuilder controlBuilder = subBuilderObject as ControlBuilder;
                if (controlBuilder != null) {
                    // Used to deal with the device filter conditionals
                    CodeStatementCollection currentStatements = null;
                    CodeStatementCollection nextStatements = statements;
                    PropertyEntry previous = null;
                    string previousControlName = null;
                    bool newControl = true;
                    
                    foreach (BoundPropertyEntry entry in controlBuilder.BoundPropertyEntries) {

                        // Skip all entries that are not two-way
                        if (!entry.TwoWayBound)
                            continue;

                        // Reset the "previous" Property Entry if we're not looking at the same control.
                        // If we don't do this, Two controls that have conditionals on the same named property will have
                        // their conditionals incorrectly merged.
                        if (String.Compare(previousControlName, entry.ControlID, StringComparison.Ordinal) != 0) {
                            previous = null;
                            newControl = true;
                        }
                        else {
                            newControl = false;
                        }

                        previousControlName = entry.ControlID;
                        currentStatements = nextStatements;

                        HandleDeviceFilterConditional(ref previous, entry, statements, ref currentStatements, out nextStatements);

                        // Only declare the variable if it hasn't already been declared by a previous filter
                        // or property binding on the same control.
                        if (newControl) {
                            // {{controlType}} {{controlID}};
                            // eg. TextBox t1;
                            CodeVariableDeclarationStatement controlDecl = new CodeVariableDeclarationStatement(entry.ControlType, entry.ControlID);
                            topLevelStatements.Add(controlDecl);

                            // {{controlID}} = ({{controlType}})container.FindControl("{{controlID}}");
                            // eg. t1 = (TextBox)container.FindControl("t1");
                            CodeMethodInvokeExpression findControlCallExpression = new CodeMethodInvokeExpression(
                                    new CodeVariableReferenceExpression(containerVarName), "FindControl");
                            string findControlParameter = entry.ControlID;
                            findControlCallExpression.Parameters.Add(new CodePrimitiveExpression(findControlParameter));
                            CodeCastExpression castExpression = new CodeCastExpression(entry.ControlType, findControlCallExpression);

                            CodeAssignStatement findControlAssign = new CodeAssignStatement(new CodeVariableReferenceExpression(entry.ControlID),
                                                                                            castExpression);
                            findControlAssign.LinePragma = linePragma;
                            topLevelStatements.Add(findControlAssign);
                        }

                        // if ({{controlID}} != null)
                        //     table["{{fieldName}}"] = {{controlID}}.{{propertyName}});
                        // eg. if (t1 != null)
                        // eg.     table["field"] = t1.Text);
                        CodeConditionStatement ifStatement = new CodeConditionStatement();
                        CodeBinaryOperatorExpression ensureControlExpression = new CodeBinaryOperatorExpression();
                        ensureControlExpression.Operator = CodeBinaryOperatorType.IdentityInequality;
                        ensureControlExpression.Left = new CodeVariableReferenceExpression(entry.ControlID);
                        ensureControlExpression.Right = new CodePrimitiveExpression(null);
                        ifStatement.Condition = ensureControlExpression;

                        string fieldParameter = entry.FieldName;
                        CodeIndexerExpression tableIndexer = new CodeIndexerExpression(new CodeVariableReferenceExpression(tableVarName),
                                                                                       new CodePrimitiveExpression(fieldParameter));

                        // VJ# does not support automatic boxing of value types, so passing a simple type, say a bool, into a method
                        // expecting an object will give a compiler error.  We are working around this issue by adding special code for
                        // VJ# that will cast the expression for boxing.  When the VJ# team adds implicit boxing of value types, we
                        // should remove this code.  VSWhidbey 269028
                        CodeExpression controlPropertyExpression = CodeDomUtility.BuildPropertyReferenceExpression(new CodeVariableReferenceExpression(entry.ControlID), entry.Name);
                        if (_usingVJSCompiler) {
                            controlPropertyExpression = CodeDomUtility.BuildJSharpCastExpression(entry.Type, controlPropertyExpression); 
                        }
                        CodeAssignStatement tableIndexAssign = new CodeAssignStatement(tableIndexer, controlPropertyExpression);

                        ifStatement.TrueStatements.Add(tableIndexAssign);
                        ifStatement.LinePragma = linePragma;
                        currentStatements.Add(ifStatement);
                    }

                    if (controlBuilder.SubBuilders.Count > 0) {
                        BuildExtractStatementsRecursive(controlBuilder.SubBuilders, statements, topLevelStatements, linePragma, tableVarName, containerVarName);
                    }

                    // Dev10 




                    ArrayList list = new ArrayList();
                    AddEntryBuildersToList(controlBuilder.ComplexPropertyEntries, list);
                    AddEntryBuildersToList(controlBuilder.TemplatePropertyEntries, list);

                    if (list.Count > 0) {
                        BuildExtractStatementsRecursive(list, statements, topLevelStatements, linePragma, tableVarName, containerVarName);
                    }
                }
            }

        }

        private void AddEntryBuildersToList(ICollection entries, ArrayList list) {
            if (entries == null || list == null) {
                return;
            }
            foreach (BuilderPropertyEntry entry in entries) {
                if (entry.Builder != null) {
                    TemplatePropertyEntry templatePropertyEntry = entry as TemplatePropertyEntry;
                    // Only add template entries that have TemplateInstance.Single
                    if (templatePropertyEntry != null && templatePropertyEntry.IsMultiple) {
                        continue;
                    }
                    list.Add(entry.Builder);
                }
            }
        }

        /*
         * Build the member field's declaration for a control
         */
        private void BuildFieldDeclaration(ControlBuilder builder) {

            // Do not generate member field for content controls.
            if (builder is System.Web.UI.WebControls.ContentBuilderInternal) {
                return;
            }

            bool hideExistingMember = false;

            // If we're using a non-default base class
            if (Parser.BaseType != null) {
                // Check if it has a non-private field or property that has a name that
                // matches the id of the control.

                Type memberType = Util.GetNonPrivateFieldType(Parser.BaseType, builder.ID);

                // Couldn't find a field, try a property (ASURT 45039)
                // 
                if (memberType == null)
                    memberType = Util.GetNonPrivatePropertyType(Parser.BaseType, builder.ID);

                if (memberType != null) {

                    if (!memberType.IsAssignableFrom(builder.ControlType)) {

                        if (!(typeof(Control)).IsAssignableFrom(memberType)) {
                            // If it's not a control, it's probably an unrelated member,
                            // and we should just hide it (VSWhidbey 217135)
                            hideExistingMember = true;
                        }
                        else {
                            throw new HttpParseException(SR.GetString(SR.Base_class_field_with_type_different_from_type_of_control,
                                builder.ID, memberType.FullName, builder.ControlType.FullName), null,
                                builder.VirtualPath, null, builder.Line);
                        }
                    }
                    else {
                        // Don't build the declaration, since the base class already declares it
                        return;
                    }
                }
            }

            // Add the field.  Make it protected if the ID was declared, and private if it was generated
            CodeMemberField field = new CodeMemberField(CodeDomUtility.BuildGlobalCodeTypeReference(
                builder.DeclareType), builder.ID);
            field.Attributes &= ~MemberAttributes.AccessMask;

            // If we need to hide an existing member, use 'new' (VSWhidbey 217135)
            if (hideExistingMember)
                field.Attributes |= MemberAttributes.New;

            field.LinePragma = CreateCodeLinePragma(builder);

            field.Attributes |= MemberAttributes.Family;

            // Set WithEvents in the UserData, so that the field will be
            // declared as WithEvents in VB (VSWhidbey 156623).
            // But only do this if it's a Control, otherwise it may not have
            // any events (VSWhidbey 283274).
            if (typeof(Control).IsAssignableFrom(builder.DeclareType)) {
                field.UserData["WithEvents"] = true;
            }

            _intermediateClass.Members.Add(field);
        }

        private string GetExpressionBuilderMethodName(string eventName, ControlBuilder builder) {
            return "__" + eventName + builder.ID;
        }

        /*
         * Return the name of a databinding method
         */
        private string BindingMethodName(ControlBuilder builder) {
            return "__DataBind" + builder.ID;
        }

        protected CodeMemberMethod BuildPropertyBindingMethod(ControlBuilder builder, bool fControlSkin) {
            // VSWhidbey 275175: Create the tempObjectVariable "__o" only when it's used.
            bool tempObjectVariableDeclared = false;

            if (builder is DataBoundLiteralControlBuilder) {
                // Get the name of the databinding method
                string methodName = BindingMethodName(builder);

                // Same linePragma in the entire method
                CodeLinePragma linePragma = CreateCodeLinePragma(builder);

                CodeMemberMethod method = new CodeMemberMethod();
                method.Name = methodName;
                method.Attributes &= ~MemberAttributes.AccessMask;
                method.Attributes |= MemberAttributes.Public;

                method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "sender"));
                method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(EventArgs), "e"));

                CodeStatementCollection topMethodStatements = new CodeStatementCollection();
                CodeStatementCollection otherMethodStatements = new CodeStatementCollection();

                // {{controlType}} target;
                CodeVariableDeclarationStatement targetDecl = new CodeVariableDeclarationStatement(builder.ControlType, "target");
                Type bindingContainerType = builder.BindingContainerType;

                CodeVariableDeclarationStatement containerDecl = new CodeVariableDeclarationStatement(bindingContainerType, "Container");

                topMethodStatements.Add(containerDecl);
                topMethodStatements.Add(targetDecl);

                // target = ({{controlType}}) sender;
                CodeAssignStatement setTarget = new CodeAssignStatement(new CodeVariableReferenceExpression(targetDecl.Name),
                                                                        new CodeCastExpression(builder.ControlType,
                                                                                               new CodeArgumentReferenceExpression("sender")));
                setTarget.LinePragma = linePragma;
                otherMethodStatements.Add(setTarget);

                // {{containerType}} Container = ({{containerType}}) target.BindingContainer;
                CodeAssignStatement setContainer = new CodeAssignStatement(new CodeVariableReferenceExpression(containerDecl.Name),
                                                                           new CodeCastExpression(bindingContainerType,
                                                                                                  new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("target"),
                                                                                                                                      "BindingContainer")));
                setContainer.LinePragma = linePragma;
                otherMethodStatements.Add(setContainer);

                DataBindingExpressionBuilder.GenerateItemTypeExpressions(builder, topMethodStatements, otherMethodStatements, linePragma, "Item");
                //Generate code for BindItem as well at design time in addition to Item for intellisense.
                //When you are in designer and as you type to set an attribute value to a data binding expression, since the control's end tag is not yet
                //typed in, we will still have a DataBoundLiteralControl (instead of a ControlBuilder) in that scenario and the below code takes care of that scenario.
                if (_designerMode) {
                    DataBindingExpressionBuilder.GenerateItemTypeExpressions(builder, topMethodStatements, otherMethodStatements, linePragma, "BindItem");
                }

                // If it's a DataBoundLiteralControl, call SetDataBoundString for each
                // of the databinding expressions
                int i = -1;
                foreach (object child in builder.SubBuilders) {
                    i++;

                    // Ignore it if it's null
                    if (child == null)
                        continue;

                    // Only deal with the databinding expressions here, which have odd index
                    if (i % 2 == 0) {
                        Debug.Assert(child is string, "child is string");
                        continue;
                    }

                    CodeBlockBuilder codeBlock = (CodeBlockBuilder) child;
                    Debug.Assert(codeBlock.BlockType == CodeBlockType.DataBinding);

                    // In designer mode, generate a much simpler assignment to make
                    // the code simpler (since it doesn't actually need to run).
                    if (_designerMode) {
                        tempObjectVariableDeclared = GenerateSimpleAssignmentAtDesignTime(tempObjectVariableDeclared, topMethodStatements, otherMethodStatements, codeBlock.Content, CreateCodeLinePragma(codeBlock));
                        continue;
                    }

                    CodeExpression expr = new CodeSnippetExpression(codeBlock.Content.Trim());
                    if (codeBlock.IsEncoded) {
                        // HttpUtility.HtmlEncode({{codeExpr}}));
                        expr = new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(
                                new CodeTypeReferenceExpression(typeof(HttpUtility)),
                               "HtmlEncode"),
                            expr);
                    }
                    else {
                        // System.Convert.ToString({{codeExpr}});
                        expr = CodeDomUtility.GenerateConvertToString(expr);
                    }

                    // target.SetDataBoundString(3, {{codeExpr}});  {{codeExpr}} is one of the above in if-else depending on IsEncoded property.
                    CodeMethodInvokeExpression methCallExpression = new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("target"), "SetDataBoundString");
                    methCallExpression.Parameters.Add(new CodePrimitiveExpression(i/2));
                    methCallExpression.Parameters.Add(expr);

                    CodeStatement setDataBoundStringCall = new CodeExpressionStatement(methCallExpression);
                    setDataBoundStringCall.LinePragma = CreateCodeLinePragma(codeBlock);
                    otherMethodStatements.Add(setDataBoundStringCall);
                }

                foreach (CodeStatement stmt in topMethodStatements) {
                    method.Statements.Add(stmt);
                }
                foreach (CodeStatement stmt in otherMethodStatements) {
                    method.Statements.Add(stmt);
                }
                _sourceDataClass.Members.Add(method);
                return method;
            }
            else {

                EventInfo eventInfo = DataBindingExpressionBuilder.Event;

                // Same linePragma in the entire method
                CodeLinePragma linePragma = CreateCodeLinePragma(builder);

                CodeMemberMethod method = null;
                CodeStatementCollection topStatements = null;
                CodeStatementCollection otherStatements = null;

                // Used to deal with the device filter conditionals
                CodeStatementCollection currentStmts;
                CodeStatementCollection nextStmts = null;
                PropertyEntry previous = null;

                bool isBindableTemplateBuilder = builder is BindableTemplateBuilder;
                bool firstEntry = true;

                bool hasTempObject = false;

                foreach (BoundPropertyEntry entry in builder.BoundPropertyEntries) {
                    // Skip two-way entries if it's a BindableTemplateBuilder or the two way entry is read only
                    if (entry.TwoWayBound && (isBindableTemplateBuilder || entry.ReadOnlyProperty))
                        continue;

                    // We only care about databinding entries here
                    if (!entry.IsDataBindingEntry)
                        continue;

                    if (firstEntry) {
                        firstEntry = false;

                        method = new CodeMemberMethod();
                        topStatements = new CodeStatementCollection();
                        otherStatements = new CodeStatementCollection();

                        // Get the name of the databinding method
                        string methodName = GetExpressionBuilderMethodName(eventInfo.Name, builder);
                        method.Name = methodName;
                        method.Attributes &= ~MemberAttributes.AccessMask;
                        method.Attributes |= MemberAttributes.Public;

                        if (_designerMode) {
                            ApplyEditorBrowsableCustomAttribute(method);
                        }

                        Type eventHandlerType = eventInfo.EventHandlerType;
                        MethodInfo mi = eventHandlerType.GetMethod("Invoke");
                        ParameterInfo[] paramInfos = mi.GetParameters();
                        foreach (ParameterInfo pi in paramInfos) {
                            method.Parameters.Add(new CodeParameterDeclarationExpression(pi.ParameterType, pi.Name));
                        }

                        nextStmts = otherStatements;

                        DataBindingExpressionBuilder.BuildExpressionSetup(builder, topStatements, otherStatements, linePragma, entry.TwoWayBound, _designerMode);

                        _sourceDataClass.Members.Add(method);
                    }

                    currentStmts = nextStmts;

                    HandleDeviceFilterConditional(ref previous, entry, otherStatements, ref currentStmts, out nextStmts);

                    // In designer mode, generate a much simpler assignment to make
                    // the code simpler (since it doesn't actually need to run).
                    if (_designerMode) {
                        int generatedColumn = tempObjectVariable.Length + BaseCodeDomTreeGenerator.GetGeneratedColumnOffset(_codeDomProvider);
                        CodeLinePragma codeLinePragma = CreateCodeLinePragma(virtualPath: builder.PageVirtualPath, lineNumber: entry.Line, column: entry.Column, generatedColumn: generatedColumn, codeLength: entry.Expression.Length);
                        tempObjectVariableDeclared = GenerateSimpleAssignmentAtDesignTime(tempObjectVariableDeclared, topStatements, otherStatements, entry.Expression, codeLinePragma);
                        continue;
                    }

                    if (entry.TwoWayBound) {
                        Debug.Assert(!entry.ReadOnlyProperty, "We should not attempt to build a data binding handler if the two way entry is read only.");
                        Debug.Assert(!entry.UseSetAttribute, "Two-way binding is not supported on expandos - this should have been prevented in ControlBuilder");
                        DataBindingExpressionBuilder.BuildEvalExpression(entry.FieldName, entry.FormatString,
                                                                         entry.Name, entry.Type, builder, topStatements, currentStmts, linePragma, entry.IsEncoded, ref hasTempObject);
                    }
                    else {
                        DataBindingExpressionBuilder.BuildExpressionStatic(entry, builder, null, topStatements, currentStmts, linePragma, entry.IsEncoded, ref hasTempObject);
                    }
                }
                if (topStatements != null) {
                    foreach (CodeStatement stmt in topStatements) {
                        method.Statements.Add(stmt);
                    }
                }
                if (otherStatements != null) {
                    foreach (CodeStatement stmt in otherStatements) {
                        method.Statements.Add(stmt);
                    }
                }
                return method;
            }
        }

        /*
         * Build the data tree for a control's render method
         */
        internal void BuildRenderMethod(ControlBuilder builder, bool fTemplate) {

            CodeMemberMethod method = new CodeMemberMethod();
            method.Attributes = MemberAttributes.Private | MemberAttributes.Final;
            method.Name = "__Render" + builder.ID;

            if (_designerMode) {
                ApplyEditorBrowsableCustomAttribute(method);
            }

            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(HtmlTextWriter), renderMethodParameterName));
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(Control), "parameterContainer"));

            _sourceDataClass.Members.Add(method);

            // VSWhidbey 275175: Create the tempObjectVariable "__o" only when it's used.
            bool tempObjectVariableDeclared = false;

            // Process the children if any
            if (builder.SubBuilders != null) {
                IEnumerator en = builder.SubBuilders.GetEnumerator();

                // Index that the control will have in its parent's Controls
                // collection.
                // 



                int controlIndex = 0;

                for (int i=0; en.MoveNext(); i++) {
                    object child = en.Current;

                    CodeLinePragma linePragma = null;

                    if (child is ControlBuilder) {
                        linePragma = CreateCodeLinePragma((ControlBuilder)child);
                    }

                    if (child is string) {
                        if (_designerMode) continue;

                        AddOutputWriteStringStatement(method.Statements, (string)child);
                    }
                    else if (child is CodeBlockBuilder) {
                        CodeBlockBuilder codeBlockBuilder = (CodeBlockBuilder)child;

                        if (codeBlockBuilder.BlockType == CodeBlockType.Expression || codeBlockBuilder.BlockType == CodeBlockType.EncodedExpression) {

                            string codeExpression = codeBlockBuilder.Content;

                            // In designer mode, generate a much simpler assignment to make
                            // the code simpler (since it doesn't actually need to run).
                            if (_designerMode) {
                                tempObjectVariableDeclared = GenerateSimpleAssignmentAtDesignTime(tempObjectVariableDeclared, method.Statements, method.Statements, codeExpression, linePragma);
                                continue;
                            }

                            // The purpose of the following logic is to improve the debugging experience.
                            // Basically, we gain control on the formatting of the generated line
                            // that calls output.Write, in order to try to make the call line up
                            // with the <%= ... %> block.  It's not always perfect, but it does a decent job
                            // and is always better than the v1 behavior.

                            // Get the Write() statement codedom tree
                            CodeStatement outputWrite = GetOutputWriteStatement(
                                new CodeSnippetExpression(codeExpression),
                                codeBlockBuilder.BlockType == CodeBlockType.EncodedExpression /*encode*/);

                            // Use codedom to generate the statement as a string in the target language
                            TextWriter w = new StringWriter(CultureInfo.InvariantCulture);
                            _codeDomProvider.GenerateCodeFromStatement(outputWrite, w, null /*CodeGeneratorOptions*/);
                            string outputWriteString = w.ToString();

                            // The '+3' is used to make sure the generated code is positioned properly to match 
                            // the location of user code (due to the <%= %> separators).
                            outputWriteString = outputWriteString.PadLeft(
                                codeBlockBuilder.Column + codeExpression.Length + 3);

                            // We can then use this string as a snippet statement
                            CodeSnippetStatement lit = new CodeSnippetStatement(outputWriteString);
                            lit.LinePragma = linePragma;
                            method.Statements.Add(lit);
                        }
                        else {
                            // It's a <% ... %> block
                            Debug.Assert(codeBlockBuilder.BlockType == CodeBlockType.Code);

                            // Pad the code block so its generated offset matches the aspx
                            string code = codeBlockBuilder.Content;
                            code = code.PadLeft(code.Length + codeBlockBuilder.Column - 1);

                            CodeSnippetStatement lit = new CodeSnippetStatement(code);
                            lit.LinePragma = linePragma;
                            method.Statements.Add(lit);
                        }
                    }
                    else if (child is CodeStatementBuilder) {

                        if (_designerMode) continue;

                        CodeStatementBuilder statementBuilder = (CodeStatementBuilder)child;

                        CodeStatement statement = statementBuilder.BuildStatement(new CodeArgumentReferenceExpression(renderMethodParameterName));

                        method.Statements.Add(statement);
                    }
                    else if (child is ControlBuilder) {

                        if (_designerMode) continue;

                        // parameterContainer.Controls['controlIndex++'].RenderControl(output)
                        CodeMethodInvokeExpression methodInvoke = new CodeMethodInvokeExpression();
                        CodeExpressionStatement methodCall = new CodeExpressionStatement(methodInvoke);
                        methodInvoke.Method.TargetObject = new CodeIndexerExpression(new CodePropertyReferenceExpression(new CodeArgumentReferenceExpression("parameterContainer"),
                                                                                                                             "Controls"),
                                                                                         new CodeExpression[] {
                                                                                             new CodePrimitiveExpression(controlIndex++),
                                                                                         });
                        methodInvoke.Method.MethodName = "RenderControl";

                        // Don't generate a line pragma on the RenderControl call, as it degrades the
                        // debugging experience (VSWhidbey 482416)

                        methodInvoke.Parameters.Add(new CodeArgumentReferenceExpression(renderMethodParameterName));
                        method.Statements.Add(methodCall);
                    }
                }
            }
        }

        private bool GenerateSimpleAssignmentAtDesignTime(bool tempObjectVariableDeclared, CodeStatementCollection topMethodStatements, CodeStatementCollection otherMethodStatements, string content, CodeLinePragma linePragma) {
            // In designer mode, add an object variable used for simplified code expression generation
            if (!tempObjectVariableDeclared) {
                tempObjectVariableDeclared = true;
                // object __o;
                topMethodStatements.Add(new CodeVariableDeclarationStatement(
                    typeof(object), tempObjectVariable));
            }

            // e.g. __o = <user expression>;
            CodeStatement simpleAssignment = new CodeAssignStatement(
                new CodeVariableReferenceExpression(tempObjectVariable),
                new CodeSnippetExpression(content));
            simpleAssignment.LinePragma = linePragma;
            otherMethodStatements.Add(simpleAssignment);
            return tempObjectVariableDeclared;
        }

        protected virtual void BuildSourceDataTreeFromBuilder(ControlBuilder builder,
                                                    bool fInTemplate, bool topLevelControlInTemplate,
                                                    PropertyEntry pse) {

            // Don't do anything for Code blocks
            if (builder is CodeBlockBuilder || builder is CodeStatementBuilder)
                return;

            // Is the current builder for a template?
            bool fTemplate = (builder is TemplateBuilder);

            // For the control name in the compiled code, we use the
            // ID if one is available (but don't use the ID inside a template)
            // Otherwise, we generate a unique name.
            if (builder.ID == null || fInTemplate) {
                // Increase the control count to generate unique ID's
                _controlCount++;

                builder.ID = "__control" + _controlCount.ToString(NumberFormatInfo.InvariantInfo);
                builder.IsGeneratedID = true;
            }

            // Process the children
            if (builder.SubBuilders != null) {
                foreach (object child in builder.SubBuilders) {
                    if (child is ControlBuilder) {
                        // Do not treat it as top level control in template if the control is at top-level of a file.
                        bool isTopLevelCtrlInTemplate =
                            fTemplate && typeof(Control).IsAssignableFrom(((ControlBuilder)child).ControlType) && !(builder is RootBuilder);
                        BuildSourceDataTreeFromBuilder((ControlBuilder)child, fInTemplate, isTopLevelCtrlInTemplate, null);
                    }
                }
            }

            foreach (TemplatePropertyEntry entry in builder.TemplatePropertyEntries) {
                bool inTemplate = true;

                // If the template container does not allow multiple instances,
                // treat the controls as if not in templates.
                if (entry.PropertyInfo != null) {
                    inTemplate = entry.IsMultiple;
                }

                BuildSourceDataTreeFromBuilder(((TemplatePropertyEntry)entry).Builder, inTemplate, false /*topLevelControlInTemplate*/, entry);
            }

            foreach (ComplexPropertyEntry entry in builder.ComplexPropertyEntries) {
                // Don't create a build method for inner property strings
                if (!(entry.Builder is StringPropertyBuilder)) {
                    BuildSourceDataTreeFromBuilder(((ComplexPropertyEntry)entry).Builder, fInTemplate, false /*topLevelControlInTemplate*/, entry);
                }
            }

            // Build a field declaration for the control if ID is defined on the control.
            // (Not a generated ID)
            if (!builder.IsGeneratedID)
                BuildFieldDeclaration(builder);

            CodeMemberMethod buildMethod = null;
            CodeMemberMethod dataBindingMethod = null;

            // Skip the rest if we're only generating the intermediate class
            if (_sourceDataClass != null) {

                if (!_designerMode) {
                    // Build a Build method for the control
                    buildMethod = BuildBuildMethod(builder, fTemplate, fInTemplate, topLevelControlInTemplate, pse, false);
                }

                // Build a Render method for the control, unless it has no code
                if (builder.HasAspCode) {
                    BuildRenderMethod(builder, fTemplate);
                }

                // Build a method to extract values from the template
                BuildExtractMethod(builder);

                // Build a property binding method for the control
                dataBindingMethod = BuildPropertyBindingMethod(builder, false);
            }

            // Give the ControlBuilder a chance to look at and modify the tree
            builder.ProcessGeneratedCode(_codeCompileUnit, _intermediateClass,
                _sourceDataClass, buildMethod, dataBindingMethod);

            if (Parser.ControlBuilderInterceptor != null) {
                Parser.ControlBuilderInterceptor.OnProcessGeneratedCode(builder, _codeCompileUnit,
                    _intermediateClass, _sourceDataClass, buildMethod, dataBindingMethod, builder.AdditionalState);
            }

            // Give the ParseRecorder a chance to look at and modify the tree
            Parser.ParseRecorders.ProcessGeneratedCode(builder, _codeCompileUnit,
                _intermediateClass, _sourceDataClass, buildMethod, dataBindingMethod);
        }

        internal virtual CodeExpression BuildStringPropertyExpression(PropertyEntry pse) {
            string value = String.Empty;
            if (pse is SimplePropertyEntry) {
                value = (string)((SimplePropertyEntry)pse).Value;
            }
            else {
                Debug.Assert(pse is ComplexPropertyEntry);
                ComplexPropertyEntry cpe = (ComplexPropertyEntry)pse;
                value = (string)((StringPropertyBuilder)cpe.Builder).BuildObject();
            }

            return CodeDomUtility.GenerateExpressionForValue(pse.PropertyInfo, value, typeof(string));
        }

        protected virtual CodeAssignStatement BuildTemplatePropertyStatement(CodeExpression ctrlRefExpr) {

            // e.g. __ctrl.TemplateControl = this;
            CodeAssignStatement assign = new CodeAssignStatement();
            assign.Left = new CodePropertyReferenceExpression(ctrlRefExpr, "TemplateControl");
            assign.Right = new CodeThisReferenceExpression();
            return assign;
        }

        /*
         * Return the name of an extract method
         */
        private string ExtractMethodName(ControlBuilder builder) {
            return extractTemplateValuesMethodPrefix + builder.ID;
        }


        private Type GetCtrlTypeForBuilder(ControlBuilder builder, bool fTemplate) {

            if (builder is RootBuilder && builder.ControlType != null)
                return builder.ControlType;

            if (fTemplate)
                return typeof(Control);

            return builder.ControlType;
        }

        protected string GetMethodNameForBuilder(string prefix, ControlBuilder builder) {
            if (builder is RootBuilder) {
                return prefix + "Tree";
            }
            else {
                return prefix + builder.ID;
            }
        }

        /*
         * Helper method to generate the device filter conditionals.  e.g.
         *   if (this.TestDeviceFilter("FilterName")) {
         *       // ...
         *   }
         *   else {
         *       // ...
         *   }
         */
        private void HandleDeviceFilterConditional(
            ref PropertyEntry previous, PropertyEntry current,
            CodeStatementCollection topStmts,
            ref CodeStatementCollection currentStmts,
            out CodeStatementCollection nextStmts) {

            bool sameAsPrevious = (previous != null) && StringUtil.EqualsIgnoreCase(previous.Name, current.Name);

            if (current.Filter.Length != 0) {
                if (!sameAsPrevious) {
                    // If the current property entry is not the same as the previous entries,
                    // we need to start a new block of code
                    currentStmts = topStmts;
                    previous = null;
                }

                CodeConditionStatement ifStmt = new CodeConditionStatement();
                CodeMethodInvokeExpression methCallExpression = new CodeMethodInvokeExpression(
                    new CodeThisReferenceExpression(), "TestDeviceFilter");
                methCallExpression.Parameters.Add(new CodePrimitiveExpression(current.Filter));
                ifStmt.Condition = methCallExpression;
                currentStmts.Add(ifStmt);

                // The current entry needs to go in the 'if' clause
                currentStmts = ifStmt.TrueStatements;

                // The next entry will tentatively go in the 'else' clause, unless it is
                // for a different property (which we would catch next time around)
                nextStmts = ifStmt.FalseStatements;

                previous = current;
            }
            else {
                // If we're switching to a new property, we need to add to the top-level statements (not the false block of an if)
                if (!sameAsPrevious) {
                    currentStmts = topStmts;
                }
                nextStmts = topStmts;
                previous = null;
            }
        }

        protected virtual bool UseResourceLiteralString(string s) {

            // If the string is long enough, and the compiler supports it, use a UTF8 resource
            // string for performance
            return PageParser.EnableLongStringsAsResources &&
                s.Length >= minLongLiteralStringLength &&
                _codeDomProvider.Supports(GeneratorSupport.Win32Resources);
        }
    }
}
