using System.Globalization;

namespace System.Net {
    //
    // support class for Validation related stuff.
    //
    internal static class ValidationHelper {

        public static string [] EmptyArray = new string[0];

        internal static readonly char[]  InvalidMethodChars =
                new char[]{
                ' ',
                '\r',
                '\n',
                '\t'
                };

        // invalid characters that cannot be found in a valid method-verb or http header
        internal static readonly char[]  InvalidParamChars =
                new char[]{
                '(',
                ')',
                '<',
                '>',
                '@',
                ',',
                ';',
                ':',
                '\\',
                '"',
                '\'',
                '/',
                '[',
                ']',
                '?',
                '=',
                '{',
                '}',
                ' ',
                '\t',
                '\r',
                '\n'};

        public static string [] MakeEmptyArrayNull(string [] stringArray) {
            if ( stringArray == null || stringArray.Length == 0 ) {
                return null;
            } else {
                return stringArray;
            }
        }

        public static string MakeStringNull(string stringValue) {
            if ( stringValue == null || stringValue.Length == 0) {
                return null;
            } else {
                return stringValue;
            }
        }

        /*
        // Consider removing.
        public static string MakeStringEmpty(string stringValue) {
            if ( stringValue == null || stringValue.Length == 0) {
                return String.Empty;
            } else {
                return stringValue;
            }
        }
        */

#if TRAVE
        /*
        // Consider removing.
        public static int HashCode(object objectValue) {
            if (objectValue == null) {
                return -1;
            } else {
                return objectValue.GetHashCode();
            }
        }
        */
#endif

        public static string ExceptionMessage(Exception exception) {
            if (exception==null) {
                return string.Empty;
            }
            if (exception.InnerException==null) {
                return exception.Message;
            }
            return exception.Message + " (" + ExceptionMessage(exception.InnerException) + ")";
        }

        public static string ToString(object objectValue) {
            if (objectValue == null) {
                return "(null)";
            } else if (objectValue is string && ((string)objectValue).Length==0) {
                return "(string.empty)";
            } else if (objectValue is Exception) {
                return ExceptionMessage(objectValue as Exception);
            } else if (objectValue is IntPtr) {
                return "0x" + ((IntPtr)objectValue).ToString("x");
            } else {
                return objectValue.ToString();
            }
        }

        public static string HashString(object objectValue) {
            if (objectValue == null) {
                return "(null)";
            } else if (objectValue is string && ((string)objectValue).Length==0) {
                return "(string.empty)";
            } else {
                return objectValue.GetHashCode().ToString(NumberFormatInfo.InvariantInfo);
            }
        }

        public static bool IsInvalidHttpString(string stringValue) {
            return stringValue.IndexOfAny(InvalidParamChars)!=-1;
        }

        public static bool IsBlankString(string stringValue) {
            return stringValue==null || stringValue.Length==0;
        }

        /*
        // Consider removing.
        public static bool ValidateUInt32(long address) {
            // on false, API should throw new ArgumentOutOfRangeException("address");
            return address>=0x00000000 && address<=0xFFFFFFFF;
        }
        */

        public static bool ValidateTcpPort(int port) {
            // on false, API should throw new ArgumentOutOfRangeException("port");
            return port>=IPEndPoint.MinPort && port<=IPEndPoint.MaxPort;
        }

        public static bool ValidateRange(int actual, int fromAllowed, int toAllowed) {
            // on false, API should throw new ArgumentOutOfRangeException("argument");
            return actual>=fromAllowed && actual<=toAllowed;
        }

        /*
        // Consider removing.
        public static bool ValidateRange(long actual, long fromAllowed, long toAllowed) {
            // on false, API should throw new ArgumentOutOfRangeException("argument");
            return actual>=fromAllowed && actual<=toAllowed;
        }
        */

        // There are threading tricks a malicious app can use to create an ArraySegment with mismatched 
        // array/offset/count.  Copy locally and make sure they're valid before using them.
        internal static void ValidateSegment(ArraySegment<byte> segment) {
            if (segment == null || segment.Array == null) {
                throw new ArgumentNullException("segment");
            }
            // Length zero is explicitly allowed
            if (segment.Offset < 0 || segment.Count < 0 
                || segment.Count > (segment.Array.Length - segment.Offset)) {
                throw new ArgumentOutOfRangeException("segment");
            }
        }
    }
}