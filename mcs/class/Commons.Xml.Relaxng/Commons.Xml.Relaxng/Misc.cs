//
// Commons.Xml.Relaxng.General.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// 2003 Atsushi Enomoto "No rights reserved."
//
using System;
using System.Collections;
using System.IO;
using System.Xml;
using Commons.Xml.Relaxng.Derivative;

namespace Commons.Xml.Relaxng
{
	public class Util
	{
		public static string ResolveUri (string baseUri, string href)
		{
#if true
			Uri uri = null;
			if (baseUri != null && baseUri.Length > 0)
				uri = new Uri (baseUri);
			Uri result = new XmlUrlResolver ().ResolveUri (uri, href);
			if (result.Query.Length > 0 || result.Fragment.Length > 0)
				throw new RelaxngException ("Invalid URI format: " + href);
			return result.ToString ();
#else
			// If baseUri does not exist, then it is only the way.
			if (baseUri == String.Empty)
				return href;

			// If href itself is a uri, then return it directly.
			try {
				return new Uri (href).ToString ();
			} catch (UriFormatException) {
			}

			// If baseUri is a valid uri, then make relative uri.
			try {
				return new Uri (
					new Uri (baseUri), href).ToString ();
			} catch (UriFormatException) {
			}

			// Otherwise, they might be filesystem path.
			if (Path.IsPathRooted (href))
				return href;

			return Path.Combine (
				Path.GetDirectoryName (baseUri), href);
#endif
		}

		public static string NormalizeWhitespace (string s)
		{
			if (s.Length == 0)
				return s;

			char [] ca = s.ToCharArray ();
			int j = 0;
			for (int i = 0; i < ca.Length; i++) {
				switch (ca [i]) {
				case ' ':
				case '\r':
				case '\t':
				case '\n':
					if (j == 0)
						break;
					if (ca [j - 1] != ' ')
						ca [j++] = ' ';
					break;
				default:
					ca [j++] = ca [i];
					break;
				}
			}
			if (j == 0)
				return String.Empty;
			string r = new string (ca, 0, (ca [j - 1] != ' ') ? j : j - 1);
			return r;
		}

		public static bool IsWhitespace (string s)
		{
			for (int i = 0; i < s.Length; i++) {
				switch (s [i]) {
				case ' ':
				case '\t':
				case '\n':
				case '\r':
					continue;
				default:
					return false;
				}
			}
			return true;
		}
	}
}

