//------------------------------------------------------------------------------
// <copyright file="altserialization.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * AltSerialization.cs
 * 
 * Copyright (c) 1998-2000, Microsoft Corporation
 * 
 */

namespace System.Web.Util {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Web.SessionState;
    
    internal static class AltSerialization {
        enum TypeID : byte {
            String = 1,
            Int32,
            Boolean,
            DateTime,
            Decimal,
            Byte,
            Char,
            Single,
            Double,
            SByte,
            Int16,
            Int64,
            UInt16,
            UInt32,
            UInt64,
            TimeSpan,
            Guid,
            IntPtr,
            UIntPtr,
            Object,
            Null,
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "This method is safe critical (we're allowed to call the SerializationSurrogateSelector property getter).")]
        internal static void WriteValueToStream(Object value, BinaryWriter writer) {
            if (value == null) {
                writer.Write((byte)TypeID.Null);
            } 
            else if (value is String) {
                writer.Write((byte)TypeID.String);
                writer.Write((String) value);
            }
            else if (value is Int32) {
                writer.Write((byte)TypeID.Int32);
                writer.Write((Int32) value);
            }
            else if (value is Boolean) {
                writer.Write((byte)TypeID.Boolean);
                writer.Write((Boolean) value);
            }
            else if (value is DateTime) {
                writer.Write((byte)TypeID.DateTime);
                writer.Write(((DateTime) value).Ticks);
            }
            else if (value is Decimal) {
                writer.Write((byte)TypeID.Decimal);
                int[] bits = Decimal.GetBits((Decimal)value);
                for (int i = 0; i < 4; i++) {
                    writer.Write((int)bits[i]);
                }
            }
            else if (value is Byte) {
                writer.Write((byte)TypeID.Byte);
                writer.Write((byte) value);
            }
            else if (value is Char) {
                writer.Write((byte)TypeID.Char);
                writer.Write((char) value);
            }
            else if (value is Single) {
                writer.Write((byte)TypeID.Single);
                writer.Write((float) value);
            }
            else if (value is Double) {
                writer.Write((byte)TypeID.Double);
                writer.Write((double) value);
            }
            else if (value is SByte) {
                writer.Write((byte)TypeID.SByte);
                writer.Write((SByte) value);
            }
            else if (value is Int16) {
                writer.Write((byte)TypeID.Int16);
                writer.Write((short) value);
            }
            else if (value is Int64) {
                writer.Write((byte)TypeID.Int64);
                writer.Write((long) value);
            }
            else if (value is UInt16) {
                writer.Write((byte)TypeID.UInt16);
                writer.Write((UInt16) value);
            }
            else if (value is UInt32) {
                writer.Write((byte)TypeID.UInt32);
                writer.Write((UInt32) value);
            }
            else if (value is UInt64) {
                writer.Write((byte)TypeID.UInt64);
                writer.Write((UInt64) value);
            }
            else if (value is TimeSpan) {
                writer.Write((byte)TypeID.TimeSpan);
                writer.Write(((TimeSpan) value).Ticks);
            }
            else if (value is Guid) {
                writer.Write((byte)TypeID.Guid);
                Guid guid = (Guid) value;
                byte[] bits = guid.ToByteArray();
                writer.Write(bits);
            }
            else if (value is IntPtr) {
                writer.Write((byte)TypeID.IntPtr);
                IntPtr  v = (IntPtr) value;
                if (IntPtr.Size == 4) {
                    writer.Write((Int32)v.ToInt32());
                }
                else {
                    Debug.Assert(IntPtr.Size == 8);
                    writer.Write((Int64)v.ToInt64());
                }
            }
            else if (value is UIntPtr) {
                writer.Write((byte)TypeID.UIntPtr);
                UIntPtr  v = (UIntPtr) value;
                if (UIntPtr.Size == 4) {
                    writer.Write((UInt32)v.ToUInt32());
                }
                else {
                    Debug.Assert(UIntPtr.Size == 8);
                    writer.Write((UInt64)v.ToUInt64());
                }
            }
            else {
                writer.Write((byte)TypeID.Object);
                BinaryFormatter formatter = new BinaryFormatter();
                if (SessionStateUtility.SerializationSurrogateSelector != null) {
                    formatter.SurrogateSelector = SessionStateUtility.SerializationSurrogateSelector;
                }
                try {
                    formatter.Serialize(writer.BaseStream, value);
                } catch (Exception innerException) {
                    HttpException outerException = new HttpException(SR.GetString(SR.Cant_serialize_session_state), innerException);
                    outerException.SetFormatter(new UseLastUnhandledErrorFormatter(outerException));
                    throw outerException;
                }
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "This method is safe critical (we're allowed to call the SerializationSurrogateSelector property getter).")]
        internal static Object ReadValueFromStream(BinaryReader reader) {
            TypeID  id;
            Object  value = null;

            id = (TypeID) reader.ReadByte();
            switch (id) {
                case TypeID.String:            
                    value = reader.ReadString();
                    break;

                case TypeID.Int32:
                    value = reader.ReadInt32();
                    break;

                case TypeID.Boolean:
                    value = reader.ReadBoolean();
                    break;

                case TypeID.DateTime:
                    value = new DateTime(reader.ReadInt64());
                    break;

                case TypeID.Decimal:
                    {
                        int[] bits = new int[4];
                        for (int i = 0; i < 4; i++) {
                            bits[i] = reader.ReadInt32();
                        }

                        value = new Decimal(bits);
                    }
                    break;

                case TypeID.Byte:
                    value = reader.ReadByte();
                    break;

                case TypeID.Char:
                    value = reader.ReadChar();
                    break;

                case TypeID.Single:
                    value = reader.ReadSingle();
                    break;

                case TypeID.Double:
                    value = reader.ReadDouble();
                    break;

                case TypeID.SByte:
                    value = reader.ReadSByte();
                    break;

                case TypeID.Int16:
                    value = reader.ReadInt16();
                    break;

                case TypeID.Int64:
                    value = reader.ReadInt64();
                    break;

                case TypeID.UInt16:
                    value = reader.ReadUInt16();
                    break;

                case TypeID.UInt32:
                    value = reader.ReadUInt32();
                    break;

                case TypeID.UInt64:
                    value = reader.ReadUInt64();
                    break;

                case TypeID.TimeSpan:
                    value = new TimeSpan(reader.ReadInt64());
                    break;

                case TypeID.Guid:
                    {
                        byte[] bits = reader.ReadBytes(16);
                        value = new Guid(bits);
                    }
                    break;

                case TypeID.IntPtr:
                    if (IntPtr.Size == 4) {
                        value = new IntPtr(reader.ReadInt32());
                    }
                    else {
                        Debug.Assert(IntPtr.Size == 8);
                        value = new IntPtr(reader.ReadInt64());
                    }
                    break;

                case TypeID.UIntPtr:
                    if (UIntPtr.Size == 4) {
                        value = new UIntPtr(reader.ReadUInt32());
                    }
                    else {
                        Debug.Assert(UIntPtr.Size == 8);
                        value = new UIntPtr(reader.ReadUInt64());
                    }
                    break;

                case TypeID.Object:
                    BinaryFormatter formatter = new BinaryFormatter();
                    if (SessionStateUtility.SerializationSurrogateSelector != null) {
                        formatter.SurrogateSelector = SessionStateUtility.SerializationSurrogateSelector;
                    }
                    value = formatter.Deserialize(reader.BaseStream);
                    break;

                case TypeID.Null:            
                    value = null;
                    break;
            }

            return value;
        }
    }
}

