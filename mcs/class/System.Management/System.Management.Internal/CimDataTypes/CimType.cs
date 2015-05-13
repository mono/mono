/******************************************************************************
* The MIT License
* Copyright (c) 2007 Novell Inc.,  www.novell.com
*
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to  permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/

// Authors:
// 		Thomas Wiest (twiest@novell.com)
//		Rusty Howell  (rhowell@novell.com)
//
// (C)  Novell Inc.


using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Internal
{
    /// <summary>
    /// Utils class for CimType
    /// </summary>
    internal class CimTypeUtils
    {
        #region Methods

        /// <summary>
        /// Returns true if the type is a reference type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsEmbeddedObjectType(CimType type)
        {
            return (type == CimType.EMBEDDEDCLASS) || (type == CimType.EMBEDDEDINSTANCE); 
        }
        /// <summary>
        /// Returns true if the type is a reference type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsReferenceType(CimType type)
        {
            return (type == CimType.REFERENCE); 
        }

        /// <summary>
        /// Returns true if the type is a numeric type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool isNumericType(CimType type)
        {
            switch (type)
            {
                case CimType.UINT8:
                case CimType.SINT8:
                case CimType.UINT16:
                case CimType.SINT16:
                case CimType.UINT32:
                case CimType.SINT32:
                case CimType.UINT64:
                case CimType.SINT64:
                case CimType.REAL32:
                case CimType.REAL64:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Converts a string to a bool
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool StrToBool(string str)
        {
            switch (str.ToLower())
            {
                case "true":
                    return true;

                case "false":
                    return false;

                default:
                    throw (new Exception("Not a Boolean String"));
            }
        }

        /// <summary>
        /// Converts a nullable CimType to a string
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string CimTypeToStr(CimType? type)
        {
            if (type == null)
                throw new Exception("Not implemented yet");

            return CimTypeToStr((CimType) type);
        }

        /// <summary>
        /// Converts a CimType to a string
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string CimTypeToStr(CimType type)
        {
            switch (type)
            {
                case CimType.CIMNULL:
                    return "CimNull";

                case CimType.UINT8:
                    return "UInt8";

                case CimType.SINT8:
                    return "SInt8";

                case CimType.UINT16:
                    return "UInt16";

                case CimType.SINT16:
                    return "SInt16";

                case CimType.UINT32:
                    return "UInt32";

                case CimType.SINT32:
                    return "SInt32";

                case CimType.UINT64:
                    return "UInt64";

                case CimType.SINT64:
                    return "SInt64";

                case CimType.STRING:
                    return "String";

                case CimType.BOOLEAN:
                    return "Boolean";

                case CimType.REAL32:
                    return "Real32";

                case CimType.REAL64:
                    return "Real64";

                case CimType.DATETIME:
                    return "DateTime";

                case CimType.CHAR16:
                    return "Char16";

                case CimType.REFERENCE:
                    return "Reference";

                case CimType.EMBEDDEDCLASS:
                    return "EmbeddedClass";

                case CimType.EMBEDDEDINSTANCE:
                    return "EmbeddedInstance";

                case CimType.MAXDATATYPE:
                    return "MaxDataType";

                case CimType.INVALID:
                    return "Invalid";

                default:
                    throw new Exception("Not implemented yet");
            }
        }

        /// <summary>
        /// Converts a string to a CimType
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static CimType StrToCimType(string str)
        {
            //  boolean | string | char16 | uint8 | sint8 | uint16 | sint16 | uint32 | 
         //  sint32 | uint64 | sint64 | datetime | real32 | real64)"
            switch (str.ToLower())
            {
                case "boolean":
                    return CimType.BOOLEAN;
                case "string":
                    return CimType.STRING;
                case "char16":
                    return CimType.CHAR16;
                case "uint8":
                    return CimType.UINT8;
                case "sint8":
                    return CimType.SINT8;
                case "uint16":
                    return CimType.UINT16;
                case "sint16":
                    return CimType.SINT16;
                case "uint32":
                    return CimType.UINT32;
                case "sint32":
                    return CimType.SINT32;
                case "uint64":
                    return CimType.UINT64;
                case "sint64":
                    return CimType.SINT64;
                case "datetime":
                    return CimType.DATETIME;
                case "real32":
                    return CimType.REAL32;
                case "real64":
                    return CimType.REAL64;
                default:
                    throw new Exception("Unknown CimType");
            }
        }

        #endregion
    }

    /// <summary>
    /// The defined types for a non-reference CimProperty, CimQualifier, or non-reference CimParameter
    /// </summary>
    public enum CimType
    {
        CIMNULL = 0,		// Null type
        UINT8 = 1,		// Unsigned 8-bit integer  				(SUPPORTED)
        SINT8 = 2,		// Signed 8-bit integer    				(SUPPORTED)
        UINT16 = 3,		// Unsigned 16-bit integer 				(SUPPORTED)
        SINT16 = 4,		// Signed 16-bit integer					(SUPPORTED)
        UINT32 = 5,		// Unsigned 32-bit integer					(SUPPORTED)
        SINT32 = 6,		// Signed 32-bit integer					(SUPPORTED)
        UINT64 = 7,		// Unsigned 64-bit integer					(SUPPORTED)
        SINT64 = 8,		// Signed 64-bit integer					(SUPPORTED)
        STRING = 9,		// Unicode string								(SUPPORTED)
        BOOLEAN = 10,		// boolean										(SUPPORTED)
        REAL32 = 11,		// IEEE 4-byte floating-point				(SUPPORTED)
        REAL64 = 12,		// IEEE 8-byte floating-point				(SUPPORTED)
        DATETIME = 13,		// date-time as a string					(SUPPORTED)
        CHAR16 = 14,		// 16-bit UCS-2 character					(SUPPORTED)
        REFERENCE = 15,		// Reference type								(SUPPORTED)
        EMBEDDEDCLASS = 16,		// Embedded Class				(SUPPORTED)
        EMBEDDEDINSTANCE = 17,		// Embedded Instance				(SUPPORTED)
        MAXDATATYPE = 18,		// Marker for valid checks
        INVALID = 99		// Invalid type
    };
}
