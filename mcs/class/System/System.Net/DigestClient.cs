//
// System.Net.DigestClient.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Novell, Inc (http://www.ximian.com)
//

using System.IO;
using System.Security.Cryptography;
using System.Text;
namespace System.Net
{
	//
	// This works with apache mod_digest
	//TODO:
	//	MD5-sess
	//	qop
	//	cnonce et al.
	//	See RFC 2617 for details.
	//

	class DigestHeaderParser
	{
		string header;
		int length;
		int pos;
		string realm, opaque, nonce, algorithm;
		static string [] keywords = { "realm", "opaque", "nonce", "algorithm" };
		string [] values = new string [keywords.Length];

		public DigestHeaderParser (string header)
		{
			this.header = header.Trim ();
		}

		public string Realm {
			get { return values [0]; }
		}

		public string Opaque {
			get { return values [1]; }
		}

		public string Nonce {
			get { return values [2]; }
		}
		
		public string Algorithm {
			get { return values [3]; }
		}
		
		public bool Parse ()
		{
			if (!header.ToLower ().StartsWith ("digest "))
				return false;

			pos = 6;
			length = this.header.Length;
			while (pos < length) {
				string key, value;
				if (!GetKeywordAndValue (out key, out value))
					return false;

				SkipWhitespace ();
				if (pos < length && header [pos] == ',')
					pos++;

				int idx = Array.IndexOf (keywords, (key));
				if (idx == -1)
					continue;

				if (values [idx] != null)
					return false;

				values [idx] = value;
			}

			if (Realm == null || Nonce == null)
				return false;

			return true;
		}

		void SkipWhitespace ()
		{
			char c = ' ';
			while (pos < length && (c == ' ' || c == '\t' || c == '\r' || c == '\n')) {
				c = header [pos++];
			}
			pos--;
		}
		
		void SkipNonWhitespace ()
		{
			char c = 'a';
			while (pos < length && c != ' ' && c != '\t' && c != '\r' && c != '\n') {
				c = header [pos++];
			}
			pos--;
		}
		
		string GetKey ()
		{
			SkipWhitespace ();
			int begin = pos;
			while (pos < length && header [pos] != '=') {
				pos++;
			}
			
			string key = header.Substring (begin, pos - begin).Trim ().ToLower ();
			return key;
		}

		bool GetKeywordAndValue (out string key, out string value)
		{
			key = null;
			value = null;
			key = GetKey ();
			if (pos >= length)
				return false;

			SkipWhitespace ();
			if (pos + 1 >= length || header [pos++] != '=')
				return false;

			SkipWhitespace ();
			if (pos + 1 >= length || header [pos++] != '"')
				return false;

			int beginQ = pos;
			pos = header.IndexOf ('"', pos);
			if (pos == -1)
				return false;

			value = header.Substring (beginQ, pos - beginQ);
			pos += 2;
			return true;
		}
	}
	
	class DigestClient : IAuthenticationModule
	{
		public DigestClient ()
		{
		}

		static string GetHexString (byte [] bytes)
		{
			StringBuilder result = new StringBuilder (bytes.Length * 2);
			foreach (byte b in bytes)
				result.AppendFormat ("{0:x2}", (int) b);

			return result.ToString ();
		}

		public Authorization Authenticate (string challenge, WebRequest webRequest, ICredentials credentials)
		{
			if (credentials == null || challenge == null)
				return null;

			HttpWebRequest request = webRequest as HttpWebRequest;
			if (request == null)
				return null;

			NetworkCredential cred = credentials.GetCredential (request.AuthUri, "digest");
			string userName = cred.UserName;
			if (userName == null || userName == "")
				return null;

			DigestHeaderParser parser = new DigestHeaderParser (challenge);
			if (!parser.Parse ())
				return null;

			Encoding enc = Encoding.Default;
			MD5 md5 = new MD5CryptoServiceProvider ();
			string password = cred.Password;

			// A1: user ":" realm ":" password
			string a1str = String.Format ("{0}:{1}:{2}", userName, parser.Realm, password);
			byte [] a1 = md5.ComputeHash (enc.GetBytes (a1str));
			a1str = GetHexString (a1);
			// A2: method ":" path
			string a2str = String.Format ("{0}:{1}", request.Method, request.Address.PathAndQuery);
			byte [] a2 = md5.ComputeHash (enc.GetBytes (a2str));
			a2str = GetHexString (a2);
			
			// Response: a1 ":" nonce ":" a2
			string respString = String.Format ("{0}:{1}:{2}", a1str, parser.Nonce, a2str);
			byte [] respBytes = md5.ComputeHash (enc.GetBytes (respString));
			respString = GetHexString (respBytes);

			StringBuilder response = new StringBuilder ();
			response.AppendFormat ("Digest username=\"{0}\", ", userName);
			response.AppendFormat ("realm=\"{0}\", ", parser.Realm);
			response.AppendFormat ("nonce=\"{0}\", ", parser.Nonce);
			response.AppendFormat ("uri=\"{0}\", ", request.Address.PathAndQuery);
			response.AppendFormat ("response=\"{0}\"", respString);
			if (parser.Opaque != null)
				response.AppendFormat (", opaque=\"{0}\"", parser.Opaque);

			if (parser.Algorithm != null)
				response.AppendFormat (", algorithm=\"{0}\"", parser.Algorithm);

			return new Authorization (response.ToString ());
		}

		[MonoTODO]
		public Authorization PreAuthenticate (WebRequest webRequest, ICredentials credentials)
		{
			throw new NotImplementedException ();
		}

		public string AuthenticationType {
			get { return "Digest"; }
		}

		public bool CanPreAuthenticate {
			get { return true; }
		}
	}
}

