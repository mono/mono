//------------------------------------------------------------------------------
//  <copyright file="SqlSer.cs" company="Microsoft Corporation">
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
// <owner current="false" primary="false">venkar</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Runtime.CompilerServices;

namespace Microsoft.SqlServer.Server {

    internal class SerializationHelperSql9 {
        // Don't let anyone create an instance of this class.
        private SerializationHelperSql9() {}

        // Get the m_size of the serialized stream for this type, in bytes.
        // This method creates an instance of the type using the public
        // no-argument constructor, serializes it, and returns the m_size
        // in bytes.
        // Prevent inlining so that reflection calls are not moved to caller that may be in a different assembly that may have a different grant set.
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static int SizeInBytes(Type t) {
            return SizeInBytes(Activator.CreateInstance(t));
        }

        // Get the m_size of the serialized stream for this type, in bytes.
        internal static int SizeInBytes(object instance) {
            Type t = instance.GetType();
            Format k = GetFormat(t);
            DummyStream stream = new DummyStream();
            Serializer ser = GetSerializer(instance.GetType());
            ser.Serialize(stream, instance);
            return (int) stream.Length;
        }

        internal static void Serialize(Stream s, object instance) {
            GetSerializer(instance.GetType()).Serialize(s, instance);
        }

        internal static object Deserialize(Stream s, Type resultType) {
            return GetSerializer(resultType).Deserialize(s);
        }

        private static Format GetFormat(Type t) {
            return GetUdtAttribute(t).Format;
        }

        //cache the relationship between a type and its serializer
        //this is expensive to compute since it involves traversing the
        //custom attributes of the type using reflection.
        //
        //use a per-thread cache, so that there are no synchronization
        //issues when accessing cache entries from multiple threads.
        [ThreadStatic]
        private static Hashtable m_types2Serializers;

        private static Serializer GetSerializer(Type t) {
            if (m_types2Serializers == null)
                m_types2Serializers = new Hashtable();

            Serializer s = (Serializer) m_types2Serializers[t];
            if (s == null) {
                s = (Serializer) GetNewSerializer(t);
                m_types2Serializers[t] = s;
            }
            return s;
        }

        internal static int GetUdtMaxLength(Type t) {
            SqlUdtInfo udtInfo = SqlUdtInfo.GetFromType(t);
            
            if (Format.Native == udtInfo.SerializationFormat) {
                //In the native format, the user does not specify the
                //max byte size, it is computed from the type definition
                return SerializationHelperSql9.SizeInBytes(t);
            }
            else {
                //In all other formats, the user specifies the maximum size in bytes.
                return udtInfo.MaxByteSize;
            }
        }

        private static object[] GetCustomAttributes (Type t) {
            return t.GetCustomAttributes(typeof(SqlUserDefinedTypeAttribute), false);
        }

        internal static SqlUserDefinedTypeAttribute GetUdtAttribute(Type t) {
            SqlUserDefinedTypeAttribute udtAttr = null;
            object[] attr = GetCustomAttributes (t);

            if (attr != null && attr.Length == 1) {
                udtAttr = (SqlUserDefinedTypeAttribute) attr[0];
            }
            else {
                throw InvalidUdtException.Create(t, Res.SqlUdtReason_NoUdtAttribute);
            }
            return udtAttr;
        }

        // Create a new serializer for the given type.
        private static Serializer GetNewSerializer(Type t) {
            SqlUserDefinedTypeAttribute udtAttr = GetUdtAttribute(t);
            Format k = GetFormat(t);

            switch (k) {
                case Format.Native:
                    return new NormalizedSerializer(t);
                case Format.UserDefined:
                    return new BinarySerializeSerializer(t);
                case Format.Unknown: // should never happen, but fall through
                default:
                    throw ADP.InvalidUserDefinedTypeSerializationFormat(k);
            }
        }
    }

    // The base serializer class.
    internal abstract class Serializer {
        public abstract object Deserialize(Stream s);
        public abstract void Serialize(Stream s, object o);
        protected Type m_type;

        protected Serializer(Type t) {
            this.m_type = t;
        }
    }

    internal sealed class NormalizedSerializer: Serializer {
        BinaryOrderedUdtNormalizer m_normalizer;
        bool m_isFixedSize;
        int m_maxSize;

        internal NormalizedSerializer(Type t): base(t) {
            SqlUserDefinedTypeAttribute udtAttr = SerializationHelperSql9.GetUdtAttribute(t);
            this.m_normalizer = new BinaryOrderedUdtNormalizer(t, true);
            this.m_isFixedSize = udtAttr.IsFixedLength;
            this.m_maxSize = this.m_normalizer.Size;
        }

        public override void Serialize(Stream s, object o) {
            m_normalizer.NormalizeTopObject(o, s);
        }

        public override object Deserialize(Stream s) {
            object result = m_normalizer.DeNormalizeTopObject(this.m_type, s);
            return result;
        }
    }

    internal sealed class BinarySerializeSerializer: Serializer {
        internal BinarySerializeSerializer(Type t): base(t) {
        }

        public override void Serialize(Stream s, object o) {
            BinaryWriter w = new BinaryWriter(s);
            ((IBinarySerialize)o).Write(w);
        }

        // Prevent inlining so that reflection calls are not moved to caller that may be in a different assembly that may have a different grant set.
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override object Deserialize(Stream s) {
            object instance = Activator.CreateInstance(m_type);
            BinaryReader r = new BinaryReader(s);
            ((IBinarySerialize)instance).Read(r);
            return instance;
        }
    }

    // A dummy stream class, used to get the number of bytes written
    // to the stream.
    internal sealed class DummyStream: Stream {
        private long m_size;

        public DummyStream() {
        }

        private void DontDoIt() {
            throw new Exception(Res.GetString(Res.Sql_InternalError));
        }

        public override bool CanRead {
            get {
                return false;
            }
        }

        public override bool CanWrite {
            get {
                return true;
            }
        }

        public override bool CanSeek {
            get {
                return false;
            }
        }

        public override long Position {
            get {
                return this.m_size;
            }
            set {
                this.m_size = value;
            }
        }

        public override long Length {
            get {
                return this.m_size;
            }
        }

        public override void SetLength(long value) {
            this.m_size = value;
        }

        public override long Seek(long value, SeekOrigin loc) {
            DontDoIt();
            return -1;
        }

        public override void Flush() {
        }

        public override int Read(byte[] buffer, int offset, int count) {
            DontDoIt();
            return -1;
        }

        public override void Write(byte[] buffer, int offset, int count) {
            this.m_size += count;
        }
    }
}
