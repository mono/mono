// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

//
// BigInt.cs
//
// 11/06/2002
// 

namespace System.Security.Cryptography
{
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    //
    // This is a pretty "crude" implementation of BigInt arithmetic operations.
    // This class is used in particular to convert certificate serial numbers from
    // hexadecimal representation to decimal format and vice versa.
    //
    // We are not very concerned about the perf characterestics of this implementation
    // for now. We perform all operations up to 128 bytes (which is enough for the current
    // purposes although this constant can be increased). Any overflow bytes are going to be lost.
    //
    // A BigInt is represented as a little endian byte array of size 128 bytes. All
    // arithmetic operations are performed in base 0x100 (256). The algorithms used
    // are simply the common primary school techniques for doing operations in base 10.
    //

    internal sealed class BigInt {
        private byte[] m_elements;
        private const int m_maxbytes = 128; // 128 bytes is the maximum we can handle.
                                            // This means any overflow bits beyond 128 bytes
                                            // will be lost when doing arithmetic operations.
        private const int m_base     = 0x100;
        private int m_size           = 0;

        internal BigInt () {
            m_elements = new byte[m_maxbytes];
        }

        internal BigInt(byte b) {
            m_elements = new byte[m_maxbytes];
            SetDigit(0, b);
        }

        //
        // Gets or sets the size of a BigInt.
        //

        internal int Size {
            get {
                return m_size;
            }
            set {
                if (value > m_maxbytes)
                    m_size = m_maxbytes;
                if (value < 0)
                    m_size = 0;
                m_size = value;
            }
        }

        //
        // Gets the digit at the specified index.
        //

        internal byte GetDigit (int index) {
            if (index < 0 || index >= m_size)
                return 0;

            return m_elements[index];
        }

        //
        // Sets the digit at the specified index.
        //

        internal void SetDigit (int index, byte digit) {
            if (index >= 0 && index < m_maxbytes) {
                m_elements[index] = digit;
                if (index >= m_size && digit != 0)
                    m_size = (index + 1);
                if (index == m_size - 1 && digit == 0)
                    m_size--;
            }
        }

        internal void SetDigit (int index, byte digit, ref int size) {
            if (index >= 0 && index < m_maxbytes) {
                m_elements[index] = digit;
                if (index >= size && digit != 0)
                    size = (index + 1);
                if (index == size - 1 && digit == 0)
                    size = (size - 1);
            }
        }

        //
        // overloaded operators.
        //

        public static bool operator < (BigInt value1, BigInt value2) {
            if (value1 == null)
                return true;
            else if (value2 == null)
                return false;

            int Len1 = value1.Size;
            int Len2 = value2.Size;

            if (Len1 != Len2) 
                return (Len1 < Len2);

            while (Len1-- > 0) {
                if (value1.m_elements[Len1] != value2.m_elements[Len1])
                    return (value1.m_elements[Len1] < value2.m_elements[Len1]);
            }

            return false;
        }

        public static bool operator > (BigInt value1, BigInt value2) {
            if (value1 == null)
                return false;
            else if (value2 == null)
                return true;

            int Len1 = value1.Size;
            int Len2 = value2.Size;

            if (Len1 != Len2) 
                return (Len1 > Len2);

            while (Len1-- > 0) {
                if (value1.m_elements[Len1] != value2.m_elements[Len1])
                    return (value1.m_elements[Len1] > value2.m_elements[Len1]);
            }

            return false;
        }

        public static bool operator == (BigInt value1, BigInt value2) {
            if ((Object) value1 == null)
                return ((Object) value2 == null);
            else if ((Object) value2 == null)
                return ((Object) value1 == null);

            int Len1 = value1.Size;
            int Len2 = value2.Size;

            if (Len1 != Len2) 
                return false;

            for (int index = 0; index < Len1; index++) {
                if (value1.m_elements[index] != value2.m_elements[index]) 
                    return false;
            }

            return true;
        }

        public static bool operator != (BigInt value1, BigInt value2)  {
            return !(value1 == value2);
        }

        public override bool Equals (Object obj) {
            if (obj is BigInt) {
                return (this == (BigInt) obj);
            }
            return false;
        }
    
        public override int GetHashCode () {
            int hash = 0;
            for (int index = 0; index < m_size; index++) {
                hash += GetDigit(index);
            }
            return hash;
        }

        //
        // Adds a and b and outputs the result in c.
        //

        internal static void Add (BigInt a, byte b, ref BigInt c) {
            byte carry = b;
            int sum = 0;

            int size = a.Size;
            int newSize = 0;
            for (int index = 0; index < size; index++) {
                sum = a.GetDigit(index) + carry;
                c.SetDigit(index, (byte) (sum & 0xFF), ref newSize);
                carry = (byte) ((sum >> 8) & 0xFF);
            }

            if (carry != 0)
                c.SetDigit(a.Size, carry, ref newSize);

            c.Size = newSize;
        }

        //
        // Negates a BigInt value. Each byte is complemented, then we add 1 to it.
        //

        internal static void Negate (ref BigInt a) {
            int newSize = 0;
            for (int index = 0; index < m_maxbytes; index++) {
                a.SetDigit(index, (byte) (~a.GetDigit(index) & 0xFF), ref newSize);
            }
            for (int index = 0; index < m_maxbytes; index++) {
                a.SetDigit(index, (byte) (a.GetDigit(index) + 1), ref newSize);
                if ((a.GetDigit(index) & 0xFF) != 0) break;
                a.SetDigit(index, (byte) (a.GetDigit(index) & 0xFF), ref newSize);
            }            
            a.Size = newSize;
        }

        //
        // Subtracts b from a and outputs the result in c.
        //

        internal static void Subtract (BigInt a, BigInt b, ref BigInt c) {
            byte borrow = 0;
            int diff = 0;

            if (a < b) {
                Subtract(b, a, ref c);
                Negate(ref c);
                return;
            }

            int index = 0;
            int size = a.Size;
            int newSize = 0;
            for (index = 0; index < size; index++) {
                diff = a.GetDigit(index) - b.GetDigit(index) - borrow;
                borrow = 0;
                if (diff < 0) {
                    diff += m_base;
                    borrow = 1;
                }
                c.SetDigit(index, (byte) (diff & 0xFF), ref newSize);
            }

            c.Size = newSize;
        }

        //
        // multiplies a BigInt by an integer.
        //

        private void Multiply (int b) {
            if (b == 0) {
                Clear();
                return;
            }

            int carry = 0, product = 0;
            int size = this.Size;
            int newSize = 0;
            for (int index = 0; index < size; index++) {
                product = b * GetDigit(index) + carry;
                carry = product / m_base;
                SetDigit(index, (byte) (product % m_base), ref newSize);
            }

            if (carry != 0) {
                byte[] bytes = BitConverter.GetBytes(carry);
                for (int index = 0; index < bytes.Length; index++) {
                    SetDigit(size + index, bytes[index], ref newSize);
                }
            }

            this.Size = newSize;
        }

        private static void Multiply (BigInt a, int b, ref BigInt c) {
            if (b == 0) {
                c.Clear();
                return;
            }
                
            int carry = 0, product = 0;
            int size = a.Size;
            int newSize = 0;
            for (int index = 0; index < size; index++) {
                product = b * a.GetDigit(index) + carry;
                carry = product / m_base;
                c.SetDigit(index, (byte) (product % m_base), ref newSize);
            }

            if (carry != 0) {
                byte[] bytes = BitConverter.GetBytes(carry);
                for (int index = 0; index < bytes.Length; index++) {
                    c.SetDigit(size + index, bytes[index], ref newSize);
                }
            }

            c.Size = newSize;
        }

        //
        // Divides a BigInt by a single byte.
        //

        private void Divide (int b) {
            int carry = 0, quotient = 0;
            int bLen = this.Size;
            
            int newSize = 0;
            while (bLen-- > 0) {
                quotient = m_base * carry + GetDigit(bLen);
                carry = quotient % b;
                SetDigit(bLen, (byte) (quotient / b), ref newSize);
            }

            this.Size = newSize;
        }

        //
        // Integer division of one BigInt by another.
        //

        internal static void Divide (BigInt numerator, BigInt denominator, ref BigInt quotient, ref BigInt remainder) {
            // Avoid extra computations in special cases.

            if (numerator < denominator) {
                quotient.Clear();
                remainder.CopyFrom(numerator);
                return;
            }
    
            if (numerator == denominator) {
                quotient.Clear(); quotient.SetDigit(0, 1); 
                remainder.Clear();
                return;
            }

            BigInt dividend = new BigInt();
            dividend.CopyFrom(numerator);
            BigInt divisor = new BigInt();
            divisor.CopyFrom(denominator);

            uint zeroCount = 0;
            // We pad the divisor with zeros until its size equals that of the dividend.
            while (divisor.Size < dividend.Size) {
                divisor.Multiply(m_base);
                zeroCount++; 
            }

            if (divisor > dividend) {
                divisor.Divide(m_base);
                zeroCount--;
            }

            // Use school division techniques, make a guess for how many times
            // divisor goes into dividend, making adjustment if necessary.
            int a = 0;
            int b = 0;
            int c = 0;

            BigInt hold = new BigInt();
            quotient.Clear();
            for (int index = 0; index <= zeroCount; index++) {
                a = dividend.Size == divisor.Size ? dividend.GetDigit(dividend.Size - 1) :
                                                    m_base * dividend.GetDigit(dividend.Size - 1) + dividend.GetDigit(dividend.Size - 2);
                b = divisor.GetDigit(divisor.Size - 1);
                c = a / b;

                if (c >= m_base) 
                    c = 0xFF;

                Multiply(divisor, c, ref hold);
                while (hold > dividend) {
                    c--;
                    Multiply(divisor, c, ref hold);
                }

                quotient.Multiply(m_base);
                Add(quotient, (byte) c, ref quotient);
                Subtract(dividend, hold, ref dividend);
                divisor.Divide(m_base);
            }
            remainder.CopyFrom(dividend);
        }

        //
        // copies a BigInt value.
        //

        internal void CopyFrom (BigInt a) {
            Array.Copy(a.m_elements, m_elements, m_maxbytes);
            m_size = a.m_size;
        }

        //
        // This method returns true if the BigInt is equal to 0, false otherwise.
        //

        internal bool IsZero () {
            for (int index = 0; index < m_size; index++) {
                if (m_elements[index] != 0)
                    return false;
            }
            return true;
        }

        //
        // returns the array in machine format, i.e. little endian format (as an integer).
        //

        internal byte[] ToByteArray() {
            byte[] result = new byte[this.Size];
            Array.Copy(m_elements, result, this.Size);
            return result;
        }

        //
        // zeroizes the content of the internal array.
        //

        internal void Clear () {
            m_size = 0;
        }

        //
        // Imports a hexadecimal string into a BigInt bit representation.
        //

        internal void FromHexadecimal (string hexNum) {
            byte[] hex = X509Utils.DecodeHexString(hexNum);
            Array.Reverse(hex);
            int size = X509Utils.GetHexArraySize(hex);
            Array.Copy(hex, m_elements, size);
            this.Size = size;
        }

        //
        // Imports a decimal string into a BigInt bit representation.
        //

        internal void FromDecimal (string decNum) {
            BigInt c = new BigInt();
            BigInt tmp = new BigInt();
            int length = decNum.Length;
            for (int index = 0; index < length; index++) {
                // just ignore invalid characters. No need to raise an exception.
                if (decNum[index] > '9' || decNum[index] < '0')
                    continue;
                Multiply(c, 10, ref tmp);
                Add(tmp, (byte) (decNum[index] - '0'), ref c);
            }
            CopyFrom(c);
        }

        //
        // Exports the BigInt representation as a decimal string.
        //

        private static readonly char[] decValues = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
        internal string ToDecimal ()
        {
            if (IsZero())
                return "0";

            BigInt ten = new BigInt(0xA);
            BigInt numerator = new BigInt();
            BigInt quotient = new BigInt();
            BigInt remainder = new BigInt();

            numerator.CopyFrom(this);

            // Each hex digit can account for log(16) = 1.21 decimal digits. Times two hex digits in a byte
            // and m_size bytes used in this BigInt, yields the maximum number of characters for the decimal
            // representation of the BigInt.
            char[] dec = new char[(int)Math.Ceiling(m_size * 2 * 1.21)];

            int index = 0;
            do
            {
                Divide(numerator, ten, ref quotient, ref remainder);
                dec[index++] = decValues[remainder.IsZero() ? 0 : (int)remainder.m_elements[0]];
                numerator.CopyFrom(quotient);
            } while (quotient.IsZero() == false);

            Array.Reverse(dec, 0, index);
            return new String(dec, 0, index);
        }
    }
}
