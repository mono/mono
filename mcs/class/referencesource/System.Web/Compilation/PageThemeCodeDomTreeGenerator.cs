//------------------------------------------------------------------------------
// <copyright file="PageThemeCodeDomTreeGenerator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.ComponentModel;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Globalization;
using System.IO;
using System.Web.UI;
using System.Web.Util;
using Debug=System.Web.Util.Debug;

namespace System.Web.Compilation {

    internal class PageThemeCodeDomTreeGenerator : BaseTemplateCodeDomTreeGenerator {

        private Hashtable _controlSkinTypeNameCollection = new Hashtable();
        private ArrayList _controlSkinBuilderEntryList = new ArrayList();

        private int _controlCount = 0;
        private CodeTypeReference _controlSkinDelegateType = new CodeTypeReference(typeof(ControlSkinDelegate));
        private CodeTypeReference _controlSkinType = new CodeTypeReference(typeof(ControlSkin));

        private PageThemeParser _themeParser;
        private const string _controlSkinsVarName = "__controlSkins";
        private const string _controlSkinsPropertyName = "ControlSkins";
        private const string _linkedStyleSheetsVarName = "__linkedStyleSheets";
        private const string _linkedStyleSheetsPropertyName = "LinkedStyleSheets";

        internal PageThemeCodeDomTreeGenerator(PageThemeParser parser) : base(parser) {
            _themeParser = parser;
        }

        private void AddMemberOverride(string name, Type type, CodeExpression expr) {
            CodeMemberProperty member = new CodeMemberProperty();
            member.Name = name;
            member.Attributes = MemberAttributes.Override | MemberAttributes.Family;
            member.Type = new CodeTypeReference(type.FullName);
            CodeMethodReturnStatement returnStmt = new CodeMethodReturnStatement(expr);
            member.GetStatements.Add(returnStmt);
            _sourceDataClass.Members.Add(member);
        }

        private void BuildControlSkins(CodeStatementCollection statements) {
            foreach (ControlSkinBuilderEntry entry in _controlSkinBuilderEntryList) {
                string skinID = entry.SkinID;
                ControlBuilder builder = entry.Builder;
                statements.Add(BuildControlSkinAssignmentStatement(builder, skinID));
            }
        }

        private CodeStatement BuildControlSkinAssignmentStatement(
            ControlBuilder builder, string skinID) {

            Type controlType = builder.ControlType;

            string keyVarName = GetMethodNameForBuilder(buildMethodPrefix, builder) + "_skinKey";

            // e.g.
            // private static object __BuildControl__control3_skinKey = PageTheme.CreateSkinKey(typeof({controlType}), {skinID});
            CodeMemberField field = new CodeMemberField(typeof(object), keyVarName);
            field.Attributes = MemberAttributes.Static | MemberAttributes.Private;
            CodeMethodInvokeExpression cmie = new CodeMethodInvokeExpression();
            cmie.Method = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(typeof(PageTheme)), "CreateSkinKey");
            cmie.Parameters.Add(new CodeTypeOfExpression(controlType));
            cmie.Parameters.Add(new CodePrimitiveExpression(skinID));
            field.InitExpression = cmie;
            _sourceDataClass.Members.Add(field);

            // e.g. this.__namedControlSkins[keyVarName] =
            //          new System.Web.UI.ControlSkin(typeof(System.Web.UI.WebControls.Label),
            //          new System.Web.UI.ControlSkinDelegate(this.__BuildControl__control3));
            CodeFieldReferenceExpression varExpr = new CodeFieldReferenceExpression(
                new CodeThisReferenceExpression(),
                _controlSkinsVarName);

            CodeIndexerExpression indexerExpr = new CodeIndexerExpression(
                varExpr,
                new CodeExpression[] {
                    new CodeVariableReferenceExpression(keyVarName)
                }
            );

            CodeDelegateCreateExpression del = new CodeDelegateCreateExpression(
                                                            _controlSkinDelegateType,
                                                            new CodeThisReferenceExpression(),
                                                            GetMethodNameForBuilder(buildMethodPrefix, builder));
            CodeObjectCreateExpression valueExpr = new CodeObjectCreateExpression(_controlSkinType);
            valueExpr.Parameters.Add(new CodeTypeOfExpression(controlType));
            valueExpr.Parameters.Add(del);

            return new CodeAssignStatement(indexerExpr, valueExpr);
        }

        private void BuildControlSkinMember() {
            // e.g.
            // private System.Collections.Specialized.HybridDictionary __cssFileList =
            //     new System.Collections.Specialized.HybridDictionary(2);
            int initialSize = _controlSkinBuilderEntryList.Count;
            CodeMemberField field = new CodeMemberField(typeof(HybridDictionary).FullName, _controlSkinsVarName);
            CodeObjectCreateExpression expr = new CodeObjectCreateExpression(typeof(HybridDictionary));
            expr.Parameters.Add(new CodePrimitiveExpression(initialSize));
            field.InitExpression = expr;
            _sourceDataClass.Members.Add(field);
        }

        private void BuildControlSkinProperty() {
            // e.g.
            //  protected override System.Collections.IDictionary ControlSkins {
            //       get { return this.__controlSkins; }
            //  }
            CodeFieldReferenceExpression accessExpr = new CodeFieldReferenceExpression(
                                                            new CodeThisReferenceExpression(),
                                                            _controlSkinsVarName);

            AddMemberOverride(_controlSkinsPropertyName, typeof(IDictionary), accessExpr);
        }

        private void BuildLinkedStyleSheetMember() {

            // e.g.
            // private System.String[] __linkedStyleSheets = new String[] {
            //     "linkedStyleSheet1 vdirs",
            //     "linkedStyleSheet2 vdirs",
            // }
            CodeMemberField field = new CodeMemberField(typeof(String[]), _linkedStyleSheetsVarName);

            if (_themeParser.CssFileList != null && _themeParser.CssFileList.Count > 0) {
                CodeExpression[] cssFiles = new CodeExpression[_themeParser.CssFileList.Count];
                int i = 0;
                foreach(String cssFile in _themeParser.CssFileList) {
                    cssFiles[i++] = new CodePrimitiveExpression(cssFile);
                }
                
                CodeArrayCreateExpression initExpr = new CodeArrayCreateExpression(typeof(String), cssFiles);
                field.InitExpression = initExpr;
            }
            else {
                field.InitExpression = new CodePrimitiveExpression(null);
            }
            _sourceDataClass.Members.Add(field);
        }

        private void BuildLinkedStyleSheetProperty() {
            // e.g.
            //  protected override String[] LinkedStyleSheets {
            //       get { return this.__linkedStyleSheets; }
            //  }
            CodeFieldReferenceExpression accessExpr = new CodeFieldReferenceExpression(
                                                            new CodeThisReferenceExpression(),
                                                            _linkedStyleSheetsVarName);

            AddMemberOverride(_linkedStyleSheetsPropertyName, typeof(String[]), accessExpr);
        }

        protected override void BuildInitStatements(CodeStatementCollection trueStatements, CodeStatementCollection topLevelStatements) {
            base.BuildInitStatements(trueStatements, topLevelStatements);
            BuildControlSkins(topLevelStatements);
        }

        protected override void BuildMiscClassMembers() {
            base.BuildMiscClassMembers();

            AddMemberOverride(templateSourceDirectoryName, typeof(String),
                new CodePrimitiveExpression(_themeParser.VirtualDirPath.VirtualPathString));

            BuildSourceDataTreeFromBuilder(_themeParser.RootBuilder,
                false /*fInTemplate*/, false /*topLevelControlInTemplate*/, null /*pse*/);

            BuildControlSkinMember();
            BuildControlSkinProperty();
            BuildLinkedStyleSheetMember();
            BuildLinkedStyleSheetProperty();
        }

        protected override void BuildSourceDataTreeFromBuilder(ControlBuilder builder,
                                                bool fInTemplate, bool topLevelControlInTemplate,
                                                PropertyEntry pse) {

            // Don't do anything for code blocks
            if (builder is CodeBlockBuilder)
                return;

            // Is the current builder for a template?
            bool fTemplate = (builder is TemplateBuilder);

            // Is the current builder the root builder?
            bool fRootBuilder = (builder == _themeParser.RootBuilder);

            // Is this a control theme?
            bool fControlSkin = !fInTemplate && !fTemplate && topLevelControlInTemplate;

            // Ignore the ID attribute, always auto generate ID.
            _controlCount++;
            builder.ID = "__control" + _controlCount.ToString(NumberFormatInfo.InvariantInfo);
            builder.IsGeneratedID = true;

            // Check for the SkinID property.
            if (fControlSkin && !(builder is DataBoundLiteralControlBuilder)) {

                Type ctrlType = builder.ControlType;
                Debug.Assert(typeof(Control).IsAssignableFrom(ctrlType));
                Debug.Assert(ThemeableAttribute.IsTypeThemeable(ctrlType));

                string skinID = builder.SkinID;
                object skinKey = PageTheme.CreateSkinKey(builder.ControlType, skinID);

                if (_controlSkinTypeNameCollection.Contains(skinKey)) {
                    if (String.IsNullOrEmpty(skinID)) {
                        throw new HttpParseException(SR.GetString(SR.Page_theme_default_theme_already_defined,
                            builder.ControlType.FullName), null, builder.VirtualPath, null, builder.Line);
                    }
                    else {
                        throw new HttpParseException(SR.GetString(SR.Page_theme_skinID_already_defined, skinID),
                            null, builder.VirtualPath, null, builder.Line);
                    }
                }

                _controlSkinTypeNameCollection.Add(skinKey, true);
                _controlSkinBuilderEntryList.Add(new ControlSkinBuilderEntry(builder, skinID));
            }

            // Process the children
            // only root builders and template builders are processed.
            if (builder.SubBuilders != null) {
                foreach (object child in builder.SubBuilders) {
                    if (child is ControlBuilder) {
                        bool isTopLevelCtrlInTemplate = fTemplate && typeof(Control).IsAssignableFrom(((ControlBuilder)child).ControlType);
                        BuildSourceDataTreeFromBuilder((ControlBuilder)child, fInTemplate, isTopLevelCtrlInTemplate, null);
                    }
                }
            }

            foreach (TemplatePropertyEntry entry in builder.TemplatePropertyEntries) {
                BuildSourceDataTreeFromBuilder(((TemplatePropertyEntry)entry).Builder, true, false /*topLevelControlInTemplate*/, entry);
            }

            foreach (ComplexPropertyEntry entry in builder.ComplexPropertyEntries) {
                if (!(entry.Builder is StringPropertyBuilder)) {
                    BuildSourceDataTreeFromBuilder(((ComplexPropertyEntry)entry).Builder, fInTemplate, false /*topLevelControlInTemplate*/, entry);
                }
            }

            // Build a Build method for the control
            // fControlSkin indicates whether the method is a theme build method.
            if (!fRootBuilder) {
                BuildBuildMethod(builder, fTemplate, fInTemplate, topLevelControlInTemplate, pse,
                    fControlSkin);
            }

            // Build a Render method for the control, unless it has no code
            if (!fControlSkin && builder.HasAspCode) {
                BuildRenderMethod(builder, fTemplate);
            }

            // Build a method to extract values from the template
            BuildExtractMethod(builder);

            // Build a property binding method for the control
            BuildPropertyBindingMethod(builder, fControlSkin);
        }

        internal override CodeExpression BuildStringPropertyExpression(PropertyEntry pse) {
            // Make the UrlProperty based on virtualDirPath for control themes.
            if (pse.PropertyInfo != null) {
                UrlPropertyAttribute urlAttrib = Attribute.GetCustomAttribute(pse.PropertyInfo, typeof(UrlPropertyAttribute)) as UrlPropertyAttribute;

                if (urlAttrib != null) {
                    if (pse is SimplePropertyEntry) {
                        SimplePropertyEntry spse = (SimplePropertyEntry)pse;

                        string strValue = (string)spse.Value;
                        if (UrlPath.IsRelativeUrl(strValue) && !UrlPath.IsAppRelativePath(strValue)) {
                            spse.Value = UrlPath.MakeVirtualPathAppRelative(UrlPath.Combine(_themeParser.VirtualDirPath.VirtualPathString, strValue));
                        }
                    }
                    else {
                        Debug.Assert(pse is ComplexPropertyEntry);
                        ComplexPropertyEntry cpe = (ComplexPropertyEntry)pse;
                        StringPropertyBuilder builder = (StringPropertyBuilder)cpe.Builder;

                        string strValue = (string)builder.BuildObject();
                        if (UrlPath.IsRelativeUrl(strValue) && !UrlPath.IsAppRelativePath(strValue)) {
                            cpe.Builder = new StringPropertyBuilder(UrlPath.MakeVirtualPathAppRelative(UrlPath.Combine(_themeParser.VirtualDirPath.VirtualPathString, strValue)));
                        }
                    }
                }
            }

            return base.BuildStringPropertyExpression(pse);
        }

        protected override CodeAssignStatement BuildTemplatePropertyStatement(CodeExpression ctrlRefExpr) {

            // e.g. __ctrl.AppRelativeTemplateSourceDirectory = this.AppRelativeTemplateSourceDirectory;
            CodeAssignStatement assign = new CodeAssignStatement();
            assign.Left = new CodePropertyReferenceExpression(ctrlRefExpr, templateSourceDirectoryName);
            assign.Right = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), templateSourceDirectoryName);
            return assign;
        }

        protected override string GetGeneratedClassName() {
            string className = _themeParser.VirtualDirPath.FileName;
            className = System.Web.UI.Util.MakeValidTypeNameFromString(className);

            return className;
        }

        protected override bool UseResourceLiteralString(string s) {
            // never use resource literal string, page theme does not support the required methods.
            return false;
        }

        // Don't build the Profile property in Theme classes
        protected override bool NeedProfileProperty { get { return false; } }

        private class ControlSkinBuilderEntry {
            private ControlBuilder _builder;
            private string _id;

            public ControlSkinBuilderEntry (ControlBuilder builder, string skinID) {
                _builder = builder;
                _id = skinID;
            }

            public ControlBuilder Builder {
                get { return _builder; }
            }

            public String SkinID {
                get { return _id == null? String.Empty : _id; }
            }
        }
    }
}
