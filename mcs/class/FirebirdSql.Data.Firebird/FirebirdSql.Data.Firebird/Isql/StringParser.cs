/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2003, 2005 Abel Eduardo Pereira
 *	All Rights Reserved.
 */

using System;
using System.Globalization;

namespace FirebirdSql.Data.Firebird.Isql
{
	/// <summary>
	/// StringParser parses	a string returnning	the	(sub)strings between tokens.
	/// </summary>
	/// <example>
	/// An example of how to use this class.
	/// <code>
	/// [STAThread]
	/// static void	Main(string[] args)	{
	/// 	int	currentIndex = 0;
	/// 	string s = ".NET Framework doesn't have a string parsing class?!";
	/// 	StringParser parser = new StringParser(s, false);			
	/// 	while (currentIndex	&lt; s.Length) {
	/// 		Console.WriteLine("Returned Index: {0}", currentIndex = parser.ParseNext());
	/// 		Console.WriteLine("Chars scanned: {0}",	parser.CharsParsed);
	/// 		Console.WriteLine("Parsing result: {0}\n", parser.Result);
	/// 	}
	/// }
	/// </code>
	/// <para>The output:</para>
	/// <code>
	/// Returned Index:	5
	/// Chars scanned: 5
	/// Parsing	result:	.NET
	///
	/// Returned Index:	15
	/// Chars scanned: 10
	/// Parsing	result:	Framework
	///
	/// Returned Index:	23
	/// Chars scanned: 8
	/// Parsing	result:	doesn't
	///
	/// Returned Index:	28
	/// Chars scanned: 5
	/// Parsing	result:	have
	///
	/// Returned Index:	30
	/// Chars scanned: 2
	/// Parsing	result:	a
	///
	/// Returned Index:	37
	/// Chars scanned: 7
	/// Parsing	result:	string
	///
	/// Returned Index:	45
	/// Chars scanned: 8
	/// Parsing	result:	parsing
	///
	/// Returned Index:	52
	/// Chars scanned: 7
	/// Parsing	result:	class?!
	/// </code>
	/// </example>
	public class StringParser 
	{
		#region Fields

		private	int		charsParsed;
		private	string	source;
		private	string	token;
		private	int		currentIndex;
		private	bool	caseSensitive;
		private	int		sourceLength;
		private	string	result;
		private	int		tokenLength;
		
		#endregion

		#region Properties

		/// <summary>
		/// Loaded after a parsing operation with the number of	chars parsed.
		/// </summary>
		public int CharsParsed 
		{
			get	{ return this.charsParsed; }
		}

		/// <summary>
		/// Loaded after a parsing operation with the string that was found	between	tokens.
		/// </summary>
		public string Result 
		{
			get	{ return this.result; }
		}

		/// <summary>
		/// The	string separator. The default value	is a white space: 0x32 ASCII code.
		/// </summary>
		public string Token	
		{
			get	{ return this.token; }
			set
			{
				if (value == null || value.Length == 0)	
				{
					throw new Exception("Token is empty!");
				}
				this.token = value;
				this.tokenLength = this.token.Length;
			}
		}

		/// <summary>
		/// Returns	the	length of the string that is being parsed.
		/// </summary>
		public int Length 
		{
			get	 { return this.sourceLength; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Creates	an instance	of StringParser.
		/// </summary>
		/// <param name="caseSensitive">Indicates if parser	system should be case-sensitive	(true) or case-intensitive (false).</param>
		public StringParser(bool caseSensitive)	
		{
			this.caseSensitive = caseSensitive;
			this.token = " ";
			this.tokenLength = this.token.Length;
		}

		/// <summary>
		/// Creates	an instance	of StringParser.
		/// </summary>
		/// <param name="targetString">Indicates if	parser system should be	case-sensitive (true) or case-intensitive (false).</param>
		/// <param name="caseSensitive">The	string to parse.</param>
		/// <remarks>By	defining the string	(to	parse) in constructor you can call directly	the	method <see	cref="ParseNext"/>
		/// without	having to initializate the target string on	<see cref="Parse(System.String)"/> method. See the example for further details.
		/// </remarks>
		public StringParser(string targetString, bool caseSensitive) 
		{
			this.caseSensitive = caseSensitive;
			this.token = " ";
			this.tokenLength = this.token.Length;
			this.source		 = targetString;
			this.sourceLength = targetString.Length;
		}

		#endregion

		#region Methods
		
		/// <summary>
		/// Parses target string attempting	to determine the (sub)string between the beginning of this string and the <see cref="Token"/>.
		/// After the parse is complete system will load into <see cref="CharsParsed"/> then number of chars scanned and into <see cref="Result"/>
		/// the	string that	was	found.
		/// </summary>
		/// <param name="targetString">The string to be	parsed.</param>
		/// <returns>The index of the char next	char after the <see	cref="Token"/> end.</returns>
		/// <remarks>If	nothing	is parsed the method will return -1. Case the <see cref="Token"/> wasn't found until the end of	the	string the method retuns 
		/// (in	<see cref="Result"/>) the string found between the starting	index and the end of the string. </remarks>
		public int Parse(string	targetString) 
		{
			return Parse(targetString, 0);
		}

		/// <summary>
		/// Parses target string attempting	to determine the (sub)string between the index <b>start</b>	of this	string and the <see	cref="Token"/>.
		 /// After the parse is complete system will load into <see cref="CharsParsed"/> then number of chars scanned and into <see cref="Result"/>
		/// the	string that	was	found.
		/// </summary>
		/// <param name="targetString">The string to be	parsed.</param>
		/// <param name="start">The	start index	for	parsing	purposes.</param>
		/// <returns>The index of the char next	char after the <c>Token</c>	end.</returns>
		/// <remarks>If	nothing	is parsed the method will return -1. Case the <see cref="Token"/> wasn't found until the end of	the	string the method returns 
		/// (in	<see cref="Result"/>) the string found between the starting	index and the end of the string. </remarks>
		public int Parse(string	targetString, int start) 
		{
			this.sourceLength = targetString.Length;
			
			if (start >= this.sourceLength)	
			{
				throw new Exception("Cannot start parsing after the end of the string.");
			}
			
			this.source = targetString;
			
			int	i = start;
			while (i < this.sourceLength) 
			{
				if (string.Compare(this.source[i].ToString(), this.token[0].ToString(),	!this.caseSensitive, CultureInfo.CurrentCulture) == 0) 
				{
					if (string.Compare(this.source.Substring(i,	this.tokenLength), this.token, !this.caseSensitive,	CultureInfo.CurrentCulture) == 0)	
					{
						i += this.tokenLength; 
						break;
					}
				}
				i++;
			}
			
			this.currentIndex = i;
			if (this.currentIndex != this.sourceLength)	
			{
				this.charsParsed = this.currentIndex - start;
				this.result = this.source.Substring(start, this.currentIndex - start - this.tokenLength);
			}
			else 
			{
				this.charsParsed = this.currentIndex - start;
				this.result = this.source.Substring(start);
			}

			return this.currentIndex;
		}

		/// <summary>
		/// <para>Repeats the parsing starting on the index	returned by <see cref="Parse(System.String)"/> method.</para>
		/// You	can	also call <b>ParseNext</b> directly	(without calling <see cref="Parse(System.String)"/>) if	you	define the text	to be parsed at	instance construction.	
		/// </summary>
		/// <returns>The index of the char next	char after the <see	cref="Token"/> end.</returns>
		/// <remarks>If	nothing	is parsed the method will return -1. Case the <see cref="Token"/> wasn't found until the end of	the	string the method returns 
		/// (in	<see cref="Result"/>) the string found between the starting	index and the end of the string.</remarks>
		public int ParseNext() 
		{
			if (this.currentIndex >= this.sourceLength)	
			{
				throw new Exception("Cannot start parsing after the end of the string.");
			}
			
			int	i = this.currentIndex;
			while (i < this.sourceLength) 
			{
				if (string.Compare(this.source[i].ToString(), this.token[0].ToString(),	!this.caseSensitive, CultureInfo.CurrentCulture) == 0) 
				{
					if (string.Compare(this.source.Substring(i,	this.tokenLength), this.token, !this.caseSensitive,	CultureInfo.CurrentCulture) == 0)	
					{
						i += this.tokenLength; 
						break;
					}
				}
				i++;
			}
			
			if (i != this.sourceLength)	
			{
				this.charsParsed = i - this.currentIndex;
				this.result = this.source.Substring(this.currentIndex, i - this.currentIndex - this.tokenLength);
			}
			else 
			{
				this.charsParsed = i - this.currentIndex;
				this.result = this.source.Substring(this.currentIndex);
			}
			
			return this.currentIndex = i;
		}	
		
		/// <summary>
		/// Returns	the	index of the substring in the string. If the substring does	not	exists the method returns <b>-1</b>.
		/// </summary>
		/// <param name="substring">The	string to be located.</param>
		/// <returns>The index of the substring	or -1 if the string	does not exists	within the source string.
		/// If the the substring is	empty method returns 0.</returns>
		/// <remarks>The instance parses for the substring in a	case sensitive or intensive	way, as you	specify	at 
		/// class construction.</remarks>
		public int IndexOf(string substring) 
		{
			return IndexOf(substring, 0);
		}

		/// <summary>
		/// Returns	the	index of the substring in the string starting on index <b>startIndex</b>. 
		/// If the substring does not exists the method	returns	<b>-1</b>.
		/// </summary>
		/// <param name="substring">The	string to be located.</param>
		/// <param name="startIndex">The start index of	the	source string where	parser will	start.</param>
		/// <returns>The index of the substring	or -1 if the string	does not exists	within the source string.
		/// If the the substring is	empty method returns <i>startIndex</i>.</returns>
		/// <remarks>The instance parses for the substring in a	case sensitive or intensive	way, as you	specify	at 
		/// class construction.</remarks>
		public int IndexOf(string substring, int startIndex)
		{
			if (startIndex >= this.sourceLength)
			{
				throw new IndexOutOfRangeException("Start index out of bounds.");
			}

			if (substring == null || substring.Length == 0)
			{
				return startIndex;
			}

			int	i = startIndex;
			while (i < this.sourceLength) 
			{
				if (String.Compare(this.source[i].ToString(), substring[0].ToString(), !this.caseSensitive,	CultureInfo.CurrentCulture) == 0)
				{
					if (substring != null && substring.Length == 1)
					{
						return i;
					}

					int	j = i +	1;
					while ((j <	this.sourceLength) && ((j -	i) < substring.Length))
					{
						if (String.Compare(this.source[j].ToString(), substring[j -	i].ToString(), !this.caseSensitive,	CultureInfo.CurrentCulture) == 0)
						{
							j++;
						}
						else 
						{
							break;
						}
					}

					if ((j - i) == substring.Length)
					{
						return i;
					}
				}

				i++;
			}

			return -1;
		}

		/// <summary>
		/// Overrided method that returns the string to	be parsed.
		/// </summary>
		/// <returns>The string	to be parsed.</returns>
		public override	string ToString() 
		{
			return this.source;
		}

		#endregion

		#region Static Methods

		/// <summary>
		/// Indicates if the string	specified as <b>source</b> starts with the <b>token</b>	string.
		/// </summary>
		/// <param name="source">The source	string.</param>
		/// <param name="token">The	token that is intended to find.</param>
		/// <param name="ignoreCase">Indicated is char case	should be ignored.</param>
		/// <returns>Returns <b>true</b> if	the	<b>token</b> precedes the <b>source</b>.</returns>
		public static bool StartsWith(string source, string	token, bool	ignoreCase)	
		{
			if (source.Length <	token.Length)
			{
				return false;
			}

			return string.Compare(token, source.Substring(0, token.Length),	ignoreCase,	CultureInfo.CurrentCulture) == 0;
		}

		#endregion
	}
}