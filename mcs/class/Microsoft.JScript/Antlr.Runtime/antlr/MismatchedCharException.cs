using System;
using StringBuilder			= System.Text.StringBuilder;

using BitSet				= antlr.collections.impl.BitSet;

namespace antlr
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/RIGHTS.html
	*
	* $Id: MismatchedCharException.cs,v 1.1 2003/04/22 04:56:13 cesar Exp $
	*/

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//
	
	public class MismatchedCharException : RecognitionException
	{
		/*
		* Returns a clean error message (no line number/column information)
		*/
		override public string Message
		{
			get 
			{
				StringBuilder sb = new StringBuilder();
			
				switch (mismatchType)
				{
					case CharTypeEnum.CharType: 
						sb.Append("expecting ");   appendCharName(sb, expecting);
						sb.Append(", found ");     appendCharName(sb, foundChar);
						break;
				
					case CharTypeEnum.NotCharType: 
						sb.Append("expecting anything but '");
						appendCharName(sb, expecting);
						sb.Append("'; got it anyway");
						break;
				
					case CharTypeEnum.RangeType: 				
					case CharTypeEnum.NotRangeType: 
						sb.Append("expecting token ");
						if (mismatchType == CharTypeEnum.NotRangeType)
							sb.Append("NOT ");
						sb.Append("in range: ");
						appendCharName(sb, expecting);
						sb.Append("..");
						appendCharName(sb, upper);
						sb.Append(", found ");
						appendCharName(sb, foundChar);
						break;
				
					case CharTypeEnum.SetType: 
					case CharTypeEnum.NotSetType: 
						sb.Append("expecting " + (mismatchType == CharTypeEnum.NotSetType ? "NOT " : "") + "one of (");
						int[] elems = bset.toArray();
						for (int i = 0; i < elems.Length; i++) 
						{
							appendCharName(sb, elems[i]);
						}
						sb.Append("), found ");
						appendCharName(sb, foundChar);
						break;
				
					default: 
						sb.Append(base.Message);
						break;				
				}			
				return sb.ToString();
			}
		}

		// Types of chars

		public enum CharTypeEnum
		{
			CharType = 1,
			NotCharType = 2,
			RangeType = 3,
			NotRangeType = 4,
			SetType = 5,
			NotSetType = 6
		}
		
		// One of the above
		public CharTypeEnum mismatchType;
		
		// what was found on the input stream
		public int foundChar;
		
		// For CHAR/NOT_CHAR and RANGE/NOT_RANGE
		public int expecting;
		
		// For RANGE/NOT_RANGE (expecting is lower bound of range)
		public int upper;
		
		// For SET/NOT_SET
		public BitSet bset;
		
		// who knows...they may want to ask scanner questions
		public CharScanner scanner;
		
		/*
		* MismatchedCharException constructor comment.
		*/
		public MismatchedCharException() : base("Mismatched char")
		{
		}
		
		// Expected range / not range
		public MismatchedCharException(char c, char lower, char upper_, bool matchNot, CharScanner scanner_) : 
					base("Mismatched char", scanner_.getFilename(), scanner_.getLine(), scanner_.getColumn())
		{
			mismatchType = matchNot ? CharTypeEnum.NotRangeType : CharTypeEnum.RangeType;
			foundChar = c;
			expecting = lower;
			upper = upper_;
			scanner = scanner_;
		}
		
		// Expected token / not token
		public MismatchedCharException(char c, char expecting_, bool matchNot, CharScanner scanner_) : 
					base("Mismatched char", scanner_.getFilename(), scanner_.getLine(), scanner_.getColumn())
		{
			mismatchType = matchNot ? CharTypeEnum.NotCharType : CharTypeEnum.CharType;
			foundChar = c;
			expecting = expecting_;
			scanner = scanner_;
		}
		
		// Expected BitSet / not BitSet
		public MismatchedCharException(char c, BitSet set_, bool matchNot, CharScanner scanner_) :
					base("Mismatched char", scanner_.getFilename(), scanner_.getLine(), scanner_.getColumn())
		{
			mismatchType = matchNot ? CharTypeEnum.NotSetType : CharTypeEnum.SetType;
			foundChar = c;
			bset = set_;
			scanner = scanner_;
		}		

		/// <summary>
		/// Append a char to the msg buffer.  If special, then show escaped version
		/// </summary>
		/// <param name="sb">Message buffer</param>
		/// <param name="c">Char to append</param>
		private void appendCharName(StringBuilder sb, int c) 
		{
			switch (c) 
			{
				case 65535 :
					// 65535 = (char) -1 = EOF
					sb.Append("'<EOF>'");
					break;
				case '\n' :
					sb.Append(@"'\n'");
					break;
				case '\r' :
					sb.Append(@"'\r'");
					break;
				case '\t' :
					sb.Append(@"'\t'");
					break;
				default :
					sb.Append('\'');
					sb.Append((char) c);
					sb.Append('\'');
					break;
			}
		}
	}
}