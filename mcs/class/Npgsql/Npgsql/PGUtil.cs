// created on 1/6/2002 at 22:27

// Npgsql.PGUtil.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA


using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Resources;

namespace Npgsql
{
    /// <summary>
    /// Represent the frontend/backend protocol version.
    /// </summary>
    public enum ProtocolVersion
    {
        Unknown,
        Version2,
        Version3
    }

    /// <summary>
    /// Represent the backend server version.
    /// </summary>
    public sealed class ServerVersion
    {
        public static readonly Int32 ProtocolVersion2 = 2 << 16; // 131072
        public static readonly Int32 ProtocolVersion3 = 3 << 16; // 196608

        private Int32   _Major;
        private Int32   _Minor;
        private Int32   _Patch;

        internal ServerVersion(Int32 Major, Int32 Minor, Int32 Patch)
        {
            _Major = Major;
            _Minor = Minor;
            _Patch = Patch;
        }

        /// <summary>
        /// Server version major number.
        /// </summary>
        public Int32 Major
        { get { return _Major; } }

        /// <summary>
        /// Server version minor number.
        /// </summary>
        public Int32 Minor
        { get { return _Minor; } }

        /// <summary>
        /// Server version patch level number.
        /// </summary>
        public Int32 Patch
        { get { return _Patch; } }

        public static bool operator == (ServerVersion One, ServerVersion TheOther)
        {
            return
              One._Major == TheOther._Major &&
              One._Minor == TheOther._Minor &&
              One._Patch == TheOther._Patch;
        }

        public static bool operator != (ServerVersion One, ServerVersion TheOther)
        {
            return ! (One == TheOther);
        }

        public static bool operator > (ServerVersion One, ServerVersion TheOther)
        {
            return
                (One._Major > TheOther._Major) ||
                (One._Major == TheOther._Major && One._Minor > TheOther._Minor) ||
                (One._Major == TheOther._Major && One._Minor == TheOther._Minor && One._Patch > TheOther._Patch);
        }

        public static bool operator >= (ServerVersion One, ServerVersion TheOther)
        {
            return
                (One._Major > TheOther._Major) ||
                (One._Major == TheOther._Major && One._Minor > TheOther._Minor) ||
                (One._Major == TheOther._Major && One._Minor == TheOther._Minor && One._Patch >= TheOther._Patch);
        }

        public static bool operator < (ServerVersion One, ServerVersion TheOther)
        {
            return
                (One._Major < TheOther._Major) ||
                (One._Major == TheOther._Major && One._Minor < TheOther._Minor) ||
                (One._Major == TheOther._Major && One._Minor == TheOther._Minor && One._Patch < TheOther._Patch);
        }

        public static bool operator <= (ServerVersion One, ServerVersion TheOther)
        {
            return
                (One._Major < TheOther._Major) ||
                (One._Major == TheOther._Major && One._Minor < TheOther._Minor) ||
                (One._Major == TheOther._Major && One._Minor == TheOther._Minor && One._Patch <= TheOther._Patch);
        }

        public override bool Equals(object O)
        {
            return (O.GetType() == this.GetType() && this == (ServerVersion)O);
        }

        public override int GetHashCode()
        {
            return _Major ^ _Minor ^ _Patch;
        }

        /// <summary>
        /// Returns the string representation of this version in three place dot notation (Major.Minor.Patch).
        /// </summary>
        public override String ToString()
        {
            return string.Format("{0}.{1}.{2}", _Major, _Minor, _Patch);
        }
    }

    internal enum FormatCode:
    short
    {
        Text = 0,
        Binary = 1
    }

    ///<summary>
    /// This class provides many util methods to handle
    /// reading and writing of PostgreSQL protocol messages.
    /// </summary>
    internal abstract class PGUtil
    {
        // Logging related values
        private static readonly String CLASSNAME = "PGUtil";
        private static ResourceManager resman = new ResourceManager(typeof(PGUtil));

        ///<summary>
        /// This method takes a ProtocolVersion and returns an integer
        /// version number that the Postgres backend will recognize in a
        /// startup packet.
        /// </summary>
        public static Int32 ConvertProtocolVersion(ProtocolVersion Ver)
        {
            switch (Ver) {
            case ProtocolVersion.Version2 :
                return ServerVersion.ProtocolVersion2;

            case ProtocolVersion.Version3 :
                return ServerVersion.ProtocolVersion3;

            }

            // CHECKME
            // should we throw?
            return 0;
        }

        /// <summary>
        /// This method takes a version string as returned by SELECT VERSION() and returns
        /// a valid version string ("7.2.2" for example).
        /// This is only needed when running protocol version 2.
        /// This does not do any validity checks.
        /// </summary>
        public static string ExtractServerVersion (string VersionString)
        {
            Int32               Start = 0, End = 0;

            // find the first digit and assume this is the start of the version number
            for ( ; Start < VersionString.Length && ! char.IsDigit(VersionString[Start]) ; Start++);

            End = Start;

            // read until hitting whitespace, which should terminate the version number
            for ( ; End < VersionString.Length && ! char.IsWhiteSpace(VersionString[End]) ; End++);

            return VersionString.Substring(Start, End - Start + 1);
        }

        /// <summary>
        /// This method takes a version string ("7.4.1" for example) and produces
        /// the required integer version numbers (7, 4, and 1).
        /// </summary>
        public static ServerVersion ParseServerVersion (string VersionString)
        {
            String[]        Parts;

            Parts = VersionString.Split('.');

            if (Parts.Length < 2) {
                throw new FormatException(String.Format("Internal: Backend sent bad version string: {0}", VersionString));
            }

            try {
                if (Parts.Length == 2) {
                    // Coerce it into a 3-part version.
                    return new ServerVersion(ConvertBeginToInt32(Parts[0]), ConvertBeginToInt32(Parts[1]), 0);
                } else {
                    // If there are more than 3 parts, just ignore the extras, rather than rejecting it.
                    return new ServerVersion(ConvertBeginToInt32(Parts[0]), ConvertBeginToInt32(Parts[1]), ConvertBeginToInt32(Parts[2]));
                }
            } catch (Exception E) {
                throw new FormatException(String.Format("Internal: Backend sent bad version string: {0}", VersionString), E);
            }
        }

        /// <summary>
        /// Convert the beginning numeric part of the given string to Int32.
        /// For example:
        ///   Strings "12345ABCD" and "12345.54321" would both be converted to int 12345.
        /// </summary>
        private static Int32 ConvertBeginToInt32(String Raw)
        {
            Int32         Length = 0;
            for ( ; Length < Raw.Length && Char.IsNumber(Raw[Length]) ; Length++);
            return Convert.ToInt32(Raw.Substring(0, Length));
        }

        ///<summary>
        /// This method gets a C NULL terminated string from the network stream.
        /// It keeps reading a byte in each time until a NULL byte is returned.
        /// It returns the resultant string of bytes read.
        /// This string is sent from backend.
        /// </summary>
        public static String ReadString(Stream network_stream, Encoding encoding)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ReadString");

            ArrayList     buffer = new ArrayList();
            Byte          b;
            String        string_read;

            // [FIXME] Is this cast always safe?
            b = (Byte)network_stream.ReadByte();
            while(b != 0)
            {
                buffer.Add(b);
                b = (Byte)network_stream.ReadByte();
            }

            string_read = encoding.GetString((Byte[])buffer.ToArray(typeof(Byte)));
            NpgsqlEventLog.LogMsg(resman, "Log_StringRead", LogLevel.Debug, string_read);

            return string_read;
        }

        ///<summary>
        /// This method gets a length terminated string from a network stream.
        /// It returns the resultant string of bytes read.
        /// This string is sent from backend.
        /// </summary>
        public static String ReadString(Stream network_stream, Encoding encoding, Int32 length)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ReadString");

            ArrayList     buffer = new ArrayList();
            Byte          b;
            String        string_read;

            for (int C = 0 ; C < length ; C++)
            {
                // [FIXME] Is this cast always safe?
                b = (Byte)network_stream.ReadByte();
                buffer.Add(b);
            }

            string_read = encoding.GetString((Byte[])buffer.ToArray(typeof(Byte)));
            NpgsqlEventLog.LogMsg(resman, "Log_StringRead", LogLevel.Debug, string_read);

            return string_read;
        }

        ///<summary>
        /// This method writes a C NULL terminated string to the network stream.
        /// It appends a NULL terminator to the end of the String.
        /// </summary>
        public static void WriteString(String the_string, Stream network_stream, Encoding encoding)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "WriteString");

            network_stream.Write(encoding.GetBytes(the_string + '\x00') , 0, encoding.GetByteCount(the_string) + 1);
        }

        ///<summary>
        /// This method writes a C NULL terminated string limited in length to the
        /// backend server.
        /// It pads the string with null bytes to the size specified.
        /// </summary>
        public static void WriteLimString(String the_string, Int32 n, Stream network_stream, Encoding encoding)
        {
            NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "WriteLimString");

            // [FIXME] Parameters should be validated. And what about strings
            // larger than or equal to n?

            // Pad the string to the specified value.
            String string_padded = the_string.PadRight(n, '\x00');

            network_stream.Write(encoding.GetBytes(string_padded), 0, n);
        }

        public static void CheckedStreamRead(Stream stream, Byte[] buffer, Int32 offset, Int32 size)
        {
            Int32 bytes_from_stream = 0;
            Int32 total_bytes_read = 0;

            do
            {
                bytes_from_stream = stream.Read(buffer, offset + total_bytes_read, size);
                total_bytes_read += bytes_from_stream;
                size -= bytes_from_stream;
            }
            while(size > 0);
        }


        /// <summary>
        /// Write a 32-bit integer to the given stream in the correct byte order.
        /// </summary>
        public static void WriteInt32(Stream stream, Int32 value)
        {
            stream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value)), 0, 4);
        }

        /// <summary>
        /// Read a 32-bit integer from the given stream in the correct byte order.
        /// </summary>
        public static Int32 ReadInt32(Stream stream, Byte[] buffer)
        {
            CheckedStreamRead(stream, buffer, 0, 4);
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, 0));

        }

        /// <summary>
        /// Write a 16-bit integer to the given stream in the correct byte order.
        /// </summary>
        public static void WriteInt16(Stream stream, Int16 value)
        {
            stream.Write(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value)), 0, 2);
        }

        /// <summary>
        /// Read a 16-bit integer from the given stream in the correct byte order.
        /// </summary>
        public static Int16 ReadInt16(Stream stream, Byte[] buffer)
        {
            CheckedStreamRead(stream, buffer, 0, 2);
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, 0));

        }
    }
}
