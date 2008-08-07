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
// Novell.Directory.Ldap.Utilclass.DN.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap.Utilclass
{
	
	/// <summary> A DN encapsulates a Distinguished Name (an ldap name with context). A DN
	/// does not need to be fully distinguished, or extend to the Root of a
	/// directory.  It provides methods to get information about the DN and to
	/// manipulate the DN.  
	/// 
	///  The following are examples of valid DN:
	/// <ul>
	/// <li>cn=admin,ou=marketing,o=corporation</li>
	/// <li>cn=admin,ou=marketing</li>
	/// <li>2.5.4.3=admin,ou=marketing</li>
	/// <li>oid.2.5.4.3=admin,ou=marketing</li>
	/// </ul>
	/// 
	/// Note: Multivalued attributes are all considered to be one
	/// component and are represented in one RDN (see RDN)
	/// 
	/// 
	/// </summary>
	/// <seealso cref="RDN">
	/// </seealso>
	
	public class DN : System.Object
	{
		private void  InitBlock()
		{
			rdnList = new System.Collections.ArrayList();
		}
		/// <summary> Retrieves a list of RDN Objects, or individual names of the DN</summary>
		/// <returns> list of RDNs
		/// </returns>
		virtual public System.Collections.ArrayList RDNs
		{
			get
			{
				int size = rdnList.Count;
				System.Collections.ArrayList v = new System.Collections.ArrayList(size);
				for (int i = 0; i < size; i++)
				{
					v.Add(rdnList[i]);
				}
				return v;
			}
			
		}
		/// <summary> Returns the Parent of this DN</summary>
		/// <returns> Parent DN
		/// </returns>
		virtual public DN Parent
		{
			get
			{
				DN parent = new DN();
				parent.rdnList = (System.Collections.ArrayList) this.rdnList.Clone();
				if (parent.rdnList.Count >= 1)
					parent.rdnList.Remove(rdnList[0]); //remove first object
				return parent;
			}
			
		}
		
		//parser state identifiers.
		private const int LOOK_FOR_RDN_ATTR_TYPE = 1;
		private const int ALPHA_ATTR_TYPE = 2;
		private const int OID_ATTR_TYPE = 3;
		private const int LOOK_FOR_RDN_VALUE = 4;
		private const int QUOTED_RDN_VALUE = 5;
		private const int HEX_RDN_VALUE = 6;
		private const int UNQUOTED_RDN_VALUE = 7;
		
		/* State transition table:  Parsing starts in state 1.
		
		State   COMMA   DIGIT   "Oid."  ALPHA   EQUAL   QUOTE   SHARP   HEX
		--------------------------------------------------------------------
		1       Err     3       3       2       Err     Err     Err     Err
		2       Err     Err     Err     2       4       Err     Err     Err
		3       Err     3       Err     Err     4       Err     Err     Err
		4       Err     7       Err     7       Err     5       6       7
		5       1       5       Err     5       Err     1       Err     7
		6       1       6       Err     Err     Err     Err     Err     6
		7       1       7       Err     7       Err     Err     Err     7
		
		*/
		
		
		private System.Collections.ArrayList rdnList;
		
		public DN()
		{
			InitBlock();
			return ;
		}
		/// <summary> Constructs a new DN based on the specified string representation of a
		/// distinguished name. The syntax of the DN must conform to that specified
		/// in RFC 2253.
		/// 
		/// </summary>
		/// <param name="dnString">a string representation of the distinguished name
		/// </param>
		/// <exception>  IllegalArgumentException  if the the value of the dnString
		/// parameter does not adhere to the syntax described in
		/// RFC 2253
		/// </exception>
		public DN(System.String dnString)
		{
			InitBlock();
			/* the empty string is a valid DN */
			if (dnString.Length == 0)
				return ;
			
			char currChar;
			char nextChar;
			int currIndex;
			char[] tokenBuf = new char[dnString.Length];
			int tokenIndex;
			int lastIndex;
			int valueStart;
			int state;
			int trailingSpaceCount = 0;
			System.String attrType = "";
			System.String attrValue = "";
			System.String rawValue = "";
			int hexDigitCount = 0;
			RDN currRDN = new RDN();
			
			//indicates whether an OID number has a first digit of ZERO
			bool firstDigitZero = false;
			
			tokenIndex = 0;
			currIndex = 0;
			valueStart = 0;
			state = LOOK_FOR_RDN_ATTR_TYPE;
			lastIndex = dnString.Length - 1;
			while (currIndex <= lastIndex)
			{
				currChar = dnString[currIndex];
				switch (state)
				{
					
					case LOOK_FOR_RDN_ATTR_TYPE: 
						while (currChar == ' ' && (currIndex < lastIndex))
							currChar = dnString[++currIndex];
						if (isAlpha(currChar))
						{
							if (dnString.Substring(currIndex).StartsWith("oid.") || dnString.Substring(currIndex).StartsWith("OID."))
							{
								//form is "oid.###.##.###... or OID.###.##.###...
								currIndex += 4; //skip oid. prefix and get to actual oid
								if (currIndex > lastIndex)
									throw new System.ArgumentException(dnString);
								currChar = dnString[currIndex];
								if (isDigit(currChar))
								{
									tokenBuf[tokenIndex++] = currChar;
									state = OID_ATTR_TYPE;
								}
								else
									throw new System.ArgumentException(dnString);
							}
							else
							{
								tokenBuf[tokenIndex++] = currChar;
								state = ALPHA_ATTR_TYPE;
							}
						}
						else if (isDigit(currChar))
						{
							--currIndex;
							state = OID_ATTR_TYPE;
						}
						else if (!(System.Char.GetUnicodeCategory(currChar) == System.Globalization.UnicodeCategory.SpaceSeparator))
							throw new System.ArgumentException(dnString);
						break;
					
					
					case ALPHA_ATTR_TYPE: 
						if (isAlpha(currChar) || isDigit(currChar) || (currChar == '-'))
							tokenBuf[tokenIndex++] = currChar;
						else
						{
							//skip any spaces
							while ((currChar == ' ') && (currIndex < lastIndex))
								currChar = dnString[++currIndex];
							if (currChar == '=')
							{
								attrType = new System.String(tokenBuf, 0, tokenIndex);
								tokenIndex = 0;
								state = LOOK_FOR_RDN_VALUE;
							}
							else
								throw new System.ArgumentException(dnString);
						}
						break;
					
					
					case OID_ATTR_TYPE: 
						if (!isDigit(currChar))
							throw new System.ArgumentException(dnString);
						firstDigitZero = (currChar == '0')?true:false;
						tokenBuf[tokenIndex++] = currChar;
						currChar = dnString[++currIndex];
						
						if ((isDigit(currChar) && firstDigitZero) || (currChar == '.' && firstDigitZero))
						{
							throw new System.ArgumentException(dnString);
						}
						
						//consume all numbers.
						while (isDigit(currChar) && (currIndex < lastIndex))
						{
							tokenBuf[tokenIndex++] = currChar;
							currChar = dnString[++currIndex];
						}
						if (currChar == '.')
						{
							tokenBuf[tokenIndex++] = currChar;
							//The state remains at OID_ATTR_TYPE
						}
						else
						{
							//skip any spaces
							while (currChar == ' ' && (currIndex < lastIndex))
								currChar = dnString[++currIndex];
							if (currChar == '=')
							{
								attrType = new System.String(tokenBuf, 0, tokenIndex);
								tokenIndex = 0;
								state = LOOK_FOR_RDN_VALUE;
							}
							else
								throw new System.ArgumentException(dnString);
						}
						break;
					
					
					case LOOK_FOR_RDN_VALUE: 
						while (currChar == ' ')
						{
							if (currIndex < lastIndex)
								currChar = dnString[++currIndex];
							else
								throw new System.ArgumentException(dnString);
						}
						if (currChar == '"')
						{
							state = QUOTED_RDN_VALUE;
							valueStart = currIndex;
						}
						else if (currChar == '#')
						{
							hexDigitCount = 0;
							tokenBuf[tokenIndex++] = currChar;
							valueStart = currIndex;
							state = HEX_RDN_VALUE;
						}
						else
						{
							valueStart = currIndex;
							//check this character again in the UNQUOTED_RDN_VALUE state
							currIndex--;
							state = UNQUOTED_RDN_VALUE;
						}
						break;
					
					
					case UNQUOTED_RDN_VALUE: 
						if (currChar == '\\')
						{
							if (!(currIndex < lastIndex))
								throw new System.ArgumentException(dnString);
							currChar = dnString[++currIndex];
							if (isHexDigit(currChar))
							{
								if (!(currIndex < lastIndex))
									throw new System.ArgumentException(dnString);
								nextChar = dnString[++currIndex];
								if (isHexDigit(nextChar))
								{
									tokenBuf[tokenIndex++] = hexToChar(currChar, nextChar);
									trailingSpaceCount = 0;
								}
								else
									throw new System.ArgumentException(dnString);
							}
							else if (needsEscape(currChar) || currChar == '#' || currChar == '=' || currChar == ' ')
							{
								tokenBuf[tokenIndex++] = currChar;
								trailingSpaceCount = 0;
							}
							else
								throw new System.ArgumentException(dnString);
						}
						else if (currChar == ' ')
						{
							trailingSpaceCount++;
							tokenBuf[tokenIndex++] = currChar;
						}
						else if ((currChar == ',') || (currChar == ';') || (currChar == '+'))
						{
							attrValue = new System.String(tokenBuf, 0, tokenIndex - trailingSpaceCount);
							rawValue = dnString.Substring(valueStart, (currIndex - trailingSpaceCount) - (valueStart));
							
							currRDN.add(attrType, attrValue, rawValue);
							if (currChar != '+')
							{
								rdnList.Add(currRDN);
								currRDN = new RDN();
							}
							
							trailingSpaceCount = 0;
							tokenIndex = 0;
							state = LOOK_FOR_RDN_ATTR_TYPE;
						}
						else if (needsEscape(currChar))
						{
							throw new System.ArgumentException(dnString);
						}
						else
						{
							trailingSpaceCount = 0;
							tokenBuf[tokenIndex++] = currChar;
						}
						break; //end UNQUOTED RDN VALUE
					
					
					case QUOTED_RDN_VALUE: 
						if (currChar == '"')
						{
							rawValue = dnString.Substring(valueStart, (currIndex + 1) - (valueStart));
							if (currIndex < lastIndex)
								currChar = dnString[++currIndex];
							//skip any spaces
							while ((currChar == ' ') && (currIndex < lastIndex))
								currChar = dnString[++currIndex];
							if ((currChar == ',') || (currChar == ';') || (currChar == '+') || (currIndex == lastIndex))
							{
								attrValue = new System.String(tokenBuf, 0, tokenIndex);
								
								currRDN.add(attrType, attrValue, rawValue);
								if (currChar != '+')
								{
									rdnList.Add(currRDN);
									currRDN = new RDN();
								}
								trailingSpaceCount = 0;
								tokenIndex = 0;
								state = LOOK_FOR_RDN_ATTR_TYPE;
							}
							else
								throw new System.ArgumentException(dnString);
						}
						else if (currChar == '\\')
						{
							currChar = dnString[++currIndex];
							if (isHexDigit(currChar))
							{
								nextChar = dnString[++currIndex];
								if (isHexDigit(nextChar))
								{
									tokenBuf[tokenIndex++] = hexToChar(currChar, nextChar);
									trailingSpaceCount = 0;
								}
								else
									throw new System.ArgumentException(dnString);
							}
							else if (needsEscape(currChar) || currChar == '#' || currChar == '=' || currChar == ' ')
							{
								tokenBuf[tokenIndex++] = currChar;
								trailingSpaceCount = 0;
							}
							else
								throw new System.ArgumentException(dnString);
						}
						else
							tokenBuf[tokenIndex++] = currChar;
						break; //end QUOTED RDN VALUE
					
					
					case HEX_RDN_VALUE: 
						if ((!isHexDigit(currChar)) || (currIndex > lastIndex))
						{
							//check for odd number of hex digits
							if ((hexDigitCount % 2) != 0 || hexDigitCount == 0)
								throw new System.ArgumentException(dnString);
							else
							{
								rawValue = dnString.Substring(valueStart, (currIndex) - (valueStart));
								//skip any spaces
								while ((currChar == ' ') && (currIndex < lastIndex))
									currChar = dnString[++currIndex];
								if ((currChar == ',') || (currChar == ';') || (currChar == '+') || (currIndex == lastIndex))
								{
									attrValue = new System.String(tokenBuf, 0, tokenIndex);
									
									//added by cameron
									currRDN.add(attrType, attrValue, rawValue);
									if (currChar != '+')
									{
										rdnList.Add(currRDN);
										currRDN = new RDN();
									}
									tokenIndex = 0;
									state = LOOK_FOR_RDN_ATTR_TYPE;
								}
								else
								{
									throw new System.ArgumentException(dnString);
								}
							}
						}
						else
						{
							tokenBuf[tokenIndex++] = currChar;
							hexDigitCount++;
						}
						break; //end HEX RDN VALUE
					} //end switch
				currIndex++;
			} //end while
			
			//check ending state
			if (state == UNQUOTED_RDN_VALUE || (state == HEX_RDN_VALUE && (hexDigitCount % 2) == 0) && hexDigitCount != 0)
			{
				attrValue = new System.String(tokenBuf, 0, tokenIndex - trailingSpaceCount);
				rawValue = dnString.Substring(valueStart, (currIndex - trailingSpaceCount) - (valueStart));
				currRDN.add(attrType, attrValue, rawValue);
				rdnList.Add(currRDN);
			}
			else if (state == LOOK_FOR_RDN_VALUE)
			{
				//empty value is valid
				attrValue = "";
				rawValue = dnString.Substring(valueStart);
				currRDN.add(attrType, attrValue, rawValue);
				rdnList.Add(currRDN);
			}
			else
			{
				throw new System.ArgumentException(dnString);
			}
		} //end DN constructor (string dn)
		
		
		/// <summary> Checks a character to see if it is an ascii alphabetic character in
		/// ranges 65-90 or 97-122.
		/// 
		/// </summary>
		/// <param name="ch">the character to be tested.
		/// </param>
		/// <returns>  <code>true</code> if the character is an ascii alphabetic
		/// character
		/// </returns>
		private bool isAlpha(char ch)
		{
			if (((ch < 91) && (ch > 64)) || ((ch < 123) && (ch > 96)))
			//ASCII A-Z
				return true;
			else
				return false;
		}
		
		
		/// <summary> Checks a character to see if it is an ascii digit (0-9) character in
		/// the ascii value range 48-57.
		/// 
		/// </summary>
		/// <param name="ch">the character to be tested.
		/// </param>
		/// <returns>  <code>true</code> if the character is an ascii alphabetic
		/// character
		/// </returns>
		private bool isDigit(char ch)
		{
			if ((ch < 58) && (ch > 47))
			//ASCII 0-9
				return true;
			else
				return false;
		}
		
		/// <summary> Checks a character to see if it is valid hex digit 0-9, a-f, or
		/// A-F (ASCII value ranges 48-47, 65-70, 97-102).
		/// 
		/// </summary>
		/// <param name="ch">the character to be tested.
		/// </param>
		/// <returns>  <code>true</code> if the character is a valid hex digit
		/// </returns>
		
		private static bool isHexDigit(char ch)
		{
			if (((ch < 58) && (ch > 47)) || ((ch < 71) && (ch > 64)) || ((ch < 103) && (ch > 96)))
			//ASCII A-F
				return true;
			else
				return false;
		}
		
		/// <summary> Checks a character to see if it must always be escaped in the
		/// string representation of a DN.  We must tests for space, sharp, and
		/// equals individually.
		/// 
		/// </summary>
		/// <param name="ch">the character to be tested.
		/// </param>
		/// <returns>  <code>true</code> if the character needs to be escaped in at
		/// least some instances.
		/// </returns>
		private bool needsEscape(char ch)
		{
			if ((ch == ',') || (ch == '+') || (ch == '\"') || (ch == ';') || (ch == '<') || (ch == '>') || (ch == '\\'))
				return true;
			else
				return false;
		}
		
		/// <summary> Converts two valid hex digit characters that form the string
		/// representation of an ascii character value to the actual ascii
		/// character.
		/// 
		/// </summary>
		/// <param name="hex1">the hex digit for the high order byte.
		/// </param>
		/// <param name="hex0">the hex digit for the low order byte.
		/// </param>
		/// <returns>  the character whose value is represented by the parameters.
		/// </returns>
		
		private static char hexToChar(char hex1, char hex0)
		{
			int result;
			
			if ((hex1 < 58) && (hex1 > 47))
			//ASCII 0-9
				result = (hex1 - 48) * 16;
			else if ((hex1 < 71) && (hex1 > 64))
			//ASCII a-f
				result = (hex1 - 55) * 16;
			else if ((hex1 < 103) && (hex1 > 96))
			//ASCII A-F
				result = (hex1 - 87) * 16;
			else
				throw new System.ArgumentException("Not hex digit");
			
			if ((hex0 < 58) && (hex0 > 47))
			//ASCII 0-9
				result += (hex0 - 48);
			else if ((hex0 < 71) && (hex0 > 64))
			//ASCII a-f
				result += (hex0 - 55);
			else if ((hex0 < 103) && (hex0 > 96))
			//ASCII A-F
				result += (hex0 - 87);
			else
				throw new System.ArgumentException("Not hex digit");
			
			return (char) result;
		}
		
		/// <summary> Creates and returns a string that represents this DN.  The string
		/// follows RFC 2253, which describes String representation of DN's and
		/// RDN's
		/// 
		/// </summary>
		/// <returns> A DN string.
		/// </returns>
		public override System.String ToString()
		{
			int length = rdnList.Count;
			System.String dn = "";
			if (length < 1)
				return null;
			dn = rdnList[0].ToString();
			for (int i = 1; i < length; i++)
			{
				dn += ("," + rdnList[i].ToString());
			}
			return dn;
		}
		
		
		/// <summary> Compares this DN to the specified DN to determine if they are equal.
		/// 
		/// </summary>
		/// <param name="toDN">the DN to compare to
		/// </param>
		/// <returns>  <code>true</code> if the DNs are equal; otherwise
		/// <code>false</code>
		/// </returns>

		public System.Collections.ArrayList getrdnList()
		{
			return this.rdnList;
		}
		public  override bool Equals(System.Object toDN)
		{
			return Equals((DN) toDN);
		}
		public   bool Equals(DN toDN)
		{
			System.Collections.ArrayList aList=toDN.getrdnList();
			int length = aList.Count;
			
			if (this.rdnList.Count != length)
				return false;
			
			for (int i = 0; i < length; i++)
			{
				if (!((RDN) rdnList[i]).equals((RDN) toDN.getrdnList()[i]))
					return false;
			}
			return true;
		}
		
		/// <summary> return a string array of the individual RDNs contained in the DN
		/// 
		/// </summary>
		/// <param name="noTypes">  If true, returns only the values of the
		/// components, and not the names, e.g. "Babs
		/// Jensen", "Accounting", "Acme", "us" - instead of
		/// "cn=Babs Jensen", "ou=Accounting", "o=Acme", and
		/// "c=us".
		/// </param>
		/// <returns>  <code>String[]</code> containing the rdns in the DN with
		/// the leftmost rdn in the first element of the array
		/// 
		/// </returns>
		public virtual System.String[] explodeDN(bool noTypes)
		{
			int length = rdnList.Count;
			System.String[] rdns = new System.String[length];
			for (int i = 0; i < length; i++)
				rdns[i] = ((RDN) rdnList[i]).toString(noTypes);
			return rdns;
		}
		
		/// <summary> Retrieves the count of RDNs, or individule names, in the Distinguished name</summary>
		/// <returns> the count of RDN
		/// </returns>
		public virtual int countRDNs()
		{
			return rdnList.Count;
		}
		
		/// <summary>Determines if this DN is <I>contained</I> by the DN passed in.  For
		/// example:  "cn=admin, ou=marketing, o=corporation" is contained by
		/// "o=corporation", "ou=marketing, o=corporation", and "ou=marketing"
		/// but <B>not</B> by "cn=admin" or "cn=admin,ou=marketing,o=corporation"
		/// Note: For users of Netscape's SDK this method is comparable to contains
		/// 
		/// </summary>
		/// <param name="containerDN">of a container
		/// </param>
		/// <returns> true if containerDN contains this DN
		/// </returns>
		public virtual bool isDescendantOf(DN containerDN)
		{
			int i = containerDN.rdnList.Count - 1; //index to an RDN of the ContainerDN
			int j = this.rdnList.Count - 1; //index to an RDN of the ContainedDN
			//Search from the end of the DN for an RDN that matches the end RDN of
			//containerDN.
			while (!((RDN) this.rdnList[j]).equals((RDN) containerDN.rdnList[i]))
			{
				j--;
				if (j <= 0)
					return false;
				//if the end RDN of containerDN does not have any equal
				//RDN in rdnList, then containerDN does not contain this DN
			}
			i--; //avoid a redundant compare
			j--;
			//step backwards to verify that all RDNs in containerDN exist in this DN
			for (; i >= 0 && j >= 0; i--, j--)
			{
				if (!((RDN) this.rdnList[j]).equals((RDN) containerDN.rdnList[i]))
					return false;
			}
			if (j == 0 && i == 0)
			//the DNs are identical and thus not contained
				return false;
			
			return true;
		}
		
		/// <summary> Adds the RDN to the beginning of the current DN.</summary>
		/// <param name="rdn">an RDN to be added
		/// </param>
		public virtual void  addRDN(RDN rdn)
		{
			rdnList.Insert(0, rdn);
		}
		
		/// <summary> Adds the RDN to the beginning of the current DN.</summary>
		/// <param name="rdn">an RDN to be added
		/// </param>
		public virtual void  addRDNToFront(RDN rdn)
		{
			rdnList.Insert(0, rdn);
		}
		
		/// <summary> Adds the RDN to the end of the current DN</summary>
		/// <param name="rdn">an RDN to be added
		/// </param>
		public virtual void  addRDNToBack(RDN rdn)
		{
			rdnList.Add(rdn);
		}
	} //end class DN
}
