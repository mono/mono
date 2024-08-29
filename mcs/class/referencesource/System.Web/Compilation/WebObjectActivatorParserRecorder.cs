//------------------------------------------------------------------------------
// <copyright file="ControlCreateParseRecorder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

 namespace System.Web.Compilation {
    using System;
    using System.CodeDom;
    using System.Web.UI;
    using System.Reflection;
    using System.Runtime.InteropServices;

     class WebObjectActivatorParseRecorder : ParseRecorder {
        public override void ProcessGeneratedCode(ControlBuilder builder, CodeCompileUnit codeCompileUnit, CodeTypeDeclaration baseType,
            CodeTypeDeclaration derivedType, CodeMemberMethod buildMethod, CodeMemberMethod dataBindingMethod) {
            // Use similar logic as PageInspector to locate the control create statement
            if (derivedType != null && typeof(Control).IsAssignableFrom(builder.ControlType) && buildMethod != null) {
                var codeAssignStatement = FindControlCreateStatement(builder.ControlType, buildMethod.Statements);
                if (codeAssignStatement != null) {
                    ReplaceControlCreateStatement(builder.ControlType, codeAssignStatement, buildMethod.Statements);
                }
            }

             base.ProcessGeneratedCode(builder, codeCompileUnit, baseType, derivedType, buildMethod, dataBindingMethod);
        }

         private static CodeAssignStatement FindControlCreateStatement(Type controlType, CodeStatementCollection statements) {
            foreach (var statement in statements) {
                var codeAssignStatement = statement as CodeAssignStatement;
                if (codeAssignStatement != null) {
                    var objCreateExpr = codeAssignStatement.Right as CodeObjectCreateExpression;

                     if (objCreateExpr != null && objCreateExpr.CreateType.BaseType == controlType.ToString() && objCreateExpr.Parameters.Count == 0
                        && codeAssignStatement.Left is CodeVariableReferenceExpression) {
                        return codeAssignStatement;
                    }
                }
            }
            return null;
        }

         private static void ReplaceControlCreateStatement(Type ctrlType, CodeAssignStatement objAssignStatement, CodeStatementCollection statements) {
            /* Generate code like below
             
            IServiceProvider __activator = HttpRuntime.WebObjectActivator;
             if (activator != null) {
                _ctrl = (ctrlType)activator.GetService(ctrlType);
            }
             // if default c-tor exists
            else {
                _ctrl = new ....
            }
            // if no default c-tor
            else {
                throw new InvalidOperationException(SR.GetString(SR.Could_not_create_type_instance, ctrlType))
            }
             */
            var webObjectActivatorExpr = new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("System.Web.HttpRuntime"), "WebObjectActivator");
            var activatorRefExpr = new CodeVariableReferenceExpression("__activator");            

             var getServiceExpr = new CodeMethodInvokeExpression(webObjectActivatorExpr, "GetService", new CodeTypeOfExpression(ctrlType));
            var castExpr = new CodeCastExpression(new CodeTypeReference(ctrlType), getServiceExpr);
            var createObjectStatement = new CodeConditionStatement() {
                Condition = new CodeBinaryOperatorExpression(activatorRefExpr,
                                                             CodeBinaryOperatorType.IdentityInequality,
                                                             new CodePrimitiveExpression(null))
            };
            createObjectStatement.TrueStatements.Add(new CodeAssignStatement(objAssignStatement.Left, castExpr));

             // If default c-tor exists
            if (DoesTypeHaveDefaultCtor(ctrlType)) {
                createObjectStatement.FalseStatements.Add(objAssignStatement);
            }
            else {
                var throwExceptionStatement = new CodeThrowExceptionStatement(new CodeObjectCreateExpression(
                    new CodeTypeReference(typeof(System.InvalidOperationException)), 
                    new CodeExpression[] {new CodePrimitiveExpression(SR.GetString(SR.Could_not_create_type_instance, ctrlType))}));
                createObjectStatement.FalseStatements.Add(throwExceptionStatement);
            }

             // replace the old assign statement
            var indexOfStatement = statements.IndexOf(objAssignStatement);
            statements.Insert(indexOfStatement, createObjectStatement);
            statements.Insert(indexOfStatement, new CodeAssignStatement(activatorRefExpr, webObjectActivatorExpr));
            statements.Insert(indexOfStatement, new CodeVariableDeclarationStatement(typeof(IServiceProvider), "__activator"));
            statements.Remove(objAssignStatement);
        }

         private static bool DoesTypeHaveDefaultCtor(Type type) {
            if (type.GetConstructor(Type.EmptyTypes) != null) {
                return true;
            }
            else {
                foreach(var ctor in type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)) {
                    if(DoesAllConstructorParametersHaveDefaultValue(ctor)) {
                        return true;
                    }
                }
                return false;
            }
        }

         private static bool DoesAllConstructorParametersHaveDefaultValue(ConstructorInfo ctor) {
            foreach (var parameter in ctor.GetParameters()) {
                var hasOptionalAttribute = false;
                foreach(var attr in parameter.CustomAttributes) {
                    if(attr.AttributeType == typeof(OptionalAttribute)) {
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
    }
}