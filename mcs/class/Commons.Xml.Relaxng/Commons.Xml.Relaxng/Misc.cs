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
	public class RngException : Exception
	{
		string debugXml;

		public RngException () : base () {}
		public RngException (string message) : base (message) {}
		public RngException (string message, Exception innerException)
			: base (message, innerException) {}
		public RngException (string message, RdpPattern invalidatedPattern)
			: base (message)
		{
			debugXml = RdpUtil.DebugRdpPattern (invalidatedPattern, new Hashtable ());
		}
	}

	public class Util
	{
		public static string ResolveUri (string baseUri, string href)
		{
			lock (baseUri) {
				lock (href) {
					return resolveUri (baseUri, href);
				}
			}
		}

		private static string resolveUri (string baseUri, string href)
		{
			// If baseUri does not exist, then it is only the way.
			if (baseUri == String.Empty)
				return href;

			// If href itself is a uri, then return it directly.
			try {
				return new Uri (href).AbsoluteUri;
			} catch (UriFormatException) {
			}

			// If baseUri is a valid uri, then make relative uri.
			try {
				return new Uri (
					new Uri (baseUri), href).AbsoluteUri;
			} catch (UriFormatException) {
			}

			// Otherwise, they might be filesystem path.
			if (Path.IsPathRooted (href))
				return href;

			return Path.Combine (
				Path.GetDirectoryName (baseUri), href);
		}
	}

}

