// ---------------------------------------------------------------------------
// Copyright (C) 2006 Microsoft Corporation All Rights Reserved
// ---------------------------------------------------------------------------

#define CODE_ANALYSIS
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.Activities.Common;

namespace System.Workflow.Activities.Rules
{
    public interface IRuleExpression
    {
        RuleExpressionInfo Validate(RuleValidation validation, bool isWritten);
        RuleExpressionResult Evaluate(RuleExecution execution);
        void AnalyzeUsage(RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier);
        [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters", MessageId = "0#")]
        void Decompile(StringBuilder stringBuilder, CodeExpression parentExpression);
        bool Match(CodeExpression expression);
        CodeExpression Clone();
    }

    internal abstract class RuleExpressionInternal
    {
        internal abstract RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten);
        internal abstract RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution);
        internal abstract void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier);
        internal abstract void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression);
        internal abstract bool Match(CodeExpression leftExpression, CodeExpression rightExpression);
        internal abstract CodeExpression Clone(CodeExpression expression);
    }


    #region "this" expression

    // CodeThisReferenceExpression
    internal class ThisExpression : RuleExpressionInternal
    {
        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            if (isWritten)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.CannotWriteToExpression, typeof(CodeThisReferenceExpression).ToString());
                ValidationError error = new ValidationError(message, ErrorNumbers.Error_InvalidAssignTarget);
                error.UserData[RuleUserDataKeys.ErrorObject] = expression;
                validation.Errors.Add(error);
                return null;
            }

            return new RuleExpressionInfo(validation.ThisType);
        }

        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            if (analysis.ForWrites && !isWritten)            // If we're tracking writes, then ignore things that aren't written.
                return;
            else if (!analysis.ForWrites && !isRead)   // ... and vice-versa
                return;

            StringBuilder sb = new StringBuilder("this/");
            for (RulePathQualifier q = qualifier; q != null; q = q.Next)
            {
                sb.Append(q.Name);
                if (q.Name == "*")
                {
                    if (q.Next != null)
                        throw new NotSupportedException(Messages.InvalidWildCardInPathQualifier);
                }
                else
                {
                    sb.Append("/");
                }
            }

            // Add the symbol to our set.
            analysis.AddSymbol(sb.ToString());
        }

        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            return execution.ThisLiteralResult;
        }

        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            stringBuilder.Append("this");
        }

        internal override CodeExpression Clone(CodeExpression expression)
        {
            return new CodeThisReferenceExpression();
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            // We already verified their types match.
            return true;
        }
    }

    #endregion

    #region Primitive expression

    // CodePrimitiveExpression
    internal class PrimitiveExpression : RuleExpressionInternal
    {
        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            if (isWritten)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.CannotWriteToExpression, typeof(CodePrimitiveExpression).ToString());
                ValidationError error = new ValidationError(message, ErrorNumbers.Error_InvalidAssignTarget);
                error.UserData[RuleUserDataKeys.ErrorObject] = expression;
                validation.Errors.Add(error);
                return null;
            }

            CodePrimitiveExpression primitiveExpr = (CodePrimitiveExpression)expression;
            Type resultType = (primitiveExpr.Value != null) ? primitiveExpr.Value.GetType() : typeof(NullLiteral);
            return new RuleExpressionInfo(resultType);
        }

        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            // Literal values have no interesting dependencies or side-effects.
        }

        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            CodePrimitiveExpression primitiveExpr = (CodePrimitiveExpression)expression;
            return new RuleLiteralResult(primitiveExpr.Value);
        }

        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            CodePrimitiveExpression primitiveExpr = (CodePrimitiveExpression)expression;
            RuleDecompiler.DecompileObjectLiteral(stringBuilder, primitiveExpr.Value);
        }

        internal override CodeExpression Clone(CodeExpression expression)
        {
            CodePrimitiveExpression primitiveExpr = (CodePrimitiveExpression)expression;
            object clonedValue = ConditionHelper.CloneObject(primitiveExpr.Value);
            return new CodePrimitiveExpression(clonedValue);
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            CodePrimitiveExpression primitiveExpr = (CodePrimitiveExpression)expression;
            CodePrimitiveExpression comperandPrimitive = (CodePrimitiveExpression)comperand;

            if (primitiveExpr.Value == comperandPrimitive.Value)
                return true;

            if (primitiveExpr.Value == null || comperandPrimitive.Value == null)
                return false;

            return primitiveExpr.Value.Equals(comperandPrimitive.Value);
        }
    }

    #endregion

    #region Binary expression

    // CodeBinaryOperatorExpression
    internal class BinaryExpression : RuleExpressionInternal
    {
        #region Validate

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            string message;
            ValidationError error;

            CodeBinaryOperatorExpression binaryExpr = (CodeBinaryOperatorExpression)expression;

            // Early exit from this if a cycle is detected.
            if (!validation.PushParentExpression(binaryExpr))
                return null;

            if (isWritten)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.CannotWriteToExpression, typeof(CodeBinaryOperatorExpression).ToString());
                error = new ValidationError(message, ErrorNumbers.Error_InvalidAssignTarget);
                error.UserData[RuleUserDataKeys.ErrorObject] = binaryExpr;
                validation.Errors.Add(error);
            }

            RuleExpressionInfo lhsExprInfo = null;
            RuleExpressionInfo rhsExprInfo = null;

            if (binaryExpr.Left == null)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.NullBinaryOpLHS, binaryExpr.Operator.ToString());
                error = new ValidationError(message, ErrorNumbers.Error_LeftOperandMissing);
                error.UserData[RuleUserDataKeys.ErrorObject] = binaryExpr;
                validation.Errors.Add(error);
            }
            else
            {
                if (binaryExpr.Left is CodeTypeReferenceExpression)
                {
                    message = string.Format(CultureInfo.CurrentCulture, Messages.CodeExpressionNotHandled, binaryExpr.Left.GetType().FullName);
                    error = new ValidationError(message, ErrorNumbers.Error_CodeExpressionNotHandled);
                    error.UserData[RuleUserDataKeys.ErrorObject] = binaryExpr.Left;
                    validation.AddError(error);
                    return null;
                }

                lhsExprInfo = RuleExpressionWalker.Validate(validation, binaryExpr.Left, false);
            }

            if (binaryExpr.Right == null)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.NullBinaryOpRHS, binaryExpr.Operator.ToString());
                error = new ValidationError(message, ErrorNumbers.Error_RightOperandMissing);
                error.UserData[RuleUserDataKeys.ErrorObject] = binaryExpr;
                validation.Errors.Add(error);
            }
            else
            {
                if (binaryExpr.Right is CodeTypeReferenceExpression)
                {
                    message = string.Format(CultureInfo.CurrentCulture, Messages.CodeExpressionNotHandled, binaryExpr.Right.GetType().FullName);
                    error = new ValidationError(message, ErrorNumbers.Error_CodeExpressionNotHandled);
                    error.UserData[RuleUserDataKeys.ErrorObject] = binaryExpr.Right;
                    validation.AddError(error);
                    return null;
                }

                rhsExprInfo = RuleExpressionWalker.Validate(validation, binaryExpr.Right, false);
            }

            validation.PopParentExpression();

            RuleBinaryExpressionInfo resultExprInfo = null;

            if (lhsExprInfo != null && rhsExprInfo != null)
            {
                Type lhsType = lhsExprInfo.ExpressionType;
                Type rhsType = rhsExprInfo.ExpressionType;

                switch (binaryExpr.Operator)
                {
                    case CodeBinaryOperatorType.Add:
                    case CodeBinaryOperatorType.Subtract:
                    case CodeBinaryOperatorType.Multiply:
                    case CodeBinaryOperatorType.Divide:
                    case CodeBinaryOperatorType.Modulus:
                    case CodeBinaryOperatorType.BitwiseAnd:
                    case CodeBinaryOperatorType.BitwiseOr:
                        resultExprInfo = ArithmeticLiteral.ResultType(binaryExpr.Operator, lhsType, binaryExpr.Left, rhsType, binaryExpr.Right, validation, out error);
                        if (resultExprInfo == null)
                        {
                            // check if constants are used with ulongs, as we should do some extra "conversions"
                            if (((lhsType == typeof(ulong)) && (PromotionPossible(rhsType, binaryExpr.Right)))
                                || ((rhsType == typeof(ulong)) && (PromotionPossible(lhsType, binaryExpr.Left))))
                            {
                                resultExprInfo = new RuleBinaryExpressionInfo(lhsType, rhsType, typeof(ulong));
                            }
                            else
                            {
                                error.UserData[RuleUserDataKeys.ErrorObject] = binaryExpr;
                                validation.Errors.Add(error);
                            }
                        }
                        break;

                    case CodeBinaryOperatorType.IdentityEquality:
                    case CodeBinaryOperatorType.IdentityInequality:
                        resultExprInfo = new RuleBinaryExpressionInfo(lhsType, rhsType, typeof(bool));
                        break;

                    case CodeBinaryOperatorType.ValueEquality:
                        resultExprInfo = Literal.AllowedComparison(lhsType, binaryExpr.Left, rhsType, binaryExpr.Right, binaryExpr.Operator, validation, out error);
                        if (resultExprInfo == null)
                        {
                            // check if constants are used with ulongs, as we should do some extra "conversions"
                            if (((lhsType == typeof(ulong)) && (PromotionPossible(rhsType, binaryExpr.Right)))
                                || ((rhsType == typeof(ulong)) && (PromotionPossible(lhsType, binaryExpr.Left))))
                            {
                                resultExprInfo = new RuleBinaryExpressionInfo(lhsType, rhsType, typeof(bool));
                            }
                            else
                            {
                                error.UserData[RuleUserDataKeys.ErrorObject] = binaryExpr;
                                validation.Errors.Add(error);
                            }
                        }
                        break;

                    case CodeBinaryOperatorType.LessThan:
                    case CodeBinaryOperatorType.LessThanOrEqual:
                    case CodeBinaryOperatorType.GreaterThan:
                    case CodeBinaryOperatorType.GreaterThanOrEqual:
                        resultExprInfo = Literal.AllowedComparison(lhsType, binaryExpr.Left, rhsType, binaryExpr.Right, binaryExpr.Operator, validation, out error);
                        if (resultExprInfo == null)
                        {
                            // check if constants are used with ulongs, as we should do some extra "conversions"
                            if (((lhsType == typeof(ulong)) && (PromotionPossible(rhsType, binaryExpr.Right)))
                                || ((rhsType == typeof(ulong)) && (PromotionPossible(lhsType, binaryExpr.Left))))
                            {
                                resultExprInfo = new RuleBinaryExpressionInfo(lhsType, rhsType, typeof(bool));
                            }
                            else
                            {
                                error.UserData[RuleUserDataKeys.ErrorObject] = binaryExpr;
                                validation.Errors.Add(error);
                            }
                        }
                        break;

                    case CodeBinaryOperatorType.BooleanAnd:
                    case CodeBinaryOperatorType.BooleanOr:
                        resultExprInfo = new RuleBinaryExpressionInfo(lhsType, rhsType, typeof(bool));
                        if (lhsType != typeof(bool))
                        {
                            message = string.Format(CultureInfo.CurrentCulture, Messages.LogicalOpBadTypeLHS, binaryExpr.Operator.ToString(),
                                (lhsType == typeof(NullLiteral)) ? Messages.NullValue : RuleDecompiler.DecompileType(lhsType));
                            error = new ValidationError(message, ErrorNumbers.Error_LeftOperandInvalidType);
                            error.UserData[RuleUserDataKeys.ErrorObject] = binaryExpr;
                            validation.Errors.Add(error);
                            resultExprInfo = null;
                        }
                        if (rhsType != typeof(bool))
                        {
                            message = string.Format(CultureInfo.CurrentCulture, Messages.LogicalOpBadTypeRHS, binaryExpr.Operator.ToString(),
                                (rhsType == typeof(NullLiteral)) ? Messages.NullValue : RuleDecompiler.DecompileType(rhsType));
                            error = new ValidationError(message, ErrorNumbers.Error_RightOperandInvalidType);
                            error.UserData[RuleUserDataKeys.ErrorObject] = binaryExpr;
                            validation.Errors.Add(error);
                            resultExprInfo = null;
                        }
                        break;

                    default:
                        {
                            message = string.Format(CultureInfo.CurrentCulture, Messages.BinaryOpNotSupported, binaryExpr.Operator.ToString());
                            error = new ValidationError(message, ErrorNumbers.Error_CodeExpressionNotHandled);
                            error.UserData[RuleUserDataKeys.ErrorObject] = binaryExpr;
                            validation.Errors.Add(error);
                        }
                        break;
                }
            }

            // Validate any RuleAttributes, if present.
            if (resultExprInfo != null)
            {
                MethodInfo method = resultExprInfo.MethodInfo;
                if (method != null)
                {
                    object[] attrs = method.GetCustomAttributes(typeof(RuleAttribute), true);
                    if (attrs != null && attrs.Length > 0)
                    {
                        Stack<MemberInfo> methodStack = new Stack<MemberInfo>();
                        methodStack.Push(method);

                        bool allAttributesValid = true;
                        foreach (RuleAttribute ruleAttr in attrs)
                        {
                            if (!ruleAttr.Validate(validation, method, method.DeclaringType, method.GetParameters()))
                                allAttributesValid = false;
                        }

                        methodStack.Pop();

                        if (!allAttributesValid)
                            return null;
                    }
                }
            }

            return resultExprInfo;
        }

        /// <summary>
        /// Check that the expression is a constant, and is promotable to type ULONG.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static bool PromotionPossible(Type type, CodeExpression expression)
        {
            // C# 2.0, section 6.1.6, int/long constants can be promoted to ulong as long as in range
            if (type == typeof(int))
            {
                CodePrimitiveExpression primitive = expression as CodePrimitiveExpression;
                if (primitive != null)
                {
                    int i = (int)primitive.Value;
                    return (i >= 0);
                }
            }
            else if (type == typeof(long))
            {
                CodePrimitiveExpression primitive = expression as CodePrimitiveExpression;
                if (primitive != null)
                {
                    long l = (long)primitive.Value;
                    return (l >= 0);
                }
            }
            return false;
        }
        #endregion

        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            CodeBinaryOperatorExpression binaryExpr = (CodeBinaryOperatorExpression)expression;

            // Get the method info from the validation so we can look for [RuleRead] and [RuleWrite] attributes.
            RuleBinaryExpressionInfo expressionInfo = analysis.Validation.ExpressionInfo(binaryExpr) as RuleBinaryExpressionInfo;
            if (expressionInfo != null)
            {
                // we may be calling a method, not a default operator
                MethodInfo method = expressionInfo.MethodInfo;
                if (method != null)
                {
                    List<CodeExpression> attributedExprs = new List<CodeExpression>();
                    CodeExpressionCollection arguments = new CodeExpressionCollection();
                    arguments.Add(binaryExpr.Left);
                    arguments.Add(binaryExpr.Right);
                    CodeExpression targetObject = new CodeTypeReferenceExpression(method.DeclaringType);
                    analysis.AnalyzeRuleAttributes(method, targetObject, qualifier, arguments, method.GetParameters(), attributedExprs);
                }
            }

            // Analyze the left & right children.
            RuleExpressionWalker.AnalyzeUsage(analysis, binaryExpr.Left, true, false, null);
            RuleExpressionWalker.AnalyzeUsage(analysis, binaryExpr.Right, true, false, null);
        }

        #region Evaluate
        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            CodeBinaryOperatorExpression binaryExpr = (CodeBinaryOperatorExpression)expression;

            object lhsValue = RuleExpressionWalker.Evaluate(execution, binaryExpr.Left).Value;
            CodeBinaryOperatorType operation = binaryExpr.Operator;
            // short-circuit ANDs and ORs
            if (operation == CodeBinaryOperatorType.BooleanAnd)
            {
                if ((bool)lhsValue)
                {
                    // LHS is true, need to look at RHS
                    object rhsValue = RuleExpressionWalker.Evaluate(execution, binaryExpr.Right).Value;
                    return new RuleLiteralResult(rhsValue);
                }
                else
                    // LHS is false, so result is false
                    return new RuleLiteralResult(false);
            }
            else if (operation == CodeBinaryOperatorType.BooleanOr)
            {
                if ((bool)lhsValue)
                    // LHS is true, so result is true
                    return new RuleLiteralResult(true);
                else
                {
                    // LHS is false, so need to look at RHS
                    object rhsValue = RuleExpressionWalker.Evaluate(execution, binaryExpr.Right).Value;
                    return new RuleLiteralResult(rhsValue);
                }
            }
            else
            {
                object resultValue;
                object rhsValue = RuleExpressionWalker.Evaluate(execution, binaryExpr.Right).Value;
                RuleBinaryExpressionInfo expressionInfo = execution.Validation.ExpressionInfo(binaryExpr) as RuleBinaryExpressionInfo;
                if (expressionInfo == null)  // Oops, someone forgot to validate.
                {
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.ExpressionNotValidated);
                    InvalidOperationException exception = new InvalidOperationException(message);
                    exception.Data[RuleUserDataKeys.ErrorObject] = binaryExpr;
                    throw exception;
                }
                MethodInfo methodInfo = expressionInfo.MethodInfo;
                if (methodInfo != null)
                {
                    if (methodInfo == Literal.ObjectEquality)
                    {
                        resultValue = (lhsValue == rhsValue);
                    }
                    else
                    {
                        ParameterInfo[] existingParameters = methodInfo.GetParameters();
                        object[] parameters = new object[2];
                        parameters[0] = Executor.AdjustType(expressionInfo.LeftType, lhsValue, existingParameters[0].ParameterType);
                        parameters[1] = Executor.AdjustType(expressionInfo.RightType, rhsValue, existingParameters[1].ParameterType);
                        resultValue = methodInfo.Invoke(null, parameters);
                    }
                }
                else
                {
                    resultValue = EvaluateBinaryOperation(binaryExpr, expressionInfo.LeftType, lhsValue, operation, expressionInfo.RightType, rhsValue);
                }
                return new RuleLiteralResult(resultValue);
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static object EvaluateBinaryOperation(CodeBinaryOperatorExpression binaryExpr, Type lhsType, object lhsValue, CodeBinaryOperatorType operation, Type rhsType, object rhsValue)
        {
            Literal leftLiteral;
            Literal rightLiteral;
            ArithmeticLiteral leftArithmetic;
            ArithmeticLiteral rightArithmetic;
            string message;
            RuleEvaluationException exception;

            switch (operation)
            {
                case CodeBinaryOperatorType.Add:
                    leftArithmetic = ArithmeticLiteral.MakeLiteral(lhsType, lhsValue);
                    if (leftArithmetic == null)
                        break;
                    rightArithmetic = ArithmeticLiteral.MakeLiteral(rhsType, rhsValue);
                    if (rightArithmetic == null)
                        break;
                    return leftArithmetic.Add(rightArithmetic);
                case CodeBinaryOperatorType.Subtract:
                    leftArithmetic = ArithmeticLiteral.MakeLiteral(lhsType, lhsValue);
                    if (leftArithmetic == null)
                        break;
                    rightArithmetic = ArithmeticLiteral.MakeLiteral(rhsType, rhsValue);
                    if (rightArithmetic == null)
                        break;
                    return leftArithmetic.Subtract(rightArithmetic);
                case CodeBinaryOperatorType.Multiply:
                    leftArithmetic = ArithmeticLiteral.MakeLiteral(lhsType, lhsValue);
                    if (leftArithmetic == null)
                        break;
                    rightArithmetic = ArithmeticLiteral.MakeLiteral(rhsType, rhsValue);
                    if (rightArithmetic == null)
                        break;
                    return leftArithmetic.Multiply(rightArithmetic);
                case CodeBinaryOperatorType.Divide:
                    leftArithmetic = ArithmeticLiteral.MakeLiteral(lhsType, lhsValue);
                    if (leftArithmetic == null)
                        break;
                    rightArithmetic = ArithmeticLiteral.MakeLiteral(rhsType, rhsValue);
                    if (rightArithmetic == null)
                        break;
                    return leftArithmetic.Divide(rightArithmetic);
                case CodeBinaryOperatorType.Modulus:
                    leftArithmetic = ArithmeticLiteral.MakeLiteral(lhsType, lhsValue);
                    if (leftArithmetic == null)
                        break;
                    rightArithmetic = ArithmeticLiteral.MakeLiteral(rhsType, rhsValue);
                    if (rightArithmetic == null)
                        break;
                    return leftArithmetic.Modulus(rightArithmetic);
                case CodeBinaryOperatorType.BitwiseAnd:
                    leftArithmetic = ArithmeticLiteral.MakeLiteral(lhsType, lhsValue);
                    if (leftArithmetic == null)
                        break;
                    rightArithmetic = ArithmeticLiteral.MakeLiteral(rhsType, rhsValue);
                    if (rightArithmetic == null)
                        break;
                    return leftArithmetic.BitAnd(rightArithmetic);
                case CodeBinaryOperatorType.BitwiseOr:
                    leftArithmetic = ArithmeticLiteral.MakeLiteral(lhsType, lhsValue);
                    if (leftArithmetic == null)
                        break;
                    rightArithmetic = ArithmeticLiteral.MakeLiteral(rhsType, rhsValue);
                    if (rightArithmetic == null)
                        break;
                    return leftArithmetic.BitOr(rightArithmetic);

                case CodeBinaryOperatorType.ValueEquality:
                    leftLiteral = Literal.MakeLiteral(lhsType, lhsValue);
                    if (leftLiteral == null)
                        break;
                    rightLiteral = Literal.MakeLiteral(rhsType, rhsValue);
                    if (rightLiteral == null)
                        break;
                    return leftLiteral.Equal(rightLiteral);
                case CodeBinaryOperatorType.IdentityEquality:
                    return lhsValue == rhsValue;
                case CodeBinaryOperatorType.IdentityInequality:
                    return lhsValue != rhsValue;

                case CodeBinaryOperatorType.LessThan:
                    leftLiteral = Literal.MakeLiteral(lhsType, lhsValue);
                    if (leftLiteral == null)
                        break;
                    rightLiteral = Literal.MakeLiteral(rhsType, rhsValue);
                    if (rightLiteral == null)
                        break;
                    return leftLiteral.LessThan(rightLiteral);
                case CodeBinaryOperatorType.LessThanOrEqual:
                    leftLiteral = Literal.MakeLiteral(lhsType, lhsValue);
                    if (leftLiteral == null)
                        break;
                    rightLiteral = Literal.MakeLiteral(rhsType, rhsValue);
                    if (rightLiteral == null)
                        break;
                    return leftLiteral.LessThanOrEqual(rightLiteral);
                case CodeBinaryOperatorType.GreaterThan:
                    leftLiteral = Literal.MakeLiteral(lhsType, lhsValue);
                    if (leftLiteral == null)
                        break;
                    rightLiteral = Literal.MakeLiteral(rhsType, rhsValue);
                    if (rightLiteral == null)
                        break;
                    return leftLiteral.GreaterThan(rightLiteral);
                case CodeBinaryOperatorType.GreaterThanOrEqual:
                    leftLiteral = Literal.MakeLiteral(lhsType, lhsValue);
                    if (leftLiteral == null)
                        break;
                    rightLiteral = Literal.MakeLiteral(rhsType, rhsValue);
                    if (rightLiteral == null)
                        break;
                    return leftLiteral.GreaterThanOrEqual(rightLiteral);

                default:
                    // should never happen
                    // BooleanAnd & BooleanOr short-circuited before call
                    // Assign disallowed at validation time
                    message = string.Format(CultureInfo.CurrentCulture, Messages.BinaryOpNotSupported, operation.ToString());
                    exception = new RuleEvaluationException(message);
                    exception.Data[RuleUserDataKeys.ErrorObject] = binaryExpr;
                    throw exception;
            }

            message = string.Format(CultureInfo.CurrentCulture,
                Messages.BinaryOpFails,
                operation.ToString(),
                RuleDecompiler.DecompileType(lhsType),
                RuleDecompiler.DecompileType(rhsType));
            exception = new RuleEvaluationException(message);
            exception.Data[RuleUserDataKeys.ErrorObject] = binaryExpr;
            throw exception;
        }
        #endregion

        #region Decompile

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            bool mustParenthesize = false;
            CodeBinaryOperatorExpression binaryExpr = (CodeBinaryOperatorExpression)expression;

            if (binaryExpr.Left == null)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.NullBinaryOpLHS, binaryExpr.Operator.ToString());
                RuleEvaluationException exception = new RuleEvaluationException(message);
                exception.Data[RuleUserDataKeys.ErrorObject] = binaryExpr;
                throw exception;
            }
            if (binaryExpr.Right == null)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.NullBinaryOpRHS, binaryExpr.Operator.ToString());
                RuleEvaluationException exception = new RuleEvaluationException(message);
                exception.Data[RuleUserDataKeys.ErrorObject] = binaryExpr;
                throw exception;
            }

            string opString;

            switch (binaryExpr.Operator)
            {
                case CodeBinaryOperatorType.Modulus:
                    opString = " % ";
                    break;
                case CodeBinaryOperatorType.Multiply:
                    opString = " * ";
                    break;
                case CodeBinaryOperatorType.Divide:
                    opString = " / ";
                    break;

                case CodeBinaryOperatorType.Subtract:
                    opString = " - ";
                    break;
                case CodeBinaryOperatorType.Add:
                    opString = " + ";
                    break;

                case CodeBinaryOperatorType.LessThan:
                    opString = " < ";
                    break;
                case CodeBinaryOperatorType.LessThanOrEqual:
                    opString = " <= ";
                    break;
                case CodeBinaryOperatorType.GreaterThan:
                    opString = " > ";
                    break;
                case CodeBinaryOperatorType.GreaterThanOrEqual:
                    opString = " >= ";
                    break;

                case CodeBinaryOperatorType.IdentityEquality:
                case CodeBinaryOperatorType.ValueEquality:
                    opString = " == ";
                    break;
                case CodeBinaryOperatorType.IdentityInequality:
                    opString = " != ";
                    break;

                case CodeBinaryOperatorType.BitwiseAnd:
                    opString = " & ";
                    break;

                case CodeBinaryOperatorType.BitwiseOr:
                    opString = " | ";
                    break;

                case CodeBinaryOperatorType.BooleanAnd:
                    opString = " && ";
                    break;

                case CodeBinaryOperatorType.BooleanOr:
                    opString = " || ";
                    break;

                default:
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.BinaryOpNotSupported, binaryExpr.Operator.ToString());
                    NotSupportedException exception = new NotSupportedException(message);
                    exception.Data[RuleUserDataKeys.ErrorObject] = binaryExpr;
                    throw exception;
            }

            CodeExpression leftExpr = binaryExpr.Left;
            CodeExpression rightExpr = binaryExpr.Right;


            if (binaryExpr.Operator == CodeBinaryOperatorType.ValueEquality)
            {
                // Look for special cases:
                //    LHS == false              --> ! LHS
                // or
                //    (LHS == expr) == false    --> LHS != expr

                CodePrimitiveExpression rhsPrimitive = rightExpr as CodePrimitiveExpression;
                if (rhsPrimitive != null)
                {
                    object rhsValue = rhsPrimitive.Value;
                    if (rhsValue != null)
                    {
                        // we don't have the comparison "==null"
                        if (rhsValue.GetType() == typeof(bool) && (bool)rhsValue == false)
                        {
                            // We have comparison "== false".

                            CodeBinaryOperatorExpression lhsBinary = leftExpr as CodeBinaryOperatorExpression;
                            if (lhsBinary != null && lhsBinary.Operator == CodeBinaryOperatorType.ValueEquality)
                            {
                                // We have the pattern
                                //      (expr1 == expr2) == false
                                // Treat this as:
                                //      expr1 != expr2

                                opString = " != ";

                                leftExpr = lhsBinary.Left;
                                rightExpr = lhsBinary.Right;
                            }
                            else
                            {
                                // We have the pattern
                                //      LHS == false
                                // Treat this as:
                                //      ! LHS

                                mustParenthesize = RuleDecompiler.MustParenthesize(leftExpr, parentExpression);
                                if (mustParenthesize)
                                    stringBuilder.Append("(");

                                // Note the "parentExpression" passed to the child decompile... cast is the only
                                // built-in operation that has "unary" precedence, so pass that as the parent
                                // to get the parenthesization right. .
                                stringBuilder.Append("!");
                                RuleExpressionWalker.Decompile(stringBuilder, leftExpr, new CodeCastExpression());

                                if (mustParenthesize)
                                    stringBuilder.Append(")");

                                return;
                            }
                        }
                    }
                }
            }
            else if (binaryExpr.Operator == CodeBinaryOperatorType.Subtract)
            {
                // Look for the special case:
                //    0 - RHS       --> - RHS

                CodePrimitiveExpression lhsPrimitive = leftExpr as CodePrimitiveExpression;
                if (lhsPrimitive != null && lhsPrimitive.Value != null)
                {
                    object lhsValue = lhsPrimitive.Value;

                    // Check if the LHS is zero.  We'll only check a few types (decimal,
                    // double, float, int, long), since these occur most often (and the 
                    // unsigned types are all illegal).
                    TypeCode tc = Type.GetTypeCode(lhsValue.GetType());
                    bool isZero = false;
                    switch (tc)
                    {
                        case TypeCode.Decimal:
                            isZero = ((decimal)lhsValue) == 0;
                            break;

                        case TypeCode.Double:
                            isZero = ((double)lhsValue) == 0;
                            break;

                        case TypeCode.Single:
                            isZero = ((float)lhsValue) == 0;
                            break;

                        case TypeCode.Int32:
                            isZero = ((int)lhsValue) == 0;
                            break;

                        case TypeCode.Int64:
                            isZero = ((long)lhsValue) == 0;
                            break;
                    }

                    if (isZero)
                    {
                        mustParenthesize = RuleDecompiler.MustParenthesize(rightExpr, parentExpression);
                        if (mustParenthesize)
                            stringBuilder.Append("(");

                        // Note the "parentExpression" passed to the child decompile... cast is the only
                        // built-in operation that has "unary" precedence, so pass that as the parent
                        // to get the parenthesization right.  
                        stringBuilder.Append("-");
                        RuleExpressionWalker.Decompile(stringBuilder, rightExpr, new CodeCastExpression());

                        if (mustParenthesize)
                            stringBuilder.Append(")");

                        return;
                    }
                }
            }

            mustParenthesize = RuleDecompiler.MustParenthesize(binaryExpr, parentExpression);
            if (mustParenthesize)
                stringBuilder.Append("(");

            RuleExpressionWalker.Decompile(stringBuilder, leftExpr, binaryExpr);
            stringBuilder.Append(opString);
            RuleExpressionWalker.Decompile(stringBuilder, rightExpr, binaryExpr);

            if (mustParenthesize)
                stringBuilder.Append(")");
        }
        #endregion

        internal override CodeExpression Clone(CodeExpression expression)
        {
            CodeBinaryOperatorExpression binaryExpr = (CodeBinaryOperatorExpression)expression;

            CodeBinaryOperatorExpression newOp = new CodeBinaryOperatorExpression();
            newOp.Operator = binaryExpr.Operator;
            newOp.Left = RuleExpressionWalker.Clone(binaryExpr.Left);
            newOp.Right = RuleExpressionWalker.Clone(binaryExpr.Right);
            return newOp;
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            CodeBinaryOperatorExpression binaryExpr = (CodeBinaryOperatorExpression)expression;

            CodeBinaryOperatorExpression comperandBinary = (CodeBinaryOperatorExpression)comperand;
            return (binaryExpr.Operator == comperandBinary.Operator
                && RuleExpressionWalker.Match(binaryExpr.Left, comperandBinary.Left)
                && RuleExpressionWalker.Match(binaryExpr.Right, comperandBinary.Right));
        }
    }

    #endregion

    #region Field ref expression

    // CodeFieldReferenceExpression
    internal class FieldReferenceExpression : RuleExpressionInternal
    {
        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            string message;

            CodeFieldReferenceExpression fieldRefExpr = (CodeFieldReferenceExpression)expression;

            if (fieldRefExpr.TargetObject == null)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.NullFieldTarget, fieldRefExpr.FieldName);
                ValidationError error = new ValidationError(message, ErrorNumbers.Error_ParameterNotSet);
                error.UserData[RuleUserDataKeys.ErrorObject] = fieldRefExpr;
                validation.Errors.Add(error);
                return null;
            }

            // Early exit from this if a cycle is detected.
            if (!validation.PushParentExpression(fieldRefExpr))
                return null;

            RuleExpressionInfo targetExprInfo = RuleExpressionWalker.Validate(validation, fieldRefExpr.TargetObject, false);

            validation.PopParentExpression();

            if (targetExprInfo == null)     // error occurred, so simply return
                return null;

            Type targetType = targetExprInfo.ExpressionType;
            if (targetType == null)         // no type, so must have been an error already
                return null;

            if (targetType == typeof(NullLiteral))
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.NullFieldTarget, fieldRefExpr.FieldName);
                ValidationError error = new ValidationError(message, ErrorNumbers.Error_BindingTypeMissing);
                error.UserData[RuleUserDataKeys.ErrorObject] = fieldRefExpr;
                validation.Errors.Add(error);
                return null;
            }

            BindingFlags bindingFlags = BindingFlags.Public;
            if (fieldRefExpr.TargetObject is CodeTypeReferenceExpression)
                bindingFlags |= BindingFlags.Static | BindingFlags.FlattenHierarchy;
            else
                bindingFlags |= BindingFlags.Instance;
            if (validation.AllowInternalMembers(targetType))
                bindingFlags |= BindingFlags.NonPublic;

            FieldInfo fi = targetType.GetField(fieldRefExpr.FieldName, bindingFlags);
            if (fi == null)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.UnknownField, fieldRefExpr.FieldName, RuleDecompiler.DecompileType(targetType));
                ValidationError error = new ValidationError(message, ErrorNumbers.Error_CannotResolveMember);
                error.UserData[RuleUserDataKeys.ErrorObject] = fieldRefExpr;
                validation.Errors.Add(error);
                return null;
            }

            if (fi.FieldType == null)
            {
                // This can only happen with a design-time type.
                message = string.Format(CultureInfo.CurrentCulture, Messages.CouldNotDetermineMemberType, fieldRefExpr.FieldName);
                ValidationError error = new ValidationError(message, ErrorNumbers.Error_CouldNotDetermineMemberType);
                error.UserData[RuleUserDataKeys.ErrorObject] = fieldRefExpr;
                validation.Errors.Add(error);
                return null;
            }

            if (isWritten && fi.IsLiteral)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.FieldSetNotAllowed, fieldRefExpr.FieldName, RuleDecompiler.DecompileType(targetType));
                ValidationError error = new ValidationError(message, ErrorNumbers.Error_InvalidAssignTarget);
                error.UserData[RuleUserDataKeys.ErrorObject] = fieldRefExpr;
                validation.Errors.Add(error);
                return null;
            }

            if (!validation.ValidateMemberAccess(fieldRefExpr.TargetObject, targetType, fi, fi.Name, fieldRefExpr))
                return null;

            // Is it possible to set fi by validation.ResolveFieldOrProperty(targetType, fieldExpr.FieldName)?
            validation.IsAuthorized(fi.FieldType);
            return new RuleFieldExpressionInfo(fi);
        }

        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            CodeFieldReferenceExpression fieldRefExpr = (CodeFieldReferenceExpression)expression;
            CodeExpression targetObject = fieldRefExpr.TargetObject;
            RuleExpressionWalker.AnalyzeUsage(analysis, targetObject, isRead, isWritten, new RulePathQualifier(fieldRefExpr.FieldName, qualifier));
        }

        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            CodeFieldReferenceExpression fieldRefExpr = (CodeFieldReferenceExpression)expression;
            object target = RuleExpressionWalker.Evaluate(execution, fieldRefExpr.TargetObject).Value;

            RuleFieldExpressionInfo fieldExprInfo = execution.Validation.ExpressionInfo(fieldRefExpr) as RuleFieldExpressionInfo;
            if (fieldExprInfo == null)  // Oops, someone forgot to validate.
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.ExpressionNotValidated);
                InvalidOperationException exception = new InvalidOperationException(message);
                exception.Data[RuleUserDataKeys.ErrorObject] = fieldRefExpr;
                throw exception;
            }

            FieldInfo fi = fieldExprInfo.FieldInfo;

            return new RuleFieldResult(target, fi);
        }

        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            CodeFieldReferenceExpression fieldRefExpr = (CodeFieldReferenceExpression)expression;

            CodeExpression targetObject = fieldRefExpr.TargetObject;
            if (targetObject == null)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.NullFieldTarget, fieldRefExpr.FieldName);
                RuleEvaluationException exception = new RuleEvaluationException(message);
                exception.Data[RuleUserDataKeys.ErrorObject] = fieldRefExpr;
                throw exception;
            }

            RuleExpressionWalker.Decompile(stringBuilder, targetObject, fieldRefExpr);
            stringBuilder.Append('.');
            stringBuilder.Append(fieldRefExpr.FieldName);
        }

        internal override CodeExpression Clone(CodeExpression expression)
        {
            CodeFieldReferenceExpression fieldRefExpr = (CodeFieldReferenceExpression)expression;

            CodeFieldReferenceExpression newField = new CodeFieldReferenceExpression();
            newField.FieldName = fieldRefExpr.FieldName;
            newField.TargetObject = RuleExpressionWalker.Clone(fieldRefExpr.TargetObject);
            return newField;
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            CodeFieldReferenceExpression fieldRefExpr = (CodeFieldReferenceExpression)expression;

            CodeFieldReferenceExpression newField = (CodeFieldReferenceExpression)comperand;
            return (fieldRefExpr.FieldName == newField.FieldName
                && RuleExpressionWalker.Match(fieldRefExpr.TargetObject, newField.TargetObject));
        }
    }

    #endregion

    #region Property ref expression

    // CodePropertyReferenceExpression
    internal class PropertyReferenceExpression : RuleExpressionInternal
    {
        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            string message;

            CodePropertyReferenceExpression propGetExpr = (CodePropertyReferenceExpression)expression;

            if (propGetExpr.TargetObject == null)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.NullPropertyTarget, propGetExpr.PropertyName);
                ValidationError error = new ValidationError(message, ErrorNumbers.Error_ParameterNotSet);
                error.UserData[RuleUserDataKeys.ErrorObject] = propGetExpr;
                validation.Errors.Add(error);
                return null;
            }

            // Early exit from this if a cycle is detected.
            if (!validation.PushParentExpression(propGetExpr))
                return null;

            RuleExpressionInfo targetExprInfo = RuleExpressionWalker.Validate(validation, propGetExpr.TargetObject, false);

            validation.PopParentExpression();

            if (targetExprInfo == null)     // error occurred, so simply return
                return null;

            Type targetType = targetExprInfo.ExpressionType;
            if (targetType == null)         // no type, so must have been an error already
                return null;

            if (targetType == typeof(NullLiteral))
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.NullPropertyTarget, propGetExpr.PropertyName);
                ValidationError error = new ValidationError(message, ErrorNumbers.Error_BindingTypeMissing);
                error.UserData[RuleUserDataKeys.ErrorObject] = propGetExpr;
                validation.Errors.Add(error);
                return null;
            }

            bool includeNonPublic = false;
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;
            if (validation.AllowInternalMembers(targetType))
            {
                bindingFlags |= BindingFlags.NonPublic;
                includeNonPublic = true;
            }
            PropertyInfo pi = validation.ResolveProperty(targetType, propGetExpr.PropertyName, bindingFlags);

            if (pi == null)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.UnknownProperty, propGetExpr.PropertyName, RuleDecompiler.DecompileType(targetType));
                ValidationError error = new ValidationError(message, ErrorNumbers.Error_CannotResolveMember);
                error.UserData[RuleUserDataKeys.ErrorObject] = propGetExpr;
                validation.Errors.Add(error);
                return null;
            }

            if (pi.PropertyType == null)
            {
                // This can only happen with a design-time type.
                message = string.Format(CultureInfo.CurrentCulture, Messages.CouldNotDetermineMemberType, propGetExpr.PropertyName);
                ValidationError error = new ValidationError(message, ErrorNumbers.Error_CouldNotDetermineMemberType);
                error.UserData[RuleUserDataKeys.ErrorObject] = propGetExpr;
                validation.Errors.Add(error);
                return null;
            }

            MethodInfo accessorMethod = isWritten ? pi.GetSetMethod(includeNonPublic) : pi.GetGetMethod(includeNonPublic);
            if (accessorMethod == null)
            {
                string baseMessage = isWritten ? Messages.UnknownPropertySet : Messages.UnknownPropertyGet;
                message = string.Format(CultureInfo.CurrentCulture, baseMessage, propGetExpr.PropertyName, RuleDecompiler.DecompileType(targetType));
                ValidationError error = new ValidationError(message, ErrorNumbers.Error_CannotResolveMember);
                error.UserData[RuleUserDataKeys.ErrorObject] = propGetExpr;
                validation.Errors.Add(error);
                return null;
            }

            if (!validation.ValidateMemberAccess(propGetExpr.TargetObject, targetType, accessorMethod, propGetExpr.PropertyName, propGetExpr))
                return null;

            // Validate any RuleAttributes, if present.
            object[] attrs = pi.GetCustomAttributes(typeof(RuleAttribute), true);
            if (attrs != null && attrs.Length > 0)
            {
                Stack<MemberInfo> methodStack = new Stack<MemberInfo>();
                methodStack.Push(pi);

                bool allAttributesValid = true;
                foreach (RuleAttribute ruleAttr in attrs)
                {
                    if (!ruleAttr.Validate(validation, pi, targetType, null))
                        allAttributesValid = false;
                }

                methodStack.Pop();

                if (!allAttributesValid)
                    return null;
            }

            return new RulePropertyExpressionInfo(pi, pi.PropertyType, false);
        }


        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            string message;

            CodePropertyReferenceExpression propGetExpr = (CodePropertyReferenceExpression)expression;

            // Evaluate the target object and get its type.
            CodeExpression targetObject = propGetExpr.TargetObject;
            RuleExpressionInfo targetExprInfo = analysis.Validation.ExpressionInfo(targetObject);
            if (targetExprInfo == null)  // Oops, someone forgot to validate.
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.ExpressionNotValidated);
                InvalidOperationException exception = new InvalidOperationException(message);
                exception.Data[RuleUserDataKeys.ErrorObject] = targetObject;
                throw exception;
            }

            // Get the property info from the validator so we can look for [RuleRead] and [RuleWrite] attributes.
            RulePropertyExpressionInfo propExprInfo = analysis.Validation.ExpressionInfo(propGetExpr) as RulePropertyExpressionInfo;
            if (propExprInfo == null)  // Oops, someone forgot to validate.
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.ExpressionNotValidated);
                InvalidOperationException exception = new InvalidOperationException(message);
                exception.Data[RuleUserDataKeys.ErrorObject] = propGetExpr;
                throw exception;
            }

            PropertyInfo pi = propExprInfo.PropertyInfo;

            // Look for RuleAttribute's on the invoked property.
            List<CodeExpression> attributedExprs = new List<CodeExpression>();
            analysis.AnalyzeRuleAttributes(pi, targetObject, qualifier, null, null, attributedExprs);

            // See if the target object needs default analysis.
            if (!attributedExprs.Contains(targetObject))
            {
                // The property had no [RuleRead] or [RuleWrite] attributes.  Just qualify the target object with
                // the property name and proceed with the analysis.
                RuleExpressionWalker.AnalyzeUsage(analysis, targetObject, isRead, isWritten, new RulePathQualifier(pi.Name, qualifier));
            }
        }

        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            CodePropertyReferenceExpression propGetExpr = (CodePropertyReferenceExpression)expression;

            object target = RuleExpressionWalker.Evaluate(execution, propGetExpr.TargetObject).Value;

            RulePropertyExpressionInfo propExprInfo = execution.Validation.ExpressionInfo(propGetExpr) as RulePropertyExpressionInfo;
            if (propExprInfo == null)  // Oops, someone forgot to validate.
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.ExpressionNotValidated);
                InvalidOperationException exception = new InvalidOperationException(message);
                exception.Data[RuleUserDataKeys.ErrorObject] = propGetExpr;
                throw exception;
            }

            PropertyInfo pi = propExprInfo.PropertyInfo;
            return new RulePropertyResult(pi, target, null);
        }

        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            CodePropertyReferenceExpression propGetExpr = (CodePropertyReferenceExpression)expression;

            CodeExpression targetObject = propGetExpr.TargetObject;
            if (targetObject == null)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.NullPropertyTarget, propGetExpr.PropertyName);
                RuleEvaluationException exception = new RuleEvaluationException(message);
                exception.Data[RuleUserDataKeys.ErrorObject] = propGetExpr;
                throw exception;
            }

            RuleExpressionWalker.Decompile(stringBuilder, targetObject, propGetExpr);
            stringBuilder.Append('.');
            stringBuilder.Append(propGetExpr.PropertyName);
        }

        internal override CodeExpression Clone(CodeExpression expression)
        {
            CodePropertyReferenceExpression propGetExpr = (CodePropertyReferenceExpression)expression;

            CodePropertyReferenceExpression newProperty = new CodePropertyReferenceExpression();
            newProperty.PropertyName = propGetExpr.PropertyName;
            newProperty.TargetObject = RuleExpressionWalker.Clone(propGetExpr.TargetObject);
            return newProperty;
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            CodePropertyReferenceExpression propGetExpr = (CodePropertyReferenceExpression)expression;

            CodePropertyReferenceExpression newProperty = (CodePropertyReferenceExpression)comperand;
            return (propGetExpr.PropertyName == newProperty.PropertyName
                && RuleExpressionWalker.Match(propGetExpr.TargetObject, newProperty.TargetObject));
        }
    }

    #endregion

    #region Method invoke expression

    // CodeMethodInvokeExpression
    internal class MethodInvokeExpression : RuleExpressionInternal
    {
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            Type targetType = null;
            RuleMethodInvokeExpressionInfo methodInvokeInfo = null;
            string message;
            ValidationError error = null;
            BindingFlags bindingFlags = BindingFlags.Public;

            CodeMethodInvokeExpression invokeExpr = (CodeMethodInvokeExpression)expression;

            if (isWritten)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.CannotWriteToExpression, typeof(CodeMethodInvokeExpression).ToString());
                error = new ValidationError(message, ErrorNumbers.Error_InvalidAssignTarget);
                error.UserData[RuleUserDataKeys.ErrorObject] = invokeExpr;
                validation.Errors.Add(error);
                return null;
            }

            if ((invokeExpr.Method == null) || (invokeExpr.Method.TargetObject == null))
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.NullMethodTarget, invokeExpr.Method.MethodName);
                error = new ValidationError(message, ErrorNumbers.Error_ParameterNotSet);
                error.UserData[RuleUserDataKeys.ErrorObject] = invokeExpr;
                validation.Errors.Add(error);
                return null; // Fatal error; discontinue validation of this object.
            }

            if ((invokeExpr.Method.TypeArguments != null) && (invokeExpr.Method.TypeArguments.Count > 0))
            {
                error = new ValidationError(Messages.GenericMethodsNotSupported, ErrorNumbers.Error_CodeExpressionNotHandled);
                error.UserData[RuleUserDataKeys.ErrorObject] = invokeExpr;
                validation.Errors.Add(error);
                return null;
            }

            try
            {
                // Early exit from this if a cycle is detected.
                if (!validation.PushParentExpression(invokeExpr))
                    return null;

                RuleExpressionInfo targetExprInfo = RuleExpressionWalker.Validate(validation, invokeExpr.Method.TargetObject, false);
                if (targetExprInfo == null)     // error occurred, so simply return
                    return null;

                targetType = targetExprInfo.ExpressionType;
                if (targetType == null)
                    return null;

                // if an error occurred (targetType == null), continue on to validate the arguments
                if (targetType == typeof(NullLiteral))
                {
                    message = string.Format(CultureInfo.CurrentCulture, Messages.NullMethodTarget, invokeExpr.Method.MethodName);
                    error = new ValidationError(message, ErrorNumbers.Error_BindingTypeMissing);
                    error.UserData[RuleUserDataKeys.ErrorObject] = invokeExpr;
                    validation.Errors.Add(error);
                    targetType = null; // force exit after validating the arguments
                }

                List<CodeExpression> argExprs = new List<CodeExpression>();

                bool hasInvalidArgument = false;
                if (invokeExpr.Parameters != null)
                {
                    for (int i = 0; i < invokeExpr.Parameters.Count; ++i)
                    {
                        CodeExpression argExpr = invokeExpr.Parameters[i];
                        if (argExpr == null)
                        {
                            message = string.Format(CultureInfo.CurrentCulture, Messages.NullMethodParameter, i.ToString(CultureInfo.CurrentCulture), invokeExpr.Method.MethodName);
                            error = new ValidationError(message, ErrorNumbers.Error_ParameterNotSet);
                            error.UserData[RuleUserDataKeys.ErrorObject] = invokeExpr;
                            validation.Errors.Add(error);
                            targetType = null; // force exit after validating the rest of the arguments
                        }
                        else
                        {
                            if (argExpr is CodeTypeReferenceExpression)
                            {
                                message = string.Format(CultureInfo.CurrentCulture, Messages.CodeExpressionNotHandled, argExpr.GetType().FullName);
                                error = new ValidationError(message, ErrorNumbers.Error_CodeExpressionNotHandled);
                                error.UserData[RuleUserDataKeys.ErrorObject] = argExpr;
                                validation.AddError(error);

                                hasInvalidArgument = true;
                            }

                            // Validate the argument.
                            RuleExpressionInfo argExprInfo = RuleExpressionWalker.Validate(validation, argExpr, false);
                            if (argExprInfo == null)
                                hasInvalidArgument = true;
                            argExprs.Add(argExpr);
                        }
                    }
                }

                // Stop further validation if there was a problem with the target expression.
                if (targetType == null)
                    return null;

                // Stop further validation if there was a problem with any of the arguments.
                if (hasInvalidArgument)
                    return null;

                if (invokeExpr.Method.TargetObject is CodeTypeReferenceExpression)
                    bindingFlags |= BindingFlags.Static | BindingFlags.FlattenHierarchy;
                else
                    bindingFlags |= BindingFlags.Instance;
                if (validation.AllowInternalMembers(targetType))
                    bindingFlags |= BindingFlags.NonPublic;

                // Everything okay so far, try to resolve the method.
                methodInvokeInfo = validation.ResolveMethod(targetType, invokeExpr.Method.MethodName, bindingFlags, argExprs, out error);
                if ((methodInvokeInfo == null) && (invokeExpr.UserData.Contains(RuleUserDataKeys.QualifiedName)))
                {
                    // failed to resolve the method, but a fully qualified type name is around
                    // load the type, add it to the assemblies, and try again
                    string qualifiedName = invokeExpr.UserData[RuleUserDataKeys.QualifiedName] as string;
                    Type containingClassType = validation.ResolveType(qualifiedName);
                    if (containingClassType != null)
                    {
                        validation.DetermineExtensionMethods(containingClassType.Assembly);
                        methodInvokeInfo = validation.ResolveMethod(targetType, invokeExpr.Method.MethodName, bindingFlags, argExprs, out error);
                    }
                }
                if (methodInvokeInfo == null)
                {
                    error.UserData[RuleUserDataKeys.ErrorObject] = invokeExpr;
                    validation.Errors.Add(error);
                    return null;
                }
            }
            finally
            {
                validation.PopParentExpression();
            }


            MethodInfo mi = methodInvokeInfo.MethodInfo;

            if (mi.ReturnType == null)
            {
                // This can only happen with a design-time type.
                message = string.Format(CultureInfo.CurrentCulture, Messages.CouldNotDetermineMemberType, invokeExpr.Method.MethodName);
                error = new ValidationError(message, ErrorNumbers.Error_CouldNotDetermineMemberType);
                error.UserData[RuleUserDataKeys.ErrorObject] = invokeExpr;
                validation.Errors.Add(error);
                return null;
            }

            if (!validation.ValidateMemberAccess(invokeExpr.Method.TargetObject, targetType, mi, invokeExpr.Method.MethodName, invokeExpr))
                return null;

            // Validate any RuleAttributes, if present.
            object[] attrs = mi.GetCustomAttributes(typeof(RuleAttribute), true);
            if (attrs != null && attrs.Length > 0)
            {
                Stack<MemberInfo> methodStack = new Stack<MemberInfo>();
                methodStack.Push(mi);

                bool allAttributesValid = true;
                foreach (RuleAttribute ruleAttr in attrs)
                {
                    if (!ruleAttr.Validate(validation, mi, targetType, mi.GetParameters()))
                        allAttributesValid = false;
                }

                methodStack.Pop();

                if (!allAttributesValid)
                    return null;
            }

            // if this is an extension method, save the type information
            if (mi is ExtensionMethodInfo)
            {
                invokeExpr.UserData[RuleUserDataKeys.QualifiedName] = mi.DeclaringType.AssemblyQualifiedName;
            }

            return methodInvokeInfo;
        }

        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            string message;

            CodeMethodInvokeExpression invokeExpr = (CodeMethodInvokeExpression)expression;

            // Get the target object's type.
            CodeExpression targetObject = invokeExpr.Method.TargetObject;
            RuleExpressionInfo targetExprInfo = analysis.Validation.ExpressionInfo(targetObject);
            if (targetExprInfo == null)  // Oops, someone forgot to validate.
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.ExpressionNotValidated);
                InvalidOperationException exception = new InvalidOperationException(message);
                exception.Data[RuleUserDataKeys.ErrorObject] = targetObject;
                throw exception;
            }

            // Get the method info from the validation so we can look for [RuleRead] and [RuleWrite] attributes.
            RuleMethodInvokeExpressionInfo methodExprInfo = analysis.Validation.ExpressionInfo(invokeExpr) as RuleMethodInvokeExpressionInfo;
            if (methodExprInfo == null)  // Oops, someone forgot to validate.
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.ExpressionNotValidated);
                InvalidOperationException exception = new InvalidOperationException(message);
                exception.Data[RuleUserDataKeys.ErrorObject] = invokeExpr;
                throw exception;
            }

            MethodInfo mi = methodExprInfo.MethodInfo;

            // Look for RuleAttribute's on the invoked method.
            List<CodeExpression> attributedExprs = new List<CodeExpression>();
            analysis.AnalyzeRuleAttributes(mi, targetObject, qualifier, invokeExpr.Parameters, mi.GetParameters(), attributedExprs);

            // See if the target object needs default analysis.
            if (!attributedExprs.Contains(targetObject))
            {
                // No applicable [RuleRead] or [RuleWrite] attributes were found on the target object.

                // If we're analyzing for dependencies, assume that this method uses the
                // value of the target object, but nothing beneath it.

                RuleExpressionWalker.AnalyzeUsage(analysis, targetObject, true, false, null);
            }

            // See if any of the arguments need default analysis.
            for (int i = 0; i < invokeExpr.Parameters.Count; ++i)
            {
                CodeExpression argExpr = invokeExpr.Parameters[i];

                if (!attributedExprs.Contains(argExpr))
                {
                    // Similar to the target object, we assume that this method can reads the value
                    // of the parameter, but none of its members.
                    RuleExpressionWalker.AnalyzeUsage(analysis, argExpr, true, false, null);
                }
            }
        }

        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            string message;

            CodeMethodInvokeExpression invokeExpr = (CodeMethodInvokeExpression)expression;

            object target = RuleExpressionWalker.Evaluate(execution, invokeExpr.Method.TargetObject).Value;

            RuleMethodInvokeExpressionInfo invokeExprInfo = execution.Validation.ExpressionInfo(invokeExpr) as RuleMethodInvokeExpressionInfo;
            if (invokeExprInfo == null)  // Oops, someone forgot to validate.
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.ExpressionNotValidated);
                InvalidOperationException exception = new InvalidOperationException(message);
                exception.Data[RuleUserDataKeys.ErrorObject] = invokeExpr;
                throw exception;
            }

            MethodInfo mi = invokeExprInfo.MethodInfo;

            if (!mi.IsStatic && target == null)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.TargetEvaluatedNullMethod, invokeExpr.Method.MethodName);
                RuleEvaluationException exception = new RuleEvaluationException(message);
                exception.Data[RuleUserDataKeys.ErrorObject] = invokeExpr;
                throw exception;
            }

            object[] arguments = null;
            RuleExpressionResult[] outArgumentResults = null;

            if (invokeExpr.Parameters != null && invokeExpr.Parameters.Count > 0)
            {
                int actualArgCount = invokeExpr.Parameters.Count;

                ParameterInfo[] parmInfos = mi.GetParameters();

                arguments = new object[parmInfos.Length];

                int numFixedParameters = parmInfos.Length;
                if (invokeExprInfo.NeedsParamsExpansion)
                    numFixedParameters -= 1;

                int i;

                // Evaluate the fixed portion of the parameter list.
                for (i = 0; i < numFixedParameters; ++i)
                {
                    Type argType = execution.Validation.ExpressionInfo(invokeExpr.Parameters[i]).ExpressionType;
                    RuleExpressionResult argResult = RuleExpressionWalker.Evaluate(execution, invokeExpr.Parameters[i]);

                    // Special procesing of direction expressions to keep track of out arguments (& ref).
                    CodeDirectionExpression direction = invokeExpr.Parameters[i] as CodeDirectionExpression;
                    if (direction != null && (direction.Direction == FieldDirection.Ref || direction.Direction == FieldDirection.Out))
                    {
                        // lazy creation of fieldsToSet
                        if (outArgumentResults == null)
                            outArgumentResults = new RuleExpressionResult[invokeExpr.Parameters.Count];
                        // keep track of this out expression so we can set it later
                        outArgumentResults[i] = argResult;

                        // don't evaluate out arguments
                        if (direction.Direction != FieldDirection.Out)
                            arguments[i] = Executor.AdjustType(argType, argResult.Value, parmInfos[i].ParameterType);
                    }
                    else
                    {
                        // treat as in
                        arguments[i] = Executor.AdjustType(argType, argResult.Value, parmInfos[i].ParameterType);
                    }
                }

                if (numFixedParameters < actualArgCount)
                {
                    // This target method had a params array, and we are calling it with an
                    // expanded parameter list.  E.g.,
                    //      void foo(int x, params string[] y)
                    // with the invocation:
                    //      foo(5, "crud", "kreeble", "glorp")
                    // We need to translate this to:
                    //      foo(5, new string[] { "crud", "kreeble", "glorp" })

                    ParameterInfo lastParamInfo = parmInfos[numFixedParameters];

                    Type arrayType = lastParamInfo.ParameterType;
                    System.Diagnostics.Debug.Assert(arrayType.IsArray);
                    Type elementType = arrayType.GetElementType();

                    Array paramsArray = (Array)arrayType.InvokeMember(arrayType.Name, BindingFlags.CreateInstance, null, null, new object[] { actualArgCount - i }, CultureInfo.CurrentCulture);
                    for (; i < actualArgCount; ++i)
                    {
                        Type argType = execution.Validation.ExpressionInfo(invokeExpr.Parameters[i]).ExpressionType;
                        RuleExpressionResult argResult = RuleExpressionWalker.Evaluate(execution, invokeExpr.Parameters[i]);
                        paramsArray.SetValue(Executor.AdjustType(argType, argResult.Value, elementType), i - numFixedParameters);
                    }

                    arguments[numFixedParameters] = paramsArray;
                }
            }

            object result;
            try
            {
                result = mi.Invoke(target, arguments);
            }
            catch (TargetInvocationException e)
            {
                // if there is no inner exception, leave it untouched
                if (e.InnerException == null)
                    throw;
                message = string.Format(CultureInfo.CurrentCulture, Messages.Error_MethodInvoke,
                    RuleDecompiler.DecompileType(mi.ReflectedType), mi.Name, e.InnerException.Message);
                throw new TargetInvocationException(message, e.InnerException);
            }

            // any out/ref parameters that need to be assigned?
            if (outArgumentResults != null)
            {
                for (int i = 0; i < invokeExpr.Parameters.Count; ++i)
                {
                    if (outArgumentResults[i] != null)
                        outArgumentResults[i].Value = arguments[i];
                }
            }

            return new RuleLiteralResult(result);
        }

        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            CodeMethodInvokeExpression invokeExpr = (CodeMethodInvokeExpression)expression;

            if ((invokeExpr.Method == null) || (invokeExpr.Method.TargetObject == null))
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.NullMethodTarget, invokeExpr.Method.MethodName);
                RuleEvaluationException exception = new RuleEvaluationException(message);
                exception.Data[RuleUserDataKeys.ErrorObject] = invokeExpr;
                throw exception;
            }

            // Decompile the target expression.
            CodeExpression targetObject = invokeExpr.Method.TargetObject;
            RuleExpressionWalker.Decompile(stringBuilder, targetObject, invokeExpr);

            stringBuilder.Append('.');
            stringBuilder.Append(invokeExpr.Method.MethodName);

            // Decompile the arguments
            stringBuilder.Append('(');

            if (invokeExpr.Parameters != null)
            {
                for (int i = 0; i < invokeExpr.Parameters.Count; ++i)
                {
                    CodeExpression paramExpr = invokeExpr.Parameters[i];
                    if (paramExpr == null)
                    {
                        string message = string.Format(CultureInfo.CurrentCulture, Messages.NullMethodTypeParameter, i.ToString(CultureInfo.CurrentCulture), invokeExpr.Method.MethodName);
                        RuleEvaluationException exception = new RuleEvaluationException(message);
                        exception.Data[RuleUserDataKeys.ErrorObject] = invokeExpr;
                        throw exception;
                    }

                    if (i > 0)
                        stringBuilder.Append(", ");

                    RuleExpressionWalker.Decompile(stringBuilder, paramExpr, null);
                }
            }

            stringBuilder.Append(')');
        }

        internal override CodeExpression Clone(CodeExpression expression)
        {
            CodeMethodInvokeExpression invokeExpr = (CodeMethodInvokeExpression)expression;

            CodeMethodInvokeExpression newMethod = new CodeMethodInvokeExpression();
            newMethod.Method = CloneMethodReference(invokeExpr.Method);
            foreach (CodeExpression argument in invokeExpr.Parameters)
                newMethod.Parameters.Add(RuleExpressionWalker.Clone(argument));
            return newMethod;
        }

        private static CodeMethodReferenceExpression CloneMethodReference(CodeMethodReferenceExpression oldReference)
        {
            CodeMethodReferenceExpression newReference = new CodeMethodReferenceExpression();
            newReference.MethodName = oldReference.MethodName;
            newReference.TargetObject = RuleExpressionWalker.Clone(oldReference.TargetObject);
            foreach (CodeTypeReference typeReference in oldReference.TypeArguments)
                newReference.TypeArguments.Add(TypeReferenceExpression.CloneType(typeReference));
            ConditionHelper.CloneUserData(oldReference, newReference);
            return newReference;
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            CodeMethodInvokeExpression invokeExpr = (CodeMethodInvokeExpression)expression;

            CodeMethodInvokeExpression newMethod = (CodeMethodInvokeExpression)comperand;
            if (invokeExpr.Method.MethodName != newMethod.Method.MethodName)
                return false;
            if (!RuleExpressionWalker.Match(invokeExpr.Method.TargetObject, newMethod.Method.TargetObject))
                return false;
            if (invokeExpr.Parameters.Count != newMethod.Parameters.Count)
                return false;
            for (int i = 0; i < invokeExpr.Parameters.Count; ++i)
            {
                if (!RuleExpressionWalker.Match(invokeExpr.Parameters[i], newMethod.Parameters[i]))
                    return false;
            }
            return true;
        }
    }

    #endregion

    #region Direction expression (in/out/ref)

    // CodeDirectionExpression
    internal class DirectionExpression : RuleExpressionInternal
    {
        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            CodeDirectionExpression directionExpr = (CodeDirectionExpression)expression;

            if (isWritten)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.CannotWriteToExpression, typeof(CodeDirectionExpression).ToString());
                ValidationError error = new ValidationError(message, ErrorNumbers.Error_InvalidAssignTarget);
                error.UserData[RuleUserDataKeys.ErrorObject] = directionExpr;
                validation.Errors.Add(error);
                return null;
            }

            // direction specified, make sure that something is specified
            if (directionExpr.Expression == null)
            {
                ValidationError error = new ValidationError(Messages.NullDirectionTarget, ErrorNumbers.Error_ParameterNotSet);
                error.UserData[RuleUserDataKeys.ErrorObject] = directionExpr;
                validation.Errors.Add(error);
                return null;
            }

            if (directionExpr.Expression is CodeTypeReferenceExpression)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.CodeExpressionNotHandled, directionExpr.Expression.GetType().FullName);
                ValidationError error = new ValidationError(message, ErrorNumbers.Error_CodeExpressionNotHandled);
                error.UserData[RuleUserDataKeys.ErrorObject] = directionExpr.Expression;
                validation.AddError(error);
                return null;
            }

            // validate the parameter
            RuleExpressionInfo paramExprInfo;
            bool isRef;
            if (directionExpr.Direction == FieldDirection.Ref)
            {
                // ref parameters require that we both read and write the value
                isRef = true;
                paramExprInfo = RuleExpressionWalker.Validate(validation, directionExpr.Expression, false);
                if (paramExprInfo == null)
                    return null;
                paramExprInfo = RuleExpressionWalker.Validate(validation, directionExpr.Expression, true);
            }
            else if (directionExpr.Direction == FieldDirection.Out)
            {
                // out parameters mean that we only write to it
                isRef = true;
                paramExprInfo = RuleExpressionWalker.Validate(validation, directionExpr.Expression, true);
            }
            else
            {
                // other parameters are treated as in, so we need to be able to read them
                isRef = false;
                paramExprInfo = RuleExpressionWalker.Validate(validation, directionExpr.Expression, false);
            }
            if (paramExprInfo == null)
                return null;

            // determine it's type
            Type parameterType = paramExprInfo.ExpressionType;
            if (parameterType == null)
                return null;

            if (parameterType != typeof(NullLiteral))
            {
                // adjust type if necessary
                if (isRef && !parameterType.IsByRef)
                    parameterType = parameterType.MakeByRefType();
            }
            return new RuleExpressionInfo(parameterType);
        }

        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            CodeDirectionExpression directionExpr = (CodeDirectionExpression)expression;
            CodeExpression paramExpr = directionExpr.Expression;

            bool argIsWritten = false;
            bool argIsRead = true;
            RulePathQualifier argQualifier = null;
            switch (directionExpr.Direction)
            {
                case FieldDirection.In:
                    // We assume that all children (* suffix) of this argument can be read.
                    argIsWritten = false;
                    argIsRead = true;
                    argQualifier = new RulePathQualifier("*", null);
                    break;

                case FieldDirection.Ref:
                    // When this happens in a condition, we treat this like an "in" (above): all
                    // children (* suffix) of this argument are read.  When this happens in an
                    // action, we treat this like an "out" (below):  we assume this argument is
                    // modified (no suffix).
                    argIsWritten = true;
                    argIsRead = true;
                    argQualifier = analysis.ForWrites ? null : new RulePathQualifier("*", null);
                    break;

                case FieldDirection.Out:
                    // We assume that this argument is modified (no suffix).
                    argIsWritten = true;
                    argIsRead = false;
                    argQualifier = null;
                    break;
            }

            RuleExpressionWalker.AnalyzeUsage(analysis, paramExpr, argIsRead, argIsWritten, argQualifier);
        }

        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            // For evaluation purposes, ignore the direction.  It is handled specifically in the
            // method invoke Evaluate method.
            CodeDirectionExpression directionExpr = (CodeDirectionExpression)expression;
            return RuleExpressionWalker.Evaluate(execution, directionExpr.Expression);
        }

        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            CodeDirectionExpression directionExpr = (CodeDirectionExpression)expression;

            string direction = null;
            if (directionExpr.Direction == FieldDirection.Out)
                direction = "out ";
            else if (directionExpr.Direction == FieldDirection.Ref)
                direction = "ref ";

            if (direction != null)
                stringBuilder.Append(direction);

            RuleExpressionWalker.Decompile(stringBuilder, directionExpr.Expression, directionExpr);
        }

        internal override CodeExpression Clone(CodeExpression expression)
        {
            CodeDirectionExpression directionExpr = (CodeDirectionExpression)expression;
            CodeDirectionExpression newDirection = new CodeDirectionExpression();
            newDirection.Direction = directionExpr.Direction;
            newDirection.Expression = RuleExpressionWalker.Clone(directionExpr.Expression);
            return newDirection;
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            CodeDirectionExpression directionExpr = (CodeDirectionExpression)expression;
            CodeDirectionExpression newDirection = (CodeDirectionExpression)comperand;
            return (directionExpr.Direction == newDirection.Direction &&
                RuleExpressionWalker.Match(directionExpr.Expression, newDirection.Expression));
        }
    }

    #endregion

    #region Type Reference expression

    // CodeTypeReferenceExpression
    internal class TypeReferenceExpression : RuleExpressionInternal
    {
        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            CodeTypeReferenceExpression typeRefExpr = (CodeTypeReferenceExpression)expression;

            if (typeRefExpr.Type == null)
            {
                ValidationError error = new ValidationError(Messages.NullTypeType, ErrorNumbers.Error_ParameterNotSet);
                error.UserData[RuleUserDataKeys.ErrorObject] = typeRefExpr;
                validation.Errors.Add(error);
                return null;
            }

            if (isWritten)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.CannotWriteToExpression, typeof(CodeTypeReferenceExpression).ToString());
                ValidationError error = new ValidationError(message, ErrorNumbers.Error_InvalidAssignTarget);
                error.UserData[RuleUserDataKeys.ErrorObject] = typeRefExpr;
                validation.Errors.Add(error);
                return null;
            }

            Type resultType = validation.ResolveType(typeRefExpr.Type);

            return new RuleExpressionInfo(resultType);
        }

        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            // These introduce no interesting dependencies or side-effects.
        }

        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            // Type references don't evaluate to any value.
            return new RuleLiteralResult(null);
        }

        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            CodeTypeReferenceExpression typeRefExpr = (CodeTypeReferenceExpression)expression;
            RuleDecompiler.DecompileType(stringBuilder, typeRefExpr.Type);
        }

        internal override CodeExpression Clone(CodeExpression expression)
        {
            CodeTypeReferenceExpression typeRefExpr = (CodeTypeReferenceExpression)expression;
            CodeTypeReferenceExpression newType = new CodeTypeReferenceExpression(CloneType(typeRefExpr.Type));
            return newType;
        }

        static internal CodeTypeReference CloneType(CodeTypeReference oldType)
        {
            if (oldType == null)
                return null;

            CodeTypeReference newType = new CodeTypeReference();
            newType.ArrayElementType = CloneType(oldType.ArrayElementType);
            newType.ArrayRank = oldType.ArrayRank;
            newType.BaseType = oldType.BaseType;
            foreach (CodeTypeReference typeReference in oldType.TypeArguments)
                newType.TypeArguments.Add(CloneType(typeReference));

            ConditionHelper.CloneUserData(oldType, newType);

            return newType;
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            CodeTypeReferenceExpression typeRefExpr = (CodeTypeReferenceExpression)expression;
            CodeTypeReferenceExpression newType = (CodeTypeReferenceExpression)comperand;
            return MatchType(typeRefExpr.Type, newType.Type);
        }

        static internal bool MatchType(CodeTypeReference typeRef1, CodeTypeReference typeRef2)
        {
            if (typeRef1.BaseType != typeRef2.BaseType)
                return false;

            if (typeRef1.TypeArguments.Count != typeRef2.TypeArguments.Count)
                return false;
            for (int i = 0; i < typeRef1.TypeArguments.Count; ++i)
            {
                CodeTypeReference trArg1 = typeRef1.TypeArguments[i];
                CodeTypeReference trArg2 = typeRef2.TypeArguments[i];

                if (!MatchType(trArg1, trArg2))
                    return false;
            }

            return true;
        }
    }

    #endregion

    #region Cast expression

    // CodeCastExpression
    internal class CastExpression : RuleExpressionInternal
    {
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            string message;

            CodeCastExpression castExpr = (CodeCastExpression)expression;

            if (isWritten)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.CannotWriteToExpression, typeof(CodeCastExpression).ToString());
                ValidationError error = new ValidationError(message, ErrorNumbers.Error_InvalidAssignTarget);
                error.UserData[RuleUserDataKeys.ErrorObject] = castExpr;
                validation.Errors.Add(error);
                return null;
            }

            if (castExpr.Expression == null)
            {
                ValidationError error = new ValidationError(Messages.NullCastExpr, ErrorNumbers.Error_ParameterNotSet);
                error.UserData[RuleUserDataKeys.ErrorObject] = castExpr;
                validation.Errors.Add(error);
                return null;
            }

            if (castExpr.Expression is CodeTypeReferenceExpression)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.CodeExpressionNotHandled, castExpr.Expression.GetType().FullName);
                ValidationError error = new ValidationError(message, ErrorNumbers.Error_CodeExpressionNotHandled);
                error.UserData[RuleUserDataKeys.ErrorObject] = castExpr.Expression;
                validation.AddError(error);
                return null;
            }

            if (castExpr.TargetType == null)
            {
                ValidationError error = new ValidationError(Messages.NullCastType, ErrorNumbers.Error_ParameterNotSet);
                error.UserData[RuleUserDataKeys.ErrorObject] = castExpr;
                validation.Errors.Add(error);
                return null;
            }

            // Figure out the operand type.
            RuleExpressionInfo operandInfo = RuleExpressionWalker.Validate(validation, castExpr.Expression, false);
            if (operandInfo == null)
                return null;
            Type fromType = operandInfo.ExpressionType;

            Type toType = validation.ResolveType(castExpr.TargetType);
            if (toType == null)
                return null;

            if (fromType == typeof(NullLiteral))
            {
                // Casting from null value.
                if (ConditionHelper.IsNonNullableValueType(toType))
                {
                    message = string.Format(CultureInfo.CurrentCulture, Messages.CastOfNullInvalid, RuleDecompiler.DecompileType(toType));
                    ValidationError error = new ValidationError(message, ErrorNumbers.Error_ParameterNotSet);
                    error.UserData[RuleUserDataKeys.ErrorObject] = castExpr;
                    validation.Errors.Add(error);
                    return null;
                }
            }
            else
            {
                // Unwrap nullables to make life easy.
                Type fromType2 = fromType;
                if (ConditionHelper.IsNullableValueType(fromType2))
                    fromType2 = fromType2.GetGenericArguments()[0];

                Type toType2 = toType;
                if (ConditionHelper.IsNullableValueType(toType2))
                    toType2 = toType2.GetGenericArguments()[0];

                bool canConvert = false;
                if (fromType2.IsValueType && toType2.IsValueType)
                {
                    // Convert.ChangeType doesn't handle enum <--> numeric
                    // and float/double/decimal <--> char, which are allowed
                    if (fromType2.IsEnum)
                    {
                        canConvert = (toType2.IsEnum) || IsNumeric(toType2);
                    }
                    else if (toType2.IsEnum)
                    {
                        // don't need to check fromType for enum since it's handled above
                        canConvert = IsNumeric(fromType2);
                    }
                    else if (fromType2 == typeof(char))
                    {
                        canConvert = IsNumeric(toType2);
                    }
                    else if (toType2 == typeof(char))
                    {
                        canConvert = IsNumeric(fromType2);
                    }
                    else if (fromType2.IsPrimitive && toType2.IsPrimitive)
                    {
                        try
                        {
                            // note: this also allows bool <--> numeric conversions
                            object fromValueDefault = Activator.CreateInstance(fromType2);
                            Convert.ChangeType(fromValueDefault, toType2, CultureInfo.CurrentCulture);
                            canConvert = true;
                        }
                        catch (Exception)
                        {
                            canConvert = false;
                        }
                    }
                }

                if (!canConvert)
                {
                    // We can cast up or down an inheritence hierarchy,
                    // as well as support explicit and implicit overrides
                    ValidationError error;
                    canConvert = RuleValidation.ExplicitConversionSpecified(fromType, toType, out error);
                    if (error != null)
                    {
                        error.UserData[RuleUserDataKeys.ErrorObject] = castExpr;
                        validation.Errors.Add(error);
                        return null;
                    }
                }

                if (!canConvert)
                {
                    message = string.Format(CultureInfo.CurrentCulture, Messages.CastIncompatibleTypes, RuleDecompiler.DecompileType(fromType), RuleDecompiler.DecompileType(toType));
                    ValidationError error = new ValidationError(message, ErrorNumbers.Error_ParameterNotSet);
                    error.UserData[RuleUserDataKeys.ErrorObject] = castExpr;
                    validation.Errors.Add(error);
                    return null;
                }
            }

            return new RuleExpressionInfo(toType);
        }

        private static bool IsNumeric(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Char:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
                default:
                    return false;
            }
        }

        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            // Just analyze the child.
            CodeCastExpression castExpr = (CodeCastExpression)expression;
            RuleExpressionWalker.AnalyzeUsage(analysis, castExpr.Expression, true, false, null);
        }

        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            string message;

            CodeCastExpression castExpr = (CodeCastExpression)expression;

            // Evaluate the operand.
            object operandValue = RuleExpressionWalker.Evaluate(execution, castExpr.Expression).Value;

            // Get the cast-to type.
            RuleExpressionInfo castExprInfo = execution.Validation.ExpressionInfo(castExpr);
            if (castExprInfo == null)  // Oops, someone forgot to validate.
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.ExpressionNotValidated);
                InvalidOperationException exception = new InvalidOperationException(message);
                exception.Data[RuleUserDataKeys.ErrorObject] = castExpr;
                throw exception;
            }
            Type toType = castExprInfo.ExpressionType;

            // Handle null operand result.
            if (operandValue == null)
            {
                // Here we are casting null to something. If it is a value type we can't do it.
                if (ConditionHelper.IsNonNullableValueType(toType))
                {
                    message = string.Format(CultureInfo.CurrentCulture, Messages.CastIncompatibleTypes, Messages.NullValue, RuleDecompiler.DecompileType(toType));
                    RuleEvaluationException exception = new RuleEvaluationException(message);
                    exception.Data[RuleUserDataKeys.ErrorObject] = castExpr;
                    throw exception;
                }
                // If it's not a value type, null is good.
            }
            else
            {
                Type operandType = execution.Validation.ExpressionInfo(castExpr.Expression).ExpressionType;
                operandValue = Executor.AdjustTypeWithCast(operandType, operandValue, toType);
            }

            return new RuleLiteralResult(operandValue);
        }

        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            CodeCastExpression castExpr = (CodeCastExpression)expression;

            CodeExpression targetObject = castExpr.Expression;
            if (targetObject == null)
            {
                RuleEvaluationException exception = new RuleEvaluationException(Messages.NullCastExpr);
                exception.Data[RuleUserDataKeys.ErrorObject] = castExpr;
                throw exception;
            }

            if (castExpr.TargetType == null)
            {
                RuleEvaluationException exception = new RuleEvaluationException(Messages.NullCastType);
                exception.Data[RuleUserDataKeys.ErrorObject] = castExpr;
                throw exception;
            }

            bool mustParenthesize = RuleDecompiler.MustParenthesize(castExpr, parentExpression);
            if (mustParenthesize)
                stringBuilder.Append("(");

            stringBuilder.Append("(");
            RuleDecompiler.DecompileType(stringBuilder, castExpr.TargetType);
            stringBuilder.Append(")");
            RuleExpressionWalker.Decompile(stringBuilder, targetObject, castExpr);

            if (mustParenthesize)
                stringBuilder.Append(")");
        }

        internal override CodeExpression Clone(CodeExpression expression)
        {
            CodeCastExpression castExpr = (CodeCastExpression)expression;
            CodeCastExpression newCast = new CodeCastExpression();
            newCast.TargetType = TypeReferenceExpression.CloneType(castExpr.TargetType);
            newCast.Expression = RuleExpressionWalker.Clone(castExpr.Expression);
            return newCast;
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            CodeCastExpression castExpr = (CodeCastExpression)expression;
            CodeCastExpression castComperand = (CodeCastExpression)comperand;

            return TypeReferenceExpression.MatchType(castExpr.TargetType, castComperand.TargetType) &&
                RuleExpressionWalker.Match(castExpr.Expression, castComperand.Expression);
        }
    }

    #endregion

    #region Indexer Expression (indexer properties)

    // CodeIndexerExpression
    internal class IndexerPropertyExpression : RuleExpressionInternal
    {
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            string message;
            ValidationError error = null;
            RulePropertyExpressionInfo propExprInfo = null;
            bool includeNonPublic = false;
            Type targetType = null;

            CodeIndexerExpression indexerExpr = (CodeIndexerExpression)expression;

            CodeExpression targetObject = indexerExpr.TargetObject;
            if (targetObject == null)
            {
                error = new ValidationError(Messages.NullIndexerTarget, ErrorNumbers.Error_ParameterNotSet);
                error.UserData[RuleUserDataKeys.ErrorObject] = indexerExpr;
                validation.Errors.Add(error);
                return null;
            }

            if (targetObject is CodeTypeReferenceExpression)
            {
                error = new ValidationError(Messages.IndexersCannotBeStatic, ErrorNumbers.Error_ParameterNotSet);
                error.UserData[RuleUserDataKeys.ErrorObject] = indexerExpr;
                validation.Errors.Add(error);
                return null;
            }

            if (indexerExpr.Indices == null || indexerExpr.Indices.Count == 0)
            {
                error = new ValidationError(Messages.MissingIndexExpressions, ErrorNumbers.Error_ParameterNotSet);
                error.UserData[RuleUserDataKeys.ErrorObject] = indexerExpr;
                validation.Errors.Add(error);
                return null;
            }

            try
            {
                // Early exit from this if a cycle is detected.
                if (!validation.PushParentExpression(indexerExpr))
                    return null;

                RuleExpressionInfo targetExprInfo = RuleExpressionWalker.Validate(validation, indexerExpr.TargetObject, false);
                if (targetExprInfo == null)     // error occurred, so simply return
                    return null;

                targetType = targetExprInfo.ExpressionType;
                if (targetType == null)
                    return null;

                // if an error occurred (targetType == null), continue on to validate the arguments
                if (targetType == typeof(NullLiteral))
                {
                    message = string.Format(CultureInfo.CurrentCulture, Messages.NullIndexerTarget);
                    error = new ValidationError(message, ErrorNumbers.Error_ParameterNotSet);
                    error.UserData[RuleUserDataKeys.ErrorObject] = indexerExpr;
                    validation.Errors.Add(error);
                    targetType = null; // force exit after validating the arguments
                }

                List<CodeExpression> argExprs = new List<CodeExpression>();

                bool hasInvalidArgument = false;
                for (int i = 0; i < indexerExpr.Indices.Count; ++i)
                {
                    CodeExpression argExpr = indexerExpr.Indices[i];
                    if (argExpr == null)
                    {
                        error = new ValidationError(Messages.NullIndexExpression, ErrorNumbers.Error_ParameterNotSet);
                        error.UserData[RuleUserDataKeys.ErrorObject] = indexerExpr;
                        validation.Errors.Add(error);
                        hasInvalidArgument = true;
                    }
                    else
                    {
                        CodeDirectionExpression argDirection = argExpr as CodeDirectionExpression;
                        if (argDirection != null && argDirection.Direction != FieldDirection.In)
                        {
                            // No "ref" or "out" arguments are allowed on indexer arguments.
                            error = new ValidationError(Messages.IndexerArgCannotBeRefOrOut, ErrorNumbers.Error_IndexerArgCannotBeRefOrOut);
                            error.UserData[RuleUserDataKeys.ErrorObject] = indexerExpr;
                            validation.Errors.Add(error);
                            hasInvalidArgument = true;
                        }

                        if (argExpr is CodeTypeReferenceExpression)
                        {
                            message = string.Format(CultureInfo.CurrentCulture, Messages.CodeExpressionNotHandled, argExpr.GetType().FullName);
                            error = new ValidationError(message, ErrorNumbers.Error_CodeExpressionNotHandled);
                            error.UserData[RuleUserDataKeys.ErrorObject] = argExpr;
                            validation.AddError(error);
                            hasInvalidArgument = true;
                        }

                        // Validate the argument.
                        RuleExpressionInfo argExprInfo = RuleExpressionWalker.Validate(validation, argExpr, false);
                        if (argExprInfo == null)
                            hasInvalidArgument = true;
                        else
                            argExprs.Add(argExpr);
                    }
                }

                // Stop further validation if there was a problem with the target expression.
                if (targetType == null)
                    return null;

                // Stop further validation if there was a problem with any of the arguments.
                if (hasInvalidArgument)
                    return null;

                BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
                if (validation.AllowInternalMembers(targetType))
                {
                    bindingFlags |= BindingFlags.NonPublic;
                    includeNonPublic = true;
                }

                // Everything okay so far, try to resolve the method.
                propExprInfo = validation.ResolveIndexerProperty(targetType, bindingFlags, argExprs, out error);
                if (propExprInfo == null)
                {
                    error.UserData[RuleUserDataKeys.ErrorObject] = indexerExpr;
                    validation.Errors.Add(error);
                    return null;
                }
            }
            finally
            {
                validation.PopParentExpression();
            }

            PropertyInfo pi = propExprInfo.PropertyInfo;

            MethodInfo accessorMethod = isWritten ? pi.GetSetMethod(includeNonPublic) : pi.GetGetMethod(includeNonPublic);
            if (accessorMethod == null)
            {
                string baseMessage = isWritten ? Messages.UnknownPropertySet : Messages.UnknownPropertyGet;
                message = string.Format(CultureInfo.CurrentCulture, baseMessage, pi.Name, RuleDecompiler.DecompileType(targetType));
                error = new ValidationError(message, ErrorNumbers.Error_CannotResolveMember);
                error.UserData[RuleUserDataKeys.ErrorObject] = indexerExpr;
                validation.Errors.Add(error);
                return null;
            }

            if (!validation.ValidateMemberAccess(targetObject, targetType, accessorMethod, pi.Name, indexerExpr))
                return null;

            // Validate any RuleAttributes, if present.
            object[] attrs = pi.GetCustomAttributes(typeof(RuleAttribute), true);
            if (attrs != null && attrs.Length > 0)
            {
                Stack<MemberInfo> methodStack = new Stack<MemberInfo>();
                methodStack.Push(pi);

                bool allAttributesValid = true;
                foreach (RuleAttribute ruleAttr in attrs)
                {
                    if (!ruleAttr.Validate(validation, pi, targetType, pi.GetIndexParameters()))
                        allAttributesValid = false;
                }

                methodStack.Pop();

                if (!allAttributesValid)
                    return null;
            }

            return propExprInfo;
        }


        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            string message;

            CodeIndexerExpression indexerExpr = (CodeIndexerExpression)expression;

            // Evaluate the target object and get its type.
            CodeExpression targetObject = indexerExpr.TargetObject;
            RuleExpressionInfo targetExprInfo = analysis.Validation.ExpressionInfo(targetObject);
            if (targetExprInfo == null)  // Oops, someone forgot to validate.
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.ExpressionNotValidated);
                InvalidOperationException exception = new InvalidOperationException(message);
                exception.Data[RuleUserDataKeys.ErrorObject] = targetObject;
                throw exception;
            }

            // Get the property info from the validator so we can look for [RuleRead] and [RuleWrite] attributes.
            RulePropertyExpressionInfo propExprInfo = analysis.Validation.ExpressionInfo(indexerExpr) as RulePropertyExpressionInfo;
            if (propExprInfo == null)  // Oops, someone forgot to validate.
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.ExpressionNotValidated);
                InvalidOperationException exception = new InvalidOperationException(message);
                exception.Data[RuleUserDataKeys.ErrorObject] = indexerExpr;
                throw exception;
            }

            PropertyInfo pi = propExprInfo.PropertyInfo;

            // Look for RuleAttribute's on the invoked indexer property.
            List<CodeExpression> attributedExprs = new List<CodeExpression>();
            analysis.AnalyzeRuleAttributes(pi, targetObject, qualifier, indexerExpr.Indices, pi.GetIndexParameters(), attributedExprs);

            // See if the target object needs default analysis.
            if (!attributedExprs.Contains(targetObject))
            {
                // The property had no [RuleRead] or [RuleWrite] attributes.  The target object is read or
                // written (as the case may be).

                RuleExpressionWalker.AnalyzeUsage(analysis, targetObject, isRead, isWritten, qualifier);
            }

            // See if any of the arguments need default analysis.
            for (int i = 0; i < indexerExpr.Indices.Count; ++i)
            {
                CodeExpression argExpr = indexerExpr.Indices[i];

                if (!attributedExprs.Contains(argExpr))
                {
                    // Similar to the target object, we assume that this method can reads the value
                    // of the parameter, but none of its members.
                    RuleExpressionWalker.AnalyzeUsage(analysis, argExpr, true, false, null);
                }
            }
        }

        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            string message;

            CodeIndexerExpression indexerExpr = (CodeIndexerExpression)expression;

            RulePropertyExpressionInfo propExprInfo = execution.Validation.ExpressionInfo(indexerExpr) as RulePropertyExpressionInfo;
            if (propExprInfo == null)  // Oops, someone forgot to validate.
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.ExpressionNotValidated);
                InvalidOperationException exception = new InvalidOperationException(message);
                exception.Data[RuleUserDataKeys.ErrorObject] = indexerExpr;
                throw exception;
            }

            PropertyInfo pi = propExprInfo.PropertyInfo;

            // Evaluate the target...
            object target = RuleExpressionWalker.Evaluate(execution, indexerExpr.TargetObject).Value;

            if (target == null)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.TargetEvaluatedNullIndexer);
                RuleEvaluationException exception = new RuleEvaluationException(message);
                exception.Data[RuleUserDataKeys.ErrorObject] = indexerExpr;
                throw exception;
            }

            // Evaluate the index arguments.
            int actualArgCount = indexerExpr.Indices.Count;
            ParameterInfo[] parmInfos = pi.GetIndexParameters();
            object[] indexArgs = new object[parmInfos.Length];

            int numFixedParameters = parmInfos.Length;
            if (propExprInfo.NeedsParamsExpansion)
                numFixedParameters -= 1;

            int i;
            for (i = 0; i < numFixedParameters; ++i)
            {
                Type argType = execution.Validation.ExpressionInfo(indexerExpr.Indices[i]).ExpressionType;
                RuleExpressionResult argResult = RuleExpressionWalker.Evaluate(execution, indexerExpr.Indices[i]);
                indexArgs[i] = Executor.AdjustType(argType, argResult.Value, parmInfos[i].ParameterType);
            }

            if (numFixedParameters < actualArgCount)
            {
                // This target indexer had a params array, and we are calling it with an
                // expanded parameter list.  E.g.,
                //      int this[int x, params string[] y]
                // with the invocation:
                //      x.y[5, "crud", "kreeble", "glorp"]
                // We need to translate this to:
                //      x.y[5, new string[] { "crud", "kreeble", "glorp" }]

                ParameterInfo lastParamInfo = parmInfos[numFixedParameters];

                Type arrayType = lastParamInfo.ParameterType;
                System.Diagnostics.Debug.Assert(arrayType.IsArray);
                Type elementType = arrayType.GetElementType();

                Array paramsArray = (Array)arrayType.InvokeMember(arrayType.Name, BindingFlags.CreateInstance, null, null, new object[] { actualArgCount - i }, CultureInfo.CurrentCulture);
                for (; i < actualArgCount; ++i)
                {
                    Type argType = execution.Validation.ExpressionInfo(indexerExpr.Indices[i]).ExpressionType;
                    RuleExpressionResult argResult = RuleExpressionWalker.Evaluate(execution, indexerExpr.Indices[i]);
                    paramsArray.SetValue(Executor.AdjustType(argType, argResult.Value, elementType), i - numFixedParameters);
                }

                indexArgs[numFixedParameters] = paramsArray;
            }

            RulePropertyResult result = new RulePropertyResult(pi, target, indexArgs);
            return result;
        }

        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            string message;

            CodeIndexerExpression indexerExpr = (CodeIndexerExpression)expression;

            CodeExpression targetObject = indexerExpr.TargetObject;
            if (targetObject == null)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.NullIndexerTarget);
                RuleEvaluationException exception = new RuleEvaluationException(message);
                exception.Data[RuleUserDataKeys.ErrorObject] = indexerExpr;
                throw exception;
            }

            if (indexerExpr.Indices == null || indexerExpr.Indices.Count == 0)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.MissingIndexExpressions);
                RuleEvaluationException exception = new RuleEvaluationException(message);
                exception.Data[RuleUserDataKeys.ErrorObject] = indexerExpr;
                throw exception;
            }

            RuleExpressionWalker.Decompile(stringBuilder, targetObject, indexerExpr);
            stringBuilder.Append('[');
            RuleExpressionWalker.Decompile(stringBuilder, indexerExpr.Indices[0], null);
            for (int i = 1; i < indexerExpr.Indices.Count; ++i)
            {
                stringBuilder.Append(", ");
                RuleExpressionWalker.Decompile(stringBuilder, indexerExpr.Indices[i], null);
            }
            stringBuilder.Append(']');
        }

        internal override CodeExpression Clone(CodeExpression expression)
        {
            CodeIndexerExpression indexerExpr = (CodeIndexerExpression)expression;

            CodeExpression targetObject = RuleExpressionWalker.Clone(indexerExpr.TargetObject);

            CodeExpression[] indices = new CodeExpression[indexerExpr.Indices.Count];
            for (int i = 0; i < indices.Length; ++i)
                indices[i] = RuleExpressionWalker.Clone(indexerExpr.Indices[i]);

            CodeIndexerExpression newIndexer = new CodeIndexerExpression(targetObject, indices);
            return newIndexer;
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            CodeIndexerExpression indexerExpr = (CodeIndexerExpression)expression;

            CodeIndexerExpression indexerComperand = (CodeIndexerExpression)comperand;
            if (!RuleExpressionWalker.Match(indexerExpr.TargetObject, indexerComperand.TargetObject))
                return false;

            if (indexerExpr.Indices.Count != indexerComperand.Indices.Count)
                return false;

            for (int i = 0; i < indexerExpr.Indices.Count; ++i)
            {
                if (!RuleExpressionWalker.Match(indexerExpr.Indices[i], indexerComperand.Indices[i]))
                    return false;
            }

            return true;
        }
    }

    #endregion

    #region Array Indexer Expression (indexer properties)

    // CodeArrayIndexerExpression
    internal class ArrayIndexerExpression : RuleExpressionInternal
    {
        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            string message;
            ValidationError error = null;
            Type targetType = null;

            CodeArrayIndexerExpression arrayIndexerExpr = (CodeArrayIndexerExpression)expression;

            CodeExpression targetObject = arrayIndexerExpr.TargetObject;
            if (targetObject == null)
            {
                error = new ValidationError(Messages.NullIndexerTarget, ErrorNumbers.Error_ParameterNotSet);
                error.UserData[RuleUserDataKeys.ErrorObject] = arrayIndexerExpr;
                validation.Errors.Add(error);
                return null;
            }

            if (targetObject is CodeTypeReferenceExpression)
            {
                error = new ValidationError(Messages.IndexersCannotBeStatic, ErrorNumbers.Error_ParameterNotSet);
                error.UserData[RuleUserDataKeys.ErrorObject] = arrayIndexerExpr;
                validation.Errors.Add(error);
                return null;
            }

            if (arrayIndexerExpr.Indices == null || arrayIndexerExpr.Indices.Count == 0)
            {
                error = new ValidationError(Messages.MissingIndexExpressions, ErrorNumbers.Error_ParameterNotSet);
                error.UserData[RuleUserDataKeys.ErrorObject] = arrayIndexerExpr;
                validation.Errors.Add(error);
                return null;
            }

            try
            {
                // Early exit from this if a cycle is detected.
                if (!validation.PushParentExpression(arrayIndexerExpr))
                    return null;

                RuleExpressionInfo targetExprInfo = RuleExpressionWalker.Validate(validation, arrayIndexerExpr.TargetObject, false);
                if (targetExprInfo == null)     // error occurred, so simply return
                    return null;

                targetType = targetExprInfo.ExpressionType;
                if (targetType == null)
                    return null;

                // if an error occurred (targetType == null), continue on to validate the arguments
                if (targetType == typeof(NullLiteral))
                {
                    error = new ValidationError(Messages.NullIndexerTarget, ErrorNumbers.Error_ParameterNotSet);
                    error.UserData[RuleUserDataKeys.ErrorObject] = arrayIndexerExpr;
                    validation.Errors.Add(error);
                    return null;
                }

                // The target type better be an array.
                if (!targetType.IsArray)
                {
                    message = string.Format(CultureInfo.CurrentCulture, Messages.CannotIndexType, RuleDecompiler.DecompileType(targetType));
                    error = new ValidationError(message, ErrorNumbers.Error_CannotIndexType);
                    error.UserData[RuleUserDataKeys.ErrorObject] = arrayIndexerExpr;
                    validation.Errors.Add(error);
                    return null;
                }

                int rank = targetType.GetArrayRank();
                if (arrayIndexerExpr.Indices.Count != rank)
                {
                    message = string.Format(CultureInfo.CurrentCulture, Messages.ArrayIndexBadRank, rank);
                    error = new ValidationError(message, ErrorNumbers.Error_ArrayIndexBadRank);
                    error.UserData[RuleUserDataKeys.ErrorObject] = arrayIndexerExpr;
                    validation.Errors.Add(error);
                    return null;
                }

                bool hasInvalidArgument = false;
                for (int i = 0; i < arrayIndexerExpr.Indices.Count; ++i)
                {
                    CodeExpression argExpr = arrayIndexerExpr.Indices[i];
                    if (argExpr == null)
                    {
                        error = new ValidationError(Messages.NullIndexExpression, ErrorNumbers.Error_ParameterNotSet);
                        error.UserData[RuleUserDataKeys.ErrorObject] = arrayIndexerExpr;
                        validation.Errors.Add(error);
                        hasInvalidArgument = true;
                    }
                    else
                    {
                        CodeDirectionExpression argDirection = argExpr as CodeDirectionExpression;
                        if (argDirection != null)
                        {
                            // No "ref" or "out" arguments are allowed on indexer arguments.
                            error = new ValidationError(Messages.IndexerArgCannotBeRefOrOut, ErrorNumbers.Error_IndexerArgCannotBeRefOrOut);
                            error.UserData[RuleUserDataKeys.ErrorObject] = argExpr;
                            validation.Errors.Add(error);
                            hasInvalidArgument = true;
                        }

                        if (argExpr is CodeTypeReferenceExpression)
                        {
                            message = string.Format(CultureInfo.CurrentCulture, Messages.CodeExpressionNotHandled, argExpr.GetType().FullName);
                            error = new ValidationError(message, ErrorNumbers.Error_CodeExpressionNotHandled);
                            error.UserData[RuleUserDataKeys.ErrorObject] = argExpr;
                            validation.AddError(error);
                            hasInvalidArgument = true;
                        }

                        // Validate the argument.
                        RuleExpressionInfo argExprInfo = RuleExpressionWalker.Validate(validation, argExpr, false);
                        if (argExprInfo != null)
                        {
                            Type argType = argExprInfo.ExpressionType;
                            TypeCode argTypeCode = Type.GetTypeCode(argType);

                            // Any type that is, or can be converted to: int or long.
                            switch (argTypeCode)
                            {
                                case TypeCode.Byte:
                                case TypeCode.Char:
                                case TypeCode.Int16:
                                case TypeCode.Int32:
                                case TypeCode.Int64:
                                case TypeCode.SByte:
                                case TypeCode.UInt16:
                                    break;

                                default:
                                    message = string.Format(CultureInfo.CurrentCulture, Messages.ArrayIndexBadType, RuleDecompiler.DecompileType(argType));
                                    error = new ValidationError(message, ErrorNumbers.Error_ArrayIndexBadType);
                                    error.UserData[RuleUserDataKeys.ErrorObject] = argExpr;
                                    validation.Errors.Add(error);
                                    hasInvalidArgument = true;
                                    break;
                            }
                        }
                        else
                        {
                            hasInvalidArgument = true;
                        }
                    }
                }

                // Stop further validation if there was a problem with any of the arguments.
                if (hasInvalidArgument)
                    return null;
            }
            finally
            {
                validation.PopParentExpression();
            }

            // The result type is this array's element type.
            return new RuleExpressionInfo(targetType.GetElementType());
        }


        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            // Analyze the target object, flowing down the qualifier from above.  An expression
            // like:
            //      this.a.b[2,3].c[4].d[5] = 99;
            // should produce a path similar to:
            //      this/a/b/c/d

            CodeArrayIndexerExpression arrayIndexerExpr = (CodeArrayIndexerExpression)expression;
            RuleExpressionWalker.AnalyzeUsage(analysis, arrayIndexerExpr.TargetObject, isRead, isWritten, qualifier);

            // Analyze the indexer arguments.  They are read.
            for (int i = 0; i < arrayIndexerExpr.Indices.Count; ++i)
                RuleExpressionWalker.AnalyzeUsage(analysis, arrayIndexerExpr.Indices[i], true, false, null);
        }

        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            CodeArrayIndexerExpression arrayIndexerExpr = (CodeArrayIndexerExpression)expression;

            // Evaluate the target...
            object target = RuleExpressionWalker.Evaluate(execution, arrayIndexerExpr.TargetObject).Value;

            if (target == null)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.TargetEvaluatedNullIndexer);
                RuleEvaluationException exception = new RuleEvaluationException(message);
                exception.Data[RuleUserDataKeys.ErrorObject] = arrayIndexerExpr;
                throw exception;
            }

            // Evaluate the index arguments (converting them to "longs")
            int actualArgCount = arrayIndexerExpr.Indices.Count;
            long[] indexArgs = new long[actualArgCount];

            for (int i = 0; i < actualArgCount; ++i)
            {
                Type argType = execution.Validation.ExpressionInfo(arrayIndexerExpr.Indices[i]).ExpressionType;
                object argValue = RuleExpressionWalker.Evaluate(execution, arrayIndexerExpr.Indices[i]).Value;
                indexArgs[i] = (long)Executor.AdjustType(argType, argValue, typeof(long));
            }

            RuleArrayElementResult result = new RuleArrayElementResult((Array)target, indexArgs);
            return result;
        }

        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            string message;

            CodeArrayIndexerExpression arrayIndexerExpr = (CodeArrayIndexerExpression)expression;

            CodeExpression targetObject = arrayIndexerExpr.TargetObject;
            if (targetObject == null)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.NullIndexerTarget);
                RuleEvaluationException exception = new RuleEvaluationException(message);
                exception.Data[RuleUserDataKeys.ErrorObject] = arrayIndexerExpr;
                throw exception;
            }

            if (arrayIndexerExpr.Indices == null || arrayIndexerExpr.Indices.Count == 0)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.MissingIndexExpressions);
                RuleEvaluationException exception = new RuleEvaluationException(message);
                exception.Data[RuleUserDataKeys.ErrorObject] = arrayIndexerExpr;
                throw exception;
            }

            RuleExpressionWalker.Decompile(stringBuilder, targetObject, arrayIndexerExpr);
            stringBuilder.Append('[');
            RuleExpressionWalker.Decompile(stringBuilder, arrayIndexerExpr.Indices[0], null);
            for (int i = 1; i < arrayIndexerExpr.Indices.Count; ++i)
            {
                stringBuilder.Append(", ");
                RuleExpressionWalker.Decompile(stringBuilder, arrayIndexerExpr.Indices[i], null);
            }
            stringBuilder.Append(']');
        }

        internal override CodeExpression Clone(CodeExpression expression)
        {
            CodeArrayIndexerExpression arrayIndexerExpr = (CodeArrayIndexerExpression)expression;

            CodeExpression targetObject = RuleExpressionWalker.Clone(arrayIndexerExpr.TargetObject);

            CodeExpression[] indices = new CodeExpression[arrayIndexerExpr.Indices.Count];
            for (int i = 0; i < indices.Length; ++i)
                indices[i] = RuleExpressionWalker.Clone(arrayIndexerExpr.Indices[i]);

            CodeArrayIndexerExpression newIndexer = new CodeArrayIndexerExpression(targetObject, indices);
            return newIndexer;
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            CodeArrayIndexerExpression arrayIndexerExpr = (CodeArrayIndexerExpression)expression;

            CodeArrayIndexerExpression indexerComperand = (CodeArrayIndexerExpression)comperand;
            if (!RuleExpressionWalker.Match(arrayIndexerExpr.TargetObject, indexerComperand.TargetObject))
                return false;

            if (arrayIndexerExpr.Indices.Count != indexerComperand.Indices.Count)
                return false;

            for (int i = 0; i < arrayIndexerExpr.Indices.Count; ++i)
            {
                if (!RuleExpressionWalker.Match(arrayIndexerExpr.Indices[i], indexerComperand.Indices[i]))
                    return false;
            }

            return true;
        }
    }

    #endregion

    #region Object Create expression
    internal class ObjectCreateExpression : RuleExpressionInternal
    {
        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            string message;
            ValidationError error;

            CodeObjectCreateExpression createExpression = (CodeObjectCreateExpression)expression;

            if (isWritten)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.CannotWriteToExpression, typeof(CodeObjectCreateExpression).ToString());
                error = new ValidationError(message, ErrorNumbers.Error_InvalidAssignTarget);
                error.UserData[RuleUserDataKeys.ErrorObject] = createExpression;
                validation.Errors.Add(error);
                return null;
            }

            if (createExpression.CreateType == null)
            {
                error = new ValidationError(Messages.NullTypeType, ErrorNumbers.Error_ParameterNotSet);
                error.UserData[RuleUserDataKeys.ErrorObject] = createExpression;
                validation.Errors.Add(error);
                return null;
            }

            Type resultType = validation.ResolveType(createExpression.CreateType);
            if (resultType == null)
                return null;

            // look up parameters
            List<CodeExpression> parameters = new List<CodeExpression>();
            try
            {
                // Early exit from this if a cycle is detected.
                if (!validation.PushParentExpression(createExpression))
                    return null;

                bool hasInvalidArgument = false;
                for (int i = 0; i < createExpression.Parameters.Count; ++i)
                {
                    CodeExpression parameter = createExpression.Parameters[i];
                    if (parameter == null)
                    {
                        message = string.Format(CultureInfo.CurrentCulture, Messages.NullConstructorParameter, i.ToString(CultureInfo.CurrentCulture), RuleDecompiler.DecompileType(resultType));
                        error = new ValidationError(message, ErrorNumbers.Error_ParameterNotSet);
                        error.UserData[RuleUserDataKeys.ErrorObject] = createExpression;
                        validation.Errors.Add(error);
                        hasInvalidArgument = true;
                    }
                    else
                    {
                        RuleExpressionInfo parameterInfo = RuleExpressionWalker.Validate(validation, parameter, false);
                        if (parameterInfo == null)
                            hasInvalidArgument = true;
                        parameters.Add(parameter);
                    }
                }
                // quit if parameters not valid
                if (hasInvalidArgument)
                    return null;
            }
            finally
            {
                validation.PopParentExpression();
            }

            // see if we can find the matching constructor
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            if (validation.AllowInternalMembers(resultType))
                bindingFlags |= BindingFlags.NonPublic;

            // creating a value-type object with no parameters can always be done
            if ((resultType.IsValueType) && (parameters.Count == 0))
                return new RuleExpressionInfo(resultType);

            // error if type is an abstract type
            if (resultType.IsAbstract)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.UnknownConstructor, RuleDecompiler.DecompileType(resultType));
                error = new ValidationError(message, ErrorNumbers.Error_MethodNotExists);
                error.UserData[RuleUserDataKeys.ErrorObject] = createExpression;
                validation.Errors.Add(error);
                return null;
            }

            RuleConstructorExpressionInfo constructorInvokeInfo = validation.ResolveConstructor(resultType, bindingFlags, parameters, out error);
            if (constructorInvokeInfo == null)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.UnknownConstructor, RuleDecompiler.DecompileType(resultType));
                error = new ValidationError(message, ErrorNumbers.Error_MethodNotExists);
                error.UserData[RuleUserDataKeys.ErrorObject] = createExpression;
                validation.Errors.Add(error);
                return null;
            }

            return constructorInvokeInfo;
        }

        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            CodeObjectCreateExpression createExpression = (CodeObjectCreateExpression)expression;

            // check each parameter
            foreach (CodeExpression p in createExpression.Parameters)
            {
                RuleExpressionWalker.AnalyzeUsage(analysis, p, true, false, null);
            }
        }

        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            CodeObjectCreateExpression createExpression = (CodeObjectCreateExpression)expression;

            if (createExpression.CreateType == null)
            {
                RuleEvaluationException exception = new RuleEvaluationException(Messages.NullTypeType);
                exception.Data[RuleUserDataKeys.ErrorObject] = createExpression;
                throw exception;
            }

            RuleExpressionInfo expressionInfo = execution.Validation.ExpressionInfo(createExpression);
            if (expressionInfo == null)  // Oops, someone forgot to validate.
            {
                InvalidOperationException exception = new InvalidOperationException(Messages.ExpressionNotValidated);
                exception.Data[RuleUserDataKeys.ErrorObject] = createExpression;
                throw exception;
            }

            RuleConstructorExpressionInfo createExpressionInfo = expressionInfo as RuleConstructorExpressionInfo;
            if (createExpressionInfo == null)
            {
                // it's just a regular RuleExpressionInfo, which means this is a value-type with no parameters
                return new RuleLiteralResult(Activator.CreateInstance(expressionInfo.ExpressionType));
            }

            ConstructorInfo constructor = createExpressionInfo.ConstructorInfo;
            object[] arguments = null;
            RuleExpressionResult[] outArgumentResults = null;

            if (createExpression.Parameters != null && createExpression.Parameters.Count > 0)
            {
                int actualArgCount = createExpression.Parameters.Count;
                ParameterInfo[] parmInfos = constructor.GetParameters();

                arguments = new object[parmInfos.Length];

                int numFixedParameters = parmInfos.Length;
                if (createExpressionInfo.NeedsParamsExpansion)
                    numFixedParameters -= 1;

                int i;

                // Evaluate the fixed portion of the parameter list.
                for (i = 0; i < numFixedParameters; ++i)
                {
                    Type argType = execution.Validation.ExpressionInfo(createExpression.Parameters[i]).ExpressionType;
                    RuleExpressionResult argResult = RuleExpressionWalker.Evaluate(execution, createExpression.Parameters[i]);

                    // Special procesing of direction expressions to keep track of out arguments (& ref).
                    CodeDirectionExpression direction = createExpression.Parameters[i] as CodeDirectionExpression;
                    if (direction != null && (direction.Direction == FieldDirection.Ref || direction.Direction == FieldDirection.Out))
                    {
                        // lazy creation of fieldsToSet
                        if (outArgumentResults == null)
                            outArgumentResults = new RuleExpressionResult[actualArgCount];
                        // keep track of this out expression so we can set it later
                        outArgumentResults[i] = argResult;
                    }

                    arguments[i] = Executor.AdjustType(argType, argResult.Value, parmInfos[i].ParameterType);
                }

                if (numFixedParameters < actualArgCount)
                {
                    // This target method had a params array, and we are calling it with an
                    // expanded parameter list.  E.g.,
                    //      void foo(int x, params string[] y)
                    // with the invocation:
                    //      foo(5, "crud", "kreeble", "glorp")
                    // We need to translate this to:
                    //      foo(5, new string[] { "crud", "kreeble", "glorp" })

                    ParameterInfo lastParamInfo = parmInfos[numFixedParameters];

                    Type arrayType = lastParamInfo.ParameterType;
                    System.Diagnostics.Debug.Assert(arrayType.IsArray);
                    Type elementType = arrayType.GetElementType();

                    Array paramsArray = Array.CreateInstance(elementType, actualArgCount - i);
                    for (; i < actualArgCount; ++i)
                    {
                        Type argType = execution.Validation.ExpressionInfo(createExpression.Parameters[i]).ExpressionType;
                        RuleExpressionResult argResult = RuleExpressionWalker.Evaluate(execution, createExpression.Parameters[i]);
                        paramsArray.SetValue(Executor.AdjustType(argType, argResult.Value, elementType), i - numFixedParameters);
                    }

                    arguments[numFixedParameters] = paramsArray;
                }
            }

            object result;
            try
            {
                result = constructor.Invoke(arguments);
            }
            catch (TargetInvocationException e)
            {
                // if there is no inner exception, leave it untouched
                if (e.InnerException == null)
                    throw;
                string message = string.Format(CultureInfo.CurrentCulture,
                    Messages.Error_ConstructorInvoke,
                    RuleDecompiler.DecompileType(createExpressionInfo.ExpressionType),
                    e.InnerException.Message);
                throw new TargetInvocationException(message, e.InnerException);
            }

            // any out/ref parameters that need to be assigned?
            if (outArgumentResults != null)
            {
                for (int i = 0; i < createExpression.Parameters.Count; ++i)
                {
                    if (outArgumentResults[i] != null)
                        outArgumentResults[i].Value = arguments[i];
                }
            }
            return new RuleLiteralResult(result);
        }

        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            CodeObjectCreateExpression createExpression = (CodeObjectCreateExpression)expression;

            bool mustParenthesize = RuleDecompiler.MustParenthesize(createExpression, parentExpression);
            if (mustParenthesize)
                stringBuilder.Append("(");

            stringBuilder.Append("new ");
            RuleDecompiler.DecompileType(stringBuilder, createExpression.CreateType);

            // Decompile the arguments
            stringBuilder.Append('(');
            for (int i = 0; i < createExpression.Parameters.Count; ++i)
            {
                CodeExpression paramExpr = createExpression.Parameters[i];
                if (paramExpr == null)
                {
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.NullConstructorTypeParameter, i.ToString(CultureInfo.CurrentCulture), createExpression.CreateType);
                    RuleEvaluationException exception = new RuleEvaluationException(message);
                    exception.Data[RuleUserDataKeys.ErrorObject] = createExpression;
                    throw exception;
                }

                if (i > 0)
                    stringBuilder.Append(", ");

                RuleExpressionWalker.Decompile(stringBuilder, paramExpr, null);
            }
            stringBuilder.Append(')');

            if (mustParenthesize)
                stringBuilder.Append(")");
        }

        internal override CodeExpression Clone(CodeExpression expression)
        {
            CodeObjectCreateExpression createExpression = (CodeObjectCreateExpression)expression;

            CodeObjectCreateExpression newCreate = new CodeObjectCreateExpression();
            newCreate.CreateType = TypeReferenceExpression.CloneType(createExpression.CreateType);
            foreach (CodeExpression p in createExpression.Parameters)
            {
                newCreate.Parameters.Add(RuleExpressionWalker.Clone(p));
            }
            return newCreate;
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            CodeObjectCreateExpression createExpression = (CodeObjectCreateExpression)expression;

            CodeObjectCreateExpression createComperand = comperand as CodeObjectCreateExpression;
            if (createComperand == null)
                return false;
            // check types
            if (!TypeReferenceExpression.MatchType(createExpression.CreateType, createComperand.CreateType))
                return false;
            // check parameters
            if (createExpression.Parameters.Count != createComperand.Parameters.Count)
                return false;
            for (int i = 0; i < createExpression.Parameters.Count; ++i)
                if (!RuleExpressionWalker.Match(createExpression.Parameters[i], createComperand.Parameters[i]))
                    return false;
            return true;
        }
    }
    #endregion

    #region Array Create expression
    internal class ArrayCreateExpression : RuleExpressionInternal
    {
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        internal override RuleExpressionInfo Validate(CodeExpression expression, RuleValidation validation, bool isWritten)
        {
            string message;

            CodeArrayCreateExpression createExpression = (CodeArrayCreateExpression)expression;

            if (isWritten)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.CannotWriteToExpression, typeof(CodeObjectCreateExpression).ToString());
                ValidationError error = new ValidationError(message, ErrorNumbers.Error_InvalidAssignTarget);
                error.UserData[RuleUserDataKeys.ErrorObject] = createExpression;
                validation.Errors.Add(error);
                return null;
            }

            if (createExpression.CreateType == null)
            {
                ValidationError error = new ValidationError(Messages.NullTypeType, ErrorNumbers.Error_ParameterNotSet);
                error.UserData[RuleUserDataKeys.ErrorObject] = createExpression;
                validation.Errors.Add(error);
                return null;
            }

            Type resultType = validation.ResolveType(createExpression.CreateType);
            if (resultType == null)
                return null;

            // size CodeDom has limited support for arrays (only 1 dimensional, not more)
            // we limit CodeArrayCreateExpression to a single dimension
            // (i.e. CodeArrayCreateExpression cannot define int[5,3] or int[5][3], 
            // but it is possible to define int[][3] and then use initializers to 
            // fill it in. But we only support int[] or int[3].)
            if (resultType.IsArray)
            {
                message = string.Format(CultureInfo.CurrentCulture, Messages.ArrayTypeInvalid, resultType.Name);
                ValidationError error = new ValidationError(message, ErrorNumbers.Error_ParameterNotSet);
                error.UserData[RuleUserDataKeys.ErrorObject] = createExpression;
                validation.Errors.Add(error);
                return null;
            }

            try
            {
                // Early exit from this if a cycle is detected.
                if (!validation.PushParentExpression(createExpression))
                    return null;

                if (createExpression.Size < 0)
                {
                    ValidationError error = new ValidationError(Messages.ArraySizeInvalid, ErrorNumbers.Error_ParameterNotSet);
                    error.UserData[RuleUserDataKeys.ErrorObject] = createExpression;
                    validation.Errors.Add(error);
                    return null;
                }

                // look up size (if specified)
                if (createExpression.SizeExpression != null)
                {
                    RuleExpressionInfo sizeInfo = RuleExpressionWalker.Validate(validation, createExpression.SizeExpression, false);
                    if (sizeInfo == null)
                        return null;
                    if ((sizeInfo.ExpressionType != typeof(int))
                        && (sizeInfo.ExpressionType != typeof(uint))
                        && (sizeInfo.ExpressionType != typeof(long))
                        && (sizeInfo.ExpressionType != typeof(ulong)))
                    {
                        message = string.Format(CultureInfo.CurrentCulture, Messages.ArraySizeTypeInvalid, sizeInfo.ExpressionType.Name);
                        ValidationError error = new ValidationError(message, ErrorNumbers.Error_ParameterNotSet);
                        error.UserData[RuleUserDataKeys.ErrorObject] = createExpression;
                        validation.Errors.Add(error);
                        return null;
                    }
                }
                bool parameterInvalid = false;
                for (int i = 0; i < createExpression.Initializers.Count; ++i)
                {
                    CodeExpression init = createExpression.Initializers[i];
                    if (init == null)
                    {
                        message = string.Format(CultureInfo.CurrentCulture, Messages.MissingInitializer, resultType.Name);
                        ValidationError error = new ValidationError(message, ErrorNumbers.Error_ParameterNotSet);
                        error.UserData[RuleUserDataKeys.ErrorObject] = createExpression;
                        validation.Errors.Add(error);
                        return null;
                    }
                    RuleExpressionInfo parameterInfo = RuleExpressionWalker.Validate(validation, init, false);
                    if (parameterInfo == null)
                    {
                        parameterInvalid = true;
                    }
                    else
                    {
                        // can we convert the result type to the array type?
                        ValidationError error;
                        if (!RuleValidation.StandardImplicitConversion(parameterInfo.ExpressionType, resultType, init, out error))
                        {
                            // types must match
                            if (error != null)
                            {
                                // we got an error from the conversion, so give it back as well as a new error
                                error.UserData[RuleUserDataKeys.ErrorObject] = createExpression;
                                validation.Errors.Add(error);
                            }
                            message = string.Format(CultureInfo.CurrentCulture, Messages.InitializerMismatch, i, resultType.Name);
                            error = new ValidationError(message, ErrorNumbers.Error_OperandTypesIncompatible);
                            error.UserData[RuleUserDataKeys.ErrorObject] = createExpression;
                            validation.Errors.Add(error);
                            return null;
                        }
                    }
                }
                // if any errors get out
                if (parameterInvalid)
                    return null;

                // now it gets tricky. CodeArrayCreateExpression constructors allow:
                //     1) size as int
                //     2) size as CodeExpression
                //     3) initializers as params array
                // However, we allow a size and initializers, so try to verify size >= #initializers
                // size can be an int, uint, long, or ulong
                double size = -1;
                if (createExpression.SizeExpression != null)
                {
                    CodePrimitiveExpression prim = createExpression.SizeExpression as CodePrimitiveExpression;
                    if ((prim != null) && (prim.Value != null))
                        size = (double)Executor.AdjustType(prim.Value.GetType(), prim.Value, typeof(double));
                    if (createExpression.Size > 0)
                    {
                        // both size and SizeExpression specified, complain
                        ValidationError error = new ValidationError(Messages.ArraySizeBoth, ErrorNumbers.Error_ParameterNotSet);
                        error.UserData[RuleUserDataKeys.ErrorObject] = createExpression;
                        validation.Errors.Add(error);
                        return null;
                    }
                }
                else if (createExpression.Size > 0)
                    size = createExpression.Size;

                if ((size >= 0) && (createExpression.Initializers.Count > size))
                {
                    message = string.Format(CultureInfo.CurrentCulture, Messages.InitializerCountMismatch, createExpression.Initializers.Count, size);
                    ValidationError error = new ValidationError(message, ErrorNumbers.Error_OperandTypesIncompatible);
                    error.UserData[RuleUserDataKeys.ErrorObject] = createExpression;
                    validation.Errors.Add(error);
                    return null;
                }
            }
            finally
            {
                validation.PopParentExpression();
            }
            return new RuleExpressionInfo(resultType.MakeArrayType());
        }

        internal override void AnalyzeUsage(CodeExpression expression, RuleAnalysis analysis, bool isRead, bool isWritten, RulePathQualifier qualifier)
        {
            CodeArrayCreateExpression createExpression = (CodeArrayCreateExpression)expression;

            if (createExpression.SizeExpression != null)
                RuleExpressionWalker.AnalyzeUsage(analysis, createExpression.SizeExpression, true, false, null);
            foreach (CodeExpression p in createExpression.Initializers)
                RuleExpressionWalker.AnalyzeUsage(analysis, p, true, false, null);
        }

        internal override RuleExpressionResult Evaluate(CodeExpression expression, RuleExecution execution)
        {
            CodeArrayCreateExpression createExpression = (CodeArrayCreateExpression)expression;

            if (createExpression.CreateType == null)
            {
                RuleEvaluationException exception = new RuleEvaluationException(Messages.NullTypeType);
                exception.Data[RuleUserDataKeys.ErrorObject] = createExpression;
                throw exception;
            }

            RuleExpressionInfo createExpressionInfo = execution.Validation.ExpressionInfo(createExpression);
            if (createExpression == null)  // Oops, someone forgot to validate.
            {
                InvalidOperationException exception = new InvalidOperationException(Messages.ExpressionNotValidated);
                exception.Data[RuleUserDataKeys.ErrorObject] = createExpression;
                throw exception;
            }

            // type should be an array type already
            Type type = createExpressionInfo.ExpressionType;
            Type elementType = type.GetElementType();

            // assume this has been validated, so only 1 size specified
            int size = 0;
            if (createExpression.SizeExpression != null)
            {
                Type sizeType = execution.Validation.ExpressionInfo(createExpression.SizeExpression).ExpressionType;
                RuleExpressionResult sizeResult = RuleExpressionWalker.Evaluate(execution, createExpression.SizeExpression);
                if (sizeType == typeof(int))
                    size = (int)sizeResult.Value;
                else if (sizeType == typeof(long))
                    size = (int)((long)sizeResult.Value);
                else if (sizeType == typeof(uint))
                    size = (int)((uint)sizeResult.Value);
                else if (sizeType == typeof(ulong))
                    size = (int)((ulong)sizeResult.Value);
            }
            else if (createExpression.Size != 0)
                size = createExpression.Size;
            else
                size = createExpression.Initializers.Count;

            Array result = Array.CreateInstance(elementType, size);
            if (createExpression.Initializers != null)
                for (int i = 0; i < createExpression.Initializers.Count; ++i)
                {
                    CodeExpression initializer = createExpression.Initializers[i];
                    Type initializerType = execution.Validation.ExpressionInfo(initializer).ExpressionType;
                    RuleExpressionResult initializerResult = RuleExpressionWalker.Evaluate(execution, initializer);
                    result.SetValue(Executor.AdjustType(initializerType, initializerResult.Value, elementType), i);
                }
            return new RuleLiteralResult(result);
        }

        internal override void Decompile(CodeExpression expression, StringBuilder stringBuilder, CodeExpression parentExpression)
        {
            CodeArrayCreateExpression createExpression = (CodeArrayCreateExpression)expression;

            bool mustParenthesize = RuleDecompiler.MustParenthesize(createExpression, parentExpression);
            if (mustParenthesize)
                stringBuilder.Append("(");

            stringBuilder.Append("new ");
            RuleDecompiler.DecompileType(stringBuilder, createExpression.CreateType);
            stringBuilder.Append('[');
            if (createExpression.SizeExpression != null)
                RuleExpressionWalker.Decompile(stringBuilder, createExpression.SizeExpression, null);
            else if ((createExpression.Size != 0) || (createExpression.Initializers.Count == 0))
                stringBuilder.Append(createExpression.Size);
            stringBuilder.Append(']');
            if (createExpression.Initializers.Count > 0)
            {
                stringBuilder.Append(" { ");
                for (int i = 0; i < createExpression.Initializers.Count; ++i)
                {
                    CodeExpression paramExpr = createExpression.Initializers[i];
                    if (paramExpr == null)
                    {
                        string message = string.Format(CultureInfo.CurrentCulture, Messages.NullConstructorTypeParameter, i.ToString(CultureInfo.CurrentCulture), createExpression.CreateType);
                        RuleEvaluationException exception = new RuleEvaluationException(message);
                        exception.Data[RuleUserDataKeys.ErrorObject] = createExpression;
                        throw exception;
                    }

                    if (i > 0)
                        stringBuilder.Append(", ");

                    RuleExpressionWalker.Decompile(stringBuilder, paramExpr, null);
                }
                stringBuilder.Append('}');
            }

            if (mustParenthesize)
                stringBuilder.Append(")");
        }

        internal override CodeExpression Clone(CodeExpression expression)
        {
            CodeArrayCreateExpression createExpression = (CodeArrayCreateExpression)expression;

            CodeArrayCreateExpression newCreate = new CodeArrayCreateExpression();
            newCreate.CreateType = TypeReferenceExpression.CloneType(createExpression.CreateType);
            newCreate.Size = createExpression.Size;
            if (createExpression.SizeExpression != null)
                newCreate.SizeExpression = RuleExpressionWalker.Clone(createExpression.SizeExpression);
            foreach (CodeExpression p in createExpression.Initializers)
                newCreate.Initializers.Add(RuleExpressionWalker.Clone(p));
            return newCreate;
        }

        internal override bool Match(CodeExpression expression, CodeExpression comperand)
        {
            CodeArrayCreateExpression createExpression = (CodeArrayCreateExpression)expression;

            CodeArrayCreateExpression createComperand = comperand as CodeArrayCreateExpression;
            if ((createComperand == null)
                || (createExpression.Size != createComperand.Size)
                || (!TypeReferenceExpression.MatchType(createExpression.CreateType, createComperand.CreateType)))
                return false;
            // check SizeExpression, if it exists
            if (createExpression.SizeExpression != null)
            {
                if (createComperand.SizeExpression == null)
                    return false;
                if (!RuleExpressionWalker.Match(createExpression.SizeExpression, createComperand.SizeExpression))
                    return false;
            }
            else
            {
                if (createComperand.SizeExpression != null)
                    return false;
            }
            // check initializers
            if (createExpression.Initializers.Count != createComperand.Initializers.Count)
                return false;
            for (int i = 0; i < createExpression.Initializers.Count; ++i)
                if (!RuleExpressionWalker.Match(createExpression.Initializers[i], createComperand.Initializers[i]))
                    return false;
            return true;
        }
    }
    #endregion
}
