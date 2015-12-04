//------------------------------------------------------------------------------
//  <copyright file="SqlNorm.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation. All Rights Reserved.
//     Information Contained Herein is Proprietary and Confidential.
//  </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="true">daltudov</owner>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">beysims</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="true" primary="false">vadimt</owner>
// <owner current="false" primary="false">[....]</owner>
// <owner current="false" primary="false">venkar</owner>
//------------------------------------------------------------------------------

//devnote: perf optimization: consider changing the calls to Array.Reverse to inline unsafe code

using System;
using System.Collections;
using System.Diagnostics;
using System.Data;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Runtime.CompilerServices;

namespace Microsoft.SqlServer.Server {
    
    // The class that holds the offset, field, and normalizer for
    // a particular field.
    internal sealed class FieldInfoEx: IComparable {
        internal readonly int offset;
        internal readonly FieldInfo fieldInfo;
        internal readonly Normalizer normalizer;

        internal FieldInfoEx(FieldInfo fi, int offset, Normalizer normalizer) {
            this.fieldInfo = fi;
            this.offset = offset;
            Debug.Assert(normalizer!=null, "normalizer argument should not be null!");
            this.normalizer = normalizer;
        }

        // Sort fields by field offsets.
        public int CompareTo(object other) {
            FieldInfoEx otherF = other as FieldInfoEx;
            if (otherF == null)
                return -1;
            return this.offset.CompareTo(otherF.offset);
        }
    }

    // The most complex normalizer, a udt normalizer
    internal sealed class BinaryOrderedUdtNormalizer: Normalizer {
        internal readonly FieldInfoEx[] m_fieldsToNormalize;
        private int m_size;
        private byte[] m_PadBuffer;
        internal readonly object NullInstance;
        //a boolean that tells us if a udt is a "top-level" udt,
        //i.e. one that does not require a null byte header.
        private bool m_isTopLevelUdt;

    	[System.Security.Permissions.ReflectionPermission(System.Security.Permissions.SecurityAction.Assert, MemberAccess=true)]
        private FieldInfo[] GetFields (Type t) {
            return t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        internal BinaryOrderedUdtNormalizer(Type t, bool isTopLevelUdt) {
            this.m_skipNormalize = false;
            if (this.m_skipNormalize) {
                //if skipping normalization, dont write the null
                //byte header for IsNull
                this.m_isTopLevelUdt = true;
            }
            //top level udt logic is disabled until we decide
            //what to do about nested udts
            this.m_isTopLevelUdt = true;
            //      else
            //        this.m_isTopLevelUdt = isTopLevelUdt;
            //get all the fields

            FieldInfo[] fields = GetFields (t);

            m_fieldsToNormalize = new FieldInfoEx[fields.Length];

            int i = 0;

            foreach (FieldInfo fi in fields) {
                int offset = Marshal.OffsetOf(fi.DeclaringType, fi.Name).ToInt32();
                m_fieldsToNormalize[i++] = new FieldInfoEx(fi, offset, GetNormalizer(fi.FieldType));
            }

            //sort by offset
            Array.Sort(m_fieldsToNormalize);
            //if this is not a top-level udt, do setup for null values.
            //null values need to compare less than all other values,
            //so prefix a null byte indicator.
            if (!this.m_isTopLevelUdt) {
                //get the null value for this type, special case for sql types, which
                //have a null field
                if (typeof(System.Data.SqlTypes.INullable).IsAssignableFrom(t)) {
                    PropertyInfo pi = t.GetProperty("Null",
                    BindingFlags.Public | BindingFlags.Static);
                    if (pi == null || pi.PropertyType != t) {
                        FieldInfo fi = t.GetField("Null", BindingFlags.Public | BindingFlags.Static);
                        if (fi == null || fi.FieldType != t)
                            throw new Exception("could not find Null field/property in nullable type " + t);
                        else
                            this.NullInstance = fi.GetValue(null);
                    }
                    else {
                        this.NullInstance = pi.GetValue(null, null);
                    }
                    //create the padding buffer
                    this.m_PadBuffer = new byte[this.Size-1];
                }
            }
        }

        internal bool IsNullable {
            get {
                return this.NullInstance != null;
            }
        }

        // Normalize the top-level udt
        internal void NormalizeTopObject(object udt, Stream s) {
            Normalize(null, udt, s);
        }

        // Denormalize a top-level udt and return it
        internal object DeNormalizeTopObject(Type t, Stream s) {
            return DeNormalizeInternal(t, s);
        }

        // Prevent inlining so that reflection calls are not moved to caller that may be in a different assembly that may have a different grant set.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private object DeNormalizeInternal(Type t, Stream s) {
            object result = null;
            //if nullable and not the top object, read the null marker
            if (!this.m_isTopLevelUdt && typeof(System.Data.SqlTypes.INullable).IsAssignableFrom(t)) {
                byte nullByte = (byte) s.ReadByte();
                if (nullByte == 0) {
                    result = this.NullInstance;
                    s.Read(m_PadBuffer, 0, m_PadBuffer.Length);
                    return result;
                }
            }
            if (result == null)
                result = Activator.CreateInstance(t);
            foreach (FieldInfoEx myField in m_fieldsToNormalize) {
                myField.normalizer.DeNormalize(myField.fieldInfo, result, s);
            }
            return result;
        }

        internal override void Normalize(FieldInfo fi, object obj, Stream s) {
            //      if (fi != null)
            //        Console.WriteLine("normalizing " + fi.FieldType + " pos " + s.Position);
            object inner;
            if (fi == null) {
                inner = obj;
            }
            else {
                inner = GetValue(fi, obj);
            }

            //If nullable and not the top object, write a null indicator
            System.Data.SqlTypes.INullable oNullable = inner as System.Data.SqlTypes.INullable;
            if (oNullable != null && !this.m_isTopLevelUdt) {
                if (oNullable.IsNull) {
                    s.WriteByte(0);
                    s.Write(m_PadBuffer, 0, m_PadBuffer.Length);
                    return;
                }
                else {
                    s.WriteByte(1);
                }
            }

            foreach (FieldInfoEx myField in m_fieldsToNormalize) {
                myField.normalizer.Normalize(myField.fieldInfo, inner, s);
            }
        }

        internal override void DeNormalize(FieldInfo fi, object recvr, Stream s) {
            SetValue(fi, recvr, DeNormalizeInternal(fi.FieldType, s));
        }

        internal override int Size {
            get {
                if (m_size != 0)
                    return m_size;
                if (this.IsNullable && !this.m_isTopLevelUdt)
                    m_size = 1;
                foreach (FieldInfoEx myField in m_fieldsToNormalize) {
                    m_size += myField.normalizer.Size;
                }
                return m_size;
            }
        }
    }

    internal abstract class Normalizer {
        /*
        protected internal static string GetString(byte[] array)
        {
          StringBuilder sb = new StringBuilder();
          //sb.Append("0x");
          foreach (byte b in array)
          {
            sb.Append(b.ToString("X2", CultureInfo.InvariantCulture));
          }
          return sb.ToString();
        }
        */

        protected bool m_skipNormalize;

        /*
        internal static bool IsByteOrderedUdt(Type t)
        {
          SqlUserDefinedTypeAttribute a = SerializationHelper.GetUdtAttribute(t);
          return a.IsByteOrdered;
        }
        */

        internal static Normalizer GetNormalizer(Type t) {
            Normalizer n = null;
            if (t.IsPrimitive) {
                if (t == typeof(byte))
                    n = new ByteNormalizer();
                else if (t == typeof(sbyte))
                    n = new SByteNormalizer();
                else if (t == typeof(bool))
                   n = new BooleanNormalizer();
                else if (t == typeof(short))
                    n = new ShortNormalizer();
                else if (t == typeof(ushort))
                    n = new UShortNormalizer();
                else if (t == typeof(int))
                    n = new IntNormalizer();
                else if (t == typeof(uint))
                    n = new UIntNormalizer();
                else if (t == typeof(float))
                    n = new FloatNormalizer();
                else if (t == typeof(double))
                    n = new DoubleNormalizer();
                else if (t == typeof(long))
                    n = new LongNormalizer();
                else if (t == typeof(ulong))
                    n = new ULongNormalizer();
            }
            else if (t.IsValueType) {
                n = new BinaryOrderedUdtNormalizer(t, false);
            }
            if (n == null)
                throw new Exception(Res.GetString(Res.Sql_CanotCreateNormalizer, t.FullName));
            n.m_skipNormalize = false;
            return n;
        }

        internal abstract void Normalize(FieldInfo fi, object recvr, Stream s);
        internal abstract void DeNormalize(FieldInfo fi, object recvr, Stream s);

        protected void FlipAllBits(byte[] b) {
            for (int i = 0; i < b.Length; i++)
                b[i] = (byte) ~b[i];
        }

    	[System.Security.Permissions.ReflectionPermission(System.Security.Permissions.SecurityAction.Assert, MemberAccess=true)]
        protected object GetValue(FieldInfo fi, object obj) {
            return fi.GetValue(obj);
        }

    	[System.Security.Permissions.ReflectionPermission(System.Security.Permissions.SecurityAction.Assert, MemberAccess=true)]
        protected void SetValue(FieldInfo fi, object recvr, object value) {
            fi.SetValue(recvr, value);
        }

        internal abstract int Size { get; }
    }

    internal sealed class BooleanNormalizer: Normalizer {
        internal override void Normalize(FieldInfo fi, object obj, Stream s) {
            bool b = (bool) GetValue(fi, obj);
            //      Console.WriteLine("normalized " + fi.FieldType + " " + fi.GetValue(obj)
            //        + " to " + (b?"01":"00") + " pos " + s.Position);
            s.WriteByte((byte)(b?1:0));
        }

        internal override void DeNormalize(FieldInfo fi, object recvr, Stream s) {
            byte b = (byte) s.ReadByte();
            SetValue(fi, recvr, b==1);
        }
        
        internal override int Size { get { return 1; } }
    }

    // I could not find a simple way to convert a sbyte to a byte
    // and vice versa in the framework api. Convert.ToSByte() checks that
    // the value is in range.
    // So, we just do the conversion inline.
    internal sealed class SByteNormalizer: Normalizer {
        internal override void Normalize(FieldInfo fi, object obj, Stream s) {
            sbyte sb = (sbyte) GetValue(fi, obj);
            byte b;
            unchecked {
                b = (byte) sb;
            }
            if (!this.m_skipNormalize)
                b ^= 0x80; //flip the sign bit
            s.WriteByte(b);
        }

        internal override void DeNormalize(FieldInfo fi, object recvr, Stream s) {
            byte b = (byte) s.ReadByte();
            if (!this.m_skipNormalize)
                b ^= 0x80; //flip the sign bit
            sbyte sb;
            unchecked {
                sb = (sbyte) b;
            }
            SetValue(fi, recvr, sb);
        }
    
        internal override int Size { get { return 1; } }
    }

    internal sealed class ByteNormalizer: Normalizer {
        internal override void Normalize(FieldInfo fi, object obj, Stream s) {
            byte b = (byte) GetValue(fi, obj);
            s.WriteByte(b);
        }
   
        internal override void DeNormalize(FieldInfo fi, object recvr, Stream s) {
            byte b = (byte) s.ReadByte();
            SetValue(fi, recvr, b);
        }

        internal override int Size { get { return 1; } }
    }

    internal sealed class ShortNormalizer: Normalizer {
        internal override void Normalize(FieldInfo fi, object obj, Stream s) {
            byte[] b = BitConverter.GetBytes((short) GetValue(fi, obj));
            if (!m_skipNormalize) {
                Array.Reverse(b);
                b[0] ^= 0x80;
            }
            s.Write(b, 0, b.Length);
        }

        internal override void DeNormalize(FieldInfo fi, object recvr, Stream s) {
            byte[] b = new Byte[2];
            s.Read(b, 0, b.Length);
            if (!m_skipNormalize) {
                b[0] ^= 0x80;
                Array.Reverse(b);
            }
            SetValue(fi, recvr, BitConverter.ToInt16(b, 0));
        }

        internal override int Size { get { return 2; } }
    }

    internal sealed class UShortNormalizer: Normalizer {
        internal override void Normalize(FieldInfo fi, object obj, Stream s) {
            byte[] b = BitConverter.GetBytes((ushort) GetValue(fi, obj));
            if (!m_skipNormalize) {
                Array.Reverse(b);
            }
            s.Write(b, 0, b.Length);
        }

        internal override void DeNormalize(FieldInfo fi, object recvr, Stream s) {
            byte[] b = new Byte[2];
            s.Read(b, 0, b.Length);
            if (!m_skipNormalize) {
                Array.Reverse(b);
            }
            SetValue(fi, recvr, BitConverter.ToUInt16(b, 0));
        }

        internal override int Size { get { return 2; } }
    }

    internal sealed class IntNormalizer: Normalizer {
        internal override void Normalize(FieldInfo fi, object obj, Stream s) {
            byte[] b = BitConverter.GetBytes((int) GetValue(fi, obj));
            if (!m_skipNormalize) {
                Array.Reverse(b);
                b[0] ^= 0x80;
            }
            //      Console.WriteLine("normalized " + fi.FieldType + " " + fi.GetValue(obj)
            //        + " to " + GetString(b) + " pos " + s.Position);
            s.Write(b, 0, b.Length);
        }

        internal override void DeNormalize(FieldInfo fi, object recvr, Stream s) {
            byte[] b = new Byte[4];
            s.Read(b, 0, b.Length);
            if (!m_skipNormalize) {
                b[0] ^= 0x80;
                Array.Reverse(b);
            }
            SetValue(fi, recvr, BitConverter.ToInt32(b, 0));
        }

        internal override int Size { get { return 4; } }
    }

    internal sealed class UIntNormalizer: Normalizer {
        internal override void Normalize(FieldInfo fi, object obj, Stream s) {
            byte[] b = BitConverter.GetBytes((uint) GetValue(fi, obj));
            if (!m_skipNormalize) {
                Array.Reverse(b);
            }
            s.Write(b, 0, b.Length);
        }

        internal override void DeNormalize(FieldInfo fi, object recvr, Stream s) {
            byte[] b = new byte[4];
            s.Read(b, 0, b.Length);
            if (!m_skipNormalize) {
                Array.Reverse(b);
            }
            SetValue(fi, recvr, BitConverter.ToUInt32(b, 0));
        }

        internal override int Size { get { return 4; } }
    }

    internal sealed class LongNormalizer: Normalizer {
        internal override void Normalize(FieldInfo fi, object obj, Stream s) {
            byte[] b = BitConverter.GetBytes((long) GetValue(fi, obj));
            if (!m_skipNormalize) {
                Array.Reverse(b);
                b[0] ^= 0x80;
            }
            s.Write(b, 0, b.Length);
        }

        internal override void DeNormalize(FieldInfo fi, object recvr, Stream s) {
            byte[] b = new Byte[8];
            s.Read(b, 0, b.Length);
            if (!m_skipNormalize) {
                b[0] ^= 0x80;
                Array.Reverse(b);
            }
            SetValue(fi, recvr, BitConverter.ToInt64(b, 0));
        }

        internal override int Size { get { return 8; } }
    }

    internal sealed class ULongNormalizer: Normalizer {
        internal override void Normalize(FieldInfo fi, object obj, Stream s) {
            byte[] b = BitConverter.GetBytes((ulong) GetValue(fi, obj));
            if (!m_skipNormalize) {
                Array.Reverse(b);
            }
            //      Console.WriteLine("normalized " + fi.FieldType + " " + fi.GetValue(obj)
            //        + " to " + GetString(b));
            s.Write(b, 0, b.Length);
        }

        internal override void DeNormalize(FieldInfo fi, object recvr, Stream s) {
            byte[] b = new Byte[8];
            s.Read(b, 0, b.Length);
            if (!m_skipNormalize) {
                Array.Reverse(b);
            }
            SetValue(fi, recvr, BitConverter.ToUInt64(b, 0));
        }
    
        internal override int Size { get { return 8; } }
    }

    internal sealed class FloatNormalizer: Normalizer {
        internal override void Normalize(FieldInfo fi, object obj, Stream s) {
            float f = (float) GetValue(fi, obj);
            byte[] b = BitConverter.GetBytes(f);
            if (!m_skipNormalize) {
                Array.Reverse(b);
                if ((b[0] & 0x80) == 0) {
                    // This is a positive number.
                    // Flip the highest bit
                    b[0] ^= 0x80;
                }
                else {
                    // This is a negative number.

                    // If all zeroes, means it was a negative zero.
                    // Treat it same as positive zero, so that
                    // the normalized key will compare equal.
                    if (f < 0)
                        FlipAllBits(b);
                }
            }
            s.Write(b, 0, b.Length);
        }

        internal override void DeNormalize(FieldInfo fi, object recvr, Stream s) {
            byte[] b = new Byte[4];
            s.Read(b, 0, b.Length);
            if (!m_skipNormalize) {
                if ((b[0] & 0x80) > 0) {
                    // This is a positive number.
                    // Flip the highest bit
                    b[0] ^= 0x80;
                }
                else {
                    // This is a negative number.
                    FlipAllBits(b);
                }
                Array.Reverse(b);
            }
            SetValue(fi, recvr, BitConverter.ToSingle(b, 0));
        }

        internal override int Size { get { return 4; } }
    }
    
    internal sealed class DoubleNormalizer: Normalizer {
        internal override void Normalize(FieldInfo fi, object obj, Stream s) {
            double d = (double) GetValue(fi, obj);
            byte[] b = BitConverter.GetBytes(d);
            if (!m_skipNormalize) {
                Array.Reverse(b);
                if ((b[0] & 0x80) == 0) {
                    // This is a positive number.
                    // Flip the highest bit
                    b[0] ^= 0x80;
                }
                else {
                    // This is a negative number.
                    if (d < 0) {
                        // If all zeroes, means it was a negative zero.
                        // Treat it same as positive zero, so that
                        // the normalized key will compare equal.
                        FlipAllBits(b);
                    }
                }
            }
            //      Console.WriteLine("normalized " + fi.FieldType + " " + fi.GetValue(obj)
            //        + " to " + GetString(b));
            s.Write(b, 0, b.Length);
        }

        internal override void DeNormalize(FieldInfo fi, object recvr, Stream s) {
            byte[] b = new Byte[8];
            s.Read(b, 0, b.Length);
            if (!m_skipNormalize) {
                if ((b[0] & 0x80) > 0) {
                    // This is a positive number.
                    // Flip the highest bit
                    b[0] ^= 0x80;
                }
                else {
                    // This is a negative number.
                    FlipAllBits(b);
                }
                Array.Reverse(b);
            }
            SetValue(fi, recvr, BitConverter.ToDouble(b, 0));
        }

        internal override int Size { get { return 8; } }
    }
}
