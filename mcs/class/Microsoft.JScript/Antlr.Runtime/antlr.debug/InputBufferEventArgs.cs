namespace antlr.debug
{
	using System;
	
	public class InputBufferEventArgs : ANTLREventArgs
	{
		public InputBufferEventArgs()
		{
		}

		public InputBufferEventArgs(int type, char c, int lookaheadAmount)
		{
			setValues(type, c, lookaheadAmount);
		}
	
		public virtual char Char
		{
			get	{ return this.c_;	}
			set	{ this.c_ = value;	}
		}
		public virtual int LookaheadAmount
		{
			get	{ return this.lookaheadAmount_;		}
			set	{ this.lookaheadAmount_ = value;	}
		}

		internal char c_;
		internal int lookaheadAmount_; // amount of lookahead

		public const int CONSUME = 0;
		public const int LA = 1;
		public const int MARK = 2;
		public const int REWIND = 3;
		
		
		/// <summary>This should NOT be called from anyone other than ParserEventSupport! 
		/// </summary>
		internal void  setValues(int type, char c, int la)
		{
			setValues(type);
			this.Char	= c;
			this.LookaheadAmount = la;
		}

		public override string ToString()
		{
			return "CharBufferEvent [" + (Type == CONSUME?"CONSUME, ":"LA, ") + Char + "," + LookaheadAmount + "]";
		}
	}
}