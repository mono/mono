using System;
using StringBuilder			= System.Text.StringBuilder;

using BitSet				= antlr.collections.impl.BitSet;
using AST					= antlr.collections.AST;

namespace antlr
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/RIGHTS.html
	*
	* $Id: MismatchedTokenException.cs,v 1.1 2003/04/22 04:56:13 cesar Exp $
	*/

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//
	
	public class MismatchedTokenException : RecognitionException
	{
		// Token names array for formatting
		internal string[] tokenNames;
		// The token that was encountered
		public Token token;
		// The offending AST node if tree walking
		public AST node;
		
		internal string tokenText = null; // taken from node or token object
		
		// Types of tokens
		public enum TokenTypeEnum
		{
			TokenType = 1,
			NotTokenType = 2,
			RangeType = 3,
			NotRangeType = 4,
			SetType = 5,
			NotSetType = 6
		}
		// One of the above
		public TokenTypeEnum mismatchType;
		
		// For TOKEN/NOT_TOKEN and RANGE/NOT_RANGE
		public int expecting;
		
		// For RANGE/NOT_RANGE (expecting is lower bound of range)
		public int upper;
		
		// For SET/NOT_SET
		public BitSet bset;
		
		/*Looking for AST wildcard, didn't find it */
		public MismatchedTokenException() : base("Mismatched Token: expecting any AST node", "<AST>", - 1, - 1)
		{
		}
		
		// Expected range / not range
		public MismatchedTokenException(string[] tokenNames_, AST node_, int lower, int upper_, bool matchNot) : 
					base("Mismatched Token", "<AST>", - 1, - 1)
		{
			tokenNames = tokenNames_;
			node = node_;
			if (node_ == null)
			{
				tokenText = "<empty tree>";
			}
			else
			{
				tokenText = node_.ToString();
			}
			mismatchType = matchNot ? TokenTypeEnum.NotRangeType : TokenTypeEnum.RangeType;
			expecting = lower;
			upper = upper_;
		}
		
		// Expected token / not token
		public MismatchedTokenException(string[] tokenNames_, AST node_, int expecting_, bool matchNot) :
					base("Mismatched Token", "<AST>", - 1, - 1)
		{
			tokenNames = tokenNames_;
			node = node_;
			if (node_ == null)
			{
				tokenText = "<empty tree>";
			}
			else
			{
				tokenText = node_.ToString();
			}
			mismatchType = matchNot ? TokenTypeEnum.NotTokenType : TokenTypeEnum.TokenType;
			expecting = expecting_;
		}
		
		// Expected BitSet / not BitSet
		public MismatchedTokenException(string[] tokenNames_, AST node_, BitSet set_, bool matchNot) :
					base("Mismatched Token", "<AST>", - 1, - 1)
		{
			tokenNames = tokenNames_;
			node = node_;
			if (node_ == null)
			{
				tokenText = "<empty tree>";
			}
			else
			{
				tokenText = node_.ToString();
			}
			mismatchType = matchNot ? TokenTypeEnum.NotSetType : TokenTypeEnum.SetType;
			bset = set_;
		}
		
		// Expected range / not range
		public MismatchedTokenException(string[] tokenNames_, Token token_, int lower, int upper_, bool matchNot, string fileName_) : 
					base("Mismatched Token", fileName_, token_.getLine(), token_.getColumn())
		{
			tokenNames = tokenNames_;
			token = token_;
			tokenText = token_.getText();
			mismatchType = matchNot ? TokenTypeEnum.NotRangeType : TokenTypeEnum.RangeType;
			expecting = lower;
			upper = upper_;
		}
		
		// Expected token / not token
		public MismatchedTokenException(string[] tokenNames_, Token token_, int expecting_, bool matchNot, string fileName_) :
					base("Mismatched Token", fileName_, token_.getLine(), token_.getColumn())
		{
			tokenNames = tokenNames_;
			token = token_;
			tokenText = token_.getText();
			mismatchType = matchNot ? TokenTypeEnum.NotTokenType : TokenTypeEnum.TokenType;
			expecting = expecting_;
		}
		
		// Expected BitSet / not BitSet
		public MismatchedTokenException(string[] tokenNames_, Token token_, BitSet set_, bool matchNot, string fileName_) :
					base("Mismatched Token", fileName_, token_.getLine(), token_.getColumn())
		{
			tokenNames = tokenNames_;
			token = token_;
			tokenText = token_.getText();
			mismatchType = matchNot ? TokenTypeEnum.NotSetType : TokenTypeEnum.SetType;
			bset = set_;
		}
		
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
					case TokenTypeEnum.TokenType: 
						sb.Append("expecting " + tokenName(expecting) + ", found '" + tokenText + "'");
						break;
				
					case TokenTypeEnum.NotTokenType: 
						sb.Append("expecting anything but " + tokenName(expecting) + "; got it anyway");
						break;
				
					case TokenTypeEnum.RangeType: 
						sb.Append("expecting token in range: " + tokenName(expecting) + ".." + tokenName(upper) + ", found '" + tokenText + "'");
						break;
				
					case TokenTypeEnum.NotRangeType: 
						sb.Append("expecting token NOT in range: " + tokenName(expecting) + ".." + tokenName(upper) + ", found '" + tokenText + "'");
						break;
				
					case TokenTypeEnum.SetType: case TokenTypeEnum.NotSetType: 
						sb.Append("expecting " + (mismatchType == TokenTypeEnum.NotSetType ? "NOT " : "") + "one of (");
						int[] elems = bset.toArray();
						for (int i = 0; i < elems.Length; i++)
						{
							sb.Append(" ");
							sb.Append(tokenName(elems[i]));
						}
						sb.Append("), found '" + tokenText + "'");
						break;
				
					default: 
						sb.Append(base.Message);
						break;				
				}			
				return sb.ToString();
			}
		}
		
		private string tokenName(int tokenType)
		{
			if (tokenType == Token.INVALID_TYPE)
			{
				return "<Set of tokens>";
			}
			else if (tokenType < 0 || tokenType >= tokenNames.Length)
			{
				return "<" + tokenType.ToString() + ">";
			}
			else
			{
				return tokenNames[tokenType];
			}
		}
	}
}