namespace antlr.debug
{
	using System;
	
	public class MatchEventArgs : GuessingEventArgs
	{
		public MatchEventArgs()
		{
		}
		public MatchEventArgs(int type, int val, object target, string text, int guessing, bool inverse, bool matched)
		{
			setValues(type, val, target, text, guessing, inverse, matched);
		}

		public virtual object Target
		{
			get	{ return this.target_;	}
			set	{ this.target_ = value;	}
		}

		public virtual string Text
		{
			get	{ return this.text_;	}
			set	{ this.text_ = value;	}
		}

		public virtual int Value
		{
			get	{ return this.val_;		}
			set	{ this.val_ = value;	}
		}

		internal bool Inverse
		{
			set	{ this.inverse_ = value;	}
		}

		internal bool Matched
		{
			set	{ this.matched_ = value;	}
		}

		// NOTE: for a mismatch on type STRING, the "text" is used as the lookahead
		//       value.  Normally "value" is this
		public enum ParserMatchEnums
		{
			TOKEN		= 0,
			BITSET		= 1,
			CHAR		= 2,
			CHAR_BITSET = 3,
			STRING		= 4,
			CHAR_RANGE	= 5,
		}
		public static int TOKEN = 0;
		public static int BITSET = 1;
		public static int CHAR = 2;
		public static int CHAR_BITSET = 3;
		public static int STRING = 4;
		public static int CHAR_RANGE = 5;

		private bool	inverse_;
		private bool	matched_;
		private object	target_;
		private int		val_;
		private string	text_;
		
		
		public virtual bool isInverse()
		{
			return inverse_;
		}
		public virtual bool isMatched()
		{
			return matched_;
		}
		/// <summary>This should NOT be called from anyone other than ParserEventSupport! 
		/// </summary>
		internal void  setValues(int type, int val, object target, string text, int guessing, bool inverse, bool matched)
		{
			base.setValues(type, guessing);
			this.Value = val;
			this.Target = target;
			this.Inverse = inverse;
			this.Matched = matched;
			this.Text = text;
		}
		
		public override string ToString()
		{
			return "ParserMatchEvent [" + (isMatched()?"ok,":"bad,") + (isInverse()?"NOT ":"") + (Type == TOKEN?"token,":"bitset,") + Value + "," + Target + "," + Guessing + "]";
		}
	}
}