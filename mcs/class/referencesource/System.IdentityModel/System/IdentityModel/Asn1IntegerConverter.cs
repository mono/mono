//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel
{
    using System.Collections.Generic;
    using System.Text;

    static class Asn1IntegerConverter
    {
        static List<byte[]> powersOfTwo = new List<byte[]>(new byte[][] { new byte[] { 1 } });
        readonly static char[] digitMap = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        public static string Asn1IntegerToDecimalString(byte[] asn1)
        {
            if (asn1 == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("asn1");

            if (asn1.Length == 0)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("asn1", SR.GetString(SR.LengthOfArrayToConvertMustGreaterThanZero)));


            List<byte> positiveDecimalDigits = new List<byte>((asn1.Length * 8) / 3);
            int absoluteBitNumber = 0;
            byte currentByte;

            // Since X509Certificate.GetSerialNumber return the little-endian, 
            // the most significant is at the last byte.
            for (int byteNumber = 0; byteNumber < asn1.Length - 1; byteNumber++)
            {
                currentByte = asn1[byteNumber];
                for (int i = 0; i < 8; i++)
                {
                    if ((currentByte & 1) == 1)
                    {
                        AddSecondDecimalToFirst(positiveDecimalDigits, TwoToThePowerOf(absoluteBitNumber));
                    }
                    absoluteBitNumber++;
                    currentByte >>= 1;
                }
            }

            // Special case the most significant bit of the most significant byte as a negative value
            currentByte = asn1[asn1.Length - 1];
            for (int i = 0; i < 7; i++)
            {
                if ((currentByte & 1) == 1)
                {
                    AddSecondDecimalToFirst(positiveDecimalDigits, TwoToThePowerOf(absoluteBitNumber));
                }
                absoluteBitNumber++;
                currentByte >>= 1;
            }

            StringBuilder result = new StringBuilder(positiveDecimalDigits.Count + 1);
            List<byte> resultDigits = null;
            if (currentByte == 0)
            {
                // positive number
                resultDigits = positiveDecimalDigits;
            }
            else
            {
                // negative number
                List<byte> negativeDecimalDigits = new List<byte>(TwoToThePowerOf(absoluteBitNumber));
                SubtractSecondDecimalFromFirst(negativeDecimalDigits, positiveDecimalDigits);
                resultDigits = negativeDecimalDigits;
                result.Append('-');
            }
            int d;
            for (d = resultDigits.Count - 1; d >= 0; d--)
            {
                if (resultDigits[d] != 0)
                    break;
            }

            if (d < 0 && asn1.Length > 0)
            {
                // This is a special case where the result contains 0
                result.Append(digitMap[0]);
            }
            else
            {
                while (d >= 0)
                {
                    result.Append(digitMap[resultDigits[d--]]);
                }
            }

            return result.ToString();
        }

        static byte[] TwoToThePowerOf(int n)
        {
            lock (powersOfTwo)
            {
                if (n >= powersOfTwo.Count)
                {
                    for (int power = powersOfTwo.Count; power <= n; power++)
                    {
                        List<byte> decimalDigits = new List<byte>(powersOfTwo[power - 1]);
                        byte carryover = 0;
                        for (int i = 0; i < decimalDigits.Count; i++)
                        {
                            byte newValue = (byte)((decimalDigits[i] << 1) + carryover);
                            decimalDigits[i] = (byte)(newValue % 10);
                            carryover = (byte)(newValue / 10);
                        }
                        if (carryover > 0)
                        {
                            decimalDigits.Add(carryover);
                            carryover = 0;
                        }
                        powersOfTwo.Add(decimalDigits.ToArray());
                    }
                }
                return powersOfTwo[n];
            }
        }

        static void AddSecondDecimalToFirst(List<byte> first, byte[] second)
        {
            byte carryover = 0;
            for (int i = 0; i < second.Length || i < first.Count; i++)
            {
                if (i >= first.Count)
                {
                    first.Add(0);
                }
                byte newValue;
                if (i < second.Length)
                {
                    newValue = (byte)(first[i] + second[i] + carryover);
                }
                else
                {
                    newValue = (byte)(first[i] + carryover);
                }
                first[i] = (byte)(newValue % 10);
                carryover = (byte)(newValue / 10);
            }
            if (carryover > 0)
            {
                first.Add(carryover);
            }
        }

        static void SubtractSecondDecimalFromFirst(List<byte> first, List<byte> second)
        {
            byte borrow = 0;
            for (int i = 0; i < second.Count; i++)
            {
                int newValue = first[i] - second[i] - borrow;
                if (newValue < 0)
                {
                    borrow = 1;
                    first[i] = (byte)(newValue + 10);
                }
                else
                {
                    borrow = 0;
                    first[i] = (byte)newValue;
                }
            }
            if (borrow > 0)
            {
                for (int i = second.Count; i < first.Count; i++)
                {
                    int newValue = first[i] - borrow;
                    if (newValue < 0)
                    {
                        borrow = 1;
                        first[i] = (byte)(newValue + 10);
                    }
                    else
                    {
                        borrow = 0;
                        first[i] = (byte)newValue;
                        break;
                    }
                }
            }
            DiagnosticUtility.DebugAssert(borrow == 0, "");
        }
    }
}
