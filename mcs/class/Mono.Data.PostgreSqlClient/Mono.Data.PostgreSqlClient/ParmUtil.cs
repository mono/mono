//
// ParmUtil.cs - utility to bind variables in a SQL statement to parameters in C# code
//               This is in the PostgreSQL .NET Data provider in Mono
//
// Author: 
//    Daniel Morgan <danmorg@sc.rr.com>
//
// (c)copyright 2002 Daniel Morgan
//

// comment DEBUG_ParmUtil for production, for debug messages, uncomment
//#define DEBUG_ParmUtil

using System;
using System.Data;
using System.Text;

namespace Mono.Data.PostgreSqlClient {

	enum PostgresBindVariableCharacter {
		Semicolon,
		At,
		QuestionMark
	}

	public class ParmUtil {

		private string sql = "";
		private string resultSql = "";
		private PgSqlParameterCollection parmsCollection = null;
		
		static private PostgresBindVariableCharacter PgbindChar = PostgresBindVariableCharacter.Semicolon;
		static char bindChar;

		// static constructor
		static ParmUtil() {
			switch(PgbindChar) {
			case PostgresBindVariableCharacter.Semicolon:
				bindChar = ':';
				break;
			case PostgresBindVariableCharacter.At:
				bindChar = '@';
				break;
			case PostgresBindVariableCharacter.QuestionMark:
				// this doesn't have named parameters,
				// they must be in order
				bindChar = '?';
				break;
			}
		}
				
		public ParmUtil(string query, PgSqlParameterCollection parms) {
			sql = query;
			parmsCollection = parms;
		}
		
		public string ResultSql {
			get {
				return resultSql;
			}
		}

		// TODO: currently only works for input variables,
		//       need to do input/output, output, and return
		public string ReplaceWithParms() {

			StringBuilder result = new StringBuilder();
			char[] chars = sql.ToCharArray();
			bool bStringConstFound = false;

			for(int i = 0; i < chars.Length; i++) {
				if(chars[i] == '\'') {
					if(bStringConstFound == true)
						bStringConstFound = false;
					else
						bStringConstFound = true;

					result.Append(chars[i]);
				}
				else if(chars[i] == bindChar && 
					bStringConstFound == false) {
#if DEBUG_ParmUtil
					Console.WriteLine("Bind Variable character found...");
#endif					
					StringBuilder parm = new StringBuilder();
					i++;
					while(i <= chars.Length) {
						char ch;
						if(i == chars.Length)
							ch = ' '; // a space
						else
							ch = chars[i];

#if DEBUG_ParmUtil						
						Console.WriteLine("Is char Letter or digit?");
#endif						
						if(Char.IsLetterOrDigit(ch)) {
#if DEBUG_ParmUtil
							Console.WriteLine("Char IS letter or digit. " + 
								"Now, append char to parm StringBuilder");
#endif
							parm.Append(ch);
						}
						else {
#if DEBUG_ParmUtil
                                                        Console.WriteLine("Char is NOT letter or char. " + 
								"thus we got rest of bind variable name. ");
								
							// replace bind variable placeholder 
							// with data value constant
							Console.WriteLine("parm StringBuilder to string p...");
#endif
							string p = parm.ToString();
#if DEBUG_ParmUtil
							Console.WriteLine("calling BindReplace...");
#endif							
							bool found = BindReplace(result, p);
#if DEBUG_ParmUtil
							Console.WriteLine("    Found = " + found);
#endif
							if(found == true)
								break;
							else {						
								// *** Error Handling
								Console.WriteLine("Error: parameter not found: " + p);
								return "";
							}
						}
						i++;
					}
					i--;
				}
				else 
					result.Append(chars[i]);
			}
			
			resultSql = result.ToString();
			return resultSql;
		}

		public bool BindReplace (StringBuilder result, string p) {
			// bind variable
			bool found = false;

#if DEBUG_ParmUtil
			Console.WriteLine("Does the parmsCollection contain the parameter???: " + p);
#endif
			if(parmsCollection.Contains(p) == true) {
				// parameter found
#if DEBUG_ParmUtil
				Console.WriteLine("Parameter Found: " + p);
#endif
				PgSqlParameter prm = parmsCollection[p];

#if DEBUG_ParmUtil																	
				// DEBUG 
				Console.WriteLine("          Value: " + prm.Value);
				Console.WriteLine("      Direction: " + prm.Direction);
#endif
				// convert object to string and place
				// into SQL
				if(prm.Direction == ParameterDirection.Input) {
					string strObj = PostgresHelper.
						ObjectToString(prm.DbType, 
								prm.Value);
					result.Append(strObj);
				}
				else
					result.Append(bindChar + p);

				found = true;
			}
			return found;
		}
	}
}
