//------------------------------------------------------------------------------
// <copyright file="FilterException.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
// <owner current="false" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.Serialization;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [Serializable]
    public class InvalidExpressionException : DataException {
        protected InvalidExpressionException(SerializationInfo info, StreamingContext context)
        : base(info, context) {
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public InvalidExpressionException() : base() {}
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public InvalidExpressionException(string s) : base(s) {}
        
        public InvalidExpressionException(string message, Exception innerException)  : base(message, innerException) {}
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [Serializable]
    public class EvaluateException : InvalidExpressionException {
        protected EvaluateException(SerializationInfo info, StreamingContext context)
        : base(info, context) {
        }
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public EvaluateException() : base() {}
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public EvaluateException(string s) : base(s) {}
        
        public EvaluateException(string message, Exception innerException)  : base(message, innerException) {}
    }

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [Serializable]
    public class SyntaxErrorException : InvalidExpressionException {
        protected SyntaxErrorException(SerializationInfo info, StreamingContext context)
        : base(info, context) {
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SyntaxErrorException() : base() {}
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public SyntaxErrorException(string s) : base(s) {}
        
        public SyntaxErrorException(string message, Exception innerException)  : base(message, innerException) {}
    }

    internal sealed class ExprException {
        private ExprException() { /* prevent utility class from being insantiated*/ }

        static private OverflowException _Overflow(string error) {
            OverflowException e = new OverflowException(error);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }
        static private InvalidExpressionException _Expr(string error) {
            InvalidExpressionException e = new InvalidExpressionException(error);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }
        static private SyntaxErrorException _Syntax(string error) {
            SyntaxErrorException e = new SyntaxErrorException(error);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }
        static private EvaluateException _Eval(string error) {
            EvaluateException e = new EvaluateException(error);
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }
        static private EvaluateException _Eval(string error, Exception innerException) {
            EvaluateException e = new EvaluateException(error/*, innerException*/); // 
            ExceptionBuilder.TraceExceptionAsReturnValue(e);
            return e;
        }

        static public Exception InvokeArgument() {
            return ExceptionBuilder._Argument(Res.GetString(Res.Expr_InvokeArgument));
        }

        static public Exception NYI(string moreinfo) {
            string err = Res.GetString(Res.Expr_NYI, moreinfo);
            Debug.Fail(err);
            return _Expr(err);
        }

        static public Exception MissingOperand(OperatorInfo before) {
            return _Syntax(Res.GetString(Res.Expr_MissingOperand, Operators.ToString(before.op)));
        }

        static public Exception MissingOperator(string token) {
            return _Syntax(Res.GetString(Res.Expr_MissingOperand, token));
        }

        static public Exception TypeMismatch(string expr) {
            return _Eval(Res.GetString(Res.Expr_TypeMismatch, expr));
        }

        static public Exception FunctionArgumentOutOfRange(string arg, string func) {
            return ExceptionBuilder._ArgumentOutOfRange(arg, Res.GetString(Res.Expr_ArgumentOutofRange, func));
        }

        static public Exception ExpressionTooComplex() {
            return _Eval(Res.GetString(Res.Expr_ExpressionTooComplex));
        }

        static public Exception UnboundName(string name) {
            return _Eval(Res.GetString(Res.Expr_UnboundName, name));
        }

        static public Exception InvalidString(string str) {
            return _Syntax(Res.GetString(Res.Expr_InvalidString, str));
        }

        static public Exception UndefinedFunction(string name) {
            return _Eval(Res.GetString(Res.Expr_UndefinedFunction, name));
        }

        static public Exception SyntaxError() {
            return _Syntax(Res.GetString(Res.Expr_Syntax));
        }

        static public Exception FunctionArgumentCount(string name) {
            return _Eval(Res.GetString(Res.Expr_FunctionArgumentCount, name));
        }

        static public Exception MissingRightParen() {
            return _Syntax(Res.GetString(Res.Expr_MissingRightParen));
        }

        static public Exception UnknownToken(string token, int position) {
            return _Syntax(Res.GetString(Res.Expr_UnknownToken, token, position.ToString(CultureInfo.InvariantCulture)));
        }

        static public Exception UnknownToken(Tokens tokExpected, Tokens tokCurr, int position) {
            return _Syntax(Res.GetString(Res.Expr_UnknownToken1, tokExpected.ToString(), tokCurr.ToString(), position.ToString(CultureInfo.InvariantCulture)));
        }

        static public Exception DatatypeConvertion(Type type1, Type type2) {
            return _Eval(Res.GetString(Res.Expr_DatatypeConvertion, type1.ToString(), type2.ToString()));
        }

        static public Exception DatavalueConvertion(object value, Type type, Exception innerException) {
            return _Eval(Res.GetString(Res.Expr_DatavalueConvertion, value.ToString(), type.ToString()), innerException);
        }

        static public Exception InvalidName(string name) {
            return _Syntax(Res.GetString(Res.Expr_InvalidName, name));
        }

        static public Exception InvalidDate(string date) {
            return _Syntax(Res.GetString(Res.Expr_InvalidDate, date));
        }

        static public Exception NonConstantArgument() {
            return _Eval(Res.GetString(Res.Expr_NonConstantArgument));
        }

        static public Exception InvalidPattern(string pat) {
            return _Eval(Res.GetString(Res.Expr_InvalidPattern, pat));
        }

        static public Exception InWithoutParentheses() {
            return _Syntax(Res.GetString(Res.Expr_InWithoutParentheses));
        }

        static public Exception InWithoutList() {
            return _Syntax(Res.GetString(Res.Expr_InWithoutList));
        }

        static public Exception InvalidIsSyntax() {
            return _Syntax(Res.GetString(Res.Expr_IsSyntax));
        }

        static public Exception Overflow(Type type) {
            return _Overflow(Res.GetString(Res.Expr_Overflow, type.Name));
        }

        static public Exception ArgumentType(string function, int arg, Type type) {
            return _Eval(Res.GetString(Res.Expr_ArgumentType, function, arg.ToString(CultureInfo.InvariantCulture), type.ToString()));
        }

        static public Exception ArgumentTypeInteger(string function, int arg) {
            return _Eval(Res.GetString(Res.Expr_ArgumentTypeInteger, function, arg.ToString(CultureInfo.InvariantCulture)));
        }

        static public Exception TypeMismatchInBinop(int op, Type type1, Type type2) {
            return _Eval(Res.GetString(Res.Expr_TypeMismatchInBinop, Operators.ToString(op), type1.ToString(), type2.ToString()));
        }

        static public Exception AmbiguousBinop(int op, Type type1, Type type2) {
            return _Eval(Res.GetString(Res.Expr_AmbiguousBinop, Operators.ToString(op), type1.ToString(), type2.ToString()));
        }

        static public Exception UnsupportedOperator(int op) {
            return _Eval(Res.GetString(Res.Expr_UnsupportedOperator, Operators.ToString(op)));
        }

        static public Exception InvalidNameBracketing(string name) {
            return _Syntax(Res.GetString(Res.Expr_InvalidNameBracketing, name));
        }

        static public Exception MissingOperandBefore(string op) {
            return _Syntax(Res.GetString(Res.Expr_MissingOperandBefore, op));
        }

        static public Exception TooManyRightParentheses() {
            return _Syntax(Res.GetString(Res.Expr_TooManyRightParentheses));
        }

        static public Exception UnresolvedRelation(string name, string expr) {
            return _Eval(Res.GetString(Res.Expr_UnresolvedRelation, name, expr));
        }
        
        static internal EvaluateException BindFailure(string relationName) {
            return _Eval(Res.GetString(Res.Expr_BindFailure, relationName));
        }

        static public Exception AggregateArgument() {
            return _Syntax(Res.GetString(Res.Expr_AggregateArgument));
        }

        static public Exception AggregateUnbound(string expr) {
            return _Eval(Res.GetString(Res.Expr_AggregateUnbound, expr));
        }

        static public Exception EvalNoContext() {
            return _Eval(Res.GetString(Res.Expr_EvalNoContext));
        }

        static public Exception ExpressionUnbound(string expr) {
            return _Eval(Res.GetString(Res.Expr_ExpressionUnbound, expr));
        }

        static public Exception ComputeNotAggregate(string expr) {
            return _Eval(Res.GetString(Res.Expr_ComputeNotAggregate, expr));
        }

        static public Exception FilterConvertion(string expr) {
            return _Eval(Res.GetString(Res.Expr_FilterConvertion, expr));
        }

        static public Exception LookupArgument() {
            return _Syntax(Res.GetString(Res.Expr_LookupArgument));
        }

        static public Exception InvalidType(string typeName) {
            return _Eval(Res.GetString(Res.Expr_InvalidType, typeName));
        }

        static public Exception InvalidHoursArgument() {
            return _Eval(Res.GetString(Res.Expr_InvalidHoursArgument));
        }
        
        static public Exception InvalidMinutesArgument() {
            return _Eval(Res.GetString(Res.Expr_InvalidMinutesArgument));
        }
        
        static public Exception InvalidTimeZoneRange() {
            return _Eval(Res.GetString(Res.Expr_InvalidTimeZoneRange));
        }
        
        static public Exception MismatchKindandTimeSpan() {
            return _Eval(Res.GetString(Res.Expr_MismatchKindandTimeSpan));
        }

        static public Exception UnsupportedDataType(Type type)
        {
            return ExceptionBuilder._Argument(Res.GetString(Res.Expr_UnsupportedType, type.FullName));
        }
    }
}
