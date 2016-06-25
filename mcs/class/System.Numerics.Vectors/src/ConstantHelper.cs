// System.Numerics.ConstantHelper.cs
//
// Author:
//   Tomasz Chocyk (t.chocyk@gmail.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Runtime.CompilerServices;

namespace System.Numerics
{
    internal class ConstantHelper
    {
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static Byte GetByteWithAllBitsSet()
        {
            Byte value = 0;
            unsafe
            {
                unchecked
                {
                    *((Byte*)&value) = (Byte)0xff;
                }
            }
            return value;
        }
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static SByte GetSByteWithAllBitsSet()
        {
            SByte value = 0;
            unsafe
            {
                unchecked
                {
                    *((SByte*)&value) = (SByte)0xff;
                }
            }
            return value;
        }
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static UInt16 GetUInt16WithAllBitsSet()
        {
            UInt16 value = 0;
            unsafe
            {
                unchecked
                {
                    *((UInt16*)&value) = (UInt16)0xffff;
                }
            }
            return value;
        }
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static Int16 GetInt16WithAllBitsSet()
        {
            Int16 value = 0;
            unsafe
            {
                unchecked
                {
                    *((Int16*)&value) = (Int16)0xffff;
                }
            }
            return value;
        }
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static UInt32 GetUInt32WithAllBitsSet()
        {
            UInt32 value = 0;
            unsafe
            {
                unchecked
                {
                    *((UInt32*)&value) = (UInt32)0xffffffff;
                }
            }
            return value;
        }
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static Int32 GetInt32WithAllBitsSet()
        {
            Int32 value = 0;
            unsafe
            {
                unchecked
                {
                    *((Int32*)&value) = (Int32)0xffffffff;
                }
            }
            return value;
        }
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static UInt64 GetUInt64WithAllBitsSet()
        {
            UInt64 value = 0;
            unsafe
            {
                unchecked
                {
                    *((UInt64*)&value) = (UInt64)0xffffffffffffffff;
                }
            }
            return value;
        }
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static Int64 GetInt64WithAllBitsSet()
        {
            Int64 value = 0;
            unsafe
            {
                unchecked
                {
                    *((Int64*)&value) = (Int64)0xffffffffffffffff;
                }
            }
            return value;
        }
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static Single GetSingleWithAllBitsSet()
        {
            Single value = 0;
            unsafe
            {
                unchecked
                {
                    *((Int32*)&value) = (Int32)0xffffffff;
                }
            }
            return value;
        }
        [MethodImplAttribute(MethodImplOptions.AggressiveInlining)]
        public static Double GetDoubleWithAllBitsSet()
        {
            Double value = 0;
            unsafe
            {
                unchecked
                {
                    *((Int64*)&value) = (Int64)0xffffffffffffffff;
                }
            }
            return value;
        }
    }
}