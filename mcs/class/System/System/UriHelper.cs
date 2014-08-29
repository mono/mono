using System;
using System.Globalization;
using System.Text;
using System.Collections.Generic;

namespace System {
	internal class UriHelper {
		internal const UriFormat ToStringUnescape = (UriFormat) 0x7FFF;

		internal static bool IriParsing	{
			get { return Uri.IriParsing; }
		}

		[Flags]
		internal enum FormatFlags {
			None = 0,
			HasComponentCharactersToNormalize = 1 << 0,
			HasUriCharactersToNormalize = 1 << 1,
			HasHost = 1 << 2,
			HasFragmentPercentage = 1 << 3,
			UserEscaped = 1 << 4,
		}

		[Flags]
		internal enum UriSchemes {
			Http = 1 << 0,
			Https = 1 << 1,
			File = 1 << 2,
			Ftp = 1 << 3,
			Gopher = 1 << 4,
			Ldap = 1 << 5,
			Mailto = 1 << 6,
			NetPipe = 1 << 7,
			NetTcp = 1 << 8,
			News = 1 << 9,
			Nntp = 1 << 10,
			Telnet = 1 << 11,
			Uuid = 1 << 12,
			Custom = 1 << 13,
			CustomWithHost = 1 << 14,
			All = ~0,
			None = 0
		}

		private static UriSchemes GetScheme (string schemeName)
		{
			schemeName = schemeName.ToLower ();

			if (schemeName == "")
				return UriSchemes.None;
			if (schemeName == Uri.UriSchemeHttp)
				return UriSchemes.Http;
			if (schemeName == Uri.UriSchemeHttps)
				return UriSchemes.Https;
			if (schemeName == Uri.UriSchemeFile)
				return UriSchemes.File;
			if (schemeName == Uri.UriSchemeFtp)
				return UriSchemes.Ftp;
			if (schemeName == Uri.UriSchemeGopher)
				return UriSchemes.Gopher;
			if (schemeName == Uri.UriSchemeLdap)
				return UriSchemes.Ldap;
			if (schemeName == Uri.UriSchemeMailto)
				return UriSchemes.Mailto;
			if (schemeName == Uri.UriSchemeNetPipe)
				return UriSchemes.NetPipe;
			if (schemeName == Uri.UriSchemeNetTcp)
				return UriSchemes.NetTcp;
			if (schemeName == Uri.UriSchemeNews)
				return UriSchemes.News;
			if (schemeName == Uri.UriSchemeNntp)
				return UriSchemes.Nntp;
			if (schemeName == Uri.UriSchemeTelnet)
				return UriSchemes.Telnet;
			if (schemeName == Uri.UriSchemeUuid)
				return UriSchemes.Uuid;

			return UriSchemes.Custom;
		}

		internal static bool SchemeContains (UriSchemes keys, UriSchemes flag)
		{
			return (keys & flag) != 0;
		}

		internal static bool IsKnownScheme(string scheme)
		{
			return GetScheme(scheme) != UriSchemes.Custom;
		}

		internal static string HexEscapeMultiByte (char character)
		{
			const string hex_upper_chars = "0123456789ABCDEF";
			string ret = "";
			byte [] bytes = Encoding.UTF8.GetBytes (new [] {character});
			foreach (byte b in bytes)
				ret += "%" + hex_upper_chars [((b & 0xf0) >> 4)] + hex_upper_chars [((b & 0x0f))];

			return ret;
		}

		internal static bool SupportsQuery (string scheme)
		{
			return SupportsQuery (GetScheme (scheme));
		}

		internal static bool SupportsQuery(UriSchemes scheme)
		{
			if (SchemeContains (scheme, UriSchemes.File))
				return IriParsing;

			return !SchemeContains (scheme, UriSchemes.Ftp | UriSchemes.Gopher | UriSchemes.Nntp | UriSchemes.Telnet | UriSchemes.News);
		}

		internal static bool HasCharactersToNormalize(string str)
		{
			int len = str.Length;
			for (int i = 0; i < len; i++) {
				char c = str [i];
				if (c != '%')
					continue;

				int iStart = i;
				char surrogate;
				char x = Uri.HexUnescapeMultiByte (str, ref i, out surrogate);

				bool isEscaped = i - iStart > 1;
				if (!isEscaped)
					continue;

				if ((x >= 'A' && x <= 'Z') || (x >= 'a' && x <= 'z') || (x >= '0' && x <= '9') || 
					 x == '-' || x == '.' || x == '_' || x == '~')
					return true;

				if (x > 0x7f)
					return true;
			}

			return false;
		}

		internal static bool HasPercentage (string str)
		{
			int len = str.Length;
			for (int i = 0; i < len; i++) {
				char c = str [i];
				if (c != '%')
					continue;

				int iStart = i;
				char surrogate;
				char x = Uri.HexUnescapeMultiByte (str, ref i, out surrogate);

				bool isEscaped = i - iStart > 1;
				if (!isEscaped || x == '%')
					return true;
			}

			return false;
		}

		internal static string FormatAbsolute (string str, string schemeName,
			UriComponents component, UriFormat uriFormat, FormatFlags formatFlags = FormatFlags.None)
		{
			return Format (str, schemeName, UriKind.Absolute, component, uriFormat, formatFlags);
		}

		internal static string FormatRelative (string str, string schemeName, UriFormat uriFormat)
		{
			var formatFlags = FormatFlags.None;
			if (HasCharactersToNormalize (str))
				formatFlags |= FormatFlags.HasUriCharactersToNormalize;

			return Format (str, schemeName, UriKind.Relative, UriComponents.Path, uriFormat, formatFlags);
		}

		private static string Format (string str, string schemeName, UriKind uriKind,
			UriComponents component, UriFormat uriFormat, FormatFlags formatFlags)
		{
			if (string.IsNullOrEmpty (str))
				return "";

			if (UriHelper.HasCharactersToNormalize (str))
				formatFlags |= UriHelper.FormatFlags.HasComponentCharactersToNormalize;

			if (component == UriComponents.Fragment && UriHelper.HasPercentage (str))
				formatFlags |= UriHelper.FormatFlags.HasFragmentPercentage;

			UriSchemes scheme = GetScheme (schemeName);

			if (scheme == UriSchemes.Custom && (formatFlags & FormatFlags.HasHost) != 0)
				scheme = UriSchemes.CustomWithHost;

			var reduceAfter = UriSchemes.Http | UriSchemes.Https | UriSchemes.File | UriSchemes.NetPipe | UriSchemes.NetTcp;

			if (IriParsing) {
				reduceAfter |= UriSchemes.Ftp;
			} else if (component == UriComponents.Path) {
				if(scheme == UriSchemes.Ftp)
					str = Reduce (str.Replace ('\\', '/'), !IriParsing);
				if (scheme == UriSchemes.CustomWithHost)
					str = Reduce (str.Replace ('\\', '/'), false);
			}

			str = FormatString (str, scheme, uriKind, component, uriFormat, formatFlags);

			if (component == UriComponents.Path) {
				if (SchemeContains (scheme, reduceAfter))
					str = Reduce (str, !IriParsing);
				if(IriParsing && scheme == UriSchemes.CustomWithHost)
					str = Reduce (str, false);
			}

			return str;
		}

		private static string FormatString (string str, UriSchemes scheme, UriKind uriKind,
			UriComponents component, UriFormat uriFormat, FormatFlags formatFlags)
		{
			var s = new StringBuilder ();
			int len = str.Length;
			for (int i = 0; i < len; i++) {
				char c = str [i];
				if (c == '%') {
					int iStart = i;
					char surrogate;
					char x = Uri.HexUnescapeMultiByte (str, ref i, out surrogate);

					string cStr = str.Substring(iStart, i-iStart);
					s.Append (FormatChar (x, cStr, scheme, uriKind, component, uriFormat, formatFlags));
					if (surrogate != char.MinValue)
						s.Append (surrogate);

					i--;
				} else
					s.Append (FormatChar (c, "" + c, scheme, uriKind, component, uriFormat, formatFlags));
			}
			
			return s.ToString();
		}

		private static string FormatChar (char c, string cStr, UriSchemes scheme, UriKind uriKind,
			UriComponents component, UriFormat uriFormat, FormatFlags formatFlags)
		{
			var isEscaped = cStr.Length != 1;

			var userEscaped = (formatFlags & FormatFlags.UserEscaped) != 0;
			if (!isEscaped && !userEscaped && NeedToEscape (c, scheme, component, uriKind, uriFormat, formatFlags))
				return HexEscapeMultiByte (c);

			if (isEscaped && (userEscaped || !NeedToUnescape (c, scheme, component, uriKind, uriFormat, formatFlags))) {
				return cStr; //Keep original case
			}

			if (c == '\\' && component == UriComponents.Path) {
				if (!IriParsing && uriFormat != UriFormat.UriEscaped &&
					SchemeContains (scheme, UriSchemes.Http | UriSchemes.Https))
					return "/";

				if (SchemeContains (scheme, UriSchemes.Http | UriSchemes.Https | UriSchemes.Ftp | UriSchemes.CustomWithHost))
					return (isEscaped && uriFormat != UriFormat.UriEscaped) ? "\\" : "/";

				if (SchemeContains (scheme, UriSchemes.NetPipe | UriSchemes.NetTcp | UriSchemes.File))
					return "/";
			}

			return c.ToString (CultureInfo.InvariantCulture);
		}

		private static bool NeedToUnescape (char c, UriSchemes scheme, UriComponents component, UriKind uriKind,
			UriFormat uriFormat, FormatFlags formatFlags)
		{
			string cStr = c.ToString (CultureInfo.InvariantCulture);

			if (uriFormat == UriFormat.Unescaped)
				return true;

			UriSchemes sDecoders = UriSchemes.NetPipe | UriSchemes.NetTcp;

			if (!IriParsing)
				sDecoders |= UriSchemes.Http | UriSchemes.Https;

			if (c == '/' || c == '\\') {
				if (!IriParsing && uriKind == UriKind.Absolute && uriFormat != UriFormat.UriEscaped &&
					uriFormat != UriFormat.SafeUnescaped)
					return true;

				if (SchemeContains (scheme, UriSchemes.File)) {
					return component != UriComponents.Fragment &&
						   (component != UriComponents.Query || !IriParsing);
				}

				return component != UriComponents.Query && component != UriComponents.Fragment &&
					   SchemeContains (scheme, sDecoders);
			}

			if (c == '?') {
				//Avoid creating new query
				if (SupportsQuery (scheme) && component == UriComponents.Path)
					return false;

				if (!IriParsing && uriFormat == ToStringUnescape) {
					if (SupportsQuery (scheme))
						return component == UriComponents.Query || component == UriComponents.Fragment;

					return component == UriComponents.Fragment;
				}

				return false;
			}

			if (c == '#') {
				//Avoid creating new fragment
				if (component == UriComponents.Path || component == UriComponents.Query)
					return false;

				return false;
			}

			if (uriFormat == ToStringUnescape && !IriParsing) {
				if (uriKind == UriKind.Relative)
					return false;

				if ("$&+,;=@".Contains (cStr))
					return true;

				if (c < 0x20 || c == 0x7f)
					return true;
			}

			if (uriFormat == UriFormat.SafeUnescaped || uriFormat == ToStringUnescape) {
				if ("-._~".Contains (cStr))
					return true;

				if (" !\"'()*<>^`{}|".Contains (cStr))
					return uriKind != UriKind.Relative ||
						(IriParsing && (formatFlags & FormatFlags.HasUriCharactersToNormalize) != 0);

				if (":[]".Contains (cStr))
					return uriKind != UriKind.Relative;

				if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
					return true;

				if (c > 0x7f)
					return true;

				return false;
			}

			if (uriFormat == UriFormat.UriEscaped) {
				if (!IriParsing) {
					if (".".Contains (cStr)) {
						if (SchemeContains (scheme, UriSchemes.File))
							return component != UriComponents.Fragment;

						return component != UriComponents.Query && component != UriComponents.Fragment &&
							   SchemeContains (scheme, sDecoders);
					}

					return false;
				}

				if ("-._~".Contains (cStr))
					return true;
				
				if ((formatFlags & FormatFlags.HasUriCharactersToNormalize) != 0 &&
					"!'()*:[]".Contains (cStr))
					return true;

				if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
					return true;

				return false;
			}

			return false;
		}

		private static bool NeedToEscape (char c, UriSchemes scheme, UriComponents component, UriKind uriKind,
			UriFormat uriFormat, FormatFlags formatFlags)
		{
			string cStr = c.ToString (CultureInfo.InvariantCulture);

			if (c == '?') {
				if (uriFormat == UriFormat.Unescaped)
					return false;

				if (!SupportsQuery (scheme))
					return component != UriComponents.Fragment;

				//Avoid removing query
				if (component == UriComponents.Path)
					return false;

				return false;
			}

			if (c == '#') {
				//Avoid removing fragment
				if (component == UriComponents.Path || component == UriComponents.Query)
					return false;

				if (component == UriComponents.Fragment &&
					(uriFormat == ToStringUnescape || uriFormat == UriFormat.SafeUnescaped) &&
					(formatFlags & FormatFlags.HasFragmentPercentage) != 0)
					return true;

				if (IriParsing)
					return false;

				return uriFormat == UriFormat.UriEscaped ||
					(uriFormat != UriFormat.Unescaped && (formatFlags & FormatFlags.HasComponentCharactersToNormalize) != 0);
			}

			if (uriFormat == UriFormat.SafeUnescaped || uriFormat == ToStringUnescape) {
				if ("%".Contains (cStr))
					return uriKind != UriKind.Relative;
			}

			if (uriFormat == UriFormat.SafeUnescaped) {
				if (c < 0x20 || c == 0x7F)
					return true;
			}

			if (uriFormat == UriFormat.UriEscaped) {
				if (c < 0x20 || c >= 0x7F)
					return true;

				if (" \"%<>^`{}|".Contains (cStr))
					return true;

				if ("[]".Contains (cStr))
					return !IriParsing;

				if (c == '\\') {
					return component != UriComponents.Path ||
						   SchemeContains (scheme,
							   UriSchemes.Gopher | UriSchemes.Ldap | UriSchemes.Mailto | UriSchemes.Nntp |
							   UriSchemes.Telnet | UriSchemes.News | UriSchemes.Custom);
				}
			}

			return false;
		}

		// This is called "compacting" in the MSDN documentation
		private static string Reduce (string path, bool trimDots)
		{
			// quick out, allocation-free, for a common case
			if (path == "/")
				return path;

			List<string> result = new List<string> ();

			bool begin = true;
			for (int startpos = 0; startpos < path.Length; ) {
				int endpos = path.IndexOf ('/', startpos);
				if (endpos == -1)
					endpos = path.Length;
				string current = path.Substring (startpos, endpos-startpos);
				startpos = endpos + 1;
				if (begin && current.Length == 0) {
					begin = false;
					continue;
				}

				begin = false;
				if (current == "..") {
					int resultCount = result.Count;
					// in 2.0 profile, skip leading ".." parts
					if (resultCount == 0) {
						continue;
					}

					result.RemoveAt (resultCount - 1);
					continue;
				}

				if (current == "." ||
					(trimDots && current.EndsWith("."))) {
					current = current.TrimEnd('.');
					if (current == "" && endpos < path.Length)
						continue;
				}

				result.Add (current);
			}

			if (result.Count == 0)
				return "/";

			StringBuilder res = new StringBuilder ();

			if (path [0] == '/')
				res.Append ('/');

			bool first = true;
			foreach (string part in result) {
				if (first) {
					first = false;
				} else {
					res.Append ('/');
				}
				res.Append(part);
			}

			if (path [path.Length - 1] == '/')
				res.Append ('/');
				
			return res.ToString();
		}
	}
}
