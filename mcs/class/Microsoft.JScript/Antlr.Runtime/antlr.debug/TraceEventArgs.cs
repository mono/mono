namespace antlr.debug
{
	using System;
	
	public class TraceEventArgs : GuessingEventArgs
	{
		public TraceEventArgs()
		{
		}
		public TraceEventArgs(int type, int ruleNum, int guessing, int data)
		{
			setValues(type, ruleNum, guessing, data);
		}

		public virtual int Data
		{
			get	{ return this.data_;	}
			set	{ this.data_ = value;	}
		}

		public virtual int RuleNum
		{
			get	{ return this.ruleNum_;		}
			set	{ this.ruleNum_ = value;	}
		}

		private int ruleNum_;
		private int data_;

		public static int ENTER = 0;
		public static int EXIT = 1;
		public static int DONE_PARSING = 2;
		
		
		/// <summary>This should NOT be called from anyone other than ParserEventSupport! 
		/// </summary>
		internal void  setValues(int type, int ruleNum, int guessing, int data)
		{
			base.setValues(type, guessing);
			RuleNum = ruleNum;
			Data	= data;
		}

		public override string ToString()
		{
			return "ParserTraceEvent [" + (Type == ENTER?"enter,":"exit,") + RuleNum + "," + Guessing + "]";
		}
	}
}