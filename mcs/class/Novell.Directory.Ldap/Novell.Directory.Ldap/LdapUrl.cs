/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
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
//
// Novell.Directory.Ldap.LdapUrl.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using ArrayEnumeration = Novell.Directory.Ldap.Utilclass.ArrayEnumeration;

namespace Novell.Directory.Ldap
{
	
	/// <summary> 
	/// Encapsulates parameters of an Ldap URL query as defined in RFC2255.
	/// 
	/// An LdapUrl object can be passed to LdapConnection.search to retrieve
	/// search results.
	/// 
	/// </summary>
	/// <seealso cref="LdapConnection.Search">
	/// </seealso>
	public class LdapUrl : System.ICloneable
	{
		private void  InitBlock()
		{
			scope = DEFAULT_SCOPE;
		}
		/// <summary> Returns an array of attribute names specified in the URL.
		/// 
		/// </summary>
		/// <returns> An array of attribute names in the URL.
		/// </returns>
		virtual public System.String[] AttributeArray
		{
			get
			{
				return attrs;
			}
			
		}
		/// <summary> Returns an enumerator for the attribute names specified in the URL.
		/// 
		/// </summary>
		/// <returns> An enumeration of attribute names.
		/// </returns>
		virtual public System.Collections.IEnumerator Attributes
		{
			get
			{
				return new ArrayEnumeration(attrs);
			}
			
		}
		/// <summary> Returns any Ldap URL extensions specified, or null if none are
		/// specified. Each extension is a type=value expression. The =value part
		/// MAY be omitted. The expression MAY be prefixed with '!' if it is
		/// mandatory for evaluation of the URL.
		/// 
		/// </summary>
		/// <returns> string array of extensions.
		/// </returns>
		virtual public System.String[] Extensions
		{
			get
			{
				return extensions;
			}
			
		}
		/// <summary> Returns the search filter or <code>null</code> if none was specified.
		/// 
		/// </summary>
		/// <returns> The search filter.
		/// </returns>
		virtual public System.String Filter
		{
			get
			{
				return filter;
			}
			
		}
		/// <summary> Returns the name of the Ldap server in the URL.
		/// 
		/// </summary>
		/// <returns> The host name specified in the URL.
		/// </returns>
		virtual public System.String Host
		{
			get
			{
				return host;
			}
			
		}
		/// <summary> Returns the port number of the Ldap server in the URL.
		/// 
		/// </summary>
		/// <returns> The port number in the URL.
		/// </returns>
		virtual public int Port
		{
			get
			{
				if (port == 0)
				{
					return LdapConnection.DEFAULT_PORT;
				}
				return port;
			}
			
		}
		/// <summary> Returns the depth of search. It returns one of the following from
		/// LdapConnection: SCOPE_BASE, SCOPE_ONE, or SCOPE_SUB.
		/// 
		/// </summary>
		/// <returns> The search scope.
		/// </returns>
		virtual public int Scope
		{
			get
			{
				return scope;
			}
			
		}
		/// <summary> Returns true if the URL is of the type ldaps (Ldap over SSL, a predecessor
		/// to startTls)
		/// 
		/// </summary>
		/// <returns> whether this is a secure Ldap url or not.
		/// </returns>
		virtual public bool Secure
		{
			get
			{
				return secure;
			}
			
		}
		private static readonly int DEFAULT_SCOPE = LdapConnection.SCOPE_BASE;
		
		// Broken out parts of the URL
		private bool secure = false; // URL scheme ldap/ldaps
		private bool ipV6 = false; // TCP/IP V6
		private System.String host = null; // Host
		private int port = 0; // Port
		private System.String dn = null; // Base DN
		private System.String[] attrs = null; // Attributes
		private System.String filter = null; // Filter
		private int scope; // Scope
		private System.String[] extensions = null; // Extensions
		
		/// <summary> Constructs a URL object with the specified string as the URL.
		/// 
		/// </summary>
		/// <param name="url">     An Ldap URL string, e.g.
		/// "ldap://ldap.example.com:80/dc=example,dc=com?cn,
		/// sn?sub?(objectclass=inetOrgPerson)".
		/// 
		/// </param>
		/// <exception> MalformedURLException The specified URL cannot be parsed.
		/// </exception>
		public LdapUrl(System.String url)
		{
			InitBlock();
			parseURL(url);
			return ;
		}
		
		
		/// <summary> Constructs a URL object with the specified host, port, and DN.
		/// 
		/// This form is used to create URL references to a particular object
		/// in the directory.
		/// 
		/// </summary>
		/// <param name="host">    Host identifier of Ldap server, or null for
		/// "localhost".
		/// 
		/// </param>
		/// <param name="port">    The port number for Ldap server (use
		/// LdapConnection.DEFAULT_PORT for default port).
		/// 
		/// </param>
		/// <param name="dn">      Distinguished name of the base object of the search.
		/// 
		/// </param>
		public LdapUrl(System.String host, int port, System.String dn)
		{
			InitBlock();
			this.host = host;
			this.port = port;
			this.dn = dn;
			return ;
		}
		
		/// <summary> Constructs an Ldap URL with all fields explicitly assigned, to
		/// specify an Ldap search operation.
		/// 
		/// </summary>
		/// <param name="host">    Host identifier of Ldap server, or null for
		/// "localhost".
		/// 
		/// </param>
		/// <param name="port">    The port number for Ldap server (use
		/// LdapConnection.DEFAULT_PORT for default port).
		/// 
		/// </param>
		/// <param name="dn">      Distinguished name of the base object of the search.
		/// 
		/// 
		/// </param>
		/// <param name="attrNames">Names or OIDs of attributes to retrieve.  Passing a
		/// null array signifies that all user attributes are to be
		/// retrieved. Passing a value of "*" allows you to specify
		/// that all user attributes as well as any specified
		/// operational attributes are to be retrieved.
		/// 
		/// 
		/// </param>
		/// <param name="scope">   Depth of search (in DN namespace). Use one of
		/// SCOPE_BASE, SCOPE_ONE, SCOPE_SUB from LdapConnection.
		/// 
		/// 
		/// </param>
		/// <param name="filter">  The search filter specifying the search criteria.
		/// 
		/// 
		/// </param>
		/// <param name="extensions"> Extensions provide a mechanism to extend the
		/// functionality of Ldap URLs. Currently no
		/// Ldap URL extensions are defined. Each extension
		/// specification is a type=value expression, and  may
		/// be <code>null</code> or empty.  The =value part may be
		/// omitted. The expression may be prefixed with '!' if it
		/// is mandatory for the evaluation of the URL.
		/// </param>
		public LdapUrl(System.String host, int port, System.String dn, System.String[] attrNames, int scope, System.String filter, System.String[] extensions)
		{
			InitBlock();
			this.host = host;
			this.port = port;
			this.dn = dn;
			this.attrs = new System.String[attrNames.Length];
			attrNames.CopyTo(this.attrs, 0);
			this.scope = scope;
			this.filter = filter;
			this.extensions = new System.String[extensions.Length];
			extensions.CopyTo(this.extensions, 0);
			return ;
		}
		
		/// <summary> Constructs an Ldap URL with all fields explicitly assigned, including
		/// isSecure, to specify an Ldap search operation.
		/// 
		/// </summary>
		/// <param name="host">    Host identifier of Ldap server, or null for
		/// "localhost".
		/// 
		/// 
		/// </param>
		/// <param name="port">    The port number for Ldap server (use
		/// LdapConnection.DEFAULT_PORT for default port).
		/// 
		/// 
		/// </param>
		/// <param name="dn">      Distinguished name of the base object of the search.
		/// 
		/// 
		/// </param>
		/// <param name="attrNames">Names or OIDs of attributes to retrieve.  Passing a
		/// null array signifies that all user attributes are to be
		/// retrieved. Passing a value of "*" allows you to specify
		/// that all user attributes as well as any specified
		/// operational attributes are to be retrieved.
		/// 
		/// 
		/// </param>
		/// <param name="scope">   Depth of search (in DN namespace). Use one of
		/// SCOPE_BASE, SCOPE_ONE, SCOPE_SUB from LdapConnection.
		/// 
		/// 
		/// </param>
		/// <param name="filter">  The search filter specifying the search criteria.
		/// from LdapConnection: SCOPE_BASE, SCOPE_ONE, SCOPE_SUB.
		/// 
		/// 
		/// </param>
		/// <param name="extensions"> Extensions provide a mechanism to extend the
		/// functionality of Ldap URLs. Currently no
		/// Ldap URL extensions are defined. Each extension
		/// specification is a type=value expression, and  may
		/// be <code>null</code> or empty.  The =value part may be
		/// omitted. The expression may be prefixed with '!' if it
		/// is mandatory for the evaluation of the URL.
		/// 
		/// 
		/// </param>
		/// <param name="secure">  If true creates an Ldap URL of the ldaps type
		/// </param>
		public LdapUrl(System.String host, int port, System.String dn, System.String[] attrNames, int scope, System.String filter, System.String[] extensions, bool secure)
		{
			InitBlock();
			this.host = host;
			this.port = port;
			this.dn = dn;
			this.attrs = attrNames;
			this.scope = scope;
			this.filter = filter;
			this.extensions = new System.String[extensions.Length];
			extensions.CopyTo(this.extensions, 0);
			this.secure = secure;
			return ;
		}
		
		/// <summary> Returns a clone of this URL object.
		/// 
		/// </summary>
		/// <returns> clone of this URL object.
		/// </returns>
		public System.Object Clone()
		{
			try
			{
				return base.MemberwiseClone();
			}
			catch (System.Exception ce)
			{
				throw new System.SystemException("Internal error, cannot create clone");
			}
		}
		
		/// <summary> Decodes a URL-encoded string.
		/// 
		/// Any occurences of %HH are decoded to the hex value represented.
		/// However, this method does NOT decode "+" into " ".
		/// 
		/// </summary>
		/// <param name="URLEncoded">    String to decode.
		/// 
		/// </param>
		/// <returns> The decoded string.
		/// 
		/// </returns>
		/// <exception> MalformedURLException The URL could not be parsed.
		/// </exception>
		public static System.String decode(System.String URLEncoded)
		{
			
			
			int searchStart = 0;
			int fieldStart;
			
			fieldStart = URLEncoded.IndexOf("%", searchStart);
			// Return now if no encoded data
			if (fieldStart < 0)
			{
				return URLEncoded;
			}
			
			// Decode the %HH value and copy to new string buffer
			int fieldEnd = 0; // end of previous field
			int dataLen = URLEncoded.Length;
			
			System.Text.StringBuilder decoded = new System.Text.StringBuilder(dataLen);
			
			while (true)
			{
				if (fieldStart > (dataLen - 3))
				{
					throw new System.UriFormatException("LdapUrl.decode: must be two hex characters following escape character '%'");
				}
				if (fieldStart < 0)
					fieldStart = dataLen;
				// Copy to string buffer from end of last field to start of next
				decoded.Append(URLEncoded.Substring(fieldEnd, (fieldStart) - (fieldEnd)));
				fieldStart += 1;
				if (fieldStart >= dataLen)
					break;
				fieldEnd = fieldStart + 2;
				try
				{
					decoded.Append((char) System.Convert.ToInt32(URLEncoded.Substring(fieldStart, (fieldEnd) - (fieldStart)), 16));
				}
				catch (System.FormatException ex)
				{
					throw new System.UriFormatException("LdapUrl.decode: error converting hex characters to integer \"" + ex.Message + "\"");
				}
				searchStart = fieldEnd;
				if (searchStart == dataLen)
					break;
				fieldStart = URLEncoded.IndexOf("%", searchStart);
			}
			
			return (decoded.ToString());
		}
		
		/// <summary> Encodes an arbitrary string using the URL encoding rules.
		/// 
		///  Any illegal characters are encoded as %HH. 
		/// 
		/// </summary>
		/// <param name="toEncode">    The string to encode.
		/// 
		/// </param>
		/// <returns> The URL-encoded string.
		/// 
		/// Comment: An illegal character consists of any non graphical US-ASCII character, Unsafe, or reserved characters.
		/// </returns>
		public static System.String encode(System.String toEncode)
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder(toEncode.Length); //empty but initial capicity of 'length'
			System.String temp;
			char currChar;
			for (int i = 0; i < toEncode.Length; i++)
			{
				currChar = toEncode[i];
				if ((((int) currChar <= 0x1F) || ((int) currChar == 0x7F) || (((int) currChar >= 0x80) && ((int) currChar <= 0xFF))) || ((currChar == '<') || (currChar == '>') || (currChar == '\"') || (currChar == '#') || (currChar == '%') || (currChar == '{') || (currChar == '}') || (currChar == '|') || (currChar == '\\') || (currChar == '^') || (currChar == '~') || (currChar == '[') || (currChar == '\'')) || ((currChar == ';') || (currChar == '/') || (currChar == '?') || (currChar == ':') || (currChar == '@') || (currChar == '=') || (currChar == '&')))
				{
					temp = System.Convert.ToString(currChar, 16);
					if (temp.Length == 1)
						buffer.Append("%0" + temp);
					//if(temp.length()==2) this can only be two or one digit long.
					else
						buffer.Append("%" + System.Convert.ToString(currChar, 16));
				}
				else
					buffer.Append(currChar);
			}
			return buffer.ToString();
		}
		
		/// <summary> Returns the base distinguished name encapsulated in the URL.
		/// 
		/// </summary>
		/// <returns> The base distinguished name specified in the URL, or null if none.
		/// </returns>
		public virtual System.String getDN()
		{
			return dn;
		}
		
		/// <summary> Sets the base distinguished name encapsulated in the URL.</summary>
		/* package */
		internal virtual void  setDN(System.String dn)
		{
			this.dn = dn;
			return ;
		}
		
		/// <summary> Returns a valid string representation of this Ldap URL.
		/// 
		/// </summary>
		/// <returns> The string representation of the Ldap URL.
		/// </returns>
		public override System.String ToString()
		{
			System.Text.StringBuilder url = new System.Text.StringBuilder(256);
			// Scheme
			if (secure)
			{
				url.Append("ldaps://");
			}
			else
			{
				url.Append("ldap://");
			}
			// Host:port/dn
			if (ipV6)
			{
				url.Append("[" + host + "]");
			}
			else
			{
				url.Append(host);
			}
			
			// Port not specified
			if (port != 0)
			{
				url.Append(":" + port);
			}
			
			if (((System.Object) dn == null) && (attrs == null) && (scope == DEFAULT_SCOPE) && ((System.Object) filter == null) && (extensions == null))
			{
				return url.ToString();
			}
			
			url.Append("/");
			
			if ((System.Object) dn != null)
			{
				url.Append(dn);
			}
			
			if ((attrs == null) && (scope == DEFAULT_SCOPE) && ((System.Object) filter == null) && (extensions == null))
			{
				return url.ToString();
			}
			
			// attributes
			url.Append("?");
			if (attrs != null)
			{
				//should we check also for attrs != "*"
				for (int i = 0; i < attrs.Length; i++)
				{
					url.Append(attrs[i]);
					if (i < (attrs.Length - 1))
					{
						url.Append(",");
					}
				}
			}
			
			if ((scope == DEFAULT_SCOPE) && ((System.Object) filter == null) && (extensions == null))
			{
				return url.ToString();
			}
			
			// scope
			url.Append("?");
			if (scope != DEFAULT_SCOPE)
			{
				if (scope == LdapConnection.SCOPE_ONE)
				{
					url.Append("one");
				}
				else
				{
					url.Append("sub");
				}
			}
			
			if (((System.Object) filter == null) && (extensions == null))
			{
				return url.ToString();
			}
			
			// filter
			if ((System.Object) filter == null)
			{
				url.Append("?");
			}
			else
			{
				url.Append("?" + Filter);
			}
			
			if (extensions == null)
			{
				return url.ToString();
			}
			
			// extensions
			url.Append("?");
			if (extensions != null)
			{
				for (int i = 0; i < extensions.Length; i++)
				{
					url.Append(extensions[i]);
					if (i < (extensions.Length - 1))
					{
						url.Append(",");
					}
				}
			}
			return url.ToString();
		}
		
		private System.String[] parseList(System.String listStr, char delimiter, int listStart, int listEnd)
		// end of list + 1
		{
			System.String[] list;
			// Check for and empty string
			if ((listEnd - listStart) < 1)
			{
				return null;
			}
			// First count how many items are specified
			int itemStart = listStart;
			int itemEnd;
			int itemCount = 0;
			while (itemStart > 0)
			{
				// itemStart == 0 if no delimiter found
				itemCount += 1;
				itemEnd = listStr.IndexOf((System.Char) delimiter, itemStart);
				if ((itemEnd > 0) && (itemEnd < listEnd))
				{
					itemStart = itemEnd + 1;
				}
				else
				{
					break;
				}
			}
			// Now fill in the array with the attributes
			itemStart = listStart;
			list = new System.String[itemCount];
			itemCount = 0;
			while (itemStart > 0)
			{
				itemEnd = listStr.IndexOf((System.Char) delimiter, itemStart);
				if (itemStart <= listEnd)
				{
					if (itemEnd < 0)
						itemEnd = listEnd;
					if (itemEnd > listEnd)
						itemEnd = listEnd;
					list[itemCount] = listStr.Substring(itemStart, (itemEnd) - (itemStart));
					itemStart = itemEnd + 1;
					itemCount += 1;
				}
				else
				{
					break;
				}
			}
			return list;
		}
		
		
		private void  parseURL(System.String url)
		{
			int scanStart = 0;
			int scanEnd = url.Length;
			
			if ((System.Object) url == null)
				throw new System.UriFormatException("LdapUrl: URL cannot be null");
			
			// Check if URL is enclosed by < & >
			if (url[scanStart] == '<')
			{
				if (url[scanEnd - 1] != '>')
					throw new System.UriFormatException("LdapUrl: URL bad enclosure");
				scanStart += 1;
				scanEnd -= 1;
			}
			
			// Determine the URL scheme and set appropriate default port
			if (url.Substring(scanStart, (scanStart + 4) - (scanStart)).ToUpper().Equals("URL:".ToUpper()))
			{
				scanStart += 4;
			}
			if (url.Substring(scanStart, (scanStart + 7) - (scanStart)).ToUpper().Equals("ldap://".ToUpper()))
			{
				scanStart += 7;
				port = LdapConnection.DEFAULT_PORT;
			}
			else if (url.Substring(scanStart, (scanStart + 8) - (scanStart)).ToUpper().Equals("ldaps://".ToUpper()))
			{
				secure = true;
				scanStart += 8;
				port = LdapConnection.DEFAULT_SSL_PORT;
			}
			else
			{
				throw new System.UriFormatException("LdapUrl: URL scheme is not ldap");
			}
			
			// Find where host:port ends and dn begins
			int dnStart = url.IndexOf("/", scanStart);
			int hostPortEnd = scanEnd;
			bool novell = false;
			if (dnStart < 0)
			{
				/*
				* Kludge. check for ldap://111.222.333.444:389??cn=abc,o=company
				*
				* Check for broken Novell referral format.  The dn is in
				* the scope position, but the required slash is missing.
				* This is illegal syntax but we need to account for it.
				* Fortunately it can't be confused with anything real.
				*/
				dnStart = url.IndexOf("?", scanStart);
				if (dnStart > 0)
				{
					if (url[dnStart + 1] == '?')
					{
						hostPortEnd = dnStart;
						dnStart += 1;
						novell = true;
					}
					else
					{
						dnStart = - 1;
					}
				}
			}
			else
			{
				hostPortEnd = dnStart;
			}
			// Check for IPV6 "[ipaddress]:port"
			int portStart;
			int hostEnd = hostPortEnd;
			if (url[scanStart] == '[')
			{
				hostEnd = url.IndexOf((System.Char) ']', scanStart + 1);
				if ((hostEnd >= hostPortEnd) || (hostEnd == - 1))
				{
					throw new System.UriFormatException("LdapUrl: \"]\" is missing on IPV6 host name");
				}
				// Get host w/o the [ & ]
				host = url.Substring(scanStart + 1, (hostEnd) - (scanStart + 1));
				portStart = url.IndexOf(":", hostEnd);
				if ((portStart < hostPortEnd) && (portStart != - 1))
				{
					// port is specified
					port = System.Int32.Parse(url.Substring(portStart + 1, (hostPortEnd) - (portStart + 1)));
				}
				else
				{
				}
			}
			else
			{
				portStart = url.IndexOf(":", scanStart);
				// Isolate the host and port
				if ((portStart < 0) || (portStart > hostPortEnd))
				{
					// no port is specified, we keep the default
					host = url.Substring(scanStart, (hostPortEnd) - (scanStart));
				}
				else
				{
					// port specified in URL
					host = url.Substring(scanStart, (portStart) - (scanStart));
					port = System.Int32.Parse(url.Substring(portStart + 1, (hostPortEnd) - (portStart + 1)));
				}
			}
			
			scanStart = hostPortEnd + 1;
			if ((scanStart >= scanEnd) || (dnStart < 0))
				return ;
			
			// Parse out the base dn
			scanStart = dnStart + 1;
			
			int attrsStart = url.IndexOf((System.Char) '?', scanStart);
			if (attrsStart < 0)
			{
				dn = url.Substring(scanStart, (scanEnd) - (scanStart));
			}
			else
			{
				dn = url.Substring(scanStart, (attrsStart) - (scanStart));
			}
			
			scanStart = attrsStart + 1;
			// Wierd novell syntax can have nothing beyond the dn
			if ((scanStart >= scanEnd) || (attrsStart < 0) || novell)
				return ;
			
			// Parse out the attributes
			int scopeStart = url.IndexOf((System.Char) '?', scanStart);
			if (scopeStart < 0)
				scopeStart = scanEnd - 1;
			attrs = parseList(url, ',', attrsStart + 1, scopeStart);
			
			scanStart = scopeStart + 1;
			if (scanStart >= scanEnd)
				return ;
			
			// Parse out the scope
			int filterStart = url.IndexOf((System.Char) '?', scanStart);
			System.String scopeStr;
			if (filterStart < 0)
			{
				scopeStr = url.Substring(scanStart, (scanEnd) - (scanStart));
			}
			else
			{
				scopeStr = url.Substring(scanStart, (filterStart) - (scanStart));
			}
			if (scopeStr.ToUpper().Equals("".ToUpper()))
			{
				scope = LdapConnection.SCOPE_BASE;
			}
			else if (scopeStr.ToUpper().Equals("base".ToUpper()))
			{
				scope = LdapConnection.SCOPE_BASE;
			}
			else if (scopeStr.ToUpper().Equals("one".ToUpper()))
			{
				scope = LdapConnection.SCOPE_ONE;
			}
			else if (scopeStr.ToUpper().Equals("sub".ToUpper()))
			{
				scope = LdapConnection.SCOPE_SUB;
			}
			else
			{
				throw new System.UriFormatException("LdapUrl: URL invalid scope");
			}
			
			
			scanStart = filterStart + 1;
			if ((scanStart >= scanEnd) || (filterStart < 0))
				return ;
			
			// Parse out the filter
			scanStart = filterStart + 1;
			
			System.String filterStr;
			int extStart = url.IndexOf((System.Char) '?', scanStart);
			if (extStart < 0)
			{
				filterStr = url.Substring(scanStart, (scanEnd) - (scanStart));
			}
			else
			{
				filterStr = url.Substring(scanStart, (extStart) - (scanStart));
			}
			
			if (!filterStr.Equals(""))
			{
				filter = filterStr; // Only modify if not the default filter
			}
			
			
			scanStart = extStart + 1;
			if ((scanStart >= scanEnd) || (extStart < 0))
				return ;
			
			// Parse out the extensions
			int end = url.IndexOf((System.Char) '?', scanStart);
			if (end > 0)
				throw new System.UriFormatException("LdapUrl: URL has too many ? fields");
			extensions = parseList(url, ',', scanStart, scanEnd);
			
			return ;
		}
	}
}
