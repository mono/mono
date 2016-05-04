//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Expressions
{
    using System;
    using System.Activities.ExpressionParser;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Runtime;
    using System.Text.RegularExpressions;
    using System.Windows.Markup;

    [DebuggerStepThrough]
    [ContentProperty("Value")]
    public sealed class Literal<T> : CodeActivity<T>, IExpressionContainer, IValueSerializableExpression
    {
        static Regex ExpressionEscapeRegex = new Regex(@"^(%*\[)");

        public Literal()
        {
            this.UseOldFastPath = true;
        }

        public Literal(T value)
            : this()
        {
            this.Value = value;
        }

        public T Value
        {
            get;
            set;
        }

        protected override void CacheMetadata(CodeActivityMetadata metadata)
        {
            Type literalType = typeof(T);

            if (!literalType.IsValueType && literalType != TypeHelper.StringType)
            {
                metadata.AddValidationError(SR.LiteralsMustBeValueTypesOrImmutableTypes(TypeHelper.StringType, literalType));
            }
        }

        protected override T Execute(CodeActivityContext context)
        {
            return this.Value;
        }

        public override string ToString()
        {
            return this.Value == null ? "null" : this.Value.ToString();
        }

        public bool CanConvertToString(IValueSerializerContext context)
        {
            Type typeArgument;
            Type valueType;
            TypeConverter converter;

            if (this.Value == null)
            {
                return true;
            }
            
            typeArgument = typeof(T);
            valueType = this.Value.GetType();

            if (valueType == TypeHelper.StringType)
            {
                string myValue = this.Value as string;
                if (string.IsNullOrEmpty(myValue))
                {
                    return false;
                }
            }          

            converter = TypeDescriptor.GetConverter(typeArgument);
            if (typeArgument == valueType &&
                converter != null && 
                converter.CanConvertTo(TypeHelper.StringType) && 
                converter.CanConvertFrom(TypeHelper.StringType))
            {               
                if (valueType == typeof(DateTime))
                {
                    DateTime literalValue = (DateTime)(object)this.Value;
                    return IsShortTimeFormattingSafe(literalValue);
                }

                if (valueType == typeof(DateTimeOffset))
                {
                    DateTimeOffset literalValue = (DateTimeOffset)(object)this.Value;
                    return IsShortTimeFormattingSafe(literalValue);
                }

                return true;
            }

            return false;
        }

        static bool IsShortTimeFormattingSafe(DateTime literalValue)
        {
            if (literalValue.Second == 0 && literalValue.Millisecond == 0 && literalValue.Kind == DateTimeKind.Unspecified)
            {
                // Dev10's DateTime's string conversion lost seconds, milliseconds, the remaining ticks and DateTimeKind data.
                // In Dev11, DateTime is special-cased, and is expanded to the property element syntax under a certain condition,
                // so that all aspects of DateTime data are completely preserved after xaml roundtrip.

                DateTime noLeftOverTicksDateTime = new DateTime(
                    literalValue.Year,
                    literalValue.Month,
                    literalValue.Day,
                    literalValue.Hour,
                    literalValue.Minute,
                    literalValue.Second,
                    literalValue.Millisecond,
                    literalValue.Kind);

                if (literalValue.Ticks == noLeftOverTicksDateTime.Ticks)
                {
                    // Dev10 DateTime string conversion does not preserve leftover ticks
                    return true;
                }
            }

            return false;
        }

        static bool IsShortTimeFormattingSafe(DateTimeOffset literalValue)
        {
            // DateTimeOffset is similar to DateTime in how its Dev10 string conversion did not preserve seconds, milliseconds, the remaining ticks and DateTimeKind data.
            return IsShortTimeFormattingSafe(literalValue.DateTime);
        }
        
        [SuppressMessage(FxCop.Category.Globalization, FxCop.Rule.SpecifyIFormatProvider,
            Justification = "we really do want the string as-is")]
        public string ConvertToString(IValueSerializerContext context)
        {
            Type typeArgument;
            Type valueType;
            TypeConverter converter;

            if (this.Value == null)
            {
                return "[Nothing]";
            }

            typeArgument = typeof(T);
            valueType = this.Value.GetType();
            converter = TypeDescriptor.GetConverter(typeArgument);
            
            Fx.Assert(typeArgument == valueType &&
                converter != null &&
                converter.CanConvertTo(TypeHelper.StringType) &&
                converter.CanConvertFrom(TypeHelper.StringType),
                "Literal target type T and the return type mismatch or something wrong with its typeConverter!");

            // handle a Literal<string> of "[...]" by inserting escape chararcter '%' at the front
            if (typeArgument == TypeHelper.StringType)
            {
                string originalString = Convert.ToString(this.Value);
                if (originalString.EndsWith("]", StringComparison.Ordinal) && ExpressionEscapeRegex.IsMatch(originalString))
                {
                    return "%" + originalString;
                }
            }
            return converter.ConvertToString(context, this.Value);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeValue()
        {
            return !object.Equals(this.Value, default(T));
        }
    }
}
