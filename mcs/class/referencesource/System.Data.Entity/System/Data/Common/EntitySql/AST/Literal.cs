//---------------------------------------------------------------------
// <copyright file="Literal.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Common.EntitySql.AST
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    /// Defines literal value kind, including the eSQL untyped NULL.
    /// </summary>
    internal enum LiteralKind
    {
        Number,
        String,
        UnicodeString,
        Boolean,
        Binary,
        DateTime,
        Time,
        DateTimeOffset,
        Guid,
        Null
    }

    /// <summary>
    /// Represents a literal ast node.
    /// </summary>
    internal sealed class Literal : Node
    {
        private readonly LiteralKind _literalKind;
        private string _originalValue;
        private bool _wasValueComputed = false;
        private object _computedValue;
        private Type _type;
        private static readonly Byte[] _emptyByteArray = new byte[0];

        /// <summary>
        /// Initializes a literal ast node.
        /// </summary>
        /// <param name="originalValue">literal value in cql string representation</param>
        /// <param name="kind">literal value class</param>
        /// <param name="query">query</param>
        /// <param name="inputPos">input position</param>
        internal Literal(string originalValue, LiteralKind kind, string query, int inputPos)
            : base(query, inputPos)
        {
            _originalValue = originalValue;
            _literalKind = kind;
        }

        /// <summary>
        /// Static factory to create boolean literals by value only.
        /// </summary>
        /// <param name="value"></param>
        internal static Literal NewBooleanLiteral(bool value) { return new Literal(value); }

        private Literal(bool boolLiteral)
            : base(null, 0)
        {
            _wasValueComputed = true;
            _originalValue = String.Empty;
            _computedValue = boolLiteral;
            _type = typeof(System.Boolean);
        }

        /// <summary>
        /// True if literal is a number.
        /// </summary>
        internal bool IsNumber
        {
            get
            {
                return (_literalKind == LiteralKind.Number);
            }
        }

        /// <summary>
        /// True if literal is a signed number.
        /// </summary>
        internal bool IsSignedNumber
        {
            get
            {
                return IsNumber && (_originalValue[0] == '-' || _originalValue[0] == '+');
            }
        }

        /// <summary>
        /// True if literal is a string.
        /// </summary>
        /// <remarks>
        /// <exception cref="System.Data.EntityException"></exception>
        /// </remarks>
        internal bool IsString
        {
            get
            {
                return _literalKind == LiteralKind.String || _literalKind == LiteralKind.UnicodeString;
            }
        }

        /// <summary>
        /// True if literal is a unicode string.
        /// </summary>
        /// <remarks>
        /// <exception cref="System.Data.EntityException"></exception>
        /// </remarks>
        internal bool IsUnicodeString
        {
            get
            {
                return _literalKind == LiteralKind.UnicodeString;
            }
        }

        /// <summary>
        /// True if literal is the eSQL untyped null.
        /// </summary>
        /// <remarks>
        /// <exception cref="System.Data.EntityException"></exception>
        /// </remarks>
        internal bool IsNullLiteral
        {
            get
            {
                return _literalKind == LiteralKind.Null;
            }
        }

        /// <summary>
        /// Returns the original literal value.
        /// </summary>
        internal string OriginalValue
        {
            get
            {
                return _originalValue;
            }
        }

        /// <summary>
        /// Prefix a numeric literal with a sign.
        /// </summary>
        internal void PrefixSign(string sign)
        {
            System.Diagnostics.Debug.Assert(IsNumber && !IsSignedNumber);
            System.Diagnostics.Debug.Assert(sign[0] == '-' || sign[0] == '+', "sign symbol must be + or -");
            System.Diagnostics.Debug.Assert(_computedValue == null);

            _originalValue = sign + _originalValue;
        }

        #region Computed members
        /// <summary>
        /// Returns literal converted value.
        /// </summary>
        /// <remarks>
        /// <exception cref="System.Data.EntityException"></exception>
        /// </remarks>
        internal object Value
        {
            get
            {
                ComputeValue();

                return _computedValue;
            }
        }

        /// <summary>
        /// Returns literal value type. If value is eSQL untyped null, returns null.
        /// </summary>
        /// <remarks>
        /// <exception cref="System.Data.EntityException"></exception>
        /// </remarks>
        internal Type Type
        {
            get
            {
                ComputeValue();

                return _type;
            }
        }
        #endregion

        private void ComputeValue()
        {
            if (!_wasValueComputed)
            {
                _wasValueComputed = true;

                switch (_literalKind)
                {
                    case LiteralKind.Number:
                        _computedValue = ConvertNumericLiteral(ErrCtx, _originalValue);
                        break;

                    case LiteralKind.String:
                        _computedValue = GetStringLiteralValue(_originalValue, false /* isUnicode */);
                        break;

                    case LiteralKind.UnicodeString:
                        _computedValue = GetStringLiteralValue(_originalValue, true /* isUnicode */);
                        break;

                    case LiteralKind.Boolean:
                        _computedValue = ConvertBooleanLiteralValue(ErrCtx, _originalValue);
                        break;

                    case LiteralKind.Binary:
                        _computedValue = ConvertBinaryLiteralValue(ErrCtx, _originalValue);
                        break;

                    case LiteralKind.DateTime:
                        _computedValue = ConvertDateTimeLiteralValue(ErrCtx, _originalValue);
                        break;

                    case LiteralKind.Time:
                        _computedValue = ConvertTimeLiteralValue(ErrCtx, _originalValue);
                        break;

                    case LiteralKind.DateTimeOffset:
                        _computedValue = ConvertDateTimeOffsetLiteralValue(ErrCtx, _originalValue);
                        break;

                    case LiteralKind.Guid:
                        _computedValue = ConvertGuidLiteralValue(ErrCtx, _originalValue);
                        break;

                    case LiteralKind.Null:
                        _computedValue = null;
                        break;

                    default:
                        throw EntityUtil.NotSupported(System.Data.Entity.Strings.LiteralTypeNotSupported(_literalKind.ToString()));

                }

                _type = IsNullLiteral ? null : _computedValue.GetType();
            }
        }

        #region Conversion Helpers
        static char[] numberSuffixes = new char[] { 'U', 'u', 'L', 'l', 'F', 'f', 'M', 'm', 'D', 'd' };
        static char[] floatTokens = new char[] { '.', 'E', 'e' };
        private static object ConvertNumericLiteral(ErrorContext errCtx, string numericString)
        {
            int k = numericString.IndexOfAny(numberSuffixes);
            if (-1 != k)
            {
                string suffix = numericString.Substring(k).ToUpperInvariant();
                string numberPart = numericString.Substring(0, numericString.Length - suffix.Length);
                switch (suffix)
                {
                    case "U":
                        {
                            UInt32 value;
                            if (!UInt32.TryParse(numberPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                            {
                                throw EntityUtil.EntitySqlError(errCtx, System.Data.Entity.Strings.CannotConvertNumericLiteral(numericString, "unsigned int"));
                            }
                            return value;
                        }
                        ;

                    case "L":
                        {
                            long value;
                            if (!Int64.TryParse(numberPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                            {
                                throw EntityUtil.EntitySqlError(errCtx, System.Data.Entity.Strings.CannotConvertNumericLiteral(numericString, "long"));
                            }
                            return value;
                        }
                        ;

                    case "UL":
                    case "LU":
                        {
                            UInt64 value;
                            if (!UInt64.TryParse(numberPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                            {
                                throw EntityUtil.EntitySqlError(errCtx, System.Data.Entity.Strings.CannotConvertNumericLiteral(numericString, "unsigned long"));
                            }
                            return value;
                        }
                        ;

                    case "F":
                        {
                            Single value;
                            if (!Single.TryParse(numberPart, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                            {
                                throw EntityUtil.EntitySqlError(errCtx, System.Data.Entity.Strings.CannotConvertNumericLiteral(numericString, "float"));
                            }
                            return value;
                        }
                        ;

                    case "M":
                        {
                            Decimal value;
                            if (!Decimal.TryParse(numberPart, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value))
                            {
                                throw EntityUtil.EntitySqlError(errCtx, System.Data.Entity.Strings.CannotConvertNumericLiteral(numericString, "decimal"));
                            }
                            return value;
                        }
                        ;

                    case "D":
                        {
                            Double value;
                            if (!Double.TryParse(numberPart, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                            {
                                throw EntityUtil.EntitySqlError(errCtx, System.Data.Entity.Strings.CannotConvertNumericLiteral(numericString, "double"));
                            }
                            return value;
                        }
                        ;

                }
            }

            //
            // If hit this point, try default conversion
            //
            return DefaultNumericConversion(numericString, errCtx);
        }

        /// <summary>
        /// Performs conversion of numeric strings that have no type suffix hint.
        /// </summary>
        private static object DefaultNumericConversion(string numericString, ErrorContext errCtx)
        {

            if (-1 != numericString.IndexOfAny(floatTokens))
            {
                Double value;
                if (!Double.TryParse(numericString, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                {
                    throw EntityUtil.EntitySqlError(errCtx, System.Data.Entity.Strings.CannotConvertNumericLiteral(numericString, "double"));
                }

                return value;
            }
            else
            {
                Int32 int32Value;
                if (Int32.TryParse(numericString, NumberStyles.Integer, CultureInfo.InvariantCulture, out int32Value))
                {
                    return int32Value;
                }

                Int64 int64Value;
                if (!Int64.TryParse(numericString, NumberStyles.Integer, CultureInfo.InvariantCulture, out int64Value))
                {
                    throw EntityUtil.EntitySqlError(errCtx, System.Data.Entity.Strings.CannotConvertNumericLiteral(numericString, "long"));
                }

                return int64Value;
            }

        }

        /// <summary>
        /// Converts boolean literal value.
        /// </summary>
        private static bool ConvertBooleanLiteralValue(ErrorContext errCtx, string booleanLiteralValue)
        {
            bool result = false;
            if (!Boolean.TryParse(booleanLiteralValue, out result))
            {
                throw EntityUtil.EntitySqlError(errCtx, System.Data.Entity.Strings.InvalidLiteralFormat("Boolean", booleanLiteralValue));
            }
            return result;
        }

        /// <summary>
        /// Returns the string literal value.
        /// </summary>
        private static string GetStringLiteralValue(string stringLiteralValue, bool isUnicode)
        {
            Debug.Assert(stringLiteralValue.Length >= 2);
            Debug.Assert(isUnicode == ('N' == stringLiteralValue[0]), "invalid string literal value");

            int startIndex = (isUnicode ? 2 : 1);
            char delimiter = stringLiteralValue[startIndex - 1];

            // NOTE: this is not a precondition validation. This validation is for security purposes based on the 
            // paranoid assumption that all input is evil. we should not see this exception under normal 
            // conditions.
            if (delimiter != '\'' && delimiter != '\"')
            {
                throw EntityUtil.EntitySqlError(System.Data.Entity.Strings.MalformedStringLiteralPayload);
            }

            string result = "";

            // NOTE: this is not a precondition validation. This validation is for security purposes based on the 
            // paranoid assumption that all input is evil. we should not see this exception under normal 
            // conditions.
            int before = stringLiteralValue.Split(new char[] { delimiter }).Length - 1;
            Debug.Assert(before % 2 == 0, "must have an even number of delimiters in the string literal");
            if (0 != (before % 2))
            {
                throw EntityUtil.EntitySqlError(System.Data.Entity.Strings.MalformedStringLiteralPayload);
            }

            //
            // Extract the payload and replace escaped chars that match the envelope delimiter
            //
            result = stringLiteralValue.Substring(startIndex, stringLiteralValue.Length - (1 + startIndex));
            result = result.Replace(new String(delimiter, 2), new String(delimiter, 1));

            // NOTE: this is not a precondition validation. This validation is for security purposes based on the 
            // paranoid assumption that all input is evil. we should not see this exception under normal 
            // conditions.
            int after = result.Split(new char[] { delimiter }).Length - 1;
            Debug.Assert(after == (before - 2) / 2);
            if ((after != ((before - 2) / 2)))
            {
                throw EntityUtil.EntitySqlError(System.Data.Entity.Strings.MalformedStringLiteralPayload);
            }

            return result;
        }

        /// <summary>
        /// Converts hex string to byte array.
        /// </summary>
        private static byte[] ConvertBinaryLiteralValue(ErrorContext errCtx, string binaryLiteralValue)
        {
            Debug.Assert(null != binaryLiteralValue, "binaryStringLiteral must not be null");

            if (String.IsNullOrEmpty(binaryLiteralValue))
            {
                return _emptyByteArray;
            }

            int startIndex = 0;
            int endIndex = binaryLiteralValue.Length - 1;
            Debug.Assert(startIndex <= endIndex, "startIndex <= endIndex");
            int binaryStringLen = endIndex - startIndex + 1;
            int byteArrayLen = binaryStringLen / 2;
            bool hasOddBytes = 0 != (binaryStringLen % 2);
            if (hasOddBytes)
            {
                byteArrayLen++;
            }

            byte[] binaryValue = new byte[byteArrayLen];
            int arrayIndex = 0;
            if (hasOddBytes)
            {
                binaryValue[arrayIndex++] = (byte)HexDigitToBinaryValue(binaryLiteralValue[startIndex++]);
            }

            while (startIndex < endIndex)
            {
                binaryValue[arrayIndex++] = (byte)((HexDigitToBinaryValue(binaryLiteralValue[startIndex++]) << 4) | HexDigitToBinaryValue(binaryLiteralValue[startIndex++]));
            }

            return binaryValue;
        }

        /// <summary>
        /// Parse single hex char.
        /// PRECONDITION - hexChar must be a valid hex digit.
        /// </summary>
        private static int HexDigitToBinaryValue(char hexChar)
        {
            if (hexChar >= '0' && hexChar <= '9') return (int)(hexChar - '0');
            if (hexChar >= 'A' && hexChar <= 'F') return (int)(hexChar - 'A') + 10;
            if (hexChar >= 'a' && hexChar <= 'f') return (int)(hexChar - 'a') + 10;
            Debug.Assert(false, "Invalid Hexadecimal Digit");
            throw EntityUtil.ArgumentOutOfRange("hexadecimal digit is not valid");
        }


        static readonly char[] _datetimeSeparators = new char[] { ' ', ':', '-', '.' };
        static readonly char[] _dateSeparators = new char[] { '-' };
        static readonly char[] _timeSeparators = new char[] { ':', '.' };
        static readonly char[] _datetimeOffsetSeparators = new char[] { ' ', ':', '-', '.', '+', '-' };

        /// <summary>
        /// Converts datetime literal value.
        /// </summary>
        private static DateTime ConvertDateTimeLiteralValue(ErrorContext errCtx, string datetimeLiteralValue)
        {
            string[] datetimeParts = datetimeLiteralValue.Split(_datetimeSeparators, StringSplitOptions.RemoveEmptyEntries);

            Debug.Assert(datetimeParts.Length >= 5, "datetime literal value must have at least 5 parts");

            int year;
            int month;
            int day;
            GetDateParts(datetimeLiteralValue, datetimeParts, out year, out month, out day);
            int hour;
            int minute;
            int second;
            int ticks;
            GetTimeParts(datetimeLiteralValue, datetimeParts, 3, out hour, out minute, out second, out ticks);

            Debug.Assert(year >= 1 && year <= 9999);
            Debug.Assert(month >= 1 && month <= 12);
            Debug.Assert(day >= 1 && day <= 31);
            Debug.Assert(hour >= 0 && hour <= 24);
            Debug.Assert(minute >= 0 && minute <= 59);
            Debug.Assert(second >= 0 && second <= 59);
            Debug.Assert(ticks >= 0 && ticks <= 9999999);
            DateTime dateTime = new DateTime(year, month, day, hour, minute, second, 0);
            dateTime = dateTime.AddTicks(ticks);
            return dateTime;
        }

        private static DateTimeOffset ConvertDateTimeOffsetLiteralValue(ErrorContext errCtx, string datetimeLiteralValue)
        {
            string[] datetimeParts = datetimeLiteralValue.Split(_datetimeOffsetSeparators, StringSplitOptions.RemoveEmptyEntries);

            Debug.Assert(datetimeParts.Length >= 7, "datetime literal value must have at least 7 parts");

            int year;
            int month;
            int day;
            GetDateParts(datetimeLiteralValue, datetimeParts, out year, out month, out day);
            int hour;
            int minute;
            int second;
            int ticks;
            //Copy the time parts into a different array since the last two parts will be handled in this method.
            string[] timeParts = new String[datetimeParts.Length - 2];
            Array.Copy(datetimeParts, timeParts, datetimeParts.Length - 2);
            GetTimeParts(datetimeLiteralValue, timeParts, 3, out hour, out minute, out second, out ticks);

            Debug.Assert(year >= 1 && year <= 9999);
            Debug.Assert(month >= 1 && month <= 12);
            Debug.Assert(day >= 1 && day <= 31);
            Debug.Assert(hour >= 0 && hour <= 24);
            Debug.Assert(minute >= 0 && minute <= 59);
            Debug.Assert(second >= 0 && second <= 59);
            Debug.Assert(ticks >= 0 && ticks <= 9999999);
            int offsetHours = Int32.Parse(datetimeParts[datetimeParts.Length - 2], NumberStyles.Integer, CultureInfo.InvariantCulture);
            int offsetMinutes = Int32.Parse(datetimeParts[datetimeParts.Length - 1], NumberStyles.Integer, CultureInfo.InvariantCulture);
            TimeSpan offsetTimeSpan = new TimeSpan(offsetHours, offsetMinutes, 0);

            //If DateTimeOffset had a negative offset, we should negate the timespan
            if (datetimeLiteralValue.IndexOf('+') == -1)
            {
                offsetTimeSpan = offsetTimeSpan.Negate();
            }
            DateTime dateTime = new DateTime(year, month, day, hour, minute, second, 0);
            dateTime = dateTime.AddTicks(ticks);

            try
            {
                return new DateTimeOffset(dateTime, offsetTimeSpan);
            }
            catch (ArgumentOutOfRangeException e)
            {
                throw EntityUtil.EntitySqlError(errCtx, System.Data.Entity.Strings.InvalidDateTimeOffsetLiteral(datetimeLiteralValue), e);
            }
        }

        /// <summary>
        /// Converts time literal value.
        /// </summary>
        private static TimeSpan ConvertTimeLiteralValue(ErrorContext errCtx, string datetimeLiteralValue)
        {
            string[] datetimeParts = datetimeLiteralValue.Split(_datetimeSeparators, StringSplitOptions.RemoveEmptyEntries);

            Debug.Assert(datetimeParts.Length >= 2, "time literal value must have at least 2 parts");

            int hour;
            int minute;
            int second;
            int ticks;
            GetTimeParts(datetimeLiteralValue, datetimeParts, 0, out hour, out minute, out second, out ticks);

            Debug.Assert(hour >= 0 && hour <= 24);
            Debug.Assert(minute >= 0 && minute <= 59);
            Debug.Assert(second >= 0 && second <= 59);
            Debug.Assert(ticks >= 0 && ticks <= 9999999);
            TimeSpan ts = new TimeSpan(hour, minute, second);
            ts = ts.Add(new TimeSpan(ticks));
            return ts;
        }

        private static void GetTimeParts(string datetimeLiteralValue, string[] datetimeParts, int timePartStartIndex, out int hour, out int minute, out int second, out int ticks)
        {
            hour = Int32.Parse(datetimeParts[timePartStartIndex], NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (hour > 23)
            {
                throw EntityUtil.EntitySqlError(System.Data.Entity.Strings.InvalidHour(datetimeParts[timePartStartIndex], datetimeLiteralValue));
            }
            minute = Int32.Parse(datetimeParts[++timePartStartIndex], NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (minute > 59)
            {
                throw EntityUtil.EntitySqlError(System.Data.Entity.Strings.InvalidMinute(datetimeParts[timePartStartIndex], datetimeLiteralValue));
            }
            second = 0;
            ticks = 0;
            timePartStartIndex++;
            if (datetimeParts.Length > timePartStartIndex)
            {
                second = Int32.Parse(datetimeParts[timePartStartIndex], NumberStyles.Integer, CultureInfo.InvariantCulture);
                if (second > 59)
                {
                    throw EntityUtil.EntitySqlError(System.Data.Entity.Strings.InvalidSecond(datetimeParts[timePartStartIndex], datetimeLiteralValue));
                }
                timePartStartIndex++;
                if (datetimeParts.Length > timePartStartIndex)
                {
                    //We need fractional time part to be seven digits
                    string ticksString = datetimeParts[timePartStartIndex].PadRight(7, '0');
                    ticks = Int32.Parse(ticksString, NumberStyles.Integer, CultureInfo.InvariantCulture);
                }

            }
        }

        private static void GetDateParts(string datetimeLiteralValue, string[] datetimeParts, out int year, out int month, out int day)
        {
            year = Int32.Parse(datetimeParts[0], NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (year < 1 || year > 9999)
            {
                throw EntityUtil.EntitySqlError(System.Data.Entity.Strings.InvalidYear(datetimeParts[0], datetimeLiteralValue));
            }
            month = Int32.Parse(datetimeParts[1], NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (month < 1 || month > 12)
            {
                throw EntityUtil.EntitySqlError(System.Data.Entity.Strings.InvalidMonth(datetimeParts[1], datetimeLiteralValue));
            }
            day = Int32.Parse(datetimeParts[2], NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (day < 1)
            {
                throw EntityUtil.EntitySqlError(System.Data.Entity.Strings.InvalidDay(datetimeParts[2], datetimeLiteralValue));
            }
            if (day > DateTime.DaysInMonth(year, month))
            {
                throw EntityUtil.EntitySqlError(System.Data.Entity.Strings.InvalidDayInMonth(datetimeParts[2], datetimeParts[1], datetimeLiteralValue));
            }
        }

        /// <summary>
        /// Converts guid literal value.
        /// </summary>
        private static Guid ConvertGuidLiteralValue(ErrorContext errCtx, string guidLiteralValue)
        {
            return new Guid(guidLiteralValue);
        }
        #endregion
    }
}
