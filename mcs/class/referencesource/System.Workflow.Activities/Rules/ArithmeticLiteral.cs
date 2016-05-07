// ---------------------------------------------------------------------------
// Copyright (C) 2005 Microsoft Corporation All Rights Reserved
// ---------------------------------------------------------------------------

#define CODE_ANALYSIS
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.Activities.Common;

namespace System.Workflow.Activities.Rules
{
    #region ArithmeticLiteral Class
    internal abstract class ArithmeticLiteral
    {
        #region Properties
        /// <summary>
        /// The type of the literal 
        /// </summary>
        internal protected Type m_type;

        /// <summary>
        /// Return the name of the type
        /// </summary>
        protected virtual string TypeName
        {
            get { return m_type.FullName; }
        }

        /// <summary>
        /// Get the boxed literal
        /// </summary>
        internal abstract object Value { get; }

        /// <summary>
        /// A delegate for literal factory methods
        /// </summary>
        /// <param name="literalValue"></param>
        /// <returns></returns>
        private delegate ArithmeticLiteral LiteralMaker(object literalValue);

        /// <summary>
        /// Collection of literal factory methods indexed by type
        /// </summary>
        private static Dictionary<Type, LiteralMaker> types = CreateTypesDictionary();

        /// <summary>
        /// Group types by characteristics so we can check if operation is allowed
        /// </summary>
        [Flags()]
        private enum TypeFlags
        {
            UInt16 = 0x01,
            Int32 = 0x02,
            UInt32 = 0x04,
            Int64 = 0x08,
            UInt64 = 0x10,
            Single = 0x20,
            Double = 0x40,
            Decimal = 0x80,
            Boolean = 0x100,
            String = 0x800,
            Nullable = 0x10000
        };

        /// <summary>
        /// Collection of TypeFlags for the supported value types indexed by type
        /// </summary>
        private static Dictionary<Type, TypeFlags> supportedTypes = CreateSupportedTypesDictionary();

        private static Dictionary<Type, LiteralMaker> CreateTypesDictionary()
        {
            // Create the literal class factory delegates
            Dictionary<Type, LiteralMaker> dictionary = new Dictionary<Type, LiteralMaker>(16);
            dictionary.Add(typeof(byte), MakeByte);
            dictionary.Add(typeof(sbyte), MakeSByte);
            dictionary.Add(typeof(char), MakeChar);
            dictionary.Add(typeof(short), MakeShort);
            dictionary.Add(typeof(int), MakeInt);
            dictionary.Add(typeof(long), MakeLong);
            dictionary.Add(typeof(ushort), MakeUShort);
            dictionary.Add(typeof(uint), MakeUInt);
            dictionary.Add(typeof(ulong), MakeULong);
            dictionary.Add(typeof(float), MakeFloat);
            dictionary.Add(typeof(double), MakeDouble);
            dictionary.Add(typeof(decimal), MakeDecimal);
            dictionary.Add(typeof(bool), MakeBoolean);
            dictionary.Add(typeof(string), MakeString);

            dictionary.Add(typeof(byte?), MakeByte);
            dictionary.Add(typeof(sbyte?), MakeSByte);
            dictionary.Add(typeof(char?), MakeChar);
            dictionary.Add(typeof(short?), MakeShort);
            dictionary.Add(typeof(int?), MakeInt);
            dictionary.Add(typeof(long?), MakeLong);
            dictionary.Add(typeof(ushort?), MakeUShort);
            dictionary.Add(typeof(uint?), MakeUInt);
            dictionary.Add(typeof(ulong?), MakeULong);
            dictionary.Add(typeof(float?), MakeFloat);
            dictionary.Add(typeof(double?), MakeDouble);
            dictionary.Add(typeof(decimal?), MakeDecimal);
            dictionary.Add(typeof(bool?), MakeBoolean);
            return dictionary;
        }

        static private Dictionary<Type, TypeFlags> CreateSupportedTypesDictionary()
        {
            Dictionary<Type, TypeFlags> dictionary = new Dictionary<Type, TypeFlags>(26);
            dictionary.Add(typeof(byte), TypeFlags.UInt16);
            dictionary.Add(typeof(byte?), TypeFlags.Nullable | TypeFlags.UInt16);
            dictionary.Add(typeof(sbyte), TypeFlags.Int32);
            dictionary.Add(typeof(sbyte?), TypeFlags.Nullable | TypeFlags.Int32);
            dictionary.Add(typeof(char), TypeFlags.UInt16);
            dictionary.Add(typeof(char?), TypeFlags.Nullable | TypeFlags.UInt16);
            dictionary.Add(typeof(short), TypeFlags.Int32);
            dictionary.Add(typeof(short?), TypeFlags.Nullable | TypeFlags.Int32);
            dictionary.Add(typeof(int), TypeFlags.Int32);
            dictionary.Add(typeof(int?), TypeFlags.Nullable | TypeFlags.Int32);
            dictionary.Add(typeof(long), TypeFlags.Int64);
            dictionary.Add(typeof(long?), TypeFlags.Nullable | TypeFlags.Int64);
            dictionary.Add(typeof(ushort), TypeFlags.UInt16);
            dictionary.Add(typeof(ushort?), TypeFlags.Nullable | TypeFlags.UInt16);
            dictionary.Add(typeof(uint), TypeFlags.UInt32);
            dictionary.Add(typeof(uint?), TypeFlags.Nullable | TypeFlags.UInt32);
            dictionary.Add(typeof(ulong), TypeFlags.UInt64);
            dictionary.Add(typeof(ulong?), TypeFlags.Nullable | TypeFlags.UInt64);
            dictionary.Add(typeof(float), TypeFlags.Single);
            dictionary.Add(typeof(float?), TypeFlags.Nullable | TypeFlags.Single);
            dictionary.Add(typeof(double), TypeFlags.Double);
            dictionary.Add(typeof(double?), TypeFlags.Nullable | TypeFlags.Double);
            dictionary.Add(typeof(decimal), TypeFlags.Decimal);
            dictionary.Add(typeof(decimal?), TypeFlags.Nullable | TypeFlags.Decimal);
            dictionary.Add(typeof(bool), TypeFlags.Boolean);
            dictionary.Add(typeof(bool?), TypeFlags.Nullable | TypeFlags.Boolean);
            dictionary.Add(typeof(string), TypeFlags.String);
            return dictionary;
        }
        #endregion

        #region Factory Methods
        internal static ArithmeticLiteral MakeLiteral(Type literalType, object literalValue)
        {
            LiteralMaker f;
            if (literalValue == null)
                return new NullArithmeticLiteral(literalType);
            return (types.TryGetValue(literalType, out f)) ? f(literalValue) : null;
        }

        /// <summary>
        /// Factory function for a byte type
        /// </summary>
        /// <param name="literalValue"></param>
        /// <returns></returns>
        private static ArithmeticLiteral MakeByte(object literalValue)
        {
            return new UShortArithmeticLiteral((byte)literalValue);
        }

        /// <summary>
        /// Factory function for a sbyte type
        /// </summary>
        /// <param name="literalValue"></param>
        /// <returns></returns>
        private static ArithmeticLiteral MakeSByte(object literalValue)
        {
            return new IntArithmeticLiteral((sbyte)literalValue);
        }

        /// <summary>
        /// Factory function for a char type
        /// </summary>
        /// <param name="literalValue"></param>
        /// <returns></returns>
        private static ArithmeticLiteral MakeChar(object literalValue)
        {
            char c = (char)literalValue;
            return new CharArithmeticLiteral(c);
        }

        /// <summary>
        /// Factory function for a decimal type
        /// </summary>
        /// <param name="literalValue"></param>
        /// <returns></returns>
        private static ArithmeticLiteral MakeDecimal(object literalValue)
        {
            return new DecimalArithmeticLiteral((decimal)literalValue);
        }

        /// <summary>
        /// Factory function for an Int16 type
        /// </summary>
        /// <param name="literalValue"></param>
        /// <returns></returns>
        private static ArithmeticLiteral MakeShort(object literalValue)
        {
            return new IntArithmeticLiteral((short)literalValue);
        }

        /// <summary>
        /// Factory function for an Int32 type
        /// </summary>
        /// <param name="literalValue"></param>
        /// <returns></returns>
        private static ArithmeticLiteral MakeInt(object literalValue)
        {
            return new IntArithmeticLiteral((int)literalValue);
        }

        /// <summary>
        /// Factory function for an Int64 type
        /// </summary>
        /// <param name="literalValue"></param>
        /// <returns></returns>
        private static ArithmeticLiteral MakeLong(object literalValue)
        {
            return new LongArithmeticLiteral((long)literalValue);
        }

        /// <summary>
        /// Factory function for an UInt16 type
        /// </summary>
        /// <param name="literalValue"></param>
        /// <returns></returns>
        private static ArithmeticLiteral MakeUShort(object literalValue)
        {
            return new UShortArithmeticLiteral((ushort)literalValue);
        }

        /// <summary>
        /// Factory function for an UInt32 type
        /// </summary>
        /// <param name="literalValue"></param>
        /// <returns></returns>
        private static ArithmeticLiteral MakeUInt(object literalValue)
        {
            return new UIntArithmeticLiteral((uint)literalValue);
        }

        /// <summary>
        /// Factory function for an UInt64 type
        /// </summary>
        /// <param name="literalValue"></param>
        /// <returns></returns>
        private static ArithmeticLiteral MakeULong(object literalValue)
        {
            return new ULongArithmeticLiteral((ulong)literalValue);
        }

        /// <summary>
        /// Factory function for a float type
        /// </summary>
        /// <param name="literalValue"></param>
        /// <returns></returns>
        private static ArithmeticLiteral MakeFloat(object literalValue)
        {
            return new FloatArithmeticLiteral((float)literalValue);
        }

        /// <summary>
        /// Factory function for a double type
        /// </summary>
        /// <param name="literalValue"></param>
        /// <returns></returns>
        private static ArithmeticLiteral MakeDouble(object literalValue)
        {
            return new DoubleArithmeticLiteral((double)literalValue);
        }

        /// <summary>
        /// Factory function for a bool type
        /// </summary>
        /// <param name="literalValue"></param>
        /// <returns></returns>
        private static ArithmeticLiteral MakeBoolean(object literalValue)
        {
            return new BooleanArithmeticLiteral((bool)literalValue);
        }

        /// <summary>
        /// Factory function for a String type
        /// </summary>
        /// <param name="literalValue"></param>
        /// <returns></returns>
        private static ArithmeticLiteral MakeString(object literalValue)
        {
            return new StringArithmeticLiteral(literalValue.ToString());
        }
        #endregion

        #region Type Checking Methods

        internal static RuleBinaryExpressionInfo ResultType(
            CodeBinaryOperatorType operation,
            Type lhs,
            CodeExpression lhsExpression,
            Type rhs,
            CodeExpression rhsExpression,
            RuleValidation validator,
            out ValidationError error)
        {
            // do we support the types natively?
            TypeFlags lhsType, rhsType;
            if (supportedTypes.TryGetValue(lhs, out lhsType) && supportedTypes.TryGetValue(rhs, out rhsType))
            {
                Type resultType = ResultType(operation, lhsType, rhsType);
                if (resultType != null)
                {
                    error = null;
                    return new RuleBinaryExpressionInfo(lhs, rhs, resultType);
                }
                else
                {
                    string message = string.Format(CultureInfo.CurrentCulture, Messages.ArithOpBadTypes, operation.ToString(),
                        (lhs == typeof(NullLiteral)) ? Messages.NullValue : RuleDecompiler.DecompileType(lhs),
                        (rhs == typeof(NullLiteral)) ? Messages.NullValue : RuleDecompiler.DecompileType(rhs));
                    error = new ValidationError(message, ErrorNumbers.Error_OperandTypesIncompatible);
                    return null;
                }
            }
            else
            {
                // not natively supported, see if user overrides operator
                MethodInfo opOverload = Literal.MapOperatorToMethod(operation, lhs, lhsExpression, rhs, rhsExpression, validator, out error);
                if (opOverload != null)
                    return new RuleBinaryExpressionInfo(lhs, rhs, opOverload);
                else
                    return null;
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static Type ResultType(CodeBinaryOperatorType operation, TypeFlags lhsType, TypeFlags rhsType)
        {
            TypeFlags combined = (lhsType | rhsType);
            bool nullable = (combined & TypeFlags.Nullable) == TypeFlags.Nullable;
            if (nullable) combined ^= TypeFlags.Nullable;

            switch (operation)
            {
                case CodeBinaryOperatorType.Add:
                    // string + anything or anything + string always work
                    if ((lhsType == TypeFlags.String) || (rhsType == TypeFlags.String))
                        return typeof(string);
                    goto case CodeBinaryOperatorType.Divide;

                case CodeBinaryOperatorType.Divide:
                case CodeBinaryOperatorType.Modulus:
                case CodeBinaryOperatorType.Multiply:
                case CodeBinaryOperatorType.Subtract:
                    switch (combined)
                    {
                        case TypeFlags.Decimal:
                        case TypeFlags.Decimal | TypeFlags.UInt16:
                        case TypeFlags.Decimal | TypeFlags.Int32:
                        case TypeFlags.Decimal | TypeFlags.UInt32:
                        case TypeFlags.Decimal | TypeFlags.Int64:
                        case TypeFlags.Decimal | TypeFlags.UInt64:
                            return (nullable) ? typeof(decimal?) : typeof(decimal);
                        case TypeFlags.Double:
                        case TypeFlags.Double | TypeFlags.UInt16:
                        case TypeFlags.Double | TypeFlags.Int32:
                        case TypeFlags.Double | TypeFlags.UInt32:
                        case TypeFlags.Double | TypeFlags.Int64:
                        case TypeFlags.Double | TypeFlags.UInt64:
                        case TypeFlags.Double | TypeFlags.Single:
                            return (nullable) ? typeof(double?) : typeof(double);
                        case TypeFlags.Single:
                        case TypeFlags.Single | TypeFlags.UInt16:
                        case TypeFlags.Single | TypeFlags.Int32:
                        case TypeFlags.Single | TypeFlags.UInt32:
                        case TypeFlags.Single | TypeFlags.Int64:
                        case TypeFlags.Single | TypeFlags.UInt64:
                            return (nullable) ? typeof(float?) : typeof(float);
                        case TypeFlags.Int64:
                        case TypeFlags.Int64 | TypeFlags.UInt16:
                        case TypeFlags.Int64 | TypeFlags.Int32:
                        case TypeFlags.Int64 | TypeFlags.UInt32:
                        case TypeFlags.Int32 | TypeFlags.UInt32:
                            return (nullable) ? typeof(long?) : typeof(long);
                        case TypeFlags.UInt64:
                        case TypeFlags.UInt64 | TypeFlags.UInt16:
                        case TypeFlags.UInt64 | TypeFlags.UInt32:
                            return (nullable) ? typeof(ulong?) : typeof(ulong);
                        case TypeFlags.Int32:
                        case TypeFlags.UInt16:
                        case TypeFlags.Int32 | TypeFlags.UInt16:
                            return (nullable) ? typeof(int?) : typeof(int);
                        case TypeFlags.UInt32:
                        case TypeFlags.UInt32 | TypeFlags.UInt16:
                            return (nullable) ? typeof(uint?) : typeof(uint);
                    }
                    break;

                case CodeBinaryOperatorType.BitwiseAnd:
                case CodeBinaryOperatorType.BitwiseOr:
                    switch (combined)
                    {
                        case TypeFlags.Int64:
                        case TypeFlags.Int64 | TypeFlags.UInt16:
                        case TypeFlags.Int64 | TypeFlags.Int32:
                        case TypeFlags.Int64 | TypeFlags.UInt32:
                        case TypeFlags.Int32 | TypeFlags.UInt32:
                            return (nullable) ? typeof(long?) : typeof(long);
                        case TypeFlags.UInt64:
                        case TypeFlags.UInt64 | TypeFlags.UInt16:
                        case TypeFlags.UInt64 | TypeFlags.UInt32:
                            return (nullable) ? typeof(ulong?) : typeof(ulong);
                        case TypeFlags.Int32:
                        case TypeFlags.UInt16:
                        case TypeFlags.Int32 | TypeFlags.UInt16:
                            return (nullable) ? typeof(int?) : typeof(int);
                        case TypeFlags.UInt32:
                        case TypeFlags.UInt32 | TypeFlags.UInt16:
                            return (nullable) ? typeof(uint?) : typeof(uint);
                        case TypeFlags.Boolean:
                            return (nullable) ? typeof(bool?) : typeof(bool);
                    }
                    break;
            }
            return null;
        }
        #endregion

        #region Value Type Dispatch Methods
        internal virtual object Add(ArithmeticLiteral v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.TypeName, CodeBinaryOperatorType.Add, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.m_type, CodeBinaryOperatorType.Add, this.m_type);
        }
        internal virtual object Add()
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, Messages.NullValue, CodeBinaryOperatorType.Add, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, typeof(void), CodeBinaryOperatorType.Add, this.m_type);
        }
        internal virtual object Add(int v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Add, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Add, this.m_type);
        }
        internal virtual object Add(long v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Add, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Add, this.m_type);
        }
        internal virtual object Add(char v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Add, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Add, this.m_type);
        }
        internal virtual object Add(ushort v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Add, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Add, this.m_type);
        }
        internal virtual object Add(uint v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Add, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Add, this.m_type);
        }
        internal virtual object Add(ulong v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Add, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Add, this.m_type);
        }
        internal virtual object Add(float v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Add, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Add, this.m_type);
        }
        internal virtual object Add(double v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Add, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Add, this.m_type);
        }
        internal virtual object Add(decimal v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Add, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Add, this.m_type);
        }
        internal virtual object Add(bool v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Add, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Add, this.m_type);
        }
        internal virtual object Add(string v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Add, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Add, this.m_type);
        }

        internal virtual object Subtract(ArithmeticLiteral v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.TypeName, CodeBinaryOperatorType.Subtract, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.m_type, CodeBinaryOperatorType.Subtract, this.m_type);
        }
        internal virtual object Subtract()
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, Messages.NullValue, CodeBinaryOperatorType.Subtract, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, typeof(void), CodeBinaryOperatorType.Subtract, this.m_type);
        }
        internal virtual object Subtract(int v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Subtract, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Subtract, this.m_type);
        }
        internal virtual object Subtract(long v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Subtract, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Subtract, this.m_type);
        }
        internal virtual object Subtract(ushort v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Subtract, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Subtract, this.m_type);
        }
        internal virtual object Subtract(uint v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Subtract, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Subtract, this.m_type);
        }
        internal virtual object Subtract(ulong v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Subtract, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Subtract, this.m_type);
        }
        internal virtual object Subtract(float v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Subtract, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Subtract, this.m_type);
        }
        internal virtual object Subtract(double v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Subtract, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Subtract, this.m_type);
        }
        internal virtual object Subtract(decimal v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Subtract, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Subtract, this.m_type);
        }

        internal virtual object Multiply(ArithmeticLiteral v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.TypeName, CodeBinaryOperatorType.Multiply, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.m_type, CodeBinaryOperatorType.Multiply, this.m_type);
        }
        internal virtual object Multiply()
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, Messages.NullValue, CodeBinaryOperatorType.Multiply, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, typeof(void), CodeBinaryOperatorType.Multiply, this.m_type);
        }
        internal virtual object Multiply(int v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Multiply, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Multiply, this.m_type);
        }
        internal virtual object Multiply(long v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Multiply, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Multiply, this.m_type);
        }
        internal virtual object Multiply(ushort v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Multiply, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Multiply, this.m_type);
        }
        internal virtual object Multiply(uint v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Multiply, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Multiply, this.m_type);
        }
        internal virtual object Multiply(ulong v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Multiply, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Multiply, this.m_type);
        }
        internal virtual object Multiply(float v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Multiply, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Multiply, this.m_type);
        }
        internal virtual object Multiply(double v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Multiply, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Multiply, this.m_type);
        }
        internal virtual object Multiply(decimal v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Multiply, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Multiply, this.m_type);
        }

        internal virtual object Divide(ArithmeticLiteral v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.TypeName, CodeBinaryOperatorType.Divide, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.m_type, CodeBinaryOperatorType.Divide, this.m_type);
        }
        internal virtual object Divide()
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, Messages.NullValue, CodeBinaryOperatorType.Divide, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, typeof(void), CodeBinaryOperatorType.Divide, this.m_type);
        }
        internal virtual object Divide(int v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Divide, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Divide, this.m_type);
        }
        internal virtual object Divide(long v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Divide, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Divide, this.m_type);
        }
        internal virtual object Divide(ushort v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Divide, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Divide, this.m_type);
        }
        internal virtual object Divide(uint v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Divide, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Divide, this.m_type);
        }
        internal virtual object Divide(ulong v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Divide, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Divide, this.m_type);
        }
        internal virtual object Divide(float v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Divide, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Divide, this.m_type);
        }
        internal virtual object Divide(double v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Divide, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Divide, this.m_type);
        }
        internal virtual object Divide(decimal v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Divide, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Divide, this.m_type);
        }

        internal virtual object Modulus(ArithmeticLiteral v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.m_type, CodeBinaryOperatorType.Modulus, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.m_type, CodeBinaryOperatorType.Modulus, this.m_type);
        }
        internal virtual object Modulus()
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, Messages.NullValue, CodeBinaryOperatorType.Modulus, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, typeof(void), CodeBinaryOperatorType.Modulus, this.m_type);
        }
        internal virtual object Modulus(int v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Modulus, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Modulus, this.m_type);
        }
        internal virtual object Modulus(long v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Modulus, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Modulus, this.m_type);
        }
        internal virtual object Modulus(ushort v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Modulus, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Modulus, this.m_type);
        }
        internal virtual object Modulus(uint v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Modulus, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Modulus, this.m_type);
        }
        internal virtual object Modulus(ulong v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Modulus, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Modulus, this.m_type);
        }
        internal virtual object Modulus(float v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Modulus, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Modulus, this.m_type);
        }
        internal virtual object Modulus(double v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Modulus, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Modulus, this.m_type);
        }
        internal virtual object Modulus(decimal v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.Modulus, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.Modulus, this.m_type);
        }

        internal virtual object BitAnd(ArithmeticLiteral v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.m_type, CodeBinaryOperatorType.BitwiseAnd, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.m_type, CodeBinaryOperatorType.BitwiseAnd, this.m_type);
        }
        internal virtual object BitAnd()
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, Messages.NullValue, CodeBinaryOperatorType.BitwiseAnd, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, typeof(void), CodeBinaryOperatorType.BitwiseAnd, this.m_type);
        }
        internal virtual object BitAnd(int v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.m_type);
        }
        internal virtual object BitAnd(long v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.m_type);
        }
        internal virtual object BitAnd(ushort v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.m_type);
        }
        internal virtual object BitAnd(uint v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.m_type);
        }
        internal virtual object BitAnd(ulong v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.m_type);
        }
        internal virtual object BitAnd(float v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.m_type);
        }
        internal virtual object BitAnd(double v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.m_type);
        }
        internal virtual object BitAnd(decimal v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.m_type);
        }
        internal virtual object BitAnd(bool v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.BitwiseAnd, this.m_type);
        }

        internal virtual object BitOr(ArithmeticLiteral v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.TypeName, CodeBinaryOperatorType.BitwiseOr, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.m_type, CodeBinaryOperatorType.BitwiseOr, this.m_type);
        }
        internal virtual object BitOr()
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, Messages.NullValue, CodeBinaryOperatorType.BitwiseOr, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, typeof(void), CodeBinaryOperatorType.BitwiseOr, this.m_type);
        }
        internal virtual object BitOr(int v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.m_type);
        }
        internal virtual object BitOr(long v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.m_type);
        }
        internal virtual object BitOr(ushort v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.m_type);
        }
        internal virtual object BitOr(uint v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.m_type);
        }
        internal virtual object BitOr(ulong v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.m_type);
        }
        internal virtual object BitOr(float v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.m_type);
        }
        internal virtual object BitOr(double v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.m_type);
        }
        internal virtual object BitOr(decimal v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.m_type);
        }
        internal virtual object BitOr(bool v)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Messages.IncompatibleArithmeticTypes, v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.TypeName);
            throw new RuleEvaluationIncompatibleTypesException(message, v.GetType(), CodeBinaryOperatorType.BitwiseOr, this.m_type);
        }
        #endregion
    }
    #endregion

    #region IntArithmeticLiteral Class
    internal class IntArithmeticLiteral : ArithmeticLiteral
    {
        private int m_value;
        internal IntArithmeticLiteral(int literalValue)
        {
            m_value = literalValue;
            m_type = typeof(int);
        }

        internal override object Value
        {
            get { return m_value; }
        }
        #region Add
        internal override object Add(ArithmeticLiteral v)
        {
            return v.Add(m_value);
        }
        internal override object Add()
        {
            return null;
        }
        internal override object Add(int v)
        {
            return (v + m_value);
        }
        internal override object Add(long v)
        {
            return (v + m_value);
        }
        internal override object Add(char v)
        {
            return (v + m_value);
        }
        internal override object Add(ushort v)
        {
            return (v + m_value);
        }
        internal override object Add(uint v)
        {
            return (v + m_value);
        }
        internal override object Add(ulong v)
        {
            // this should only happen when using a constant (+ve) int and an ulong
            // if that's not the case, you get an error
            return (m_value >= 0) ? (v + (ulong)m_value) : base.Add(v);
        }
        internal override object Add(float v)
        {
            return (v + m_value);
        }
        internal override object Add(double v)
        {
            return (v + m_value);
        }
        internal override object Add(decimal v)
        {
            return (v + m_value);
        }
        internal override object Add(string v)
        {
            return (v + m_value.ToString(CultureInfo.CurrentCulture));
        }
        #endregion
        #region Subtract
        internal override object Subtract(ArithmeticLiteral v)
        {
            return v.Subtract(m_value);
        }
        internal override object Subtract()
        {
            return null;
        }
        internal override object Subtract(int v)
        {
            return (v - m_value);
        }
        internal override object Subtract(long v)
        {
            return (v - m_value);
        }
        internal override object Subtract(ushort v)
        {
            return (v - m_value);
        }
        internal override object Subtract(uint v)
        {
            return (v - m_value);
        }
        internal override object Subtract(ulong v)
        {
            // this should only happen when using a constant (+ve) int and an ulong
            // if that's not the case, you get an error
            return (m_value >= 0) ? (v - (ulong)m_value) : base.Subtract(v);
        }
        internal override object Subtract(float v)
        {
            return (v - m_value);
        }
        internal override object Subtract(double v)
        {
            return (v - m_value);
        }
        internal override object Subtract(decimal v)
        {
            return (v - m_value);
        }
        #endregion
        #region Multiply
        internal override object Multiply(ArithmeticLiteral v)
        {
            return v.Multiply(m_value);
        }
        internal override object Multiply()
        {
            return null;
        }
        internal override object Multiply(int v)
        {
            return (v * m_value);
        }
        internal override object Multiply(long v)
        {
            return (v * m_value);
        }
        internal override object Multiply(ushort v)
        {
            return (v * m_value);
        }
        internal override object Multiply(uint v)
        {
            return (v * m_value);
        }
        internal override object Multiply(ulong v)
        {
            // this should only happen when using a constant (+ve) int and an ulong
            // if that's not the case, you get an error
            return (m_value >= 0) ? (v * (ulong)m_value) : base.Multiply(v);
        }
        internal override object Multiply(float v)
        {
            return (v * m_value);
        }
        internal override object Multiply(double v)
        {
            return (v * m_value);
        }
        internal override object Multiply(decimal v)
        {
            return (v * m_value);
        }
        #endregion
        #region Divide
        internal override object Divide(ArithmeticLiteral v)
        {
            return v.Divide(m_value);
        }
        internal override object Divide()
        {
            return null;
        }
        internal override object Divide(int v)
        {
            return (v / m_value);
        }
        internal override object Divide(long v)
        {
            return (v / m_value);
        }
        internal override object Divide(ushort v)
        {
            return (v / m_value);
        }
        internal override object Divide(uint v)
        {
            return (v / m_value);
        }
        internal override object Divide(ulong v)
        {
            // this should only happen when using a constant (+ve) int and an ulong
            // if that's not the case, you get an error
            return (m_value >= 0) ? (v / (ulong)m_value) : base.Divide(v);
        }
        internal override object Divide(float v)
        {
            return (v / m_value);
        }
        internal override object Divide(double v)
        {
            return (v / m_value);
        }
        internal override object Divide(decimal v)
        {
            return (v / m_value);
        }
        #endregion
        #region Modulus
        internal override object Modulus(ArithmeticLiteral v)
        {
            return v.Modulus(m_value);
        }
        internal override object Modulus()
        {
            return null;
        }
        internal override object Modulus(int v)
        {
            return (v % m_value);
        }
        internal override object Modulus(long v)
        {
            return (v % m_value);
        }
        internal override object Modulus(ushort v)
        {
            return (v % m_value);
        }
        internal override object Modulus(uint v)
        {
            return (v % m_value);
        }
        internal override object Modulus(ulong v)
        {
            // this should only happen when using a constant (+ve) int and an ulong
            // if that's not the case, you get an error
            return (m_value >= 0) ? (v % (ulong)m_value) : base.Modulus(v);
        }
        internal override object Modulus(float v)
        {
            return (v % m_value);
        }
        internal override object Modulus(double v)
        {
            return (v % m_value);
        }
        internal override object Modulus(decimal v)
        {
            return (v % m_value);
        }
        #endregion
        #region BitAnd
        internal override object BitAnd(ArithmeticLiteral v)
        {
            return v.BitAnd(m_value);
        }
        internal override object BitAnd()
        {
            return null;
        }
        internal override object BitAnd(int v)
        {
            return (v & m_value);
        }
        internal override object BitAnd(long v)
        {
            return (v & m_value);
        }
        internal override object BitAnd(ushort v)
        {
            return (v & m_value);
        }
        internal override object BitAnd(uint v)
        {
            return (v & m_value);
        }
        internal override object BitAnd(ulong v)
        {
            // this should only happen when using a constant (+ve) int and an ulong
            // if that's not the case, you get an error
            return (m_value >= 0) ? (v & (ulong)m_value) : base.BitAnd(v);
        }
        #endregion
        #region BitOr
        internal override object BitOr(ArithmeticLiteral v)
        {
            return v.BitOr(m_value);
        }
        internal override object BitOr()
        {
            return null;
        }
        internal override object BitOr(int v)
        {
            return (v | m_value);
        }
        internal override object BitOr(long v)
        {
            long l = m_value;
            return (v | l);
        }
        internal override object BitOr(ushort v)
        {
            return (v | m_value);
        }
        internal override object BitOr(uint v)
        {
            long l = m_value;
            return (v | l);
        }
        internal override object BitOr(ulong v)
        {
            // this should only happen when using a constant (+ve) int and an ulong
            // if that's not the case, you get an error
            long l = m_value;
            return (l >= 0) ? (v | (ulong)l) : base.BitOr(v);
        }
        #endregion
    }
    #endregion

    #region LongArithmeticLiteral Class
    internal class LongArithmeticLiteral : ArithmeticLiteral
    {
        private long m_value;
        internal LongArithmeticLiteral(long literalValue)
        {
            m_value = literalValue;
            m_type = typeof(long);
        }

        internal override object Value
        {
            get { return m_value; }
        }
        #region Add
        internal override object Add(ArithmeticLiteral v)
        {
            return v.Add(m_value);
        }
        internal override object Add()
        {
            return null;
        }
        internal override object Add(int v)
        {
            return (v + m_value);
        }
        internal override object Add(long v)
        {
            return (v + m_value);
        }
        internal override object Add(char v)
        {
            return (v + m_value);
        }
        internal override object Add(ushort v)
        {
            return (v + m_value);
        }
        internal override object Add(uint v)
        {
            return (v + m_value);
        }
        internal override object Add(ulong v)
        {
            // this should only happen when using a constant (+ve) long and an ulong
            // if that's not the case, you get an error
            return (m_value >= 0) ? (v + (ulong)m_value) : base.Add(v);
        }
        internal override object Add(float v)
        {
            return (v + m_value);
        }
        internal override object Add(double v)
        {
            return (v + m_value);
        }
        internal override object Add(decimal v)
        {
            return (v + m_value);
        }
        internal override object Add(string v)
        {
            return (v + m_value.ToString(CultureInfo.CurrentCulture));
        }
        #endregion
        #region Subtract
        internal override object Subtract(ArithmeticLiteral v)
        {
            return v.Subtract(m_value);
        }
        internal override object Subtract()
        {
            return null;
        }
        internal override object Subtract(int v)
        {
            return (v - m_value);
        }
        internal override object Subtract(long v)
        {
            return (v - m_value);
        }
        internal override object Subtract(ushort v)
        {
            return (v - m_value);
        }
        internal override object Subtract(uint v)
        {
            return (v - m_value);
        }
        internal override object Subtract(ulong v)
        {
            // this should only happen when using a constant (+ve) long and an ulong
            // if that's not the case, you get an error
            return (m_value >= 0) ? (v - (ulong)m_value) : base.Subtract(v);
        }
        internal override object Subtract(float v)
        {
            return (v - m_value);
        }
        internal override object Subtract(double v)
        {
            return (v - m_value);
        }
        internal override object Subtract(decimal v)
        {
            return (v - m_value);
        }
        #endregion
        #region Multiply
        internal override object Multiply(ArithmeticLiteral v)
        {
            return v.Multiply(m_value);
        }
        internal override object Multiply()
        {
            return null;
        }
        internal override object Multiply(int v)
        {
            return (v * m_value);
        }
        internal override object Multiply(long v)
        {
            return (v * m_value);
        }
        internal override object Multiply(ushort v)
        {
            return (v * m_value);
        }
        internal override object Multiply(uint v)
        {
            return (v * m_value);
        }
        internal override object Multiply(ulong v)
        {
            // this should only happen when using a constant (+ve) long and an ulong
            // if that's not the case, you get an error
            return (m_value >= 0) ? (v * (ulong)m_value) : base.Multiply(v);
        }
        internal override object Multiply(float v)
        {
            return (v * m_value);
        }
        internal override object Multiply(double v)
        {
            return (v * m_value);
        }
        internal override object Multiply(decimal v)
        {
            return (v * m_value);
        }
        #endregion
        #region Divide
        internal override object Divide(ArithmeticLiteral v)
        {
            return v.Divide(m_value);
        }
        internal override object Divide()
        {
            return null;
        }
        internal override object Divide(int v)
        {
            return (v / m_value);
        }
        internal override object Divide(long v)
        {
            return (v / m_value);
        }
        internal override object Divide(ushort v)
        {
            return (v / m_value);
        }
        internal override object Divide(uint v)
        {
            return (v / m_value);
        }
        internal override object Divide(ulong v)
        {
            // this should only happen when using a constant (+ve) long and an ulong
            // if that's not the case, you get an error
            return (m_value >= 0) ? (v / (ulong)m_value) : base.Divide(v);
        }
        internal override object Divide(float v)
        {
            return (v / m_value);
        }
        internal override object Divide(double v)
        {
            return (v / m_value);
        }
        internal override object Divide(decimal v)
        {
            return (v / m_value);
        }
        #endregion
        #region Modulus
        internal override object Modulus(ArithmeticLiteral v)
        {
            return v.Modulus(m_value);
        }
        internal override object Modulus()
        {
            return null;
        }
        internal override object Modulus(int v)
        {
            return (v % m_value);
        }
        internal override object Modulus(long v)
        {
            return (v % m_value);
        }
        internal override object Modulus(ushort v)
        {
            return (v % m_value);
        }
        internal override object Modulus(uint v)
        {
            return (v % m_value);
        }
        internal override object Modulus(ulong v)
        {
            // this should only happen when using a constant (+ve) long and an ulong
            // if that's not the case, you get an error
            return (m_value >= 0) ? (v % (ulong)m_value) : base.Modulus(v);
        }
        internal override object Modulus(float v)
        {
            return (v % m_value);
        }
        internal override object Modulus(double v)
        {
            return (v % m_value);
        }
        internal override object Modulus(decimal v)
        {
            return (v % m_value);
        }
        #endregion
        #region BitAnd
        internal override object BitAnd(ArithmeticLiteral v)
        {
            return v.BitAnd(m_value);
        }
        internal override object BitAnd()
        {
            return null;
        }
        internal override object BitAnd(int v)
        {
            return (v & m_value);
        }
        internal override object BitAnd(long v)
        {
            return (v & m_value);
        }
        internal override object BitAnd(ushort v)
        {
            return (v & m_value);
        }
        internal override object BitAnd(uint v)
        {
            return (v & m_value);
        }
        internal override object BitAnd(ulong v)
        {
            // this should only happen when using a constant (+ve) long and an ulong
            // if that's not the case, you get an error
            return (m_value >= 0) ? (v & (ulong)m_value) : base.BitAnd(v);
        }
        #endregion
        #region BitOr
        internal override object BitOr(ArithmeticLiteral v)
        {
            return v.BitOr(m_value);
        }
        internal override object BitOr()
        {
            return null;
        }
        internal override object BitOr(int v)
        {
            long l = v;
            return (l | m_value);
        }
        internal override object BitOr(long v)
        {
            return (v | m_value);
        }
        internal override object BitOr(ushort v)
        {
            return (v | m_value);
        }
        internal override object BitOr(uint v)
        {
            return (v | m_value);
        }
        internal override object BitOr(ulong v)
        {
            // this should only happen when using a constant (+ve) long and an ulong
            // if that's not the case, you get an error
            return (m_value >= 0) ? (v | (ulong)m_value) : base.BitOr(v);
        }
        #endregion
    }
    #endregion

    #region CharArithmeticLiteral Class
    internal class CharArithmeticLiteral : ArithmeticLiteral
    {
        private char m_value;
        internal CharArithmeticLiteral(char literalValue)
        {
            m_value = literalValue;
            m_type = typeof(char);
        }

        internal override object Value
        {
            get { return m_value; }
        }
        #region Add
        internal override object Add(ArithmeticLiteral v)
        {
            return v.Add(m_value);
        }
        internal override object Add()
        {
            return null;
        }
        internal override object Add(int v)
        {
            return (v + m_value);
        }
        internal override object Add(long v)
        {
            return (v + m_value);
        }
        internal override object Add(char v)
        {
            return (v + m_value);
        }
        internal override object Add(ushort v)
        {
            return (v + m_value);
        }
        internal override object Add(uint v)
        {
            return (v + m_value);
        }
        internal override object Add(ulong v)
        {
            return (v + m_value);
        }
        internal override object Add(float v)
        {
            return (v + m_value);
        }
        internal override object Add(double v)
        {
            return (v + m_value);
        }
        internal override object Add(decimal v)
        {
            return (v + m_value);
        }
        internal override object Add(string v)
        {
            return (v + m_value.ToString(CultureInfo.CurrentCulture));
        }
        #endregion
        #region Subtract
        internal override object Subtract(ArithmeticLiteral v)
        {
            return v.Subtract(m_value);
        }
        internal override object Subtract()
        {
            return null;
        }
        internal override object Subtract(int v)
        {
            return (v - m_value);
        }
        internal override object Subtract(long v)
        {
            return (v - m_value);
        }
        internal override object Subtract(ushort v)
        {
            return (v - m_value);
        }
        internal override object Subtract(uint v)
        {
            return (v - m_value);
        }
        internal override object Subtract(ulong v)
        {
            return (v - m_value);
        }
        internal override object Subtract(float v)
        {
            return (v - m_value);
        }
        internal override object Subtract(double v)
        {
            return (v - m_value);
        }
        internal override object Subtract(decimal v)
        {
            return (v - m_value);
        }
        #endregion
        #region Multiply
        internal override object Multiply(ArithmeticLiteral v)
        {
            return v.Multiply(m_value);
        }
        internal override object Multiply()
        {
            return null;
        }
        internal override object Multiply(int v)
        {
            return (v * m_value);
        }
        internal override object Multiply(long v)
        {
            return (v * m_value);
        }
        internal override object Multiply(ushort v)
        {
            return (v * m_value);
        }
        internal override object Multiply(uint v)
        {
            return (v * m_value);
        }
        internal override object Multiply(ulong v)
        {
            return (v * m_value);
        }
        internal override object Multiply(float v)
        {
            return (v * m_value);
        }
        internal override object Multiply(double v)
        {
            return (v * m_value);
        }
        internal override object Multiply(decimal v)
        {
            return (v * m_value);
        }
        #endregion
        #region Divide
        internal override object Divide(ArithmeticLiteral v)
        {
            return v.Divide(m_value);
        }
        internal override object Divide()
        {
            return null;
        }
        internal override object Divide(int v)
        {
            return (v / m_value);
        }
        internal override object Divide(long v)
        {
            return (v / m_value);
        }
        internal override object Divide(ushort v)
        {
            return (v / m_value);
        }
        internal override object Divide(uint v)
        {
            return (v / m_value);
        }
        internal override object Divide(ulong v)
        {
            return (v / m_value);
        }
        internal override object Divide(float v)
        {
            return (v / m_value);
        }
        internal override object Divide(double v)
        {
            return (v / m_value);
        }
        internal override object Divide(decimal v)
        {
            return (v / m_value);
        }
        #endregion
        #region Modulus
        internal override object Modulus(ArithmeticLiteral v)
        {
            return v.Modulus(m_value);
        }
        internal override object Modulus()
        {
            return null;
        }
        internal override object Modulus(int v)
        {
            return (v % m_value);
        }
        internal override object Modulus(long v)
        {
            return (v % m_value);
        }
        internal override object Modulus(ushort v)
        {
            return (v % m_value);
        }
        internal override object Modulus(uint v)
        {
            return (v % m_value);
        }
        internal override object Modulus(ulong v)
        {
            return (v % m_value);
        }
        internal override object Modulus(float v)
        {
            return (v % m_value);
        }
        internal override object Modulus(double v)
        {
            return (v % m_value);
        }
        internal override object Modulus(decimal v)
        {
            return (v % m_value);
        }
        #endregion
        #region BitAnd
        internal override object BitAnd(ArithmeticLiteral v)
        {
            return v.BitAnd(m_value);
        }
        internal override object BitAnd()
        {
            return null;
        }
        internal override object BitAnd(int v)
        {
            return (v & m_value);
        }
        internal override object BitAnd(long v)
        {
            return (v & m_value);
        }
        internal override object BitAnd(ushort v)
        {
            return (v & m_value);
        }
        internal override object BitAnd(uint v)
        {
            return (v & m_value);
        }
        internal override object BitAnd(ulong v)
        {
            return (v & m_value);
        }
        #endregion
        #region BitOr
        internal override object BitOr(ArithmeticLiteral v)
        {
            return v.BitOr(m_value);
        }
        internal override object BitOr()
        {
            return null;
        }
        internal override object BitOr(int v)
        {
            return (v | m_value);
        }
        internal override object BitOr(long v)
        {
            return (v | m_value);
        }
        internal override object BitOr(ushort v)
        {
            return (v | m_value);
        }
        internal override object BitOr(uint v)
        {
            return (v | m_value);
        }
        internal override object BitOr(ulong v)
        {
            return (v | m_value);
        }
        #endregion
    }
    #endregion

    #region UShortArithmeticLiteral Class
    internal class UShortArithmeticLiteral : ArithmeticLiteral
    {
        private ushort m_value;
        internal UShortArithmeticLiteral(ushort literalValue)
        {
            m_value = literalValue;
            m_type = typeof(ushort);
        }

        internal override object Value
        {
            get { return m_value; }
        }
        #region Add
        internal override object Add(ArithmeticLiteral v)
        {
            return v.Add(m_value);
        }
        internal override object Add()
        {
            return null;
        }
        internal override object Add(int v)
        {
            return (v + m_value);
        }
        internal override object Add(long v)
        {
            return (v + m_value);
        }
        internal override object Add(char v)
        {
            return (v + m_value);
        }
        internal override object Add(ushort v)
        {
            return (v + m_value);
        }
        internal override object Add(uint v)
        {
            return (v + m_value);
        }
        internal override object Add(ulong v)
        {
            return (v + m_value);
        }
        internal override object Add(float v)
        {
            return (v + m_value);
        }
        internal override object Add(double v)
        {
            return (v + m_value);
        }
        internal override object Add(decimal v)
        {
            return (v + m_value);
        }
        internal override object Add(string v)
        {
            return (v + m_value.ToString(CultureInfo.CurrentCulture));
        }
        #endregion
        #region Subtract
        internal override object Subtract(ArithmeticLiteral v)
        {
            return v.Subtract(m_value);
        }
        internal override object Subtract()
        {
            return null;
        }
        internal override object Subtract(int v)
        {
            return (v - m_value);
        }
        internal override object Subtract(long v)
        {
            return (v - m_value);
        }
        internal override object Subtract(ushort v)
        {
            return (v - m_value);
        }
        internal override object Subtract(uint v)
        {
            return (v - m_value);
        }
        internal override object Subtract(ulong v)
        {
            return (v - m_value);
        }
        internal override object Subtract(float v)
        {
            return (v - m_value);
        }
        internal override object Subtract(double v)
        {
            return (v - m_value);
        }
        internal override object Subtract(decimal v)
        {
            return (v - m_value);
        }
        #endregion
        #region Multiply
        internal override object Multiply(ArithmeticLiteral v)
        {
            return v.Multiply(m_value);
        }
        internal override object Multiply()
        {
            return null;
        }
        internal override object Multiply(int v)
        {
            return (v * m_value);
        }
        internal override object Multiply(long v)
        {
            return (v * m_value);
        }
        internal override object Multiply(ushort v)
        {
            return (v * m_value);
        }
        internal override object Multiply(uint v)
        {
            return (v * m_value);
        }
        internal override object Multiply(ulong v)
        {
            return (v * m_value);
        }
        internal override object Multiply(float v)
        {
            return (v * m_value);
        }
        internal override object Multiply(double v)
        {
            return (v * m_value);
        }
        internal override object Multiply(decimal v)
        {
            return (v * m_value);
        }
        #endregion
        #region Divide
        internal override object Divide(ArithmeticLiteral v)
        {
            return v.Divide(m_value);
        }
        internal override object Divide()
        {
            return null;
        }
        internal override object Divide(int v)
        {
            return (v / m_value);
        }
        internal override object Divide(long v)
        {
            return (v / m_value);
        }
        internal override object Divide(ushort v)
        {
            return (v / m_value);
        }
        internal override object Divide(uint v)
        {
            return (v / m_value);
        }
        internal override object Divide(ulong v)
        {
            return (v / m_value);
        }
        internal override object Divide(float v)
        {
            return (v / m_value);
        }
        internal override object Divide(double v)
        {
            return (v / m_value);
        }
        internal override object Divide(decimal v)
        {
            return (v / m_value);
        }
        #endregion
        #region Modulus
        internal override object Modulus(ArithmeticLiteral v)
        {
            return v.Modulus(m_value);
        }
        internal override object Modulus()
        {
            return null;
        }
        internal override object Modulus(int v)
        {
            return (v % m_value);
        }
        internal override object Modulus(long v)
        {
            return (v % m_value);
        }
        internal override object Modulus(ushort v)
        {
            return (v % m_value);
        }
        internal override object Modulus(uint v)
        {
            return (v % m_value);
        }
        internal override object Modulus(ulong v)
        {
            return (v % m_value);
        }
        internal override object Modulus(float v)
        {
            return (v % m_value);
        }
        internal override object Modulus(double v)
        {
            return (v % m_value);
        }
        internal override object Modulus(decimal v)
        {
            return (v % m_value);
        }
        #endregion
        #region BitAnd
        internal override object BitAnd(ArithmeticLiteral v)
        {
            return v.BitAnd(m_value);
        }
        internal override object BitAnd()
        {
            return null;
        }
        internal override object BitAnd(int v)
        {
            return (v & m_value);
        }
        internal override object BitAnd(long v)
        {
            return (v & m_value);
        }
        internal override object BitAnd(ushort v)
        {
            return (v & m_value);
        }
        internal override object BitAnd(uint v)
        {
            return (v & m_value);
        }
        internal override object BitAnd(ulong v)
        {
            return (v & m_value);
        }
        #endregion
        #region BitOr
        internal override object BitOr(ArithmeticLiteral v)
        {
            return v.BitOr(m_value);
        }
        internal override object BitOr()
        {
            return null;
        }
        internal override object BitOr(int v)
        {
            return (v | m_value);
        }
        internal override object BitOr(long v)
        {
            return (v | m_value);
        }
        internal override object BitOr(ushort v)
        {
            return (v | m_value);
        }
        internal override object BitOr(uint v)
        {
            return (v | m_value);
        }
        internal override object BitOr(ulong v)
        {
            return (v | m_value);
        }
        #endregion
    }
    #endregion

    #region UIntArithmeticLiteral Class
    internal class UIntArithmeticLiteral : ArithmeticLiteral
    {
        private uint m_value;
        internal UIntArithmeticLiteral(uint literalValue)
        {
            m_value = literalValue;
            m_type = typeof(uint);
        }
        internal override object Value
        {
            get { return m_value; }
        }
        #region Add
        internal override object Add(ArithmeticLiteral v)
        {
            return v.Add(m_value);
        }
        internal override object Add()
        {
            return null;
        }
        internal override object Add(int v)
        {
            return (v + m_value);
        }
        internal override object Add(long v)
        {
            return (v + m_value);
        }
        internal override object Add(char v)
        {
            return (v + m_value);
        }
        internal override object Add(ushort v)
        {
            return (v + m_value);
        }
        internal override object Add(uint v)
        {
            return (v + m_value);
        }
        internal override object Add(ulong v)
        {
            return (v + m_value);
        }
        internal override object Add(float v)
        {
            return (v + m_value);
        }
        internal override object Add(double v)
        {
            return (v + m_value);
        }
        internal override object Add(decimal v)
        {
            return (v + m_value);
        }
        internal override object Add(string v)
        {
            return (v + m_value.ToString(CultureInfo.CurrentCulture));
        }
        #endregion
        #region Subtract
        internal override object Subtract(ArithmeticLiteral v)
        {
            return v.Subtract(m_value);
        }
        internal override object Subtract()
        {
            return null;
        }
        internal override object Subtract(int v)
        {
            return (v - m_value);
        }
        internal override object Subtract(long v)
        {
            return (v - m_value);
        }
        internal override object Subtract(ushort v)
        {
            return (v - m_value);
        }
        internal override object Subtract(uint v)
        {
            return (v - m_value);
        }
        internal override object Subtract(ulong v)
        {
            return (v - m_value);
        }
        internal override object Subtract(float v)
        {
            return (v - m_value);
        }
        internal override object Subtract(double v)
        {
            return (v - m_value);
        }
        internal override object Subtract(decimal v)
        {
            return (v - m_value);
        }
        #endregion
        #region Multiply
        internal override object Multiply(ArithmeticLiteral v)
        {
            return v.Multiply(m_value);
        }
        internal override object Multiply()
        {
            return null;
        }
        internal override object Multiply(int v)
        {
            return (v * m_value);
        }
        internal override object Multiply(long v)
        {
            return (v * m_value);
        }
        internal override object Multiply(ushort v)
        {
            return (v * m_value);
        }
        internal override object Multiply(uint v)
        {
            return (v * m_value);
        }
        internal override object Multiply(ulong v)
        {
            return (v * m_value);
        }
        internal override object Multiply(float v)
        {
            return (v * m_value);
        }
        internal override object Multiply(double v)
        {
            return (v * m_value);
        }
        internal override object Multiply(decimal v)
        {
            return (v * m_value);
        }
        #endregion
        #region Divide
        internal override object Divide(ArithmeticLiteral v)
        {
            return v.Divide(m_value);
        }
        internal override object Divide()
        {
            return null;
        }
        internal override object Divide(int v)
        {
            return (v / m_value);
        }
        internal override object Divide(long v)
        {
            return (v / m_value);
        }
        internal override object Divide(ushort v)
        {
            return (v / m_value);
        }
        internal override object Divide(uint v)
        {
            return (v / m_value);
        }
        internal override object Divide(ulong v)
        {
            return (v / m_value);
        }
        internal override object Divide(float v)
        {
            return (v / m_value);
        }
        internal override object Divide(double v)
        {
            return (v / m_value);
        }
        internal override object Divide(decimal v)
        {
            return (v / m_value);
        }
        #endregion
        #region Modulus
        internal override object Modulus(ArithmeticLiteral v)
        {
            return v.Modulus(m_value);
        }
        internal override object Modulus()
        {
            return null;
        }
        internal override object Modulus(int v)
        {
            return (v % m_value);
        }
        internal override object Modulus(long v)
        {
            return (v % m_value);
        }
        internal override object Modulus(ushort v)
        {
            return (v % m_value);
        }
        internal override object Modulus(uint v)
        {
            return (v % m_value);
        }
        internal override object Modulus(ulong v)
        {
            return (v % m_value);
        }
        internal override object Modulus(float v)
        {
            return (v % m_value);
        }
        internal override object Modulus(double v)
        {
            return (v % m_value);
        }
        internal override object Modulus(decimal v)
        {
            return (v % m_value);
        }
        #endregion
        #region BitAnd
        internal override object BitAnd(ArithmeticLiteral v)
        {
            return v.BitAnd(m_value);
        }
        internal override object BitAnd()
        {
            return null;
        }
        internal override object BitAnd(int v)
        {
            return (v & m_value);
        }
        internal override object BitAnd(long v)
        {
            return (v & m_value);
        }
        internal override object BitAnd(ushort v)
        {
            return (v & m_value);
        }
        internal override object BitAnd(uint v)
        {
            return (v & m_value);
        }
        internal override object BitAnd(ulong v)
        {
            return (v & m_value);
        }
        #endregion
        #region BitOr
        internal override object BitOr(ArithmeticLiteral v)
        {
            return v.BitOr(m_value);
        }
        internal override object BitOr()
        {
            return null;
        }
        internal override object BitOr(int v)
        {
            long l = v;
            return (l | m_value);
        }
        internal override object BitOr(long v)
        {
            return (v | m_value);
        }
        internal override object BitOr(ushort v)
        {
            return (v | m_value);
        }
        internal override object BitOr(uint v)
        {
            return (v | m_value);
        }
        internal override object BitOr(ulong v)
        {
            return (v | m_value);
        }
        #endregion
    }
    #endregion

    #region ULongArithmeticLiteral Class
    internal class ULongArithmeticLiteral : ArithmeticLiteral
    {
        private ulong m_value;
        internal ULongArithmeticLiteral(ulong literalValue)
        {
            m_value = literalValue;
            m_type = typeof(ulong);
        }
        internal override object Value
        {
            get { return m_value; }
        }
        #region Add
        internal override object Add(ArithmeticLiteral v)
        {
            return v.Add(m_value);
        }
        internal override object Add()
        {
            return null;
        }
        internal override object Add(int v)
        {
            // this should only happen when using a constant (+ve) int and an ulong
            // if that's not the case, you get an error
            return (v >= 0) ? ((ulong)v + m_value) : base.Add(v);
        }
        internal override object Add(long v)
        {
            // this should only happen when using a constant (+ve) long and an ulong
            // if that's not the case, you get an error
            return (v >= 0) ? ((ulong)v + m_value) : base.Add(v);
        }
        internal override object Add(char v)
        {
            return (v + m_value);
        }
        internal override object Add(ushort v)
        {
            return (v + m_value);
        }
        internal override object Add(uint v)
        {
            return (v + m_value);
        }
        internal override object Add(ulong v)
        {
            return (v + m_value);
        }
        internal override object Add(float v)
        {
            return (v + m_value);
        }
        internal override object Add(double v)
        {
            return (v + m_value);
        }
        internal override object Add(decimal v)
        {
            return (v + m_value);
        }
        internal override object Add(string v)
        {
            return (v + m_value.ToString(CultureInfo.CurrentCulture));
        }
        #endregion
        #region Subtract
        internal override object Subtract(ArithmeticLiteral v)
        {
            return v.Subtract(m_value);
        }
        internal override object Subtract()
        {
            return null;
        }
        internal override object Subtract(int v)
        {
            // this should only happen when using a constant (+ve) int and an ulong
            // if that's not the case, you get an error
            return (v >= 0) ? ((ulong)v - m_value) : base.Subtract(v);
        }
        internal override object Subtract(long v)
        {
            // this should only happen when using a constant (+ve) long and an ulong
            // if that's not the case, you get an error
            return (v >= 0) ? ((ulong)v - m_value) : base.Subtract(v);
        }
        internal override object Subtract(ushort v)
        {
            return (v - m_value);
        }
        internal override object Subtract(uint v)
        {
            return (v - m_value);
        }
        internal override object Subtract(ulong v)
        {
            return (v - m_value);
        }
        internal override object Subtract(float v)
        {
            return (v - m_value);
        }
        internal override object Subtract(double v)
        {
            return (v - m_value);
        }
        internal override object Subtract(decimal v)
        {
            return (v - m_value);
        }
        #endregion
        #region Multiply
        internal override object Multiply(ArithmeticLiteral v)
        {
            return v.Multiply(m_value);
        }
        internal override object Multiply()
        {
            return null;
        }
        internal override object Multiply(int v)
        {
            // this should only happen when using a constant (+ve) int and an ulong
            // if that's not the case, you get an error
            return (v >= 0) ? ((ulong)v * m_value) : base.Multiply(v);
        }
        internal override object Multiply(long v)
        {
            // this should only happen when using a constant (+ve) long and an ulong
            // if that's not the case, you get an error
            return (v >= 0) ? ((ulong)v * m_value) : base.Multiply(v);
        }
        internal override object Multiply(ushort v)
        {
            return (v * m_value);
        }
        internal override object Multiply(uint v)
        {
            return (v * m_value);
        }
        internal override object Multiply(ulong v)
        {
            return (v * m_value);
        }
        internal override object Multiply(float v)
        {
            return (v * m_value);
        }
        internal override object Multiply(double v)
        {
            return (v * m_value);
        }
        internal override object Multiply(decimal v)
        {
            return (v * m_value);
        }
        #endregion
        #region Divide
        internal override object Divide(ArithmeticLiteral v)
        {
            return v.Divide(m_value);
        }
        internal override object Divide()
        {
            return null;
        }
        internal override object Divide(int v)
        {
            // this should only happen when using a constant (+ve) int and an ulong
            // if that's not the case, you get an error
            return (v >= 0) ? ((ulong)v / m_value) : base.Divide(v);
        }
        internal override object Divide(long v)
        {
            // this should only happen when using a constant (+ve) long and an ulong
            // if that's not the case, you get an error
            return (v >= 0) ? ((ulong)v / m_value) : base.Divide(v);
        }
        internal override object Divide(ushort v)
        {
            return (v / m_value);
        }
        internal override object Divide(uint v)
        {
            return (v / m_value);
        }
        internal override object Divide(ulong v)
        {
            return (v / m_value);
        }
        internal override object Divide(float v)
        {
            return (v / m_value);
        }
        internal override object Divide(double v)
        {
            return (v / m_value);
        }
        internal override object Divide(decimal v)
        {
            return (v / m_value);
        }
        #endregion
        #region Modulus
        internal override object Modulus(ArithmeticLiteral v)
        {
            return v.Modulus(m_value);
        }
        internal override object Modulus()
        {
            return null;
        }
        internal override object Modulus(int v)
        {
            // this should only happen when using a constant (+ve) int and an ulong
            // if that's not the case, you get an error
            return (v >= 0) ? ((ulong)v % m_value) : base.Modulus(v);
        }
        internal override object Modulus(long v)
        {
            // this should only happen when using a constant (+ve) long and an ulong
            // if that's not the case, you get an error
            return (v >= 0) ? ((ulong)v % m_value) : base.Modulus(v);
        }
        internal override object Modulus(ushort v)
        {
            return (v % m_value);
        }
        internal override object Modulus(uint v)
        {
            return (v % m_value);
        }
        internal override object Modulus(ulong v)
        {
            return (v % m_value);
        }
        internal override object Modulus(float v)
        {
            return (v % m_value);
        }
        internal override object Modulus(double v)
        {
            return (v % m_value);
        }
        internal override object Modulus(decimal v)
        {
            return (v % m_value);
        }
        #endregion
        #region BitAnd
        internal override object BitAnd(ArithmeticLiteral v)
        {
            return v.BitAnd(m_value);
        }
        internal override object BitAnd()
        {
            return null;
        }
        internal override object BitAnd(int v)
        {
            // this should only happen when using a constant (+ve) int and an ulong
            // if that's not the case, you get an error
            return (v >= 0) ? ((ulong)v & m_value) : base.BitAnd(v);
        }
        internal override object BitAnd(long v)
        {
            // this should only happen when using a constant (+ve) long and an ulong
            // if that's not the case, you get an error
            return (v >= 0) ? ((ulong)v & m_value) : base.BitAnd(v);
        }
        internal override object BitAnd(ushort v)
        {
            return (v & m_value);
        }
        internal override object BitAnd(uint v)
        {
            return (v & m_value);
        }
        internal override object BitAnd(ulong v)
        {
            return (v & m_value);
        }
        #endregion
        #region BitOr
        internal override object BitOr(ArithmeticLiteral v)
        {
            return v.BitOr(m_value);
        }
        internal override object BitOr()
        {
            return null;
        }
        internal override object BitOr(int v)
        {
            // this should only happen when using a constant (+ve) int and an ulong
            // if that's not the case, you get an error
            long l = v;
            return (l >= 0) ? ((ulong)l | m_value) : base.BitOr(v);
        }
        internal override object BitOr(long v)
        {
            // this should only happen when using a constant (+ve) long and an ulong
            // if that's not the case, you get an error
            return (v >= 0) ? ((ulong)v | m_value) : base.BitOr(v);
        }
        internal override object BitOr(ushort v)
        {
            return (v | m_value);
        }
        internal override object BitOr(uint v)
        {
            return (v | m_value);
        }
        internal override object BitOr(ulong v)
        {
            return (v | m_value);
        }
        #endregion
    }
    #endregion

    #region FloatArithmeticLiteral Class
    internal class FloatArithmeticLiteral : ArithmeticLiteral
    {
        private float m_value;
        internal FloatArithmeticLiteral(float literalValue)
        {
            m_value = literalValue;
            m_type = typeof(float);
        }

        internal override object Value
        {
            get { return m_value; }
        }
        #region Add
        internal override object Add(ArithmeticLiteral v)
        {
            return v.Add(m_value);
        }
        internal override object Add()
        {
            return null;
        }
        internal override object Add(int v)
        {
            return (v + m_value);
        }
        internal override object Add(long v)
        {
            return (v + m_value);
        }
        internal override object Add(char v)
        {
            return (v + m_value);
        }
        internal override object Add(ushort v)
        {
            return (v + m_value);
        }
        internal override object Add(uint v)
        {
            return (v + m_value);
        }
        internal override object Add(ulong v)
        {
            return (v + m_value);
        }
        internal override object Add(float v)
        {
            return (v + m_value);
        }
        internal override object Add(double v)
        {
            return (v + m_value);
        }
        internal override object Add(string v)
        {
            return (v + m_value.ToString(CultureInfo.CurrentCulture));
        }
        #endregion
        #region Subtract
        internal override object Subtract(ArithmeticLiteral v)
        {
            return v.Subtract(m_value);
        }
        internal override object Subtract()
        {
            return null;
        }
        internal override object Subtract(int v)
        {
            return (v - m_value);
        }
        internal override object Subtract(long v)
        {
            return (v - m_value);
        }
        internal override object Subtract(ushort v)
        {
            return (v - m_value);
        }
        internal override object Subtract(uint v)
        {
            return (v - m_value);
        }
        internal override object Subtract(ulong v)
        {
            return (v - m_value);
        }
        internal override object Subtract(float v)
        {
            return (v - m_value);
        }
        internal override object Subtract(double v)
        {
            return (v - m_value);
        }
        #endregion
        #region Multiply
        internal override object Multiply(ArithmeticLiteral v)
        {
            return v.Multiply(m_value);
        }
        internal override object Multiply()
        {
            return null;
        }
        internal override object Multiply(int v)
        {
            return (v * m_value);
        }
        internal override object Multiply(long v)
        {
            return (v * m_value);
        }
        internal override object Multiply(ushort v)
        {
            return (v * m_value);
        }
        internal override object Multiply(uint v)
        {
            return (v * m_value);
        }
        internal override object Multiply(ulong v)
        {
            return (v * m_value);
        }
        internal override object Multiply(float v)
        {
            return (v * m_value);
        }
        internal override object Multiply(double v)
        {
            return (v * m_value);
        }
        #endregion
        #region Divide
        internal override object Divide(ArithmeticLiteral v)
        {
            return v.Divide(m_value);
        }
        internal override object Divide()
        {
            return null;
        }
        internal override object Divide(int v)
        {
            return (v / m_value);
        }
        internal override object Divide(long v)
        {
            return (v / m_value);
        }
        internal override object Divide(ushort v)
        {
            return (v / m_value);
        }
        internal override object Divide(uint v)
        {
            return (v / m_value);
        }
        internal override object Divide(ulong v)
        {
            return (v / m_value);
        }
        internal override object Divide(float v)
        {
            return (v / m_value);
        }
        internal override object Divide(double v)
        {
            return (v / m_value);
        }
        #endregion
        #region Modulus
        internal override object Modulus(ArithmeticLiteral v)
        {
            return v.Modulus(m_value);
        }
        internal override object Modulus()
        {
            return null;
        }
        internal override object Modulus(int v)
        {
            return (v % m_value);
        }
        internal override object Modulus(long v)
        {
            return (v % m_value);
        }
        internal override object Modulus(ushort v)
        {
            return (v % m_value);
        }
        internal override object Modulus(uint v)
        {
            return (v % m_value);
        }
        internal override object Modulus(ulong v)
        {
            return (v % m_value);
        }
        internal override object Modulus(float v)
        {
            return (v % m_value);
        }
        internal override object Modulus(double v)
        {
            return (v % m_value);
        }
        #endregion
    }
    #endregion

    #region DoubleArithmeticLiteral Class
    internal class DoubleArithmeticLiteral : ArithmeticLiteral
    {
        private double m_value;
        internal DoubleArithmeticLiteral(double literalValue)
        {
            m_value = literalValue;
            m_type = typeof(double);
        }

        internal override object Value
        {
            get { return m_value; }
        }
        #region Add
        internal override object Add(ArithmeticLiteral v)
        {
            return v.Add(m_value);
        }
        internal override object Add()
        {
            return null;
        }
        internal override object Add(int v)
        {
            return (v + m_value);
        }
        internal override object Add(long v)
        {
            return (v + m_value);
        }
        internal override object Add(char v)
        {
            return (v + m_value);
        }
        internal override object Add(ushort v)
        {
            return (v + m_value);
        }
        internal override object Add(uint v)
        {
            return (v + m_value);
        }
        internal override object Add(ulong v)
        {
            return (v + m_value);
        }
        internal override object Add(float v)
        {
            return (v + m_value);
        }
        internal override object Add(double v)
        {
            return (v + m_value);
        }
        internal override object Add(string v)
        {
            return (v + m_value.ToString(CultureInfo.CurrentCulture));
        }
        #endregion
        #region Subtract
        internal override object Subtract(ArithmeticLiteral v)
        {
            return v.Subtract(m_value);
        }
        internal override object Subtract()
        {
            return null;
        }
        internal override object Subtract(int v)
        {
            return (v - m_value);
        }
        internal override object Subtract(long v)
        {
            return (v - m_value);
        }
        internal override object Subtract(ushort v)
        {
            return (v - m_value);
        }
        internal override object Subtract(uint v)
        {
            return (v - m_value);
        }
        internal override object Subtract(ulong v)
        {
            return (v - m_value);
        }
        internal override object Subtract(float v)
        {
            return (v - m_value);
        }
        internal override object Subtract(double v)
        {
            return (v - m_value);
        }
        #endregion
        #region Multiply
        internal override object Multiply(ArithmeticLiteral v)
        {
            return v.Multiply(m_value);
        }
        internal override object Multiply()
        {
            return null;
        }
        internal override object Multiply(int v)
        {
            return (v * m_value);
        }
        internal override object Multiply(long v)
        {
            return (v * m_value);
        }
        internal override object Multiply(ushort v)
        {
            return (v * m_value);
        }
        internal override object Multiply(uint v)
        {
            return (v * m_value);
        }
        internal override object Multiply(ulong v)
        {
            return (v * m_value);
        }
        internal override object Multiply(float v)
        {
            return (v * m_value);
        }
        internal override object Multiply(double v)
        {
            return (v * m_value);
        }
        #endregion
        #region Divide
        internal override object Divide(ArithmeticLiteral v)
        {
            return v.Divide(m_value);
        }
        internal override object Divide()
        {
            return null;
        }
        internal override object Divide(int v)
        {
            return (v / m_value);
        }
        internal override object Divide(long v)
        {
            return (v / m_value);
        }
        internal override object Divide(ushort v)
        {
            return (v / m_value);
        }
        internal override object Divide(uint v)
        {
            return (v / m_value);
        }
        internal override object Divide(ulong v)
        {
            return (v / m_value);
        }
        internal override object Divide(float v)
        {
            return (v / m_value);
        }
        internal override object Divide(double v)
        {
            return (v / m_value);
        }
        #endregion
        #region Modulus
        internal override object Modulus(ArithmeticLiteral v)
        {
            return v.Modulus(m_value);
        }
        internal override object Modulus()
        {
            return null;
        }
        internal override object Modulus(int v)
        {
            return (v % m_value);
        }
        internal override object Modulus(long v)
        {
            return (v % m_value);
        }
        internal override object Modulus(ushort v)
        {
            return (v % m_value);
        }
        internal override object Modulus(uint v)
        {
            return (v % m_value);
        }
        internal override object Modulus(ulong v)
        {
            return (v % m_value);
        }
        internal override object Modulus(float v)
        {
            return (v % m_value);
        }
        internal override object Modulus(double v)
        {
            return (v % m_value);
        }
        #endregion
    }
    #endregion

    #region DecimalArithmeticLiteral Class
    internal class DecimalArithmeticLiteral : ArithmeticLiteral
    {
        private decimal m_value;
        internal DecimalArithmeticLiteral(decimal literalValue)
        {
            m_value = literalValue;
            m_type = typeof(decimal);
        }

        internal override object Value
        {
            get { return m_value; }
        }
        #region Add
        internal override object Add(ArithmeticLiteral v)
        {
            return v.Add(m_value);
        }
        internal override object Add()
        {
            return null;
        }
        internal override object Add(int v)
        {
            return (v + m_value);
        }
        internal override object Add(long v)
        {
            return (v + m_value);
        }
        internal override object Add(char v)
        {
            return (v + m_value);
        }
        internal override object Add(ushort v)
        {
            return (v + m_value);
        }
        internal override object Add(uint v)
        {
            return (v + m_value);
        }
        internal override object Add(ulong v)
        {
            return (v + m_value);
        }
        internal override object Add(decimal v)
        {
            return (v + m_value);
        }
        internal override object Add(string v)
        {
            return (v + m_value.ToString(CultureInfo.CurrentCulture));
        }
        #endregion
        #region Subtract
        internal override object Subtract(ArithmeticLiteral v)
        {
            return v.Subtract(m_value);
        }
        internal override object Subtract()
        {
            return null;
        }
        internal override object Subtract(int v)
        {
            return (v - m_value);
        }
        internal override object Subtract(long v)
        {
            return (v - m_value);
        }
        internal override object Subtract(ushort v)
        {
            return (v - m_value);
        }
        internal override object Subtract(uint v)
        {
            return (v - m_value);
        }
        internal override object Subtract(ulong v)
        {
            return (v - m_value);
        }
        internal override object Subtract(decimal v)
        {
            return (v - m_value);
        }
        #endregion
        #region Multiply
        internal override object Multiply(ArithmeticLiteral v)
        {
            return v.Multiply(m_value);
        }
        internal override object Multiply()
        {
            return null;
        }
        internal override object Multiply(int v)
        {
            return (v * m_value);
        }
        internal override object Multiply(long v)
        {
            return (v * m_value);
        }
        internal override object Multiply(ushort v)
        {
            return (v * m_value);
        }
        internal override object Multiply(uint v)
        {
            return (v * m_value);
        }
        internal override object Multiply(ulong v)
        {
            return (v * m_value);
        }
        internal override object Multiply(decimal v)
        {
            return (v * m_value);
        }
        #endregion
        #region Divide
        internal override object Divide(ArithmeticLiteral v)
        {
            return v.Divide(m_value);
        }
        internal override object Divide()
        {
            return null;
        }
        internal override object Divide(int v)
        {
            return (v / m_value);
        }
        internal override object Divide(long v)
        {
            return (v / m_value);
        }
        internal override object Divide(ushort v)
        {
            return (v / m_value);
        }
        internal override object Divide(uint v)
        {
            return (v / m_value);
        }
        internal override object Divide(ulong v)
        {
            return (v / m_value);
        }
        internal override object Divide(decimal v)
        {
            return (v / m_value);
        }
        #endregion
        #region Modulus
        internal override object Modulus(ArithmeticLiteral v)
        {
            return v.Modulus(m_value);
        }
        internal override object Modulus()
        {
            return null;
        }
        internal override object Modulus(int v)
        {
            return (v % m_value);
        }
        internal override object Modulus(long v)
        {
            return (v % m_value);
        }
        internal override object Modulus(ushort v)
        {
            return (v % m_value);
        }
        internal override object Modulus(uint v)
        {
            return (v % m_value);
        }
        internal override object Modulus(ulong v)
        {
            return (v % m_value);
        }
        internal override object Modulus(decimal v)
        {
            return (v % m_value);
        }
        #endregion
    }
    #endregion

    #region BooleanArithmeticLiteral Class
    internal class BooleanArithmeticLiteral : ArithmeticLiteral
    {
        private bool m_value;
        internal BooleanArithmeticLiteral(bool literalValue)
        {
            m_value = literalValue;
            m_type = typeof(bool);
        }

        internal override object Value
        {
            get { return m_value; }
        }
        #region Add
        internal override object Add(ArithmeticLiteral v)
        {
            return v.Add(m_value);
        }
        internal override object Add()
        {
            return null;
        }
        internal override object Add(string v)
        {
            return (v + m_value.ToString(CultureInfo.CurrentCulture));
        }
        #endregion
        #region BitAnd
        internal override object BitAnd(ArithmeticLiteral v)
        {
            return v.BitAnd(m_value);
        }
        internal override object BitAnd()
        {
            // special case from section 24.3.6 on bool? type
            return (m_value == false) ? (object)false : null;
        }
        internal override object BitAnd(bool v)
        {
            return (v & m_value);
        }
        #endregion
        #region BitOr
        internal override object BitOr(ArithmeticLiteral v)
        {
            return v.BitOr(m_value);
        }
        internal override object BitOr()
        {
            // special case from section 24.3.6 on bool? type
            return (m_value == true) ? (object)true : null;
        }
        internal override object BitOr(bool v)
        {
            return (v | m_value);
        }
        #endregion
    }
    #endregion

    #region StringArithmeticLiteral Class
    internal class StringArithmeticLiteral : ArithmeticLiteral
    {
        private string m_value;
        internal StringArithmeticLiteral(string literalValue)
        {
            m_value = literalValue;
            m_type = typeof(string);
        }

        internal override object Value
        {
            get { return m_value; }
        }
        #region Add
        internal override object Add(ArithmeticLiteral v)
        {
            return v.Add(m_value);
        }
        internal override object Add()
        {
            return m_value;
        }
        internal override object Add(char v)
        {
            return (v.ToString(CultureInfo.CurrentCulture) + m_value);
        }
        internal override object Add(ushort v)
        {
            return (v.ToString(CultureInfo.CurrentCulture) + m_value);
        }
        internal override object Add(int v)
        {
            return (v.ToString(CultureInfo.CurrentCulture) + m_value);
        }
        internal override object Add(uint v)
        {
            return (v.ToString(CultureInfo.CurrentCulture) + m_value);
        }
        internal override object Add(long v)
        {
            return (v.ToString(CultureInfo.CurrentCulture) + m_value);
        }
        internal override object Add(ulong v)
        {
            return (v.ToString(CultureInfo.CurrentCulture) + m_value);
        }
        internal override object Add(float v)
        {
            return (v.ToString(CultureInfo.CurrentCulture) + m_value);
        }
        internal override object Add(double v)
        {
            return (v.ToString(CultureInfo.CurrentCulture) + m_value);
        }
        internal override object Add(decimal v)
        {
            return (v.ToString(CultureInfo.CurrentCulture) + m_value);
        }
        internal override object Add(bool v)
        {
            return (v.ToString(CultureInfo.CurrentCulture) + m_value);
        }
        internal override object Add(string v)
        {
            return (v + m_value);
        }
        #endregion
    }
    #endregion

    #region NullArithmeticLiteral Class
    internal class NullArithmeticLiteral : ArithmeticLiteral
    {
        internal NullArithmeticLiteral(Type type)
        {
            m_type = type;
        }
        protected override string TypeName
        {
            get { return Messages.NullValue; }
        }
        internal override object Value
        {
            get { return null; }
        }
        #region Add
        internal override object Add(ArithmeticLiteral v)
        {
            return v.Add();
        }
        internal override object Add()
        {
            return null;
        }
        internal override object Add(int v)
        {
            return null;
        }
        internal override object Add(long v)
        {
            return null;
        }
        internal override object Add(char v)
        {
            return null;
        }
        internal override object Add(ushort v)
        {
            return null;
        }
        internal override object Add(uint v)
        {
            return null;
        }
        internal override object Add(ulong v)
        {
            return null;
        }
        internal override object Add(float v)
        {
            return null;
        }
        internal override object Add(double v)
        {
            return null;
        }
        internal override object Add(decimal v)
        {
            return null;
        }
        internal override object Add(bool v)
        {
            return null;
        }
        internal override object Add(string v)
        {
            return null;
        }
        #endregion
        #region Subtract
        internal override object Subtract(ArithmeticLiteral v)
        {
            return v.Subtract();
        }
        internal override object Subtract()
        {
            return null;
        }
        internal override object Subtract(int v)
        {
            return null;
        }
        internal override object Subtract(long v)
        {
            return null;
        }
        internal override object Subtract(ushort v)
        {
            return null;
        }
        internal override object Subtract(uint v)
        {
            return null;
        }
        internal override object Subtract(ulong v)
        {
            return null;
        }
        internal override object Subtract(float v)
        {
            return null;
        }
        internal override object Subtract(double v)
        {
            return null;
        }
        internal override object Subtract(decimal v)
        {
            return null;
        }
        #endregion
        #region Multiply
        internal override object Multiply(ArithmeticLiteral v)
        {
            return v.Multiply();
        }
        internal override object Multiply()
        {
            return null;
        }
        internal override object Multiply(int v)
        {
            return null;
        }
        internal override object Multiply(long v)
        {
            return null;
        }
        internal override object Multiply(ushort v)
        {
            return null;
        }
        internal override object Multiply(uint v)
        {
            return null;
        }
        internal override object Multiply(ulong v)
        {
            return null;
        }
        internal override object Multiply(float v)
        {
            return null;
        }
        internal override object Multiply(double v)
        {
            return null;
        }
        internal override object Multiply(decimal v)
        {
            return null;
        }
        #endregion
        #region Divide
        internal override object Divide(ArithmeticLiteral v)
        {
            return v.Divide();
        }
        internal override object Divide()
        {
            return null;
        }
        internal override object Divide(int v)
        {
            return null;
        }
        internal override object Divide(long v)
        {
            return null;
        }
        internal override object Divide(ushort v)
        {
            return null;
        }
        internal override object Divide(uint v)
        {
            return null;
        }
        internal override object Divide(ulong v)
        {
            return null;
        }
        internal override object Divide(float v)
        {
            return null;
        }
        internal override object Divide(double v)
        {
            return null;
        }
        internal override object Divide(decimal v)
        {
            return null;
        }
        #endregion
        #region Modulus
        internal override object Modulus(ArithmeticLiteral v)
        {
            return v.Modulus();
        }
        internal override object Modulus()
        {
            return null;
        }
        internal override object Modulus(int v)
        {
            return null;
        }
        internal override object Modulus(long v)
        {
            return null;
        }
        internal override object Modulus(ushort v)
        {
            return null;
        }
        internal override object Modulus(uint v)
        {
            return null;
        }
        internal override object Modulus(ulong v)
        {
            return null;
        }
        internal override object Modulus(float v)
        {
            return null;
        }
        internal override object Modulus(double v)
        {
            return null;
        }
        internal override object Modulus(decimal v)
        {
            return null;
        }
        #endregion
        #region BitAnd
        internal override object BitAnd(ArithmeticLiteral v)
        {
            return v.BitAnd();
        }
        internal override object BitAnd()
        {
            return null;
        }
        internal override object BitAnd(int v)
        {
            return null;
        }
        internal override object BitAnd(long v)
        {
            return null;
        }
        internal override object BitAnd(ushort v)
        {
            return null;
        }
        internal override object BitAnd(uint v)
        {
            return null;
        }
        internal override object BitAnd(ulong v)
        {
            return null;
        }
        internal override object BitAnd(bool v)
        {
            // special case from section 24.3.6 on bool? type
            return (v == false) ? (object)false : null;
        }
        #endregion
        #region BitOr
        internal override object BitOr(ArithmeticLiteral v)
        {
            return v.BitOr();
        }
        internal override object BitOr()
        {
            return null;
        }
        internal override object BitOr(int v)
        {
            return null;
        }
        internal override object BitOr(long v)
        {
            return null;
        }
        internal override object BitOr(ushort v)
        {
            return null;
        }
        internal override object BitOr(uint v)
        {
            return null;
        }
        internal override object BitOr(ulong v)
        {
            return null;
        }
        internal override object BitOr(bool v)
        {
            // special case from section 24.3.6 on bool? type
            return (v == true) ? (object)true : null;
        }
        #endregion
    }
    #endregion
}
