// System.Numerics.Register.cs
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

using System.Runtime.InteropServices;

namespace System.Numerics
{
    /// <summary>
    /// A structure describing the layout of an SSE2-sized register.
    /// Contains overlapping fields representing the set of valid numeric types.
    /// Allows the generic Vector'T struct to contain an explicit field layout.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct Register
    {
        #region Internal Storage Fields
        // Internal System.Byte Fields
        [FieldOffset(0)]
        internal Byte byte_0;
        [FieldOffset(1)]
        internal Byte byte_1;
        [FieldOffset(2)]
        internal Byte byte_2;
        [FieldOffset(3)]
        internal Byte byte_3;
        [FieldOffset(4)]
        internal Byte byte_4;
        [FieldOffset(5)]
        internal Byte byte_5;
        [FieldOffset(6)]
        internal Byte byte_6;
        [FieldOffset(7)]
        internal Byte byte_7;
        [FieldOffset(8)]
        internal Byte byte_8;
        [FieldOffset(9)]
        internal Byte byte_9;
        [FieldOffset(10)]
        internal Byte byte_10;
        [FieldOffset(11)]
        internal Byte byte_11;
        [FieldOffset(12)]
        internal Byte byte_12;
        [FieldOffset(13)]
        internal Byte byte_13;
        [FieldOffset(14)]
        internal Byte byte_14;
        [FieldOffset(15)]
        internal Byte byte_15;

        // Internal System.SByte Fields
        [FieldOffset(0)]
        internal SByte sbyte_0;
        [FieldOffset(1)]
        internal SByte sbyte_1;
        [FieldOffset(2)]
        internal SByte sbyte_2;
        [FieldOffset(3)]
        internal SByte sbyte_3;
        [FieldOffset(4)]
        internal SByte sbyte_4;
        [FieldOffset(5)]
        internal SByte sbyte_5;
        [FieldOffset(6)]
        internal SByte sbyte_6;
        [FieldOffset(7)]
        internal SByte sbyte_7;
        [FieldOffset(8)]
        internal SByte sbyte_8;
        [FieldOffset(9)]
        internal SByte sbyte_9;
        [FieldOffset(10)]
        internal SByte sbyte_10;
        [FieldOffset(11)]
        internal SByte sbyte_11;
        [FieldOffset(12)]
        internal SByte sbyte_12;
        [FieldOffset(13)]
        internal SByte sbyte_13;
        [FieldOffset(14)]
        internal SByte sbyte_14;
        [FieldOffset(15)]
        internal SByte sbyte_15;

        // Internal System.UInt16 Fields
        [FieldOffset(0)]
        internal UInt16 uint16_0;
        [FieldOffset(2)]
        internal UInt16 uint16_1;
        [FieldOffset(4)]
        internal UInt16 uint16_2;
        [FieldOffset(6)]
        internal UInt16 uint16_3;
        [FieldOffset(8)]
        internal UInt16 uint16_4;
        [FieldOffset(10)]
        internal UInt16 uint16_5;
        [FieldOffset(12)]
        internal UInt16 uint16_6;
        [FieldOffset(14)]
        internal UInt16 uint16_7;

        // Internal System.Int16 Fields
        [FieldOffset(0)]
        internal Int16 int16_0;
        [FieldOffset(2)]
        internal Int16 int16_1;
        [FieldOffset(4)]
        internal Int16 int16_2;
        [FieldOffset(6)]
        internal Int16 int16_3;
        [FieldOffset(8)]
        internal Int16 int16_4;
        [FieldOffset(10)]
        internal Int16 int16_5;
        [FieldOffset(12)]
        internal Int16 int16_6;
        [FieldOffset(14)]
        internal Int16 int16_7;

        // Internal System.UInt32 Fields
        [FieldOffset(0)]
        internal UInt32 uint32_0;
        [FieldOffset(4)]
        internal UInt32 uint32_1;
        [FieldOffset(8)]
        internal UInt32 uint32_2;
        [FieldOffset(12)]
        internal UInt32 uint32_3;

        // Internal System.Int32 Fields
        [FieldOffset(0)]
        internal Int32 int32_0;
        [FieldOffset(4)]
        internal Int32 int32_1;
        [FieldOffset(8)]
        internal Int32 int32_2;
        [FieldOffset(12)]
        internal Int32 int32_3;

        // Internal System.UInt64 Fields
        [FieldOffset(0)]
        internal UInt64 uint64_0;
        [FieldOffset(8)]
        internal UInt64 uint64_1;

        // Internal System.Int64 Fields
        [FieldOffset(0)]
        internal Int64 int64_0;
        [FieldOffset(8)]
        internal Int64 int64_1;

        // Internal System.Single Fields
        [FieldOffset(0)]
        internal Single single_0;
        [FieldOffset(4)]
        internal Single single_1;
        [FieldOffset(8)]
        internal Single single_2;
        [FieldOffset(12)]
        internal Single single_3;

        // Internal System.Double Fields
        [FieldOffset(0)]
        internal Double double_0;
        [FieldOffset(8)]
        internal Double double_1;

        #endregion Internal Storage Fields
    }
}