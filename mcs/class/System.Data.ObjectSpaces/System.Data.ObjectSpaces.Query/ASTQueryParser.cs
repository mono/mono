//
// System.Data.ObjectSpaces.Query.ASTQueryParser
//
//
// Author:
//	Richard Thombs (stony@stony.org)
//

#if NET_1_2

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
