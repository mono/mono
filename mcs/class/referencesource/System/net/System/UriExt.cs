/*++
Copyright (c) 2003 Microsoft Corporation

Module Name:

    UriExt.cs

Abstract:

    Uri extensibility model Implementation.
    This file utilizes partial class feature.
    Uri.cs file contains core System.Uri functionality.

Author:
    Alexei Vopilov    Nov 21 2003

Revision History:

--*/

namespace System {
    using System.Globalization;
    using System.Text;
    using System.Runtime.InteropServices;
    using System.Diagnostics;

    public partial class Uri {
        //
        // All public ctors go through here
        //
        private void CreateThis(string uri, bool dontEscape, UriKind uriKind)
        {
            // if (!Enum.IsDefined(typeof(UriKind), uriKind)) -- We currently believe that Enum.IsDefined() is too slow 
            // to be used here.
            if ((int)uriKind < (int)UriKind.RelativeOrAbsolute || (int)uriKind > (int)UriKind.Relative) {
                throw new ArgumentException(SR.GetString(SR.net_uri_InvalidUriKind, uriKind));
            }

            m_String = uri == null? string.Empty: uri;

            if (dontEscape)
                m_Flags |= Flags.UserEscaped;

            ParsingError err = ParseScheme(m_String, ref m_Flags, ref m_Syntax);
            UriFormatException e;

            InitializeUri(err, uriKind, out e);
            if (e != null)
                throw e;
        }
        //
        private void InitializeUri(ParsingError err, UriKind uriKind, out UriFormatException e)
        {
            if (err == ParsingError.None)
            {
                if (IsImplicitFile)
                {
                    // V1 compat VsWhidbey#252282
                    // A relative Uri wins over implicit UNC path unless the UNC path is of the form "\\something" and 
                    // uriKind != Absolute
                    if (
#if !PLATFORM_UNIX
                        NotAny(Flags.DosPath) &&
#endif // !PLATFORM_UNIX
                        uriKind != UriKind.Absolute &&
                       (uriKind == UriKind.Relative || (m_String.Length >= 2 && (m_String[0] != '\\' || m_String[1] != '\\'))))

                    {
                        m_Syntax = null; //make it be relative Uri
                        m_Flags &= Flags.UserEscaped; // the only flag that makes sense for a relative uri
                        e = null;
                        return;
                        // Otheriwse an absolute file Uri wins when it's of the form "\\something"
                    }
                    //
                    // VsWhidbey#423805 and V1 compat issue
                    // We should support relative Uris of the form c:\bla or c:/bla
                    //
#if !PLATFORM_UNIX
                    else if (uriKind == UriKind.Relative && InFact(Flags.DosPath))
                    {
                        m_Syntax = null; //make it be relative Uri
                        m_Flags &= Flags.UserEscaped; // the only flag that makes sense for a relative uri
                        e = null;
                        return;
                        // Otheriwse an absolute file Uri wins when it's of the form "c:\something"
                    }
#endif // !PLATFORM_UNIX
                }
            }
            else if (err > ParsingError.LastRelativeUriOkErrIndex)
            {
                //This is a fatal error based solely on scheme name parsing
                m_String = null; // make it be invalid Uri
                e = GetException(err);
                return;
            }

            //
            //
            //
            bool hasUnicode = false;

            // Is there unicode ..
            if ((!s_ConfigInitialized) && CheckForConfigLoad(m_String)){
                InitializeUriConfig();
            }

            m_iriParsing = (s_IriParsing && ((m_Syntax == null) || m_Syntax.InFact(UriSyntaxFlags.AllowIriParsing)));
            
            if (m_iriParsing && 
                (CheckForUnicode(m_String) || CheckForEscapedUnreserved(m_String))) {
                m_Flags |= Flags.HasUnicode;
                hasUnicode = true;
                // switch internal strings
                m_originalUnicodeString = m_String; // original string location changed
            }

            if (m_Syntax != null)
            {
                if (m_Syntax.IsSimple)
                {
                    if ((err = PrivateParseMinimal()) != ParsingError.None)
                    {
                        if (uriKind != UriKind.Absolute && err <= ParsingError.LastRelativeUriOkErrIndex)
                        {
                            // RFC 3986 Section 5.4.2 - http:(relativeUri) may be considered a valid relative Uri.
                            m_Syntax = null; // convert to relative uri
                            e = null;
                            m_Flags &= Flags.UserEscaped; // the only flag that makes sense for a relative uri
                        }
                        else
                            e = GetException(err);
                    }
                    else if (uriKind == UriKind.Relative)
                    {
                        // Here we know that we can create an absolute Uri, but the user has requested only a relative one
                        e = GetException(ParsingError.CannotCreateRelative);
                    }
                    else
                        e = null;
                    // will return from here

                    if (m_iriParsing && hasUnicode){
                        // In this scenario we need to parse the whole string 
                        EnsureParseRemaining();
                    }
                }
                else
                {
                    // offer custom parser to create a parsing context
                    m_Syntax = m_Syntax.InternalOnNewUri();

                    // incase they won't call us
                    m_Flags |= Flags.UserDrivenParsing;

                    // Ask a registered type to validate this uri
                    m_Syntax.InternalValidate(this, out e);

                    if (e != null)
                    {
                        // Can we still take it as a relative Uri?
                        if (uriKind != UriKind.Absolute && err != ParsingError.None 
                            && err <= ParsingError.LastRelativeUriOkErrIndex)
                        {
                            m_Syntax = null; // convert it to relative
                            e = null;
                            m_Flags &= Flags.UserEscaped; // the only flag that makes sense for a relative uri
                        }
                    }
                    else // e == null
                    {
                        if (err != ParsingError.None || InFact(Flags.ErrorOrParsingRecursion))
                        {
                            // User parser took over on an invalid Uri
                            SetUserDrivenParsing();
                        }
                        else if (uriKind == UriKind.Relative)
                        {
                            // Here we know that custom parser can create an absolute Uri, but the user has requested only a 
                            // relative one
                            e = GetException(ParsingError.CannotCreateRelative);
                        }

                        if (m_iriParsing && hasUnicode){
                            // In this scenario we need to parse the whole string 
                            EnsureParseRemaining();
                        }
                        
                    }
                    // will return from here
                }
            }
            // If we encountered any parsing errors that indicate this may be a relative Uri, 
            // and we'll allow relative Uri's, then create one.
            else if (err != ParsingError.None && uriKind != UriKind.Absolute 
                && err <= ParsingError.LastRelativeUriOkErrIndex)
            {
                e = null;
                m_Flags &= (Flags.UserEscaped | Flags.HasUnicode); // the only flags that makes sense for a relative uri
                if (m_iriParsing && hasUnicode)
                {
                    // Iri'ze and then normalize relative uris
                    m_String = EscapeUnescapeIri(m_originalUnicodeString, 0, m_originalUnicodeString.Length,
                                                (UriComponents)0);
                    try
                    {
                        if (UriParser.ShouldUseLegacyV2Quirks)
                            m_String = m_String.Normalize(NormalizationForm.FormC);
                    }
                    catch (ArgumentException)
                    {
                        e = GetException(ParsingError.BadFormat);
                    }
                }
            }
            else
            {
               m_String = null; // make it be invalid Uri
               e = GetException(err);
            }
        }
        
        //
        // Checks if there are any unicode or escaped chars or ace to determine whether to load
        // config
        //
        private unsafe bool CheckForConfigLoad(String data)
        {
            bool initConfig = false;
            int length = data.Length;

            fixed (char* temp = data){
                for (int i = 0; i < length; ++i){

                    if ((temp[i] > '\x7f') || (temp[i] == '%') ||
                        ((temp[i] == 'x') && ((i + 3) < length) && (temp[i + 1] == 'n') && (temp[i + 2] == '-') && (temp[i + 3] == '-')))
                    {

                        // Unicode or maybe ace 
                        initConfig = true;
                        break;
                    }
                }


            }

            return initConfig;
        }

        //
        // Unescapes entire string and checks if it has unicode chars
        //
        private unsafe bool CheckForUnicode(String data)
        {
            bool hasUnicode = false;
            char[] chars = new char[data.Length];
            int count = 0;

            chars = UriHelper.UnescapeString(data, 0, data.Length, chars, ref count, c_DummyChar, c_DummyChar, 
                c_DummyChar, UnescapeMode.Unescape | UnescapeMode.UnescapeAll, null, false);

            String tempStr = new string(chars, 0, count);
            int length = tempStr.Length;
            
            fixed (char* tempPtr = tempStr){
                for (int i = 0; i < length; ++i){
                    if (tempPtr[i] > '\x7f'){
                        // Unicode 
                        hasUnicode = true;
                        break;
                    }
                }
            }
            return hasUnicode;
        }

        // Does this string have any %6A sequences that are 3986 Unreserved characters?  These should be un-escaped.
        private unsafe bool CheckForEscapedUnreserved(String data)
        {
            fixed (char* tempPtr = data)
            {
                for (int i = 0; i < data.Length - 2; ++i)
                {
                    if (tempPtr[i] == '%' && IsHexDigit(tempPtr[i + 1]) && IsHexDigit(tempPtr[i + 2])
                        && tempPtr[i + 1] >= '0' && tempPtr[i + 1] <= '7') // max 0x7F
                    {
                        char ch = UriHelper.EscapedAscii(tempPtr[i + 1], tempPtr[i + 2]);
                        if (ch != c_DummyChar && UriHelper.Is3986Unreserved(ch))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        
        //
        // Check if the char (potentially surrogate) at given offset is in the iri range
        // Takes in isQuery because because iri restrictions for query are different
        //
        internal static bool CheckIriUnicodeRange(string uri, int offset, ref bool surrogatePair, bool isQuery)
        {
            char invalidLowSurr = '\uFFFF';
            return CheckIriUnicodeRange(uri[offset],(offset + 1 < uri.Length) ? uri[offset + 1] : invalidLowSurr, 
                                        ref surrogatePair, isQuery);
        }

        //
        // Checks if provided non surrogate char lies in iri range
        //
        internal static bool CheckIriUnicodeRange(char unicode, bool isQuery)
        {
            if ((unicode >= '\u00A0' && unicode <= '\uD7FF') ||
               (unicode >= '\uF900' && unicode <= '\uFDCF') ||
               (unicode >= '\uFDF0' && unicode <= '\uFFEF') ||
               (isQuery && unicode >= '\uE000' && unicode <= '\uF8FF')){
                return true;
            }else{
                return false;
            }
        }

        //
        // Check if the highSurr is in the iri range or if highSurr and lowSurr are a surr pair then 
        // it checks if the combined char is in the range
        // Takes in isQuery because because iri restrictions for query are different
        //
        internal static bool CheckIriUnicodeRange(char highSurr, char lowSurr, ref bool surrogatePair, bool isQuery)
        {   
            bool inRange = false;
            surrogatePair = false;

            if (CheckIriUnicodeRange(highSurr, isQuery)){
                inRange = true;
            }
            else if (Char.IsHighSurrogate(highSurr)){
                if (Char.IsSurrogatePair(highSurr, lowSurr)){
                    surrogatePair = true;
                    char[] chars = new char[2] { highSurr, lowSurr };
                    string surrPair = new string(chars);
                    if (((surrPair.CompareTo("\U00010000") >= 0) && (surrPair.CompareTo("\U0001FFFD") <= 0)) ||
                        ((surrPair.CompareTo("\U00020000") >= 0) && (surrPair.CompareTo("\U0002FFFD") <= 0)) ||
                        ((surrPair.CompareTo("\U00030000") >= 0) && (surrPair.CompareTo("\U0003FFFD") <= 0)) ||
                        ((surrPair.CompareTo("\U00040000") >= 0) && (surrPair.CompareTo("\U0004FFFD") <= 0)) ||
                        ((surrPair.CompareTo("\U00050000") >= 0) && (surrPair.CompareTo("\U0005FFFD") <= 0)) ||
                        ((surrPair.CompareTo("\U00060000") >= 0) && (surrPair.CompareTo("\U0006FFFD") <= 0)) ||
                        ((surrPair.CompareTo("\U00070000") >= 0) && (surrPair.CompareTo("\U0007FFFD") <= 0)) ||
                        ((surrPair.CompareTo("\U00080000") >= 0) && (surrPair.CompareTo("\U0008FFFD") <= 0)) ||
                        ((surrPair.CompareTo("\U00090000") >= 0) && (surrPair.CompareTo("\U0009FFFD") <= 0)) ||
                        ((surrPair.CompareTo("\U000A0000") >= 0) && (surrPair.CompareTo("\U000AFFFD") <= 0)) ||
                        ((surrPair.CompareTo("\U000B0000") >= 0) && (surrPair.CompareTo("\U000BFFFD") <= 0)) ||
                        ((surrPair.CompareTo("\U000C0000") >= 0) && (surrPair.CompareTo("\U000CFFFD") <= 0)) ||
                        ((surrPair.CompareTo("\U000D0000") >= 0) && (surrPair.CompareTo("\U000DFFFD") <= 0)) ||
                        ((surrPair.CompareTo("\U000E0000") >= 0) && (surrPair.CompareTo("\U000EFFFD") <= 0)) ||
                        (isQuery && (((surrPair.CompareTo("\U000F0000") >= 0) && (surrPair.CompareTo("\U000FFFFD") <= 0)) ||
                        ((surrPair.CompareTo("\U00100000") >= 0) && (surrPair.CompareTo("\U0010FFFD") <= 0)))))
                        inRange = true;
                }
            }

            return inRange;
        }
        //
        //
        //  Returns true if the string represents a valid argument to the Uri ctor
        //  If uriKind != AbsoluteUri then certain parsing erros are ignored but Uri usage is limited
        //
        public static bool TryCreate(string uriString, UriKind uriKind, out Uri result)
        {
            if ((object)uriString == null)
            {
                result = null;
                return false;
            }
            UriFormatException e = null;
            result = CreateHelper(uriString, false, uriKind, ref e);
            return (object) e == null && result != null;
        }
        //
        public static bool TryCreate(Uri baseUri, string relativeUri, out Uri result)
        {
            Uri relativeLink;
            if (TryCreate(relativeUri, UriKind.RelativeOrAbsolute, out relativeLink))
            {
                if (!relativeLink.IsAbsoluteUri)
                    return TryCreate(baseUri, relativeLink, out result);

                result = relativeLink;
                return true;
            }
            result = null;
            return false;
        }
        //
        public static bool TryCreate(Uri baseUri, Uri relativeUri, out Uri result)
        {
            result = null;

            //Consider: Work out the baseUri==null case
            if ((object)baseUri == null || (object)relativeUri == null)
                return false;

            if (baseUri.IsNotAbsoluteUri)
                return false;

            UriFormatException e;
            string newUriString = null;

            bool dontEscape;
            if (baseUri.Syntax.IsSimple)
            {
                dontEscape = relativeUri.UserEscaped;
                result = ResolveHelper(baseUri, relativeUri, ref newUriString, ref dontEscape, out e);
            }
            else
            {
                dontEscape = false;
                newUriString = baseUri.Syntax.InternalResolve(baseUri, relativeUri, out e);
            }

            if (e != null)
                return false;

            if ((object) result == null)
                result = CreateHelper(newUriString, dontEscape, UriKind.Absolute, ref e);

            return (object) e == null && result != null && result.IsAbsoluteUri;
        }
        //
        //
        public string GetComponents(UriComponents components, UriFormat format)
        {
            if (((components & UriComponents.SerializationInfoString) != 0) && components != UriComponents.SerializationInfoString)
                throw new ArgumentOutOfRangeException("components", components, SR.GetString(SR.net_uri_NotJustSerialization));

            if ((format & ~UriFormat.SafeUnescaped) != 0)
                throw new ArgumentOutOfRangeException("format");

            if (IsNotAbsoluteUri)
            {
                if (components == UriComponents.SerializationInfoString)
                    return GetRelativeSerializationString(format);
                else
                    throw new InvalidOperationException(SR.GetString(SR.net_uri_NotAbsolute));
            }

            if (Syntax.IsSimple)
                return GetComponentsHelper(components, format);

            return Syntax.InternalGetComponents(this, components, format);
        }
        //
        //
        // This is for languages that do not support == != operators overloading
        //
        // Note that Uri.Equals will get an optimized path but is limited to true/fasle result only
        //
        public static int Compare(Uri uri1, Uri uri2, UriComponents partsToCompare, UriFormat compareFormat, 
            StringComparison comparisonType)
        {

            if ((object) uri1 == null)
            {
                if (uri2 == null)
                    return 0; // Equal
                return -1;    // null < non-null
            }
            if ((object) uri2 == null)
                return 1;     // non-null > null

            // a relative uri is always less than an absolute one
            if (!uri1.IsAbsoluteUri || !uri2.IsAbsoluteUri)
                return uri1.IsAbsoluteUri? 1: uri2.IsAbsoluteUri? -1: string.Compare(uri1.OriginalString, 
                    uri2.OriginalString, comparisonType);

            return string.Compare(
                                    uri1.GetParts(partsToCompare, compareFormat),
                                    uri2.GetParts(partsToCompare, compareFormat),
                                    comparisonType
                                  );
        }

        

        public bool IsWellFormedOriginalString()
        {
            if (IsNotAbsoluteUri || Syntax.IsSimple)
                return InternalIsWellFormedOriginalString();

            return Syntax.InternalIsWellFormedOriginalString(this);
        }

        // Consider: (perf) Making it to not create a Uri internally
        public static bool IsWellFormedUriString(string uriString, UriKind uriKind)
        {
            Uri result;

            if (!Uri.TryCreate(uriString, uriKind, out result))
                return false;

            return result.IsWellFormedOriginalString();
        }

        //
        // Internal stuff
        //

        // Returns false if OriginalString value
        // (1) is not correctly escaped as per URI spec excluding intl UNC name case
        // (2) or is an absolute Uri that represents implicit file Uri "c:\dir\file"
        // (3) or is an absolute Uri that misses a slash before path "file://c:/dir/file"
        // (4) or contains unescaped backslashes even if they will be treated
        //     as forward slashes like http:\\host/path\file or file:\\\c:\path
        //
        internal unsafe bool InternalIsWellFormedOriginalString()
        {
            if (UserDrivenParsing)
                throw new InvalidOperationException(SR.GetString(SR.net_uri_UserDrivenParsing, this.GetType().FullName));

            fixed (char* str = m_String)
            {
                ushort idx = 0;
                //
                // For a relative Uri we only care about escaping and backslashes
                //
                if (!IsAbsoluteUri)
                {
                    // my:scheme/path?query is not well formed because the colon is ambiguous
                    if (!UriParser.ShouldUseLegacyV2Quirks && CheckForColonInFirstPathSegment(m_String))
                    {
                        return false;
                    }
                    return (CheckCanonical(str, ref idx, (ushort)m_String.Length, c_EOL) 
                            & (Check.BackslashInPath | Check.EscapedCanonical)) == Check.EscapedCanonical;
                }

                //
                // (2) or is an absolute Uri that represents implicit file Uri "c:\dir\file"
                //
                if (IsImplicitFile)
                    return false;

                //This will get all the offsets, a Host name will be checked separatelly below
                EnsureParseRemaining();

                Flags nonCanonical = (m_Flags & (Flags.E_CannotDisplayCanonical | Flags.IriCanonical));
                // User, Path, Query or Fragment may have some non escaped characters
                if (((nonCanonical & Flags.E_CannotDisplayCanonical & (Flags.E_UserNotCanonical | Flags.E_PathNotCanonical |
                                        Flags.E_QueryNotCanonical | Flags.E_FragmentNotCanonical)) != Flags.Zero) &&
                    (!m_iriParsing || (m_iriParsing &&
                    (((nonCanonical & Flags.E_UserNotCanonical) == 0) || ((nonCanonical & Flags.UserIriCanonical) == 0)) &&
                    (((nonCanonical & Flags.E_PathNotCanonical) == 0) || ((nonCanonical & Flags.PathIriCanonical) == 0)) &&
                    (((nonCanonical & Flags.E_QueryNotCanonical) == 0) || ((nonCanonical & Flags.QueryIriCanonical) == 0)) &&
                    (((nonCanonical & Flags.E_FragmentNotCanonical) == 0) || ((nonCanonical & Flags.FragmentIriCanonical) == 0)))))
                {
                    return false;
                }

                // checking on scheme:\\ or file:////
                if (InFact(Flags.AuthorityFound))
                {
                    idx = (ushort)(m_Info.Offset.Scheme + m_Syntax.SchemeName.Length + 2);
                    if (idx >= m_Info.Offset.User || m_String[idx - 1] == '\\' || m_String[idx] == '\\')
                        return false;

#if !PLATFORM_UNIX
                    if (InFact(Flags.UncPath | Flags.DosPath))
                    {
                        while (++idx < m_Info.Offset.User && (m_String[idx] == '/' || m_String[idx] == '\\'))
                            return false;
                    }
#endif // !PLATFORM_UNIX
                }


                // (3) or is an absolute Uri that misses a slash before path "file://c:/dir/file"
                // Note that for this check to be more general we assert that if Path is non empty and if it requires a first slash
                // (which looks absent) then the method has to fail.
                // Today it's only possible for a Dos like path, i.e. file://c:/bla would fail below check.
                if (InFact(Flags.FirstSlashAbsent) && m_Info.Offset.Query > m_Info.Offset.Path)
                    return false;

                // (4) or contains unescaped backslashes even if they will be treated
                //     as forward slashes like http:\\host/path\file or file:\\\c:\path
                // Note we do not check for Flags.ShouldBeCompressed i.e. allow // /./ and alike as valid
                if (InFact(Flags.BackslashInPath))
                    return false;

                // Capturing a rare case like file:///c|/dir
                if (IsDosPath && m_String[m_Info.Offset.Path + SecuredPathIndex - 1] == '|')
                    return false;

                //
                // May need some real CPU processing to anwser the request
                //
                //
                // Check escaping for authority
                //
                // IPv6 hosts cannot be properly validated by CheckCannonical
                if ((m_Flags & Flags.CanonicalDnsHost) == 0 && HostType != Flags.IPv6HostType)
                {
                    idx = m_Info.Offset.User;
                    Check result = CheckCanonical(str, ref idx, (ushort)m_Info.Offset.Path, '/');
                    if (((result & (Check.ReservedFound | Check.BackslashInPath | Check.EscapedCanonical)) 
                        != Check.EscapedCanonical) 
                        && (!m_iriParsing || (m_iriParsing 
                            && ((result & (Check.DisplayCanonical | Check.FoundNonAscii | Check.NotIriCanonical))
                                != (Check.DisplayCanonical | Check.FoundNonAscii)))))
                    {
                        return false;
                    }
                }

                // Want to ensure there are slashes after the scheme
                if ((m_Flags & (Flags.SchemeNotCanonical | Flags.AuthorityFound)) 
                    == (Flags.SchemeNotCanonical | Flags.AuthorityFound))
                {
                    idx = (ushort)m_Syntax.SchemeName.Length;
                    while (str[idx++] != ':') ;
                    if (idx + 1 >= m_String.Length || str[idx] != '/' || str[idx + 1] != '/')
                        return false;
                }
            }
            //
            // May be scheme, host, port or path need some canonicalization but still the uri string is found to be a 
            // "well formed" one
            //
            return true;
        }

        //
        //
        //
        public static string UnescapeDataString(string stringToUnescape)
        {
            if ((object) stringToUnescape == null)
                throw new ArgumentNullException("stringToUnescape");

            if (stringToUnescape.Length == 0)
                return string.Empty;

            unsafe {
                fixed (char* pStr = stringToUnescape)
                {
                    int position;
                    for (position = 0; position < stringToUnescape.Length; ++position)
                        if (pStr[position] == '%')
                            break;

                    if (position == stringToUnescape.Length)
                        return stringToUnescape;

                    UnescapeMode unescapeMode = UnescapeMode.Unescape | UnescapeMode.UnescapeAll;                    
                    position = 0;
                    char[] dest = new char[stringToUnescape.Length];
                    dest = UriHelper.UnescapeString(stringToUnescape, 0, stringToUnescape.Length, dest, ref position, 
                        c_DummyChar, c_DummyChar, c_DummyChar, unescapeMode, null, false);
                    return new string(dest, 0, position);
                }
            }
        }
        //
        // Where stringToEscape is intented to be a completely unescaped URI string.
        // This method will escape any character that is not a reserved or unreserved character, including percent signs.
        // Note that EscapeUriString will also do not escape a '#' sign.
        //
        public static string EscapeUriString(string stringToEscape)
        {
            if ((object)stringToEscape == null)
                throw new ArgumentNullException("stringToEscape");

            if (stringToEscape.Length == 0)
                return string.Empty;

            int position = 0;
            char[] dest = UriHelper.EscapeString(stringToEscape, 0, stringToEscape.Length, null, ref position, true, 
                c_DummyChar, c_DummyChar, c_DummyChar);
            if ((object) dest == null)
                return stringToEscape;
            return new string(dest, 0, position);
        }
        //
        // Where stringToEscape is intended to be URI data, but not an entire URI.
        // This method will escape any character that is not an unreserved character, including percent signs.
        //
        public static string EscapeDataString(string stringToEscape)
        {
            if ((object) stringToEscape == null)
                throw new ArgumentNullException("stringToEscape");

            if (stringToEscape.Length == 0)
                return string.Empty;

            int position = 0;
            char[] dest = UriHelper.EscapeString(stringToEscape, 0, stringToEscape.Length, null, ref position, false, 
                c_DummyChar, c_DummyChar, c_DummyChar);
            if (dest == null)
                return stringToEscape;
            return new string(dest, 0, position);
        }

        //
        // Check reserved chars according to rfc 3987 in a sepecific component
        //
        internal bool CheckIsReserved(char ch, UriComponents component)
        {
            if ((component != UriComponents.Scheme) ||
                    (component != UriComponents.UserInfo) ||
                    (component != UriComponents.Host) ||
                    (component != UriComponents.Port) ||
                    (component != UriComponents.Path) ||
                    (component != UriComponents.Query) ||
                    (component != UriComponents.Fragment)
                )
                return (component == (UriComponents)0)? IsGenDelim(ch): false;
            else 
            {
                switch(component)
                {
                    // Reserved chars according to rfc 3987
                    case UriComponents.UserInfo:
                        if( ch == '/' || ch == '?' || ch == '#' || ch == '[' || ch == ']' || ch == '@' )
                            return true;
                        break;
                    case UriComponents.Host:
                        if( ch == ':' || ch == '/' || ch == '?' || ch == '#' || ch == '[' || ch == ']' || ch == '@' )
                            return true;
                        break;
                    case UriComponents.Path:
                        if( ch == '/' || ch == '?' || ch == '#' || ch == '[' || ch == ']' )
                            return true;
                        break;
                    case UriComponents.Query:
                        if(ch == '#' || ch == '[' || ch == ']')
                            return true;
                        break;
                    case UriComponents.Fragment:
                        if(ch == '#' || ch == '[' || ch == ']')
                            return true;
                        break;
                    default:
                        break;
                }
                return false;
            }
        }

        //
        // Cleans up the specified component according to Iri rules
        // a) Chars allowed by iri in a component are unescaped if found escaped
        // b) Bidi chars are stripped
        //
        // should be called only if IRI parsing is switched on 
        internal unsafe string EscapeUnescapeIri(string input, int start, int end, UriComponents component)
        {
            fixed (char *pInput = input)
            {
                return EscapeUnescapeIri(pInput, start, end, component);
            }
        }
        
        //
        // See above explanation
        //
        internal unsafe string EscapeUnescapeIri(char* pInput, int start, int end, UriComponents component)
        {

            char [] dest = new char[ end - start ];
            byte[] bytes = null;

            // Pin the array to do pointer accesses
            GCHandle destHandle = GCHandle.Alloc(dest, GCHandleType.Pinned);
            char* pDest = (char*)destHandle.AddrOfPinnedObject();

            int escapedReallocations = 0;
            const int bufferCapacityIncrease = 30;

            int next = start;
            int destOffset = 0;
            char ch;
            bool escape = false;
            bool surrogatePair = false;
            bool isUnicode = false;

            for (;next < end; ++next)
            {
                escape = false;
                surrogatePair = false;
                isUnicode = false;

                if ((ch = pInput[next]) == '%'){
                    if (next + 2 < end){
                        ch = UriHelper.EscapedAscii(pInput[next + 1], pInput[next + 2]);
                        // Do not unescape a reserved char
                        if (ch == c_DummyChar || ch == '%' || CheckIsReserved(ch, component) || UriHelper.IsNotSafeForUnescape(ch)){
                            // keep as is
                            pDest[destOffset++] = pInput[next++];
                            pDest[destOffset++] = pInput[next++];
                            pDest[destOffset++] = pInput[next];
                            continue;
                        }
                        else if (ch <= '\x7F'){
                            //ASCII
                            pDest[destOffset++] = ch;
                            next += 2;
                            continue;
                        }else{
                            // possibly utf8 encoded sequence of unicode

                            // check if safe to unescape according to Iri rules

                            int startSeq = next;
                            int byteCount = 1;
                            // lazy initialization of max size, will reuse the array for next sequences
                            if ((object)bytes == null)
                                bytes = new byte[end - next];

                            bytes[0] = (byte)ch;
                            next += 3;
                            while (next < end)
                            {
                                // Check on exit criterion
                                if ((ch = pInput[next]) != '%' || next + 2 >= end)
                                    break;

                                // already made sure we have 3 characters in str
                                ch = UriHelper.EscapedAscii(pInput[next + 1], pInput[next + 2]);

                                //invalid hex sequence ?
                                if (ch == c_DummyChar)
                                    break;
                                // character is not part of a UTF-8 sequence ?
                                else if (ch < '\x80')
                                    break;
                                else
                                {
                                    //a UTF-8 sequence
                                    bytes[byteCount++] = (byte)ch;
                                    next += 3;
                                }
                            }
                            next--; // for loop will increment

                            Encoding noFallbackCharUTF8 = (Encoding)Encoding.UTF8.Clone();
                            noFallbackCharUTF8.EncoderFallback = new EncoderReplacementFallback("");
                            noFallbackCharUTF8.DecoderFallback = new DecoderReplacementFallback("");

                            char[] unescapedChars = new char[bytes.Length];
                            int charCount = noFallbackCharUTF8.GetChars(bytes, 0, byteCount, unescapedChars, 0);

 
                            if (charCount != 0){
                                
                                // need to check for invalid utf sequences that may not have given any chars
                                
                                // check if unicode value is allowed
                                UriHelper.MatchUTF8Sequence(  pDest, dest, ref destOffset, unescapedChars, charCount, bytes, 
                                    byteCount, component == UriComponents.Query, true);
                            }
                            else
                            {
                                // copy escaped sequence as is
                                for (int i = startSeq; i <= next; ++i)
                                    pDest[destOffset++] = pInput[i];
                            }

                        }

                    }else{
                        pDest[destOffset++] = pInput[next];
                    }
                }
                else if (ch > '\x7f'){
                    // unicode

                    char ch2;

                    if ((Char.IsHighSurrogate(ch)) && (next + 1 < end)){
                        ch2 = pInput[next + 1];
                        escape = !CheckIriUnicodeRange(ch, ch2, ref surrogatePair, component == UriComponents.Query);
                        if (!escape){
                            // copy the two chars
                            pDest[destOffset++] = pInput[next++];
                            pDest[destOffset++] = pInput[next];
                        }else{
                            isUnicode = true;
                        }
                    }else{
                        if(CheckIriUnicodeRange(ch, component == UriComponents.Query)){
                            if (!IsBidiControlCharacter(ch)){
                                // copy it
                                pDest[destOffset++] = pInput[next];
                            }
                        }else{
                            // escape it
                            escape = true;
                            isUnicode = true;
                        }
                    }
                }else{
                    // just copy the character
                    pDest[destOffset++] = pInput[next];
                }

                if (escape){
                    if (escapedReallocations < 4){
                        // may need more memory since we didn't anticipate escaping
                        int newBufferLength = dest.Length + (bufferCapacityIncrease - escapedReallocations) * 3;
                        escapedReallocations = bufferCapacityIncrease;

                        char[] newDest = new char[newBufferLength];

                        fixed (char* pNewDest = newDest){
                            Buffer.Memcpy((byte *)pNewDest, (byte *)pDest, destOffset * sizeof(char));
                        }
                        if (destHandle.IsAllocated)
                            destHandle.Free();
                        dest = newDest;

                        // re-pin new dest[] array
                        destHandle = GCHandle.Alloc(dest, GCHandleType.Pinned);
                        pDest = (char*)destHandle.AddrOfPinnedObject();
                    }else{
                        if (isUnicode){
                            if (surrogatePair)
                                escapedReallocations -= 4;
                            else
                                escapedReallocations -= 3;
                        }
                        else
                            --escapedReallocations;
                    }

                    byte[] encodedBytes = new byte[4];
                    fixed (byte* pEncodedBytes = encodedBytes){
                        int encodedBytesCount = Encoding.UTF8.GetBytes(pInput + next, surrogatePair ? 2 : 1, pEncodedBytes, 4);

                        for (int count = 0; count < encodedBytesCount; ++count)
                            UriHelper.EscapeAsciiChar((char)encodedBytes[count], dest, ref destOffset);
                    }
                }
            }

            if (destHandle.IsAllocated)
                destHandle.Free();
            return new string(dest, 0 , destOffset );
        }

        // Should never be used except by the below method
        private Uri(Flags flags, UriParser uriParser, string uri)
        {
            m_Flags = flags;
            m_Syntax = uriParser;
            m_String = uri;
        }
        //
        // a Uri.TryCreate() method goes through here.
        //
        internal static Uri CreateHelper(string uriString, bool dontEscape, UriKind uriKind, ref UriFormatException e)
        {
            // if (!Enum.IsDefined(typeof(UriKind), uriKind)) -- We currently believe that Enum.IsDefined() is too slow 
            // to be used here.
            if ((int)uriKind < (int)UriKind.RelativeOrAbsolute || (int)uriKind > (int)UriKind.Relative){
                throw new ArgumentException(SR.GetString(SR.net_uri_InvalidUriKind, uriKind));
            }

            UriParser syntax = null;
            Flags flags = Flags.Zero;
            ParsingError err = ParseScheme(uriString, ref flags, ref syntax);

            if (dontEscape)
                flags |= Flags.UserEscaped;

            // We won't use User factory for these errors
            if (err != ParsingError.None)
            {
                // If it looks as a relative Uri, custom factory is ignored
                if (uriKind != UriKind.Absolute && err <= ParsingError.LastRelativeUriOkErrIndex)
                    return new Uri((flags & Flags.UserEscaped), null, uriString);

                return null;
            }

            // Cannot be relative Uri if came here
            Uri result = new Uri(flags, syntax, uriString);

            // Validate instance using ether built in or a user Parser
            try
            {
                result.InitializeUri(err, uriKind, out e);

                if (e == null)
                    return result;

                return null;
            }
            catch (UriFormatException ee)
            {
                Debug.Assert(!syntax.IsSimple, "A UriPraser threw on InitializeAndValidate.");
                e = ee;
                // A precaution since custom Parser should never throw in this case.
                return null;
            }
        }
        //
        // Resolves into either baseUri or relativeUri according to conditions OR if not possible it uses newUriString 
        // to  return combined URI strings from both Uris 
        // otherwise if e != null on output the operation has failed
        //

        internal static Uri ResolveHelper(Uri baseUri, Uri relativeUri, ref string newUriString, ref bool userEscaped, 
            out UriFormatException e)
        {
            Debug.Assert(!baseUri.IsNotAbsoluteUri && !baseUri.UserDrivenParsing, "Uri::ResolveHelper()|baseUri is not Absolute or is controlled by User Parser.");

            e = null;
            string relativeStr = string.Empty;

            if ((object)relativeUri != null)
            {
                if (relativeUri.IsAbsoluteUri)
                    return relativeUri;

                relativeStr = relativeUri.OriginalString;
                userEscaped = relativeUri.UserEscaped;
            }
            else
                relativeStr = string.Empty;

            // Here we can assert that passed "relativeUri" is indeed a relative one

            if (relativeStr.Length > 0 && (IsLWS(relativeStr[0]) || IsLWS(relativeStr[relativeStr.Length - 1])))
                relativeStr = relativeStr.Trim(_WSchars);

            if (relativeStr.Length == 0)
            {
                newUriString = baseUri.GetParts(UriComponents.AbsoluteUri, 
                    baseUri.UserEscaped ? UriFormat.UriEscaped : UriFormat.SafeUnescaped);
                return null;
            }

            // Check for a simple fragment in relative part
            if (relativeStr[0] == '#' && !baseUri.IsImplicitFile && baseUri.Syntax.InFact(UriSyntaxFlags.MayHaveFragment))
            {
                newUriString = baseUri.GetParts(UriComponents.AbsoluteUri & ~UriComponents.Fragment, 
                    UriFormat.UriEscaped) + relativeStr;
                return null;
            }
            
            // Check for a simple query in relative part
            if (relativeStr[0] == '?' && !baseUri.IsImplicitFile && baseUri.Syntax.InFact(UriSyntaxFlags.MayHaveQuery))
            {
                newUriString = baseUri.GetParts(UriComponents.AbsoluteUri & ~UriComponents.Query & ~UriComponents.Fragment, 
                    UriFormat.UriEscaped) + relativeStr;
                return null;
            }
            
            // Check on the DOS path in the relative Uri (a special case)
            if (relativeStr.Length >= 3
                && (relativeStr[1] == ':' || relativeStr[1] == '|')
                && IsAsciiLetter(relativeStr[0])
                && (relativeStr[2] == '\\' || relativeStr[2] == '/'))
            {

                if (baseUri.IsImplicitFile)
                {
                    // It could have file:/// prepended to the result but we want to keep it as *Implicit* File Uri
                    newUriString = relativeStr;
                    return null;
                }
                else if (baseUri.Syntax.InFact(UriSyntaxFlags.AllowDOSPath))
                {
                    // The scheme is not changed just the path gets replaced
                    string prefix;
                    if (baseUri.InFact(Flags.AuthorityFound))
                        prefix = baseUri.Syntax.InFact(UriSyntaxFlags.PathIsRooted) ? ":///" : "://";
                    else
                        prefix = baseUri.Syntax.InFact(UriSyntaxFlags.PathIsRooted) ? ":/" : ":";

                    newUriString = baseUri.Scheme + prefix + relativeStr;
                    return null;
                }
                // If we are here then input like "http://host/path/" + "C:\x" will produce the result  http://host/path/c:/x
            }


            ParsingError err = GetCombinedString(baseUri, relativeStr, userEscaped, ref newUriString);

            if (err != ParsingError.None)
            {
                e = GetException(err);
                return null;
            }

            if ((object)newUriString == (object)baseUri.m_String)
                return baseUri;

            return null;
        }

        private unsafe string GetRelativeSerializationString(UriFormat format)
        {
            if (format == UriFormat.UriEscaped)
            {
                if (m_String.Length == 0)
                    return string.Empty;
                int position = 0;
                char[] dest = UriHelper.EscapeString(m_String, 0, m_String.Length, null, ref position, true, 
                    c_DummyChar, c_DummyChar, '%');
                if ((object)dest == null)
                    return m_String;
                return new string(dest, 0, position);
            }

            else if (format == UriFormat.Unescaped)
                return UnescapeDataString(m_String);

            else if (format == UriFormat.SafeUnescaped)
            {
                if (m_String.Length == 0)
                    return string.Empty;

                char[] dest = new char[m_String.Length];
                int position = 0;
                dest = UriHelper.UnescapeString(m_String, 0, m_String.Length, dest, ref position, c_DummyChar, 
                    c_DummyChar, c_DummyChar, UnescapeMode.EscapeUnescape, null, false);
                return new string(dest, 0, position);
            }
            else
                throw new ArgumentOutOfRangeException("format");

        }

        //
        // UriParser helpers methods
        //
        internal string GetComponentsHelper(UriComponents uriComponents, UriFormat uriFormat)
        {
            if (uriComponents == UriComponents.Scheme)
                return m_Syntax.SchemeName;

            // A serialzation info is "almost" the same as AbsoluteUri except for IPv6 + ScopeID hostname case
            if ((uriComponents & UriComponents.SerializationInfoString) != 0)
                uriComponents |= UriComponents.AbsoluteUri;

            //This will get all the offsets, HostString will be created below if needed
            EnsureParseRemaining();

            if ((uriComponents & UriComponents.NormalizedHost) != 0)
            {
                // Down the path we rely on Host to be ON for NormalizedHost
                uriComponents |= UriComponents.Host;
            }

            //Check to see if we need the host/authotity string
            if ((uriComponents & UriComponents.Host) != 0)
                EnsureHostString(true);

            //This, single Port request is always processed here
            if (uriComponents == UriComponents.Port || uriComponents == UriComponents.StrongPort)
            {
                if (((m_Flags & Flags.NotDefaultPort) != 0) || (uriComponents == UriComponents.StrongPort 
                    && m_Syntax.DefaultPort != UriParser.NoDefaultPort))
                {
                    // recreate string from the port value
                    return m_Info.Offset.PortValue.ToString(CultureInfo.InvariantCulture);
                }
                return string.Empty;
            }

            if ((uriComponents & UriComponents.StrongPort) != 0)
            {
                // Down the path we rely on Port to be ON for StrongPort
                uriComponents |= UriComponents.Port;
            }

            //This request sometime is faster to process here
            if (uriComponents == UriComponents.Host && (uriFormat == UriFormat.UriEscaped 
                || (( m_Flags & (Flags.HostNotCanonical | Flags.E_HostNotCanonical)) == 0)))
            {
                EnsureHostString(false);
                return m_Info.Host;
            }

            switch (uriFormat)
            {
                case UriFormat.UriEscaped:
                    return GetEscapedParts(uriComponents);

                case V1ToStringUnescape:
                case UriFormat.SafeUnescaped:
                case UriFormat.Unescaped:
                    return GetUnescapedParts(uriComponents, uriFormat);

                default:
                    throw new ArgumentOutOfRangeException("uriFormat");
            }
        }
        //
        //
        public bool IsBaseOf(Uri uri)
        {
            if ((object)uri == null)
                throw new ArgumentNullException("uri");

            if (!IsAbsoluteUri)
                return false;

            if (Syntax.IsSimple)
                return IsBaseOfHelper(uri);

            return Syntax.InternalIsBaseOf(this, uri);
        }
        //
        //
        //
        internal bool IsBaseOfHelper(Uri uriLink)
        {
            //TO 
            if (!IsAbsoluteUri || UserDrivenParsing)
                return false;

            if (!uriLink.IsAbsoluteUri)
            {
                //a relative uri could have quite tricky form, it's better to fix it now.
                string newUriString = null;
                UriFormatException e;
                bool dontEscape = false;

                uriLink = ResolveHelper(this, uriLink, ref newUriString, ref dontEscape, out e);
                if (e != null)
                    return false;

                if ((object)uriLink == null)
                    uriLink = CreateHelper(newUriString, dontEscape, UriKind.Absolute, ref e);

                if (e != null)
                    return false;
            }

            if (Syntax.SchemeName != uriLink.Syntax.SchemeName)
                return false;

            // Canonicalize and test for substring match up to the last path slash
            string me = GetParts(UriComponents.AbsoluteUri & ~UriComponents.Fragment, UriFormat.SafeUnescaped);
            string she = uriLink.GetParts(UriComponents.AbsoluteUri & ~UriComponents.Fragment, UriFormat.SafeUnescaped);

            unsafe
            {
                fixed (char* pMe = me)
                {
                    fixed (char* pShe = she)
                    {
                        return UriHelper.TestForSubPath(pMe, (ushort)me.Length, pShe, (ushort)she.Length, 
                            IsUncOrDosPath || uriLink.IsUncOrDosPath);
                    }
                }
            }
        }
        //
        // Only a ctor time call
        //
        private void CreateThisFromUri(Uri otherUri)
        {
            // Clone the other guy but develop own UriInfo member
            m_Info = null;

            m_Flags = otherUri.m_Flags;
            if (InFact(Flags.MinimalUriInfoSet))
            {
                m_Flags &= ~(Flags.MinimalUriInfoSet | Flags.AllUriInfoSet | Flags.IndexMask);
                // Port / Path offset
                int portIndex = otherUri.m_Info.Offset.Path;
                if (InFact(Flags.NotDefaultPort))
                {
                    // Find the start of the port.  Account for non-canonical ports like :00123
                    while (otherUri.m_String[portIndex] != ':' && portIndex > otherUri.m_Info.Offset.Host)
                    {
                        portIndex--;
                    }
                    if (otherUri.m_String[portIndex] != ':')
                    {
                        // Something wrong with the NotDefaultPort flag.  Reset to path index
                        Debug.Assert(false, "Uri failed to locate custom port at index: " + portIndex);
                        portIndex = otherUri.m_Info.Offset.Path;
                    }
                }
                m_Flags |= (Flags)portIndex; // Port or path
            }

            m_Syntax = otherUri.m_Syntax;
            m_String = otherUri.m_String;
            m_iriParsing = otherUri.m_iriParsing;
            if (otherUri.OriginalStringSwitched){
                m_originalUnicodeString = otherUri.m_originalUnicodeString;
            }
            if (otherUri.AllowIdn && (otherUri.InFact(Flags.IdnHost) || otherUri.InFact(Flags.UnicodeHost))){
                m_DnsSafeHost = otherUri.m_DnsSafeHost;
            }
        }
    }
}
