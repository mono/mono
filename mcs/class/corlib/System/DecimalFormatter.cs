//
// System.DecimalFormatter.cs
//
// Author:
//   Martin Weindel (martin.weindel@t-online.de)
//
// (C) Martin Weindel, Derek Holden  dholden@draper.com
//

//
// Internal class for formatting decimal numbers. 

using System.Globalization;
using System.Text;
using S = System;  // only used for switching test implementation

namespace System 
{

    internal sealed class DecimalFormatter 
    {

        private static bool ParseFormat (string format, out char specifier,  out int precision)
        {		 		 
            precision = -1;
            specifier = '\0';
		    
            int length = format.Length;
            if (length < 1 || length > 3)
                return false;
		    
            char[] chars = format.ToCharArray ();
            specifier = Char.ToUpper(chars[0]);

            if (length == 1) 
                return true;
		    
            if (length == 2) 
            {
                if (chars[1] < '0' || chars[1] > '9')
                    return false;
			    
                precision = chars[1] - '0';
            } 
            else 
            {
                if (chars[1] < '0' || chars[2] < '0' || chars[1] > '9' || chars[2] > '9')
                    return false;
			    
                precision = (chars[1] - '0') * 10 + (chars[2] - '0');
            }
		    
            return true;
        }	 

        public static string NumberToString(string format, NumberFormatInfo nfi, S.Decimal value)
        {
            char specifier;
            int precision;
            if (!DecimalFormatter.ParseFormat(format, out specifier, out precision)) 
            {
                throw new FormatException ("The specified format is invalid");
            }

            int digits = -1;
            int decimals = 0;
            // first calculate number of digits or decimals needed for format
            switch (specifier)
            {
                case 'C':
                    decimals = (precision >= 0) ? precision : nfi.CurrencyDecimalDigits;
                    break;
                case 'F': goto case 'N'; 
                case 'N':
                    decimals = (precision >= 0) ? precision : nfi.NumberDecimalDigits;
                    break;
                case 'G':
                    digits = (precision >= 0) ? precision+1 : 0;
                    break;
                case 'E': 
                    digits = (precision >= 0) ? precision+1 : 7;
                    break;
                case 'P': 
                    decimals = (precision >= 0) ? precision+2 : nfi.PercentDecimalDigits+2;
                    break;
                case 'Z':
                    digits = 0;
                    break;
            }

            // get digit string
            StringBuilder sb = new StringBuilder(40);
            int decPos = 0, sign = 0;
            if (value != S.Decimal.Zero)
                S.Decimal.decimal2string(ref value, digits, decimals, sb, sb.Capacity, out decPos, out sign);

            // now build the format
            switch (specifier)
            {
                case 'C': return FormatCurrency(nfi, sb, decimals, decPos, sign);
                case 'N': return FormatNumber(nfi, sb, decimals, decPos, sign);
                case 'F': return FormatFixedPoint(nfi, sb, decimals, decPos, sign);
                case 'G': return FormatGeneral(nfi, sb, digits, decPos, sign, format[0]);
                case 'E': return FormatExponential(nfi, sb, digits, decPos, sign, format[0]);
                case 'P': return FormatPercent(nfi, sb, decimals, decPos, sign);
                case 'Z': return FormatNormalized(nfi, sb, digits, decPos, sign);
                default: 
                    throw new FormatException ("The specified format is invalid");
            }
        }

        private static void FormatFixedPointSub(NumberFormatInfo nfi, StringBuilder sb, 
            int decimals, int decPos)
        {
            int offset = 0;
            int sigDigits = sb.Length;

            if (decPos <= 0) 
            {
                sb.Insert(0, '0');
                offset++;
            }

            if (decPos > 0) offset += decPos;

            for (int i = sigDigits; i < decPos; i++)
            {
                sb.Append('0');
                offset++;
            }

            if (decimals > 0) 
            {
                for (int i = decPos; i < 0; i++) 
                {
                    sb.Insert(offset, '0');
                }

                int sigDecimals = (decPos < sigDigits) ? sigDigits - decPos : 0;
                for (int i = sigDecimals; i < decimals; i++) 
                {
                    sb.Append('0');
                }
            }
        }

        private static string FormatFixedPoint(NumberFormatInfo nfi, StringBuilder sb, 
            int decimals, int decPos, int sign)
        {
            FormatFixedPointSub(nfi, sb, decimals, decPos);

            if (decimals > 0)
            {
                sb.Insert((decPos <= 0) ? 1 : decPos, nfi.NumberDecimalSeparator);
            }

            if (sign != 0) 
            {
                sb.Insert(0, nfi.NegativeSign);
            }

            return sb.ToString();
        }

        private static string FormatExponential(NumberFormatInfo nfi, StringBuilder sb, 
            int digits, int decPos, int sign, char echar)
        {
            // append trailing zeros if needed
            if (digits > 0 && sb.Length < digits) 
            {
                for (int i = sb.Length; i < digits; i++)
                {
                    sb.Append('0');
                }
            }

            // insert decimal separator
            if (digits > 1 || (digits == 0 && sb.Length > 1)) 
            {
                sb.Insert(1, nfi.NumberDecimalSeparator);
            }

            // insert sign
            if (sign != 0)
            {
                sb.Insert(0, nfi.NegativeSign);
            }

            // append exponent
            sb.Append(echar);
            decPos--;
            sb.Append((decPos >= 0) ? nfi.PositiveSign : nfi.NegativeSign);
            if (decPos < 0) decPos *= -1;
            sb.Append((char)('0' + decPos/10));
            sb.Append((char)('0' + decPos%10));

            return sb.ToString();
        }

        private static string FormatGeneral(NumberFormatInfo nfi, StringBuilder sb, 
            int digits, int decPos, int sign, char gchar)
        {
            if (sb.Length == 0) // adjust for zero
            {
                sb.Append('0');
                decPos = 1;
                sign = 0;
            }

            if (digits == 0 && decPos >= -3) // if we should show all significant digits, we use the fixed point format
            {
                return FormatFixedPoint(nfi, sb, sb.Length - decPos, decPos, sign);
            }
            else if (digits < decPos || decPos < -3 || digits == 0) 
            {
                return FormatExponential(nfi, sb, digits, decPos, sign, (gchar == 'g') ? 'e' : 'E');
            }
            else
            {
                return FormatFixedPoint(nfi, sb, digits - decPos, decPos, sign);
            }
        }

        private static string FormatNumber(NumberFormatInfo nfi, StringBuilder sb, 
            int decimals, int decPos, int sign)
        {
            int offset = 0;
            FormatFixedPointSub(nfi, sb, decimals, decPos);

            // Groups
            if (decPos > 0) 
            {
                int[] groupSizes = nfi.NumberGroupSizes;
                if (groupSizes != null) 
                {
                    int lastSize = 0;
                    int digitCount = 0;
                    for (int i = 0; i < groupSizes.GetLength(0); i++) 
                    {
                        int size = groupSizes[i];
                        if (size > 0) 
                        {
                            digitCount += size;
                            if (digitCount < decPos) 
                            {
                                sb.Insert(decPos - digitCount, nfi.NumberGroupSeparator);
                                offset += nfi.NumberGroupSeparator.Length;
                            }
                            lastSize = size;
                        }
                    }

                    if (lastSize > 0) 
                    {
                        while (true) 
                        {
                            digitCount +=lastSize;
                            if (digitCount >= decPos) break;
                            sb.Insert(decPos - digitCount, nfi.NumberGroupSeparator);
                            offset += nfi.NumberGroupSeparator.Length;
                        }
                    }
                }
            }

            if (decimals > 0)
            {
                sb.Insert(offset + ((decPos <= 0) ? 1 : decPos), nfi.NumberDecimalSeparator);
            }

            // sign
            if (sign != 0) 
            {
                switch (nfi.NumberNegativePattern)
                {
                    case 0:
                        sb.Insert(0, '(');
                        sb.Append(')');
                        break;
                    case 1:
                        sb.Insert(0, nfi.NegativeSign);
                        break;
                    case 2:
                        sb.Insert(0, nfi.NegativeSign);
                        sb.Insert(nfi.NegativeSign.Length, ' ');
                        break;
                    case 3:
                        sb.Append(nfi.NegativeSign);
                        break;
                    case 4:
                        sb.Append(' ');
                        sb.Append(nfi.NegativeSign);
                        break;
                    default:
                        goto case 1;
                }
            } 

            return sb.ToString();
        }

        private static string FormatCurrency(NumberFormatInfo nfi, StringBuilder sb, 
            int decimals, int decPos, int sign)
        {
            throw new Exception("Not implemented yet"); //TODO: implement me
        }

        private static string FormatPercent(NumberFormatInfo nfi, StringBuilder sb, 
            int decimals, int decPos, int sign)
        {
            throw new Exception("Not implemented yet"); //TODO: implement me
        }

        private static string FormatNormalized(NumberFormatInfo nfi, StringBuilder sb, 
            int digits, int decPos, int sign)
        {
            //LAMESPEC: is this a fixed point format ?
            throw new Exception("Not implemented yet"); //TODO: implement me
        }
    }
}
