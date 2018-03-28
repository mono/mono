using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using System.Reflection;
using System.Globalization;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.Activities.Common;

namespace System.Workflow.Activities.Rules
{
    internal abstract class RuleCodeDomStatement
    {
        internal abstract bool Validate(RuleValidation validation);
        internal abstract void Execute(RuleExecution execution);
        internal abstract void AnalyzeUsage(RuleAnalysis analysis);
        internal abstract void Decompile(StringBuilder decompilation);
        internal abstract bool Match(CodeStatement expression);
        internal abstract CodeStatement Clone();
    }

    internal class ExpressionStatement : RuleCodeDomStatement
    {
        private CodeExpressionStatement exprStatement;

        private ExpressionStatement(CodeExpressionStatement exprStatement)
        {
            this.exprStatement = exprStatement;
        }

        internal static RuleCodeDomStatement Create(CodeStatement statement)
        {
            return new ExpressionStatement((CodeExpressionStatement)statement);
        }

        internal override bool Validate(RuleValidation validation)
        {
            bool success = false;

            if (exprStatement.Expression == null)
            {
                ValidationError error = new ValidationError(Messages.NullInvokeStatementExpression, ErrorNumbers.Error_ParameterNotSet);
                error.UserData[RuleUserDataKeys.ErrorObject] = exprStatement;
                validation.Errors.Add(error);
            }
            else if (exprStatement.Expression is CodeMethodInvokeExpression)
            {
                RuleExpressionInfo exprInfo = RuleExpressionWalker.Validate(validation, exprStatement.Expression, false);
                success = (exprInfo != null);
            }
            else
            {
                ValidationError error = new ValidationError(Messages.InvokeNotHandled, ErrorNumbers.Error_CodeExpressionNotHandled);
                error.UserData[RuleUserDataKeys.ErrorObject] = exprStatement;
                validation.Errors.Add(error);
            }

            return success;
        }

        internal override void AnalyzeUsage(RuleAnalysis analysis)
        {
            RuleExpressionWalker.AnalyzeUsage(analysis, exprStatement.Expression, false, false, null);
        }

        internal override void Execute(RuleExecution execution)
        {
            RuleExpressionWalker.Evaluate(execution, exprStatement.Expression);
        }

        internal override void Decompile(StringBuilder decompilation)
        {
            if (exprStatement.Expression == null)
            {
                RuleEvaluationException exception = new RuleEvaluationException(Messages.InvokeStatementNull);
                exception.Data[RuleUserDataKeys.ErrorObject] = exprStatement;
                throw exception;
            }

            RuleExpressionWalker.Decompile(decompilation, exprStatement.Expression, null);
        }

        internal override bool Match(CodeStatement comperand)
        {
            CodeExpressionStatement comperandStatement = comperand as CodeExpressionStatement;
            return ((comperandStatement != null)
                && RuleExpressionWalker.Match(exprStatement.Expression, comperandStatement.Expression));
        }

        internal override CodeStatement Clone()
        {
            CodeExpressionStatement newStatement = new CodeExpressionStatement();
            newStatement.Expression = RuleExpressionWalker.Clone(exprStatement.Expression);
            return newStatement;
        }
    }

    internal class AssignmentStatement : RuleCodeDomStatement
    {
        private CodeAssignStatement assignStatement;

        private AssignmentStatement(CodeAssignStatement assignStatement)
        {
            this.assignStatement = assignStatement;
        }

        internal static RuleCodeDomStatement Create(CodeStatement statement)
        {
            return new AssignmentStatement((CodeAssignStatement)statement);
        }

        internal override bool Validate(RuleValidation validation)
        {
            bool success = false;
            string message;
            RuleExpressionInfo lhsExprInfo = null;

            if (assignStatement.Left == null)
            {
                ValidationError error = new ValidationError(Messages.NullAssignLeft, ErrorNumbers.Error_LeftOperandMissing);
                error.UserData[RuleUserDataKeys.ErrorObject] = assignStatement;
                validation.Errors.Add(error);
            }
            else
            {
                lhsExprInfo = validation.ExpressionInfo(assignStatement.Left);
                if (lhsExprInfo == null)
                    lhsExprInfo = RuleExpressionWalker.Validate(validation, assignStatement.Left, true);
            }

            RuleExpressionInfo rhsExprInfo = null;
            if (assignStatement.Right == null)
            {
                ValidationError error = new ValidationError(Messages.NullAssignRight, ErrorNumbers.Error_RightOperandMissing);
                error.UserData[RuleUserDataKeys.ErrorObject] = assignStatement;
                validation.Errors.Add(error);
            }
            else
            {
                rhsExprInfo = RuleExpressionWalker.Validate(validation, assignStatement.Right, false);
            }

            if (lhsExprInfo != null && rhsExprInfo != null)
            {
                Type expressionType = rhsExprInfo.ExpressionType;
                Type assignmentType = lhsExprInfo.ExpressionType;

                if (assignmentType == typeof(NullLiteral))
                {
                    // Can't assign to a null literal.
                    ValidationError error = new ValidationError(Messages.NullAssignLeft, ErrorNumbers.Error_LeftOperandInvalidType);
                    error.UserData[RuleUserDataKeys.ErrorObject] = assignStatement;
                    validation.Errors.Add(error);
                    success = false;
                }
                else if (assignmentType == expressionType)
                {
                    // Easy case, they're both the same type.
                    success = true;
                }
                else
                {
                    // The types aren't the same, but it still might be a legal assignment.
                    ValidationError error = null;
                    if (!RuleValidation.TypesAreAssignable(expressionType, assignmentType, assignStatement.Right, out error))
                    {
                        if (error == null)
                        {
                            message = string.Format(CultureInfo.CurrentCulture, Messages.AssignNotAllowed, RuleDecompiler.DecompileType(expressionType), RuleDecompiler.DecompileType(assignmentType));
                            error = new ValidationError(message, ErrorNumbers.Error_OperandTypesIncompatible);
                        }
                        error.UserData[RuleUserDataKeys.ErrorObject] = assignStatement;
                        validation.Errors.Add(error);
                    }
                    else
                    {
                        success = true;
                    }
                }
            }

            return success;
        }

        internal override void AnalyzeUsage(RuleAnalysis analysis)
        {
            // The left side of the assignment is modified.
            RuleExpressionWalker.AnalyzeUsage(analysis, assignStatement.Left, false, true, null);
            // The right side of the assignment is read.
            RuleExpressionWalker.AnalyzeUsage(analysis, assignStatement.Right, true, false, null);
        }

        internal override void Execute(RuleExecution execution)
        {
            Type leftType = execution.Validation.ExpressionInfo(assignStatement.Left).ExpressionType;
            Type rightType = execution.Validation.ExpressionInfo(assignStatement.Right).ExpressionType;

            RuleExpressionResult leftResult = RuleExpressionWalker.Evaluate(execution, assignStatement.Left);
            RuleExpressionResult rightResult = RuleExpressionWalker.Evaluate(execution, assignStatement.Right);
            leftResult.Value = Executor.AdjustType(rightType, rightResult.Value, leftType);
        }

        internal override void Decompile(StringBuilder decompilation)
        {
            if (assignStatement.Right == null)
            {
                RuleEvaluationException exception = new RuleEvaluationException(Messages.AssignRightNull);
                exception.Data[RuleUserDataKeys.ErrorObject] = assignStatement;
                throw exception;
            }
            if (assignStatement.Left == null)
            {
                RuleEvaluationException exception = new RuleEvaluationException(Messages.AssignLeftNull);
                exception.Data[RuleUserDataKeys.ErrorObject] = assignStatement;
                throw exception;
            }

            RuleExpressionWalker.Decompile(decompilation, assignStatement.Left, null);
            decompilation.Append(" = ");
            RuleExpressionWalker.Decompile(decompilation, assignStatement.Right, null);
        }

        internal override bool Match(CodeStatement comperand)
        {
            CodeAssignStatement comperandStatement = comperand as CodeAssignStatement;
            return ((comperandStatement != null)
                && RuleExpressionWalker.Match(assignStatement.Left, comperandStatement.Left)
                && RuleExpressionWalker.Match(assignStatement.Right, comperandStatement.Right));
        }

        internal override CodeStatement Clone()
        {
            CodeAssignStatement newStatement = new CodeAssignStatement();
            newStatement.Left = RuleExpressionWalker.Clone(assignStatement.Left);
            newStatement.Right = RuleExpressionWalker.Clone(assignStatement.Right);
            return newStatement;
        }
    }
}
