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
// Novell.Directory.Ldap.LdapDN.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using  Novell.Directory.Ldap.Utilclass;

namespace Novell.Directory.Ldap
{
	
	/// <summary>  A utility class to facilitate composition and deomposition
	/// of distinguished names DNs.
	/// 
	/// Specifies methods for manipulating a distinguished name DN
	/// and a relative distinguished name RDN.
	/// </summary>
	public class LdapDN
	{
		/// <summary> Compares the two strings per the distinguishedNameMatch equality matching
		/// (using case-ignore matching).  IllegalArgumentException is thrown if one
		/// or both DNs are invalid.  UnsupportedOpersationException is thrown if the
		/// API implementation is not able to detemine if the DNs match or not.
		/// 
		/// </summary>
		/// <param name="dn1">           String form of the first DN to compare.
		/// 
		/// </param>
		/// <param name="dn2">           String form of the second DN to compare.
		/// 
		/// </param>
		/// <returns> Returns true if the two strings correspond to the same DN; false
		/// if the DNs are different.
		/// </returns>
		[CLSCompliantAttribute(false)]		
		public static bool equals(System.String dn1, System.String dn2)
		{
			DN dnA = new DN(dn1);
			DN dnB = new DN(dn2);
			return dnA.Equals(dnB);
		}
		
		/// <summary> Returns the RDN after escaping the characters requiring escaping.
		/// 
		/// For example, for the rdn "cn=Acme, Inc", the escapeRDN method
		/// returns "cn=Acme\, Inc".
		/// 
		/// escapeRDN escapes the AttributeValue by inserting '\' before the
		/// following chars: * ',' '+' '"' '\' 'LESSTHAN' 'GREATERTHAN' ';' 
		/// '#' if it comes at the beginning of the string, and 
		/// ' ' (space) if it comes at the beginning or the end of a string.
		/// Note that single-valued attributes can be used because of ambiguity. See
		/// RFC 2253 
		/// 
		/// </summary>
		/// <param name="rdn">           The RDN to escape.
		/// 
		/// </param>
		/// <returns> The RDN with escaping characters.
		/// </returns>
		public static System.String escapeRDN(System.String rdn)
		{
			System.Text.StringBuilder escapedS = new System.Text.StringBuilder(rdn);
			int i = 0;
			
			while (i < escapedS.Length && escapedS[i] != '=')
			{
				i++; //advance until we find the separator =
			}
			if (i == escapedS.Length)
			{
				throw new System.ArgumentException("Could not parse RDN: Attribute " + "type and name must be separated by an equal symbol, '='");
			}
			
			i++;
			//check for a space or # at the beginning of a string.
			if ((escapedS[i] == ' ') || (escapedS[i] == '#'))
			{
				escapedS.Insert(i++, '\\');
			}
			
			//loop from second char to the second to last
			for (; i < escapedS.Length; i++)
			{
				if ((escapedS[i] == ',') || (escapedS[i] == '+') || (escapedS[i] == '"') || (escapedS[i] == '\\') || (escapedS[i] == '<') || (escapedS[i] == '>') || (escapedS[i] == ';'))
				{
					escapedS.Insert(i++, '\\');
				}
			}
			
			//check last char for a space
			if (escapedS[escapedS.Length - 1] == ' ')
			{
				escapedS.Insert(escapedS.Length - 1, '\\');
			}
			return escapedS.ToString();
		}
		
		
		
		/// <summary> Returns the individual components of a distinguished name (DN).
		/// 
		/// </summary>
		/// <param name="dn">       The distinguished name, for example, "cn=Babs
		/// Jensen,ou=Accounting,o=Acme,c=US"
		/// 
		/// </param>
		/// <param name="noTypes">  If true, returns only the values of the
		/// components and not the names.  For example, "Babs
		/// Jensen", "Accounting", "Acme", "US" instead of
		/// "cn=Babs Jensen", "ou=Accounting", "o=Acme", and
		/// "c=US".
		/// 
		/// </param>
		/// <returns> An array of strings representing the individual components
		/// of a DN, or null if the DN is not valid.
		/// </returns>
		public static System.String[] explodeDN(System.String dn, bool noTypes)
		{
			DN dnToExplode = new DN(dn);
			return dnToExplode.explodeDN(noTypes);
		}
		
		/// <summary> Returns the individual components of a relative distinguished name
		/// (RDN), normalized.
		/// 
		/// </summary>
		/// <param name="rdn">    The relative distinguished name, or in other words,
		/// the left-most component of a distinguished name.
		/// 
		/// </param>
		/// <param name="noTypes">  If true, returns only the values of the
		/// components, and not the names of the component, for
		/// example "Babs Jensen" instead of "cn=Babs Jensen".
		/// 
		/// </param>
		/// <returns> An array of strings representing the individual components
		/// of an RDN, or null if the RDN is not a valid RDN.
		/// </returns>
		public static System.String[] explodeRDN(System.String rdn, bool noTypes)
		{
			RDN rdnToExplode = new RDN(rdn);
			return rdnToExplode.explodeRDN(noTypes);
		}
		
		/// <summary> Returns true if the string conforms to distinguished name syntax.</summary>
		/// <param name="dn">   String to evaluate fo distinguished name syntax.
		/// </param>
		/// <returns>      true if the dn is valid.
		/// </returns>
		public static bool isValid(System.String dn)
		{
			try
			{
				new DN(dn);
			}
			catch (System.ArgumentException iae)
			{
				return false;
			}
			return true;
		}
		
		/// <summary> Returns the DN normalized by removal of non-significant space characters
		/// as per RFC 2253, section4.
		/// 
		/// </summary>
		/// <returns>      a normalized string
		/// </returns>
		public static System.String normalize(System.String dn)
		{
			DN testDN = new DN(dn);
			return testDN.ToString();
		}
		
		
		/// <summary> Returns the RDN after unescaping the characters requiring escaping.
		/// 
		/// For example, for the rdn "cn=Acme\, Inc", the unescapeRDN method
		/// returns "cn=Acme, Inc".
		/// unescapeRDN unescapes the AttributeValue by
		/// removing the '\' when the next character fits the following:
		/// ',' '+' '"' '\' 'LESSTHAN' 'GREATERTHAN' ';'
		/// '#' if it comes at the beginning of the Attribute Name
		/// (without the '\').
		/// ' ' (space) if it comes at the beginning or the end of the Attribute Name
		/// 
		/// </summary>
		/// <param name="rdn">           The RDN to unescape.
		/// 
		/// </param>
		/// <returns> The RDN with the escaping characters removed.
		/// </returns>
		public static System.String unescapeRDN(System.String rdn)
		{
			System.Text.StringBuilder unescaped = new System.Text.StringBuilder();
			int i = 0;
			
			while (i < rdn.Length && rdn[i] != '=')
			{
				i++; //advance until we find the separator =
			}
			if (i == rdn.Length)
			{
				throw new System.ArgumentException("Could not parse rdn: Attribute " + "type and name must be separated by an equal symbol, '='");
			}
			i++;
			//check if the first two chars are "\ " (slash space) or "\#"
			if ((rdn[i] == '\\') && (i + 1 < rdn.Length - 1) && ((rdn[i + 1] == ' ') || (rdn[i + 1] == '#')))
			{
				i++;
			}
			for (; i < rdn.Length; i++)
			{
				//if the current char is a slash, not the end char, and is followed
				// by a special char then...
				if ((rdn[i] == '\\') && (i != rdn.Length - 1))
				{
					if ((rdn[i + 1] == ',') || (rdn[i + 1] == '+') || (rdn[i + 1] == '"') || (rdn[i + 1] == '\\') || (rdn[i + 1] == '<') || (rdn[i + 1] == '>') || (rdn[i + 1] == ';'))
					{
						//I'm not sure if I have to check for these special chars
						continue;
					}
					//check if the last two chars are "\ "
					else if ((rdn[i + 1] == ' ') && (i + 2 == rdn.Length))
					{
						//if the last char is a space
						continue;
					}
				}
				unescaped.Append(rdn[i]);
			}
			return unescaped.ToString();
		}
	} //end class LdapDN
}
