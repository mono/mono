//------------------------------------------------------------------------------
// <copyright file="StringUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * StringUtil class
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Util {
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;

/*
 * Various string handling utilities
 */
internal static class StringUtil {
    internal static string CheckAndTrimString(string paramValue, string paramName) {
        return CheckAndTrimString(paramValue, paramName, true);
    }

    internal static string CheckAndTrimString(string paramValue, string paramName, bool throwIfNull) {
        return CheckAndTrimString(paramValue, paramName, throwIfNull, -1);
    }

    internal static string CheckAndTrimString(string paramValue, string paramName,
                                              bool throwIfNull, int lengthToCheck) {
        if (paramValue == null) {
            if (throwIfNull) {
                throw new ArgumentNullException(paramName);
            }
            return null;
        }
        string trimmedValue = paramValue.Trim();
        if (trimmedValue.Length == 0) {
            throw new ArgumentException(
                SR.GetString(SR.PersonalizationProviderHelper_TrimmedEmptyString,
                                                 paramName));
        }
        if (lengthToCheck > -1 && trimmedValue.Length > lengthToCheck) {
            throw new ArgumentException(
                SR.GetString(SR.StringUtil_Trimmed_String_Exceed_Maximum_Length,
                                                 paramValue, paramName, lengthToCheck.ToString(CultureInfo.InvariantCulture)));
        }
        return trimmedValue;
    }

    internal static bool Equals(string s1, string s2) {
        if (s1 == s2) {
            return true;
        }

        if (String.IsNullOrEmpty(s1) && String.IsNullOrEmpty(s2)) {
            return true;
        }

        return false;
    }

    unsafe internal static bool Equals(string s1, int offset1, string s2, int offset2, int length) {
        if (offset1 < 0)
            throw new ArgumentOutOfRangeException("offset1");
        if (offset2 < 0)
            throw new ArgumentOutOfRangeException("offset2");
        if (length < 0)
            throw new ArgumentOutOfRangeException("length");
        if ((s1 == null ? 0 : s1.Length) - offset1 < length)
            throw new ArgumentOutOfRangeException(SR.GetString(SR.InvalidOffsetOrCount, "offset1", "length"));
        if ((s2 == null ? 0 : s2.Length) - offset2 < length)
            throw new ArgumentOutOfRangeException(SR.GetString(SR.InvalidOffsetOrCount, "offset2", "length"));
        if (String.IsNullOrEmpty(s1) || String.IsNullOrEmpty(s2))
            return true;

        fixed (char * pch1 = s1, pch2 = s2) {
            char * p1 = pch1 + offset1, p2 = pch2 + offset2;
            int c = length;
            while (c-- > 0) {
                if (*p1++ != *p2++)
                    return false;
            }
        }
        return true;
    }

    internal static bool EqualsIgnoreCase(string s1, string s2) {
        if (String.IsNullOrEmpty(s1) && String.IsNullOrEmpty(s2)) {
            return true;
        }
        if (String.IsNullOrEmpty(s1) || String.IsNullOrEmpty(s2)) {
            return false;
        }
        if(s2.Length != s1.Length) {
            return false;
        }
        return 0 == string.Compare(s1, 0, s2, 0, s2.Length, StringComparison.OrdinalIgnoreCase);
    }

    internal static bool EqualsIgnoreCase(string s1, int index1, string s2, int index2, int length) {
        return String.Compare(s1, index1, s2, index2, length, StringComparison.OrdinalIgnoreCase) == 0;
    }


    internal static string StringFromWCharPtr(IntPtr ip, int length) {
        unsafe {
            return new string((char *) ip, 0, length);
        }
    }

    internal static string StringFromCharPtr(IntPtr ip, int length) {
        return Marshal.PtrToStringAnsi(ip, length);
    }

    /*
     * Determines if the string ends with the specified character.
     * Fast, non-culture aware.  
     */
    internal static bool StringEndsWith(string s, char c) {
        int len = s.Length;
        return len != 0 && s[len - 1] == c;
    }

    /*
     * Determines if the first string ends with the second string.
     * Fast, non-culture aware.  
     */
    unsafe internal static bool StringEndsWith(string s1, string s2) {
        int offset = s1.Length - s2.Length;
        if (offset < 0) {
            return false;
        }
        
        fixed (char * pch1=s1, pch2=s2) {
            char * p1 = pch1 + offset, p2=pch2;
            int c = s2.Length;
            while (c-- > 0) {
                if (*p1++ != *p2++)
                    return false;
            }
        }

        return true;
    }

    /*
     * Determines if the first string ends with the second string, ignoring case.
     * Fast, non-culture aware.  
     */
    internal static bool StringEndsWithIgnoreCase(string s1, string s2) {
        int offset = s1.Length - s2.Length;
        if (offset < 0) {
            return false;
        }

        return 0 == string.Compare(s1, offset, s2, 0, s2.Length, StringComparison.OrdinalIgnoreCase);
    }

    /*
     * Determines if the string starts with the specified character.
     * Fast, non-culture aware.  
     */
    internal static bool StringStartsWith(string s, char c) {
        return s.Length != 0 && (s[0] == c);
    }

    /*
     * Determines if the first string starts with the second string.
     * Fast, non-culture aware.  
     */
    unsafe internal static bool StringStartsWith(string s1, string s2) {
        if (s2.Length > s1.Length) {
            return false;
        }
        
        fixed (char * pch1=s1, pch2=s2) {
            char * p1 = pch1, p2=pch2;
            int c = s2.Length;
            while (c-- > 0) {
                if (*p1++ != *p2++)
                    return false;
            }
        }

        return true;
    }

    /*
     * Determines if the first string starts with the second string, ignoring case.
     * Fast, non-culture aware.  
     */
    internal static bool StringStartsWithIgnoreCase(string s1, string s2) {
        if (String.IsNullOrEmpty(s1) || String.IsNullOrEmpty(s2)) {
            return false;
        }

        if (s2.Length > s1.Length) {
            return false;
        }

        return 0 == string.Compare(s1, 0, s2, 0, s2.Length, StringComparison.OrdinalIgnoreCase);
    }

    internal unsafe static void UnsafeStringCopy(string src, int srcIndex, char[] dest, int destIndex, int len) {
        // We do not verify the parameters in this function, as the callers are assumed
        // to have done so. This is important for users such as HttpWriter that already
        // do the argument checking, and are called several times per request.
        Debug.Assert(len >= 0, "len >= 0");

        Debug.Assert(src != null, "src != null");
        Debug.Assert(srcIndex >= 0, "srcIndex >= 0");
        Debug.Assert(srcIndex + len <= src.Length, "src");

        Debug.Assert(dest != null, "dest != null");
        Debug.Assert(destIndex >= 0, "destIndex >= 0");
        Debug.Assert(destIndex + len <= dest.Length, "dest");

        int cb = len * sizeof(char);
        fixed (char * pchSrc = src, pchDest = dest) {
            byte* pbSrc = (byte *) (pchSrc + srcIndex);
            byte* pbDest = (byte*) (pchDest + destIndex);

            memcpyimpl(pbSrc, pbDest, cb);
        }
    }

    internal static bool StringArrayEquals(string[] a, string [] b) {
        if ((a == null) != (b == null)) {
            return false;
        }

        if (a == null) {
            return true;
        }

        int n = a.Length;
        if (n != b.Length) {
            return false;
        }

        for (int i = 0; i < n; i++) {
            if (a[i] != b[i]) {
                return false;
            }
        }

        return true;
    }

    // This is copied from String.GetHashCode.  We want our own copy, because the result of
    // String.GetHashCode is not guaranteed to be the same from build to build.  But we want a
    // stable hash, since we persist things to disk (e.g. precomp scenario).  VSWhidbey 399279.
    internal static int GetStringHashCode(string s) {
        unsafe {
            fixed (char* src = s) {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                // 32bit machines.
                int* pint = (int*)src;
                int len = s.Length;
                while (len > 0) {
                    hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
                    if (len <= 2) {
                        break;
                    }
                    hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ pint[1];
                    pint += 2;
                    len -= 4;
                }
                return hash1 + (hash2 * 1566083941);
            }
        }
    }

    internal static int GetNullTerminatedByteArray(Encoding enc, string s, out byte[] bytes)
    {
        bytes = null;
        if (s == null)
	        return 0;

        // Encoding.GetMaxByteCount is faster than GetByteCount, but will probably allocate more
        // memory than needed.  Working with small short-lived strings here, so that's probably ok.
        bytes = new byte[enc.GetMaxByteCount(s.Length) + 1];
        return enc.GetBytes(s, 0, s.Length, bytes, 0);
    }

    internal unsafe static void memcpyimpl(byte* src, byte* dest, int len) {
        Debug.Assert(len >= 0, "Negative length in memcpyimpl!");

#if FEATURE_PAL
        // Portable naive implementation
        while (len-- > 0)
            *dest++ = *src++;
#else
#if IA64
        long dstAlign = (7 & (8 - (((long)dest) & 7))); // number of bytes to copy before dest is 8-byte aligned

        while ((dstAlign > 0) && (len > 0))
        {
            // <
            *dest++ = *src++;
            
            len--;
            dstAlign--;
        }

        if (len > 0)
        {
            long srcAlign = 8 - (((long)src) & 7);
            if (srcAlign != 8)
            {
                if (4 == srcAlign)
                {
                    while (len >= 4)
                    {
                        ((int*)dest)[0] = ((int*)src)[0];
                        dest += 4;
                        src  += 4;
                        len  -= 4;
                    }
                    
                    srcAlign = 2;   // fall through to 2-byte copies
                }
                
                if ((2 == srcAlign) || (6 == srcAlign))
                {
                    while (len >= 2)
                    {
                        ((short*)dest)[0] = ((short*)src)[0];
                        dest += 2;
                        src  += 2;
                        len  -= 2;
                    }
                }
                
                while (len-- > 0)
                {
                    *dest++ = *src++;
                }
            }
            else
            {
                if (len >= 16) 
                {
                    do 
                    {
                        ((long*)dest)[0] = ((long*)src)[0];
                        ((long*)dest)[1] = ((long*)src)[1];
                        dest += 16;
                        src += 16;
                    } while ((len -= 16) >= 16);
                }
                
                if(len > 0)  // protection against negative len and optimization for len==16*N
                {
                    if ((len & 8) != 0) 
                    {
                        ((long*)dest)[0] = ((long*)src)[0];
                        dest += 8;
                        src += 8;
                    }
                    if ((len & 4) != 0) 
                    {
                        ((int*)dest)[0] = ((int*)src)[0];
                        dest += 4;
                        src += 4;
                    }
                    if ((len & 2) != 0) 
                    {
                        ((short*)dest)[0] = ((short*)src)[0];
                        dest += 2;
                        src += 2;
                    }
                    if ((len & 1) != 0)
                    {
                        *dest++ = *src++;
                    }
                }
            }
        }
#else
        // <STRIP>This is Peter Sollich's faster memcpy implementation, from 
        // COMString.cpp.  For our strings, this beat the processor's 
        // repeat & move single byte instruction, which memcpy expands into.  
        // (You read that correctly.)
        // This is 3x faster than a simple while loop copying byte by byte, 
        // for large copies.</STRIP>
        if (len >= 16) {
            do {
#if AMD64
                ((long*)dest)[0] = ((long*)src)[0];
                ((long*)dest)[1] = ((long*)src)[1];
#else
                ((int*)dest)[0] = ((int*)src)[0];
                ((int*)dest)[1] = ((int*)src)[1];
                ((int*)dest)[2] = ((int*)src)[2];
                ((int*)dest)[3] = ((int*)src)[3];
#endif
                dest += 16;
                src += 16;
            } while ((len -= 16) >= 16);
        }
        if(len > 0)  // protection against negative len and optimization for len==16*N
        {
            if ((len & 8) != 0) {
#if AMD64
                ((long*)dest)[0] = ((long*)src)[0];
#else
                ((int*)dest)[0] = ((int*)src)[0];
                ((int*)dest)[1] = ((int*)src)[1];
#endif
                dest += 8;
                src += 8;
            }
            if ((len & 4) != 0) {
                ((int*)dest)[0] = ((int*)src)[0];
                dest += 4;
                src += 4;
            }
            if ((len & 2) != 0) {
                ((short*)dest)[0] = ((short*)src)[0];
                dest += 2;
                src += 2;
            }
            if ((len & 1) != 0)
                *dest++ = *src++;
        }
#endif
#endif // FEATURE_PAL
    }
    internal static string[] ObjectArrayToStringArray(object[] objectArray) {
        String[] stringKeys = new String[objectArray.Length];
        objectArray.CopyTo(stringKeys, 0);
        return stringKeys;
    }

}
}
