/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2003, 2005 Abel Eduardo Pereira
 *  All Rights Reserved.
 */

using System;
using System.IO;
using System.Text;
using System.Collections.Specialized;

namespace FirebirdSql.Data.Firebird.Isql
{
	/// <summary>
	/// FbScript parses a SQL file and returns its SQL statements. 
	/// The class take in consideration that the statement separator can change in code. 
	/// For instance, in Firebird databases the statement <c>SET TERM !! ;</c> will change the
	/// statement token terminator <b>;</b> into <b>!!</b>.
	/// </summary>
	public class FbScript
	{
		#region Fields

		private StringParser		parser;
		private StringCollection	results;

		#endregion

		#region Properties

		/// <summary>
		/// Returns a StringCollection containing all the SQL statements (without comments) present on the file.
		/// This property is loaded after the method call <see cref="Parse"/>.
		/// </summary>
		public StringCollection Results
		{
			get { return this.results; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Creates an instance of FbScript class.
		/// </summary>
		/// <param name="sqlFilename">The filename for the SQL file.</param>
		public FbScript(string sqlFilename)
		{
			string			script = "";
			StreamReader	reader = null;

			try
			{
				reader = File.OpenText(sqlFilename);
				script = reader.ReadToEnd();
			}
			catch
			{
				throw;
			}
			finally
			{
				if (reader != null)
				{
					reader.Close();
				}
			}

			this.results		= new StringCollection();
			this.parser			= new StringParser(RemoveComments(script), false);
			this.parser.Token	= ";";
		}

		/// <summary>
		/// Creates an instance of FbScript class.
		/// </summary>
		/// <param name="sqlCode">A <see cref="TextReader"/> instance.</param>
		/// <remarks>The all data in <see cref="TextReader"/> is read.</remarks>
		public FbScript(TextReader sqlCode)
		{
			this.results		= new StringCollection();
			this.parser			= new StringParser(RemoveComments(sqlCode.ReadToEnd()), false);
			this.parser.Token	= ";";
		}

		#endregion

		#region Methods

		/// <summary>
		/// Parses the SQL code and loads the SQL statements into the StringCollection <see cref="Results"/>.
		/// </summary>
		/// <returns>The number of statements found.</returns>
		public int Parse()
		{
			int index = 0;
			string	atomicResult;
			string	newParserToken;

			this.results.Clear();

			while (index < this.parser.Length)
			{
				index = this.parser.ParseNext();
				atomicResult = this.parser.Result.Trim();

				if (this.IsSetTermStatement(atomicResult, out newParserToken))
				{
					this.parser.Token = newParserToken;
					continue;
				}

				if (atomicResult != null && atomicResult.Length > 0)
				{
					this.results.Add(atomicResult);
				}
			}

			return this.results.Count;
		}

		/// <summary>
		/// Overrided method, returns the the SQL code to be parsed (with comments removed).
		/// </summary>
		/// <returns>The SQL code to be parsed (without comments).</returns>
		public override string ToString()
		{
			return this.parser.ToString();
		}

		#endregion

		#region Protected Static Methods

		/// <summary>
		/// Removes from the SQL code all comments of the type: /*...*/ or --
		/// </summary>
		/// <param name="source">The string containing the original SQL code.</param>
		/// <returns>A string containing the SQL code without comments.</returns>
		protected static string RemoveComments(string source)
		{
			StringBuilder	result			= new StringBuilder();
			int				i				= 0;
			int				length			= source.Length;
			bool			insideComment	= false;
			bool			insideString	= false;

			while (i < length)
			{
				if (insideComment)
				{
					if (source[i] == '*')
					{
						if ((i < length - 1) && (source[i + 1] == '/'))
						{
							i++;
							insideComment = false;
						}
					}
				}
				else
				{
					if (source[i] == '\'' || source[i] == '/' || (source[i] == '-' && ((i < length - 1) && source[i + 1] == '-')))
					{
						switch (source[i])
						{
							case '\'':
								if (!insideString && !insideComment)
								{
									insideString = true;
								}
								else
								{
									if (insideString)
									{
										insideString = false;
									}
								}
								result.Append(source[i]);
								break;

							case '/':
                                if (!insideString && (i < length - 1) && (source[i + 1] == '*'))
                                {
                                    i++;
                                    insideComment = true;
                                }
                                else
                                {
                                    if ((source[i + 1] == '*'))
                                    {
                                        Console.WriteLine("");
                                    }
                                    result.Append(source[i]);
                                }
								break;

							case '-':
								if (!insideString && (i < length - 1) && (source[i + 1] == '-'))
								{
									i++;
									while (i < length && source[i] != '\n')
									{
										i++;
									}
									i--;
								}
								break;
						}
					}
					else
					{
						result.Append(source[i]);
					}
				}

				i++;
			}

			return result.ToString();
		}

		#endregion

		#region Private Methods

		// method assumes that statement is trimmed 
		private bool IsSetTermStatement(string statement, out string newTerm)
		{
			bool result = false;

			newTerm = "";
			if (StringParser.StartsWith(statement, "SET TERM", true))
			{
				newTerm = statement.Substring(8).Trim();
				result = true;
			}

			return result;
		}

		#endregion
	}
}