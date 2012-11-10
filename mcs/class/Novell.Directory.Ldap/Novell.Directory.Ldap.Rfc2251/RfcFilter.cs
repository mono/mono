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
// Novell.Directory.Ldap.Rfc2251.RfcFilter.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap.Asn1;
using LdapException = Novell.Directory.Ldap.LdapException;
using LdapLocalException = Novell.Directory.Ldap.LdapLocalException;
using LdapSearchRequest = Novell.Directory.Ldap.LdapSearchRequest;
using Novell.Directory.Ldap.Utilclass;

namespace Novell.Directory.Ldap.Rfc2251
{
	
	/// <summary> Represents an Ldap Filter.
	/// 
	/// This filter object can be created from a String or can be built up
	/// programatically by adding filter components one at a time.  Existing filter
	/// components can be iterated though.
	/// 
	/// Each filter component has an integer identifier defined in this class.
	/// The following are basic filter components: {@link #EQUALITY_MATCH},
	/// {@link #GREATER_OR_EQUAL}, {@link #LESS_OR_EQUAL}, {@link #SUBSTRINGS},
	/// {@link #PRESENT}, {@link #APPROX_MATCH}, {@link #EXTENSIBLE_MATCH}.
	/// 
	/// More filters can be nested together into more complex filters with the
	/// following filter components: {@link #AND}, {@link #OR}, {@link #NOT} 
	/// 
	/// Substrings can have three components:
	/// <pre>
	/// Filter ::= CHOICE {
	/// and             [0] SET OF Filter,
	/// or              [1] SET OF Filter,
	/// not             [2] Filter,
	/// equalityMatch   [3] AttributeValueAssertion,
	/// substrings      [4] SubstringFilter,
	/// greaterOrEqual  [5] AttributeValueAssertion,
	/// lessOrEqual     [6] AttributeValueAssertion,
	/// present         [7] AttributeDescription,
	/// approxMatch     [8] AttributeValueAssertion,
	/// extensibleMatch [9] MatchingRuleAssertion }
	/// </pre>
	/// </summary>
	public class RfcFilter:Asn1Choice
	{
		//*************************************************************************
		// Public variables for Filter
		//*************************************************************************
		
		/// <summary> Identifier for AND component.</summary>
		public const int AND = LdapSearchRequest.AND;
		/// <summary> Identifier for OR component.</summary>
		public const int OR = LdapSearchRequest.OR;
		/// <summary> Identifier for NOT component.</summary>
		public const int NOT = LdapSearchRequest.NOT;
		/// <summary> Identifier for EQUALITY_MATCH component.</summary>
		public const int EQUALITY_MATCH = LdapSearchRequest.EQUALITY_MATCH;
		/// <summary> Identifier for SUBSTRINGS component.</summary>
		public const int SUBSTRINGS = LdapSearchRequest.SUBSTRINGS;
		/// <summary> Identifier for GREATER_OR_EQUAL component.</summary>
		public const int GREATER_OR_EQUAL = LdapSearchRequest.GREATER_OR_EQUAL;
		/// <summary> Identifier for LESS_OR_EQUAL component.</summary>
		public const int LESS_OR_EQUAL = LdapSearchRequest.LESS_OR_EQUAL;
		/// <summary> Identifier for PRESENT component.</summary>
		public const int PRESENT = LdapSearchRequest.PRESENT;
		/// <summary> Identifier for APPROX_MATCH component.</summary>
		public const int APPROX_MATCH = LdapSearchRequest.APPROX_MATCH;
		/// <summary> Identifier for EXTENSIBLE_MATCH component.</summary>
		public const int EXTENSIBLE_MATCH = LdapSearchRequest.EXTENSIBLE_MATCH;
		
		/// <summary> Identifier for INITIAL component.</summary>
		public const int INITIAL = LdapSearchRequest.INITIAL;
		/// <summary> Identifier for ANY component.</summary>
		public const int ANY = LdapSearchRequest.ANY;
		/// <summary> Identifier for FINAL component.</summary>
		public const int FINAL = LdapSearchRequest.FINAL;
		
		//*************************************************************************
		// Private variables for Filter
		//*************************************************************************
		
		private FilterTokenizer ft;
		private System.Collections.Stack filterStack;
		private bool finalFound;
		
		//*************************************************************************
		// Constructor for Filter
		//*************************************************************************
		
		/// <summary> Constructs a Filter object by parsing an RFC 2254 Search Filter String.</summary>
		public RfcFilter(System.String filter):base(null)
		{
			ChoiceValue = parse(filter);
			return ;
		}
		
		/// <summary> Constructs a Filter object that will be built up piece by piece.   </summary>
		public RfcFilter():base(null)
		{
			filterStack = new System.Collections.Stack();
			//The choice value must be set later: setChoiceValue(rootFilterTag)
			return ;
		}
		
		//*************************************************************************
		// Helper methods for RFC 2254 Search Filter parsing.
		//*************************************************************************
		
		/// <summary> Parses an RFC 2251 filter string into an ASN.1 Ldap Filter object.</summary>
		private Asn1Tagged parse(System.String filterExpr)
		{

			if ((System.Object) filterExpr == null || filterExpr.Equals(""))
			{
				filterExpr = new System.Text.StringBuilder("(objectclass=*)").ToString();
			}
			int idx;
			if ((idx = filterExpr.IndexOf((System.Char) '\\')) != - 1)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder(filterExpr);
				int i = idx;
				while (i < (sb.Length - 1))
				{
					char c = sb[i++];
					if (c == '\\')
					{
						// found '\' (backslash)
						// If V2 escape, turn to a V3 escape
						c = sb[i];
						if (c == '*' || c == '(' || c == ')' || c == '\\')
						{
							// Ldap v2 filter, convert them into hex chars
							sb.Remove(i, i + 1 - i);
							sb.Insert(i, System.Convert.ToString((int) c, 16));
							i += 2;
						}
					}
				}
				filterExpr = sb.ToString();
			}
			
			// missing opening and closing parentheses, must be V2, add parentheses
			if ((filterExpr[0] != '(') && (filterExpr[filterExpr.Length - 1] != ')'))
			{
				filterExpr = "(" + filterExpr + ")";
			}
			
			char ch = filterExpr[0];
			int len = filterExpr.Length;
			
			// missing opening parenthesis ?
			if (ch != '(')
			{
				throw new LdapLocalException(ExceptionMessages.MISSING_LEFT_PAREN, LdapException.FILTER_ERROR);
			}
			
			// missing closing parenthesis ?
			if (filterExpr[len - 1] != ')')
			{
				throw new LdapLocalException(ExceptionMessages.MISSING_RIGHT_PAREN, LdapException.FILTER_ERROR);
			}
			
			// unmatched parentheses ?
			int parenCount = 0;
			for (int i = 0; i < len; i++)
			{
				if (filterExpr[i] == '(')
				{
					parenCount++;
				}
				
				if (filterExpr[i] == ')')
				{
					parenCount--;
				}
			}
			
			if (parenCount > 0)
			{
				throw new LdapLocalException(ExceptionMessages.MISSING_RIGHT_PAREN, LdapException.FILTER_ERROR);
			}
			
			if (parenCount < 0)
			{
				throw new LdapLocalException(ExceptionMessages.MISSING_LEFT_PAREN, LdapException.FILTER_ERROR);
			}
			ft = new FilterTokenizer(this, filterExpr);
			
			return parseFilter();
		}
		
		/// <summary> Parses an RFC 2254 filter</summary>
		private Asn1Tagged parseFilter()
		{
			ft.getLeftParen();
			
			Asn1Tagged filter = parseFilterComp();
			
			ft.getRightParen();
			
			return filter;
		}
		
		/// <summary> RFC 2254 filter helper method. Will Parse a filter component.</summary>
		private Asn1Tagged parseFilterComp()
		{
			Asn1Tagged tag = null;
			int filterComp = ft.OpOrAttr;
			
			switch (filterComp)
			{
				
				case AND: 
				case OR: 
					tag = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, true, filterComp), parseFilterList(), false);
					break;
				
				case NOT: 
					tag = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, true, filterComp), parseFilter(), true);
					break;
				
				default: 
					int filterType = ft.FilterType;
					System.String value_Renamed = ft.Value;
					
					switch (filterType)
					{
						
						case GREATER_OR_EQUAL: 
						case LESS_OR_EQUAL: 
						case APPROX_MATCH: 
							tag = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, true, filterType), new RfcAttributeValueAssertion(new RfcAttributeDescription(ft.Attr), new RfcAssertionValue(unescapeString(value_Renamed))), false);
							break;
						
						case EQUALITY_MATCH: 
							if (value_Renamed.Equals("*"))
							{
								// present
								tag = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, PRESENT), new RfcAttributeDescription(ft.Attr), false);
							}
							else if (value_Renamed.IndexOf((System.Char) '*') != - 1)
							{
								// substrings parse:
								//    [initial], *any*, [final] into an Asn1SequenceOf
								SupportClass.Tokenizer sub = new SupportClass.Tokenizer(value_Renamed, "*", true);
//								SupportClass.Tokenizer sub = new SupportClass.Tokenizer(value_Renamed, "*");//, true);
								Asn1SequenceOf seq = new Asn1SequenceOf(5);
								int tokCnt = sub.Count;
								int cnt = 0;
								
								System.String lastTok = new System.Text.StringBuilder("").ToString();
								
								while (sub.HasMoreTokens())
								{
									System.String subTok = sub.NextToken();
									cnt++;
									if (subTok.Equals("*"))
									{
										// if previous token was '*', and since the current
										// token is a '*', we need to insert 'any'
										if (lastTok.Equals(subTok))
										{
											// '**'
											seq.add(new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, ANY), new RfcLdapString(unescapeString("")), false));
										}
									}
									else
									{
										// value (RfcLdapString)
										if (cnt == 1)
										{
											// initial
											seq.add(new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, INITIAL), new RfcLdapString(unescapeString(subTok)), false));
										}
										else if (cnt < tokCnt)
										{
											// any
											seq.add(new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, ANY), new RfcLdapString(unescapeString(subTok)), false));
										}
										else
										{
											// final
											seq.add(new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, FINAL), new RfcLdapString(unescapeString(subTok)), false));
										}
									}
									lastTok = subTok;
								}
								
								tag = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, true, SUBSTRINGS), new RfcSubstringFilter(new RfcAttributeDescription(ft.Attr), seq), false);
							}
							else
							{
								// simple
								tag = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, true, EQUALITY_MATCH), new RfcAttributeValueAssertion(new RfcAttributeDescription(ft.Attr), new RfcAssertionValue(unescapeString(value_Renamed))), false);
							}
							break;
						
						case EXTENSIBLE_MATCH: 
							System.String type = null, matchingRule = null;
							bool dnAttributes = false;
//							SupportClass.Tokenizer st = new StringTokenizer(ft.Attr, ":", true);
							SupportClass.Tokenizer st = new SupportClass.Tokenizer(ft.Attr, ":");//, true);
							
							bool first = true;
							while (st.HasMoreTokens())
							{
								System.String s = st.NextToken().Trim();
								if (first && !s.Equals(":"))
								{
									type = s;
								}
								// dn must be lower case to be considered dn of the Entry.
								else if (s.Equals("dn"))
								{
									dnAttributes = true;
								}
								else if (!s.Equals(":"))
								{
									matchingRule = s;
								}
								first = false;
							}
							
							tag = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, true, EXTENSIBLE_MATCH), new RfcMatchingRuleAssertion(((System.Object) matchingRule == null)?null:new RfcMatchingRuleId(matchingRule), ((System.Object) type == null)?null:new RfcAttributeDescription(type), new RfcAssertionValue(unescapeString(value_Renamed)), (dnAttributes == false)?null:new Asn1Boolean(true)), false);
							break;
						}
					break;
				
			}
			return tag;
		}
		
		/// <summary> Must have 1 or more Filters</summary>
		private Asn1SetOf parseFilterList()
		{
			Asn1SetOf set_Renamed = new Asn1SetOf();
			
			set_Renamed.add(parseFilter()); // must have at least 1 filter
			
			while (ft.peekChar() == '(')
			{
				// check for more filters
				set_Renamed.add(parseFilter());
			}
			return set_Renamed;
		}
		
		/// <summary> Convert hex character to an integer. Return -1 if char is something
		/// other than a hex char.
		/// </summary>
		internal static int hex2int(char c)
		{
			return (c >= '0' && c <= '9')?c - '0':(c >= 'A' && c <= 'F')?c - 'A' + 10:(c >= 'a' && c <= 'f')?c - 'a' + 10:- 1;
		}
		
		/// <summary> Replace escaped hex digits with the equivalent binary representation.
		/// Assume either V2 or V3 escape mechanisms:
		/// V2: \*,  \(,  \),  \\.
		/// V3: \2A, \28, \29, \5C, \00.
		/// 
		/// </summary>
		/// <param name="string">   A part of the input filter string to be converted.
		/// 
		/// </param>
		/// <returns> octet-string encoding of the specified string.
		/// </returns>
		private sbyte[] unescapeString(System.String string_Renamed)
		{
			// give octets enough space to grow
			sbyte[] octets = new sbyte[string_Renamed.Length * 3];
			// index for string and octets
			int iString, iOctets;
			// escape==true means we are in an escape sequence.
			bool escape = false;
			// escStart==true means we are reading the first character of an escape.
			bool escStart = false;
			
			int ival, length = string_Renamed.Length;
			sbyte[] utf8Bytes;
			char ch; // Character we are adding to the octet string
			char[] ca = new char[1]; // used while converting multibyte UTF-8 char
			char temp = (char) (0); // holds the value of the escaped sequence
			
			// loop through each character of the string and copy them into octets
			// converting escaped sequences when needed
			for (iString = 0, iOctets = 0; iString < length; iString++)
			{
				ch = string_Renamed[iString];
				if (escape)
				{
					if ((ival = hex2int(ch)) < 0)
					{
						// Invalid escape value(not a hex character)
						throw new LdapLocalException(ExceptionMessages.INVALID_ESCAPE, new System.Object[]{ch}, LdapException.FILTER_ERROR);
					}
					else
					{
						// V3 escaped: \\**
						if (escStart)
						{
							temp = (char) (ival << 4); // high bits of escaped char
							escStart = false;
						}
						else
						{
							temp |= (char) (ival); // all bits of escaped char
							octets[iOctets++] = (sbyte) temp;
							escStart = escape = false;
						}
					}
				}
				else if (ch == '\\')
				{
					escStart = escape = true;
				}
				else
				{
					try
					{
						// place the character into octets.
						if ((ch >= 0x01 && ch <= 0x27) || (ch >= 0x2B && ch <= 0x5B) || (ch >= 0x5D))
						{
							// found valid char
							if (ch <= 0x7f)
							{
								// char = %x01-27 / %x2b-5b / %x5d-7f
								octets[iOctets++] = (sbyte) ch;
							}
							else
							{
								// char > 0x7f, could be encoded in 2 or 3 bytes
								ca[0] = ch;
								System.Text.Encoding encoder = System.Text.Encoding.GetEncoding("utf-8"); 
								byte[] ibytes = encoder.GetBytes(new System.String(ca));
								utf8Bytes=SupportClass.ToSByteArray(ibytes);

//								utf8Bytes = new System.String(ca).getBytes("UTF-8");
								// copy utf8 encoded character into octets
								Array.Copy((System.Array) (utf8Bytes), 0, (System.Array) octets, iOctets, utf8Bytes.Length);
								iOctets = iOctets + utf8Bytes.Length;
							}
							escape = false;
						}
						else
						{
							// found invalid character
							System.String escString = "";
							ca[0] = ch;
							System.Text.Encoding encoder = System.Text.Encoding.GetEncoding("utf-8"); 
							byte[] ibytes = encoder.GetBytes(new System.String(ca));
							utf8Bytes=SupportClass.ToSByteArray(ibytes);

//							utf8Bytes = new System.String(ca).getBytes("UTF-8");
							for (int i = 0; i < utf8Bytes.Length; i++)
							{
								sbyte u = utf8Bytes[i];
								if ((u >= 0) && (u < 0x10))
								{
									escString = escString + "\\0" + System.Convert.ToString(u & 0xff, 16);
								}
								else
								{
									escString = escString + "\\" + System.Convert.ToString(u & 0xff, 16);
								}
							}
							throw new LdapLocalException(ExceptionMessages.INVALID_CHAR_IN_FILTER, new System.Object[]{ch, escString}, LdapException.FILTER_ERROR);
						}
					}
					catch (System.IO.IOException ue)
					{
						throw new System.SystemException("UTF-8 String encoding not supported by JVM");
					}
				}
			}
			
			// Verify that any escape sequence completed
			if (escStart || escape)
			{
				throw new LdapLocalException(ExceptionMessages.SHORT_ESCAPE, LdapException.FILTER_ERROR);
			}
			
			sbyte[] toReturn = new sbyte[iOctets];
//			Array.Copy((System.Array)SupportClass.ToByteArray(octets), 0, (System.Array)SupportClass.ToByteArray(toReturn), 0, iOctets);
			Array.Copy((System.Array)octets, 0, (System.Array)toReturn, 0, iOctets);

			octets = null;
			return toReturn;
		}
		
		/* **********************************************************************
		*  The following methods aid in building filters sequentially,
		*  and is used by DSMLHandler:
		***********************************************************************/
		
		/// <summary> Called by sequential filter building methods to add to a filter
		/// component.
		/// 
		/// Verifies that the specified Asn1Object can be added, then adds the
		/// object to the filter.
		/// </summary>
		/// <param name="current">  Filter component to be added to the filter
		/// @throws LdapLocalException Occurs when an invalid component is added, or
		/// when the component is out of sequence.
		/// </param>
		private void  addObject(Asn1Object current)
		{
			if (filterStack == null)
			{
				filterStack = new System.Collections.Stack();
			}
			if (choiceValue() == null)
			{
				//ChoiceValue is the root Asn1 node
				ChoiceValue = current;
			}
			else
			{
				Asn1Tagged topOfStack = (Asn1Tagged) filterStack.Peek();
				Asn1Object value_Renamed = topOfStack.taggedValue();
				if (value_Renamed == null)
				{
					topOfStack.TaggedValue = current;
					filterStack.Push(current);
//					filterStack.Add(current);
				}
				else if (value_Renamed is Asn1SetOf)
				{
					((Asn1SetOf) value_Renamed).add(current);
					//don't add this to the stack:
				}
				else if (value_Renamed is Asn1Set)
				{
					((Asn1Set) value_Renamed).add(current);
					//don't add this to the stack:
				}
				else if (value_Renamed.getIdentifier().Tag == LdapSearchRequest.NOT)
				{
					throw new LdapLocalException("Attemp to create more than one 'not' sub-filter", LdapException.FILTER_ERROR);
				}
			}
			int type = current.getIdentifier().Tag;
			if (type == AND || type == OR || type == NOT)
			{
//				filterStack.Add(current);
				filterStack.Push(current);
			}
			return ;
		}
		
		/// <summary> Creates and addes a substrings filter component.
		/// 
		/// startSubstrings must be immediatly followed by at least one
		/// {@link #addSubstring} method and one {@link #endSubstrings} method
		/// @throws Novell.Directory.Ldap.LdapLocalException
		/// Occurs when this component is created out of sequence.
		/// </summary>
		public virtual void  startSubstrings(System.String attrName)
		{
			finalFound = false;
			Asn1SequenceOf seq = new Asn1SequenceOf(5);
			Asn1Object current = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, true, SUBSTRINGS), new RfcSubstringFilter(new RfcAttributeDescription(attrName), seq), false);
			addObject(current);
			SupportClass.StackPush(filterStack, seq);
			return ;
		}
		
		/// <summary> Adds a Substring component of initial, any or final substring matching.
		/// 
		/// This method can be invoked only if startSubString was the last filter-
		/// building method called.  A substring is not required to have an 'INITIAL'
		/// substring.  However, when a filter contains an 'INITIAL' substring only
		/// one can be added, and it must be the first substring added. Any number of
		/// 'ANY' substrings can be added. A substring is not required to have a
		/// 'FINAL' substrings either.  However, when a filter does contain a 'FINAL'
		/// substring only one can be added, and it must be the last substring added.
		/// 
		/// </summary>
		/// <param name="type">Substring type: INITIAL | ANY | FINAL]
		/// </param>
		/// <param name="value">Value to use for matching
		/// @throws LdapLocalException   Occurs if this method is called out of
		/// sequence or the type added is out of sequence.
		/// </param>
		[CLSCompliantAttribute(false)]
		public virtual void  addSubstring(int type, sbyte[] value_Renamed)
		{
			try
			{
				Asn1SequenceOf substringSeq = (Asn1SequenceOf) filterStack.Peek();
				if (type != INITIAL && type != ANY && type != FINAL)
				{
					throw new LdapLocalException("Attempt to add an invalid " + "substring type", LdapException.FILTER_ERROR);
				}
				
				if (type == INITIAL && substringSeq.size() != 0)
				{
					throw new LdapLocalException("Attempt to add an initial " + "substring match after the first substring", LdapException.FILTER_ERROR);
				}
				if (finalFound)
				{
					throw new LdapLocalException("Attempt to add a substring " + "match after a final substring match", LdapException.FILTER_ERROR);
				}
				if (type == FINAL)
				{
					finalFound = true;
				}
				substringSeq.add(new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, type), new RfcLdapString(value_Renamed), false));
			}
			catch (System.InvalidCastException e)
			{
				throw new LdapLocalException("A call to addSubstring occured " + "without calling startSubstring", LdapException.FILTER_ERROR);
			}
			return ;
		}
		
		/// <summary> Completes a SubString filter component.
		/// 
		/// @throws LdapLocalException Occurs when this is called out of sequence,
		/// or the substrings filter is empty.
		/// </summary>
		public virtual void  endSubstrings()
		{
			try
			{
				finalFound = false;
				Asn1SequenceOf substringSeq = (Asn1SequenceOf) filterStack.Peek();
				if (substringSeq.size() == 0)
				{
					throw new LdapLocalException("Empty substring filter", LdapException.FILTER_ERROR);
				}
			}
			catch (System.InvalidCastException e)
			{
				throw new LdapLocalException("Missmatched ending of substrings", LdapException.FILTER_ERROR);
			}
			filterStack.Pop();
			return ;
		}
		
		/// <summary> Creates and adds an AttributeValueAssertion to the filter.
		/// 
		/// </summary>
		/// <param name="rfcType">Filter type: EQUALITY_MATCH | GREATER_OR_EQUAL
		/// | LESS_OR_EQUAL | APPROX_MATCH ]
		/// </param>
		/// <param name="attrName">Name of the attribute to be asserted
		/// </param>
		/// <param name="value">Value of the attribute to be asserted
		/// @throws LdapLocalException
		/// Occurs when the filter type is not a valid attribute assertion.
		/// </param>
		[CLSCompliantAttribute(false)]
		public virtual void  addAttributeValueAssertion(int rfcType, System.String attrName, sbyte[] value_Renamed)
		{
			if (filterStack != null && !(filterStack.Count == 0) && filterStack.Peek() is Asn1SequenceOf)
			{
				//If a sequenceof is on the stack then substring is left on the stack
				throw new LdapLocalException("Cannot insert an attribute assertion in a substring", LdapException.FILTER_ERROR);
			}
			if ((rfcType != EQUALITY_MATCH) && (rfcType != GREATER_OR_EQUAL) && (rfcType != LESS_OR_EQUAL) && (rfcType != APPROX_MATCH))
			{
				throw new LdapLocalException("Invalid filter type for AttributeValueAssertion", LdapException.FILTER_ERROR);
			}
			Asn1Object current = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, true, rfcType), new RfcAttributeValueAssertion(new RfcAttributeDescription(attrName), new RfcAssertionValue(value_Renamed)), false);
			addObject(current);
			return ;
		}
		
		/// <summary> Creates and adds a present matching to the filter.
		/// 
		/// </summary>
		/// <param name="attrName">Name of the attribute to check for presence.
		/// @throws LdapLocalException
		/// Occurs if addPresent is called out of sequence.
		/// </param>
		public virtual void  addPresent(System.String attrName)
		{
			Asn1Object current = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, false, PRESENT), new RfcAttributeDescription(attrName), false);
			addObject(current);
			return ;
		}
		
		/// <summary> Adds an extensible match to the filter.
		/// 
		/// </summary>
		/// <param name="">matchingRule
		/// OID or name of the matching rule to use for comparison
		/// </param>
		/// <param name="attrName"> Name of the attribute to match.
		/// </param>
		/// <param name="value"> Value of the attribute to match against.
		/// </param>
		/// <param name="useDNMatching">Indicates whether DN matching should be used.
		/// @throws LdapLocalException
		/// Occurs when addExtensibleMatch is called out of sequence.
		/// </param>
		[CLSCompliantAttribute(false)]
		public virtual void  addExtensibleMatch(System.String matchingRule, System.String attrName, sbyte[] value_Renamed, bool useDNMatching)
		{
			Asn1Object current = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, true, EXTENSIBLE_MATCH), new RfcMatchingRuleAssertion(((System.Object) matchingRule == null)?null:new RfcMatchingRuleId(matchingRule), ((System.Object) attrName == null)?null:new RfcAttributeDescription(attrName), new RfcAssertionValue(value_Renamed), (useDNMatching == false)?null:new Asn1Boolean(true)), false);
			addObject(current);
			return ;
		}
		
		/// <summary> Creates and adds the Asn1Tagged value for a nestedFilter: AND, OR, or
		/// NOT.
		/// 
		/// Note that a Not nested filter can only have one filter, where AND
		/// and OR do not
		/// 
		/// </summary>
		/// <param name="rfcType">Filter type:
		/// [AND | OR | NOT]
		/// @throws Novell.Directory.Ldap.LdapLocalException
		/// </param>
		public virtual void  startNestedFilter(int rfcType)
		{
			Asn1Object current;
			if (rfcType == AND || rfcType == OR)
			{
				current = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, true, rfcType), new Asn1SetOf(), false);
			}
			else if (rfcType == NOT)
			{
				current = new Asn1Tagged(new Asn1Identifier(Asn1Identifier.CONTEXT, true, rfcType), null, true);
			}
			else
			{
				throw new LdapLocalException("Attempt to create a nested filter other than AND, OR or NOT", LdapException.FILTER_ERROR);
			}
			addObject(current);
			return ;
		}
		
		/// <summary> Completes a nested filter and checks for the valid filter type.</summary>
		/// <param name="rfcType"> Type of filter to complete.
		/// @throws Novell.Directory.Ldap.LdapLocalException  Occurs when the specified
		/// type differs from the current filter component.
		/// </param>
		public virtual void  endNestedFilter(int rfcType)
		{
			if (rfcType == NOT)
			{
				//if this is a Not than Not should be the second thing on the stack
				filterStack.Pop();
			}
			int topOfStackType = ((Asn1Object) filterStack.Peek()).getIdentifier().Tag;
			if (topOfStackType != rfcType)
			{
				throw new LdapLocalException("Missmatched ending of nested filter", LdapException.FILTER_ERROR);
			}
			filterStack.Pop();
			return ;
		}
		
		/// <summary> Creates an iterator over the preparsed segments of a filter.
		/// 
		/// The first object returned by an iterator is an integer indicating the
		/// type of filter components.  Subseqence values are returned.  If a
		/// component is of type 'AND' or 'OR' or 'NOT' then the value
		/// returned is another iterator.  This iterator is used by toString.
		/// 
		/// </summary>
		/// <returns> Iterator over filter segments
		/// </returns>
		public virtual System.Collections.IEnumerator getFilterIterator()
		{
			return new FilterIterator(this, (Asn1Tagged) this.choiceValue());
		}
		
		/// <summary> Creates and returns a String representation of this filter.</summary>
		public virtual System.String filterToString()
		{
			System.Text.StringBuilder filter = new System.Text.StringBuilder();
			stringFilter(this.getFilterIterator(), filter);
			return filter.ToString();
		}
		
		/// <summary> Uses a filterIterator to create a string representation of a filter.
		/// 
		/// </summary>
		/// <param name="itr">Iterator of filter components
		/// </param>
		/// <param name="filter">Buffer to place a string representation of the filter
		/// </param>
		/// <seealso cref="FilterIterator">
		/// </seealso>
		private static void  stringFilter(System.Collections.IEnumerator itr, System.Text.StringBuilder filter)
		{
			int op = - 1;
			filter.Append('(');
			while (itr.MoveNext())
			{
				System.Object filterpart = itr.Current;
				if (filterpart is System.Int32)
				{
					op = ((System.Int32) filterpart);
					switch (op)
					{
						
						case AND: 
							filter.Append('&');
							break;
						
						case OR: 
							filter.Append('|');
							break;
						
						case NOT: 
							filter.Append('!');
							break;
						
						case EQUALITY_MATCH:  {
								filter.Append((System.String) itr.Current);
								filter.Append('=');
								sbyte[] value_Renamed = (sbyte[]) itr.Current;
								filter.Append(byteString(value_Renamed));
								break;
							}
						
						case GREATER_OR_EQUAL:  {
								filter.Append((System.String) itr.Current);
								filter.Append(">=");
								sbyte[] value_Renamed = (sbyte[]) itr.Current;
								filter.Append(byteString(value_Renamed));
								break;
							}
						
						case LESS_OR_EQUAL:  {
								filter.Append((System.String) itr.Current);
								filter.Append("<=");
								sbyte[] value_Renamed = (sbyte[]) itr.Current;
								filter.Append(byteString(value_Renamed));
								break;
							}
						
						case PRESENT: 
							filter.Append((System.String) itr.Current);
							filter.Append("=*");
							break;
						
						case APPROX_MATCH: 
							filter.Append((System.String) itr.Current);
							filter.Append("~=");
							sbyte[] value_Renamed2 = (sbyte[]) itr.Current;
							filter.Append(byteString(value_Renamed2));
							break;
						
						case EXTENSIBLE_MATCH: 
							System.String oid = (System.String) itr.Current;
							
							filter.Append((System.String) itr.Current);
							filter.Append(':');
							filter.Append(oid);
							filter.Append(":=");
							filter.Append((System.String) itr.Current);
							break;
						
						case SUBSTRINGS:  {
								filter.Append((System.String) itr.Current);
								filter.Append('=');
								bool noStarLast = false;
								while (itr.MoveNext())
								{
									op = ((System.Int32) itr.Current);
									switch (op)
									{
										
										case INITIAL: 
											filter.Append((System.String) itr.Current);
											filter.Append('*');
											noStarLast = false;
											break;
										
										case ANY: 
											if (noStarLast)
												filter.Append('*');
											filter.Append((System.String) itr.Current);
											filter.Append('*');
											noStarLast = false;
											break;
										
										case FINAL: 
											if (noStarLast)
												filter.Append('*');
											filter.Append((System.String) itr.Current);
											break;
										}
								}
								break;
							}
						}
				}
				else if (filterpart is System.Collections.IEnumerator)
				{
					stringFilter((System.Collections.IEnumerator) filterpart, filter);
				}
			}
			filter.Append(')');
		}
		
		/// <summary> Convert a UTF8 encoded string, or binary data, into a String encoded for
		/// a string filter.
		/// </summary>
		private static System.String byteString(sbyte[] value_Renamed)
		{
			System.String toReturn = null;
			if (Novell.Directory.Ldap.Utilclass.Base64.isValidUTF8(value_Renamed, true))
			{
				try
				{
					System.Text.Encoding encoder = System.Text.Encoding.GetEncoding("utf-8"); 
					char[] dchar = encoder.GetChars(SupportClass.ToByteArray(value_Renamed));
					toReturn = new String(dchar);

//					toReturn = new String(value_Renamed, "UTF-8");
				}
				catch (System.IO.IOException e)
				{
					throw new System.SystemException("Default JVM does not support UTF-8 encoding" + e);
				}
			}
			else
			{
				System.Text.StringBuilder binary = new System.Text.StringBuilder();
				for (int i = 0; i < value_Renamed.Length; i++)
				{
					//TODO repair binary output
					//Every octet needs to be escaped
					if (value_Renamed[i] >= 0)
					{
						//one character hex string
						binary.Append("\\0");
						binary.Append(System.Convert.ToString(value_Renamed[i], 16));
					}
					else
					{
						//negative (eight character) hex string
						binary.Append("\\" + System.Convert.ToString(value_Renamed[i], 16).Substring(6));
					}
				}
				toReturn = binary.ToString();
			}
			return toReturn;
		}
		
		/// <summary> This inner class wrappers the Search Filter with an iterator.
		/// This iterator will give access to all the individual components
		/// preparsed.  The first call to next will return an Integer identifying
		/// the type of filter component.  Then the component values will be returned
		/// AND, NOT, and OR components values will be returned as Iterators.
		/// </summary>
		private class FilterIterator : System.Collections.IEnumerator
		{
			public void Reset(){}
			private void  InitBlock(RfcFilter enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private RfcFilter enclosingInstance;
			/// <summary> Returns filter identifiers and components of a filter.
			/// 
			/// The first object returned is an Integer identifying
			/// its type.
			/// </summary>
			public virtual System.Object Current
			{
				get
				{
					System.Object toReturn = null;
					if (!tagReturned)
					{
						tagReturned = true;
						toReturn = root.getIdentifier().Tag;
					}
					else
					{
						Asn1Object asn1 = root.taggedValue();
						
						if (asn1 is RfcLdapString)
						{
							//one value to iterate
							hasMore = false;
							toReturn = ((RfcLdapString) asn1).stringValue();
						}
						else if (asn1 is RfcSubstringFilter)
						{
							
							RfcSubstringFilter sub = (RfcSubstringFilter) asn1;
							if (index == - 1)
							{
								//return attribute name
								index = 0;
								RfcAttributeDescription attr = (RfcAttributeDescription) sub.get_Renamed(0);
								toReturn = attr.stringValue();
							}
							else if (index % 2 == 0)
							{
								//return substring identifier
								Asn1SequenceOf substrs = (Asn1SequenceOf) sub.get_Renamed(1);
								toReturn = ((Asn1Tagged) substrs.get_Renamed(index / 2)).getIdentifier().Tag;
								index++;
							}
							else
							{
								//return substring value
								Asn1SequenceOf substrs = (Asn1SequenceOf) sub.get_Renamed(1);
								Asn1Tagged tag = (Asn1Tagged) substrs.get_Renamed(index / 2);
								RfcLdapString value_Renamed = (RfcLdapString) tag.taggedValue();
								toReturn = value_Renamed.stringValue();
								index++;
							}
							if (index / 2 >= ((Asn1SequenceOf) sub.get_Renamed(1)).size())
							{
								hasMore = false;
							}
						}
						else if (asn1 is RfcAttributeValueAssertion)
						{
							// components: =,>=,<=,~=
							RfcAttributeValueAssertion assertion = (RfcAttributeValueAssertion) asn1;
							
							if (index == - 1)
							{
								toReturn = assertion.AttributeDescription;
								index = 1;
							}
							else if (index == 1)
							{
								toReturn = assertion.AssertionValue;
								index = 2;
								hasMore = false;
							}
						}
						else if (asn1 is RfcMatchingRuleAssertion)
						{
							//Extensible match
							RfcMatchingRuleAssertion exMatch = (RfcMatchingRuleAssertion) asn1;
							if (index == - 1)
							{
								index = 0;
							}
							toReturn = ((Asn1OctetString) ((Asn1Tagged) exMatch.get_Renamed(index++)).taggedValue()).stringValue();
							if (index > 2)
							{
								hasMore = false;
							}
						}
						else if (asn1 is Asn1SetOf)
						{
							//AND and OR nested components
							Asn1SetOf set_Renamed = (Asn1SetOf) asn1;
							if (index == - 1)
							{
								index = 0;
							}
							toReturn = new FilterIterator(enclosingInstance,(Asn1Tagged) set_Renamed.get_Renamed(index++));
							if (index >= set_Renamed.size())
							{
								this.hasMore = false;
							}
						}
						else if (asn1 is Asn1Tagged)
						{
							//NOT nested component.
							toReturn = new FilterIterator(enclosingInstance,(Asn1Tagged) asn1);
							this.hasMore = false;
						}
					}
					return toReturn;
				}
				
			}
			public RfcFilter Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			internal Asn1Tagged root;
			/// <summary>indicates if the identifier for a component has been returned yet </summary>
			internal bool tagReturned = false;
			/// <summary>indexes the several parts a component may have </summary>
			internal int index = - 1;
			private bool hasMore = true;
			
			public FilterIterator(RfcFilter enclosingInstance, Asn1Tagged root)
			{
				InitBlock(enclosingInstance);
				this.root = root;
			}
			public virtual bool MoveNext()
			{
				return hasMore;
			}
			
			public void  remove()
			{
				throw new System.NotSupportedException("Remove is not supported on a filter iterator");
			}
		}
		
		/// <summary> This inner class will tokenize the components of an RFC 2254 search filter.</summary>
		internal class FilterTokenizer
		{
			private void  InitBlock(RfcFilter enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private RfcFilter enclosingInstance;
			/// <summary> Reads either an operator, or an attribute, whichever is
			/// next in the filter string.
			/// 
			/// 
			/// If the next component is an attribute, it is read and stored in the
			/// attr field of this class which may be retrieved with getAttr()
			/// and a -1 is returned. Otherwise, the int value of the operator read is
			/// returned.
			/// </summary>
			virtual public int OpOrAttr
			{
				get
				{
					int index;
					
					if (offset >= filterLength)
					{
						//"Unexpected end of filter",
						throw new LdapLocalException(ExceptionMessages.UNEXPECTED_END, LdapException.FILTER_ERROR);
					}
					int ret;
					int testChar = filter[offset];
					if (testChar == '&')
					{
						offset++;
						ret = Novell.Directory.Ldap.Rfc2251.RfcFilter.AND;
					}
					else if (testChar == '|')
					{
						offset++;
						ret = Novell.Directory.Ldap.Rfc2251.RfcFilter.OR;
					}
					else if (testChar == '!')
					{
						offset++;
						ret = Novell.Directory.Ldap.Rfc2251.RfcFilter.NOT;
					}
					else
					{
						if (filter.Substring(offset).StartsWith(":=") == true)
						{
							throw new LdapLocalException(ExceptionMessages.NO_MATCHING_RULE, LdapException.FILTER_ERROR);
						}
						
						if (filter.Substring(offset).StartsWith("::=") == true || filter.Substring(offset).StartsWith(":::=") == true)
						{
							throw new LdapLocalException(ExceptionMessages.NO_DN_NOR_MATCHING_RULE, LdapException.FILTER_ERROR);
						}
						
						
						// get first component of 'item' (attr or :dn or :matchingrule)
						System.String delims = "=~<>()";
						System.Text.StringBuilder sb = new System.Text.StringBuilder();
						
						while (delims.IndexOf((System.Char) filter[offset]) == - 1 && filter.Substring(offset).StartsWith(":=") == false)
						{
							sb.Append(filter[offset++]);
						}
						
						attr = sb.ToString().Trim();
						
						// is there an attribute name specified in the filter ?
						if (attr.Length == 0 || attr[0] == ';')
						{
							throw new LdapLocalException(ExceptionMessages.NO_ATTRIBUTE_NAME, LdapException.FILTER_ERROR);
						}
						
						for (index = 0; index < attr.Length; index++)
						{
							char atIndex = attr[index];
							if (!(System.Char.IsLetterOrDigit(atIndex) || atIndex == '-' || atIndex == '.' || atIndex == ';' || atIndex == ':'))
							{
								
								if (atIndex == '\\')
								{
									throw new LdapLocalException(ExceptionMessages.INVALID_ESC_IN_DESCR, LdapException.FILTER_ERROR);
								}
								else
								{
									throw new LdapLocalException(ExceptionMessages.INVALID_CHAR_IN_DESCR, new System.Object[]{atIndex}, LdapException.FILTER_ERROR);
								}
							}
						}
						
						// is there an option specified in the filter ?
						index = attr.IndexOf((System.Char) ';');
						if (index != - 1 && index == attr.Length - 1)
						{
							throw new LdapLocalException(ExceptionMessages.NO_OPTION, LdapException.FILTER_ERROR);
						}
						ret = - 1;
					}
					return ret;
				}
				
			}
			/// <summary> Reads an RFC 2251 filter type from the filter string and returns its
			/// int value.
			/// </summary>
			virtual public int FilterType
			{
				get
				{
					if (offset >= filterLength)
					{
						//"Unexpected end of filter",
						throw new LdapLocalException(ExceptionMessages.UNEXPECTED_END, LdapException.FILTER_ERROR);
					}
					int ret;
					if (filter.Substring(offset).StartsWith(">="))
					{
						offset += 2;
						ret = Novell.Directory.Ldap.Rfc2251.RfcFilter.GREATER_OR_EQUAL;
					}
					else if (filter.Substring(offset).StartsWith("<="))
					{
						offset += 2;
						ret = Novell.Directory.Ldap.Rfc2251.RfcFilter.LESS_OR_EQUAL;
					}
					else if (filter.Substring(offset).StartsWith("~="))
					{
						offset += 2;
						ret = Novell.Directory.Ldap.Rfc2251.RfcFilter.APPROX_MATCH;
					}
					else if (filter.Substring(offset).StartsWith(":="))
					{
						offset += 2;
						ret = Novell.Directory.Ldap.Rfc2251.RfcFilter.EXTENSIBLE_MATCH;
					}
					else if (filter[offset] == '=')
					{
						offset++;
						ret = Novell.Directory.Ldap.Rfc2251.RfcFilter.EQUALITY_MATCH;
					}
					else
					{
						//"Invalid comparison operator",
						throw new LdapLocalException(ExceptionMessages.INVALID_FILTER_COMPARISON, LdapException.FILTER_ERROR);
					}
					return ret;
				}
				
			}
			/// <summary> Reads a value from a filter string.</summary>
			virtual public System.String Value
			{
				get
				{
					if (offset >= filterLength)
					{
						//"Unexpected end of filter",
						throw new LdapLocalException(ExceptionMessages.UNEXPECTED_END, LdapException.FILTER_ERROR);
					}
					
					int idx = filter.IndexOf((System.Char) ')', offset);
					if (idx == - 1)
					{
						idx = filterLength;
					}
					System.String ret = filter.Substring(offset, (idx) - (offset));
					offset = idx;
					
					return ret;
				}
				
			}
			/// <summary> Returns the current attribute identifier.</summary>
			virtual public System.String Attr
			{
				get
				{
					return attr;
				}
				
			}
			public RfcFilter Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			
			//*************************************************************************
			// Private variables
			//*************************************************************************
			
			private System.String filter; // The filter string to parse
			private System.String attr; // Name of the attribute just parsed
			private int offset; // Offset pointer into the filter string
			private int filterLength; // Length of the filter string to parse
			
			//*************************************************************************
			// Constructor
			//*************************************************************************
			
			/// <summary> Constructs a FilterTokenizer for a filter.</summary>
			public FilterTokenizer(RfcFilter enclosingInstance, System.String filter)
			{
				InitBlock(enclosingInstance);
				this.filter = filter;
				this.offset = 0;
				this.filterLength = filter.Length;
				return ;
			}
			
			//*************************************************************************
			// Tokenizer methods
			//*************************************************************************
			
			/// <summary> Reads the current char and throws an Exception if it is not a left
			/// parenthesis.
			/// </summary>
			public void  getLeftParen()
			{
				if (offset >= filterLength)
				{
					//"Unexpected end of filter",
					throw new LdapLocalException(ExceptionMessages.UNEXPECTED_END, LdapException.FILTER_ERROR);
				}
				if (filter[offset++] != '(')
				{
					//"Missing left paren",
					throw new LdapLocalException(ExceptionMessages.EXPECTING_LEFT_PAREN, new System.Object[]{filter[offset -= 1]}, LdapException.FILTER_ERROR);
				}
				return ;
			}
			
			/// <summary> Reads the current char and throws an Exception if it is not a right
			/// parenthesis.
			/// </summary>
			public void  getRightParen()
			{
				if (offset >= filterLength)
				{
					//"Unexpected end of filter",
					throw new LdapLocalException(ExceptionMessages.UNEXPECTED_END, LdapException.FILTER_ERROR);
				}
				if (filter[offset++] != ')')
				{
					//"Missing right paren",
					throw new LdapLocalException(ExceptionMessages.EXPECTING_RIGHT_PAREN, new System.Object[]{filter[offset - 1]}, LdapException.FILTER_ERROR);
				}
				return ;
			}
			
			/// <summary> Return the current char without advancing the offset pointer. This is
			/// used by ParseFilterList when determining if there are any more
			/// Filters in the list.
			/// </summary>
			public char peekChar()
			{
				if (offset >= filterLength)
				{
					//"Unexpected end of filter",
					throw new LdapLocalException(ExceptionMessages.UNEXPECTED_END, LdapException.FILTER_ERROR);
				}
				return filter[offset];
			}
		}
	}
}
