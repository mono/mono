//
// System.Data.ObjectSpaces.Query.ASTQueryParser
//
//
// Author:
//	Richard Thombs (stony@stony.org)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0

using System;

namespace System.Data.ObjectSpaces.Query
{
	[MonoTODO()]
	public class ASTQueryParser : ASTBaseParser
	{
		public static Int16 AND;
		public static Int16 ASCEND;
		public static Int16 CAST;
		public static Int16 CONST;
		public static Int16 DESCEND;
		public static Int16 FN_FILENAME;
		public static Int16 FN_FILEPATH;
		public static Int16 FN_ISNULL;
		public static Int16 FN_LEN;
		public static Int16 FN_SUBS;
		public static Int16 FN_TRIM;
		public static Int16 IDENT;
		public static Int16 NEG;
		public static Int16 NOT;
		public static Int16 OP_EQ;
		public static Int16 OP_GR;
		public static Int16 OP_GT;
		public static Int16 OP_IIF;
		public static Int16 OP_IN;
		public static Int16 OP_LE;
		public static Int16 OP_LIKE;
		public static Int16 OP_LT;
		public static Int16 OP_NE;
		public static Int16 OR;
		public static Int16 PARAM;
		public static Int16 PARENT;
		public static Int16 REL;
		public static Int16 YYERRCODE;

		[MonoTODO()]
		public ASTQueryParser() : base()
		{
		}

		// Create an internal parse tree from the query string.
		[MonoTODO()]
		public override void Parse(string query)
		{
		}
	}
}

#endif
