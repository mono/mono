//------------------------------------------------------------------------------
// <copyright file="DataBindingExpressionBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Compilation {
    using System;
    using System.Security.Permissions;
    using System.CodeDom;
    using System.Diagnostics;
    using System.Reflection;
    using System.Web.UI;


    internal class DataBindingExpressionBuilder : ExpressionBuilder {
        private static EventInfo eventInfo;
        private const string EvalMethodName = "Eval";
        private const string GetDataItemMethodName = "GetDataItem";

        internal static EventInfo Event {
            get {
                if (eventInfo == null) {
                    eventInfo = typeof(Control).GetEvent("DataBinding");
                }

                return eventInfo;
            }
        }

        internal static void BuildEvalExpression(string field, string formatString, string propertyName,
            Type propertyType, ControlBuilder controlBuilder, CodeStatementCollection methodStatements, CodeStatementCollection statements, CodeLinePragma linePragma, bool isEncoded, ref bool hasTempObject) {

            // Altogether, this function will create a statement that looks like this:
            // if (this.Page.GetDataItem() != null) {
            //     target.{{propName}} = ({{propType}}) this.Eval(fieldName, formatString);
            // }

            //     this.Eval(fieldName, formatString)
            CodeMethodInvokeExpression evalExpr = new CodeMethodInvokeExpression();
            evalExpr.Method.TargetObject = new CodeThisReferenceExpression();
            evalExpr.Method.MethodName = EvalMethodName;
            evalExpr.Parameters.Add(new CodePrimitiveExpression(field));
            if (!String.IsNullOrEmpty(formatString)) {
                evalExpr.Parameters.Add(new CodePrimitiveExpression(formatString));
            }

            CodeStatementCollection evalStatements = new CodeStatementCollection();
            BuildPropertySetExpression(evalExpr, propertyName, propertyType, controlBuilder, methodStatements, evalStatements, linePragma, isEncoded, ref hasTempObject);

            // if (this.Page.GetDataItem() != null)
            CodeMethodInvokeExpression getDataItemExpr = new CodeMethodInvokeExpression();
            getDataItemExpr.Method.TargetObject = new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Page");
            getDataItemExpr.Method.MethodName = GetDataItemMethodName;

            CodeConditionStatement ifStmt = new CodeConditionStatement();
            ifStmt.Condition = new CodeBinaryOperatorExpression(getDataItemExpr, 
                                                                CodeBinaryOperatorType.IdentityInequality, 
                                                                new CodePrimitiveExpression(null));
            ifStmt.TrueStatements.AddRange(evalStatements);
            statements.Add(ifStmt);
        }

        private static void BuildPropertySetExpression(CodeExpression expression, string propertyName,
            Type propertyType, ControlBuilder controlBuilder, CodeStatementCollection methodStatements, CodeStatementCollection statements, CodeLinePragma linePragma, bool isEncoded, ref bool hasTempObject) {

            if (isEncoded) {
                expression = new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(
                        new CodeTypeReferenceExpression(typeof(HttpUtility)),
                       "HtmlEncode"),
                    expression);
            }

            CodeDomUtility.CreatePropertySetStatements(methodStatements, statements,
                new CodeVariableReferenceExpression("dataBindingExpressionBuilderTarget"), propertyName, propertyType,
                expression,
                linePragma);
        }

        internal static void BuildExpressionSetup(ControlBuilder controlBuilder, CodeStatementCollection methodStatements, CodeStatementCollection statements, CodeLinePragma linePragma, bool isTwoWayBound, bool designerMode) {
            // {{controlType}} target;
            CodeVariableDeclarationStatement targetDecl = new CodeVariableDeclarationStatement(controlBuilder.ControlType, "dataBindingExpressionBuilderTarget");
            methodStatements.Add(targetDecl);

            CodeVariableReferenceExpression targetExp = new CodeVariableReferenceExpression(targetDecl.Name);

            // target = ({{controlType}}) sender;
            CodeAssignStatement setTarget = new CodeAssignStatement(targetExp,
                                                                    new CodeCastExpression(controlBuilder.ControlType,
                                                                                           new CodeArgumentReferenceExpression("sender")));
            setTarget.LinePragma = linePragma;
            statements.Add(setTarget);

            Type bindingContainerType = controlBuilder.BindingContainerType;
            CodeVariableDeclarationStatement containerDecl = new CodeVariableDeclarationStatement(bindingContainerType, "Container");
            methodStatements.Add(containerDecl);

            // {{containerType}} Container = ({{containerType}}) target.BindingContainer;
            CodeAssignStatement setContainer = new CodeAssignStatement(new CodeVariableReferenceExpression(containerDecl.Name),
                                                                       new CodeCastExpression(bindingContainerType,
                                                                                              new CodePropertyReferenceExpression(targetExp,
                                                                                                                                  "BindingContainer")));
            setContainer.LinePragma = linePragma;
            statements.Add(setContainer);
            string variableName = isTwoWayBound ? "BindItem" : "Item";
            GenerateItemTypeExpressions(controlBuilder, methodStatements, statements, linePragma, variableName);
            //Generate code for other variable as well at design time in addition to runtime variable for intellisense to work.
            if (designerMode) {
                GenerateItemTypeExpressions(controlBuilder, methodStatements, statements, linePragma, isTwoWayBound ?  "Item" : "BindItem");
            }
        }

        internal static void GenerateItemTypeExpressions(ControlBuilder controlBuilder, CodeStatementCollection declarationStatements, CodeStatementCollection codeStatements, CodeLinePragma linePragma, string variableName) {
            //Strongly Typed DataControls
            string itemType = controlBuilder.ItemType;
            CodeVariableDeclarationStatement itemTypeDecl = null;

            if (!String.IsNullOrEmpty(itemType)) {
                itemTypeDecl = new CodeVariableDeclarationStatement(itemType, variableName);
                declarationStatements.Add(itemTypeDecl);

                //Model Item = (Model)Container.DataItem; - The actual type should come from the control
                CodeAssignStatement setDataItem = new CodeAssignStatement(new CodeVariableReferenceExpression(itemTypeDecl.Name),
                                                                           new CodeCastExpression(itemType,
                                                                                                  new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("Container"),
                                                                                                                                      "DataItem")));
                setDataItem.LinePragma = linePragma;
                codeStatements.Add(setDataItem);
            }
        }

        internal override void BuildExpression(BoundPropertyEntry bpe, ControlBuilder controlBuilder,
            CodeExpression controlReference, CodeStatementCollection methodStatements, CodeStatementCollection statements, CodeLinePragma linePragma, ref bool hasTempObject) {

            BuildExpressionStatic(bpe, controlBuilder, controlReference, methodStatements, statements, linePragma, bpe.IsEncoded, ref hasTempObject);
        }

        internal static void BuildExpressionStatic(BoundPropertyEntry bpe, ControlBuilder controlBuilder,
            CodeExpression controlReference, CodeStatementCollection methodStatements, CodeStatementCollection statements, CodeLinePragma linePragma, bool isEncoded, ref bool hasTempObject) {

            CodeExpression expr = new CodeSnippetExpression(bpe.Expression);
            BuildPropertySetExpression(expr, bpe.Name, bpe.Type, controlBuilder, methodStatements, statements, linePragma, isEncoded, ref hasTempObject);
        }


        public override CodeExpression GetCodeExpression(BoundPropertyEntry entry,
            object parsedData, ExpressionBuilderContext context) {
            Debug.Fail("This should never be called");
            return null;
        }
    }
}
