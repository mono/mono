// ---------------------------------------------------------------------------
// Copyright (C) 2006 Microsoft Corporation All Rights Reserved
// ---------------------------------------------------------------------------

#define CODE_ANALYSIS
using System.CodeDom;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.Activities.Common;

namespace System.Workflow.Activities.Rules
{
    #region RuleExpressionWalker

    public static class RuleExpressionWalker
    {
        #region IRuleExpression wrapper factories for CodeDom

        class CustomExpressionWrapper : RuleExpressionInternal
        {
            private IRuleExpression ruleExpr;

            internal CustomExpressionWrapper(IRuleExpression ruleExpr)
            {
                this.ruleExpr = ruleExpr;
            }

            internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
            {
                ruleExpr.AnalyzeUsage(analysis, isRead, isWritten, qualifier);
            }

            internal override CodeExpression Clone(CodeExpression expression)
            {
                return ruleExpr.Clone();
            }

            internal override void Decompile(CodeExpression expression, StringBuilder decompilation, CodeExpression parentExpression)
            {
                ruleExpr.Decompile(decompilation, parentExpression);
            }

            internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
            {
                return ruleExpr.Evaluate(execution);
            }

            internal override bool Match(CodeExpression leftExpression, CodeExpression rightExpression)
            {
                return ruleExpr.Match(rightExpression);
            }

            internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
            {
                return ruleExpr.Validate(validation, isWritten);
            }
        }

        class TypeWrapperTuple
        {
            internal Type codeDomType;
            internal RuleExpressionInternal internalExpression;

            internal TypeWrapperTuple(Type type, RuleExpressionInternal internalExpression)
            {
                this.codeDomType = type;
                this.internalExpression = internalExpression;
            }
        }

        static TypeWrapperTuple[] typeWrappers = new TypeWrapperTuple[] {
            new TypeWrapperTuple(typeof(CodeThisReferenceExpression), new ThisExpression()),
            new TypeWrapperTuple(typeof(CodePrimitiveExpression), new PrimitiveExpression()),
            new TypeWrapperTuple(typeof(CodeFieldReferenceExpression), new FieldReferenceExpression()),
            new TypeWrapperTuple(typeof(CodePropertyReferenceExpression), new PropertyReferenceExpression()),
            new TypeWrapperTuple(typeof(CodeBinaryOperatorExpression), new BinaryExpression()),
            new TypeWrapperTuple(typeof(CodeMethodInvokeExpression), new MethodInvokeExpression()),
            new TypeWrapperTuple(typeof(CodeIndexerExpression), new IndexerPropertyExpression()),
            new TypeWrapperTuple(typeof(CodeArrayIndexerExpression), new ArrayIndexerExpression()),
            new TypeWrapperTuple(typeof(CodeDirectionExpression), new DirectionExpression()),
            new TypeWrapperTuple(typeof(CodeTypeReferenceExpression), new TypeReferenceExpression()),
            new TypeWrapperTuple(typeof(CodeCastExpression), new CastExpression()),
            new TypeWrapperTuple(typeof(CodeObjectCreateExpression), new ObjectCreateExpression()),
            new TypeWrapperTuple(typeof(CodeArrayCreateExpression), new ArrayCreateExpression())
        };

        private static RuleExpressionInternal GetExpression(CodeExpression expression)
        {
            Type exprType = expression.GetType();
            int numTypeWrappers = typeWrappers.Length;
            for (int i = 0; i < numTypeWrappers; ++i)
            {
                TypeWrapperTuple tuple = typeWrappers[i];
                if (exprType == tuple.codeDomType)
                    return tuple.internalExpression;
            }

            // It's not a builtin one... try a user extension expression.
            IRuleExpression ruleExpr = expression as IRuleExpression;
            if (ruleExpr != null)
                return new CustomExpressionWrapper(ruleExpr);

            return null;
        }

        #endregion

        public static RuleExpressionInfo Validate(RuleValidation validation, CodeExpression expression, bool isWritten)
        {
            if (validation == null)
                throw new ArgumentNullException("validation");

            // See if we've visited this node before.
            // Always check if written = true
            RuleExpressionInfo resultExprInfo = null;
            if (!isWritten)
                resultExprInfo = validation.ExpressionInfo(expression);
            if (resultExprInfo == null)
            {
                // First time we've seen this node.
                RuleExpressionInternal ruleExpr = GetExpression(expression);
                if (ruleExpr == null)
                {
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.CodeExpressionNotHandled, expression.GetType().FullName);
                    ValidationError error = new ValidationError(message, ErrorNumbers.Error_CodeExpressionNotHandled);
                    error.UserData[RuleUserDataKeys.ErrorObject] = expression;

                    if (validation.Errors == null)
                    {
                        string typeName = string.Empty;
                        if ((validation.ThisType != null) && (validation.ThisType.Name != null))
                        {
                            typeName = validation.ThisType.Name;
                        }

                        string exceptionMessage = string.Format(
                            CultureInfo.CurrentCulture, Messages.ErrorsCollectionMissing, typeName);

                        throw new InvalidOperationException(exceptionMessage);
                    }
                    else
                    {
                        validation.Errors.Add(error);
                    }

                    return null;
                }

                resultExprInfo = validation.ValidateSubexpression(expression, ruleExpr, isWritten);
            }

            return resultExprInfo;
        }

        public static void AnalyzeUsage(RuleAnalysis analysis, CodeExpression expression, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            if (analysis == null)
                throw new ArgumentNullException("analysis");

            RuleExpressionInternal ruleExpr = GetExpression(expression);
            ruleExpr.AnalyzeUsage(expression, analysis, isRead, isWritten, qualifier);
        }

        public static RuleExpressionResult Evaluate(RuleExecution execution, CodeExpression expression)
        {
            if (execution == null)
                throw new ArgumentNullException("execution");

            RuleExpressionInternal ruleExpr = GetExpression(expression);
            return ruleExpr.Evaluate(expression, execution);
        }

        [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters", MessageId = "0#")]
        public static void Decompile(StringBuilder stringBuilder, CodeExpression expression, CodeExpression parentExpression)
        {
            RuleExpressionInternal ruleExpr = GetExpression(expression);
            ruleExpr.Decompile(expression, stringBuilder, parentExpression);
        }

        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
        public static bool Match(CodeExpression firstExpression, CodeExpression secondExpression)
        {
            // If they're both null, they match.
            if (firstExpression == null && secondExpression == null)
                return true;

            // If only one of them is null, there's no match.
            if (firstExpression == null || secondExpression == null)
                return false;

            if (firstExpression.GetType() != secondExpression.GetType())
                return false;

            RuleExpressionInternal ruleExpr1 = GetExpression(firstExpression);
            return ruleExpr1.Match(firstExpression, secondExpression);
        }

        public static CodeExpression Clone(CodeExpression originalExpression)
        {
            if (originalExpression == null)
                return null;

            RuleExpressionInternal ruleExpr = GetExpression(originalExpression);
            CodeExpression newExpr = ruleExpr.Clone(originalExpression);

            ConditionHelper.CloneUserData(originalExpression, newExpr);

            return newExpr;
        }
    }

    #endregion

    #region CodeDomStatementWalker (internal)

    internal static class CodeDomStatementWalker
    {
        #region RuleCodeDomStatement wrapper factories for CodeDom

        private delegate RuleCodeDomStatement WrapperCreator(CodeStatement statement);

        private static RuleCodeDomStatement GetStatement(CodeStatement statement)
        {
            Type statementType = statement.GetType();

            RuleCodeDomStatement wrapper = null;
            if (statementType == typeof(CodeExpressionStatement))
            {
                wrapper = ExpressionStatement.Create(statement);
            }
            else if (statementType == typeof(CodeAssignStatement))
            {
                wrapper = AssignmentStatement.Create(statement);
            }
            else
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.CodeStatementNotHandled, statement.GetType().FullName);
                NotSupportedException exception = new NotSupportedException(message);
                exception.Data[RuleUserDataKeys.ErrorObject] = statement;
                throw exception;
            }

            return wrapper;
        }

        #endregion

        internal static bool Validate(RuleValidation validation, CodeStatement statement)
        {
            RuleCodeDomStatement ruleStmt = GetStatement(statement);
            return ruleStmt.Validate(validation);
        }

        internal static void Execute(RuleExecution execution, CodeStatement statement)
        {
            RuleCodeDomStatement ruleStmt = GetStatement(statement);
            ruleStmt.Execute(execution);
        }

        internal static void AnalyzeUsage(RuleAnalysis analysis, CodeStatement statement)
        {
            RuleCodeDomStatement ruleStmt = GetStatement(statement);
            ruleStmt.AnalyzeUsage(analysis);
        }

        internal static void Decompile(StringBuilder stringBuilder, CodeStatement statement)
        {
            RuleCodeDomStatement ruleStmt = GetStatement(statement);
            ruleStmt.Decompile(stringBuilder);
        }

        internal static bool Match(CodeStatement firstStatement, CodeStatement secondStatement)
        {
            // If they're both null, they match.
            if (firstStatement == null && secondStatement == null)
                return true;

            // If only one of them is null, there's no match.
            if (firstStatement == null || secondStatement == null)
                return false;

            if (firstStatement.GetType() != secondStatement.GetType())
                return false;

            RuleCodeDomStatement ruleStmt = GetStatement(firstStatement);
            return ruleStmt.Match(secondStatement);
        }

        internal static CodeStatement Clone(CodeStatement statement)
        {
            if (statement == null)
                return null;
            RuleCodeDomStatement ruleStmt = GetStatement(statement);
            return ruleStmt.Clone();
        }
    }

    #endregion
}
