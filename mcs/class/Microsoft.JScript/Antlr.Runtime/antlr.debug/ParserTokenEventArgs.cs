namespace antlr.debug
{
	using System;
	
	public class TokenEventArgs : ANTLREventArgs
	{
		public TokenEventArgs()
		{
		}
		public TokenEventArgs(int type, int amount, int val)
		{
			setValues(type, amount, val);
		}

		public virtual int Amount
		{
			get	{ return amount;	}
			set	{ this.amount = value;	}
		}

		public virtual int Value
		{
			get	{ return this.value_;	}
			set { this.value_ = value;	}
		}

		private int value_;
		private int amount;

		public static int LA = 0;
		public static int CONSUME = 1;
		
		
		/// <summary>This should NOT be called from anyone other than ParserEventSupport! 
		/// </summary>
		internal void  setValues(int type, int amount, int val)
		{
			setValues(type, amount, val);
		}

		public override string ToString()
		{
			if (Type == LA)
				return "ParserTokenEvent [LA," + Amount + "," + Value + "]";
			else
				return "ParserTokenEvent [consume,1," + Value + "]";
		}
	}
}