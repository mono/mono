namespace antlr.debug
{
	using System;
	
	public abstract class GuessingEventArgs : ANTLREventArgs
	{
		public GuessingEventArgs()
		{
		}
		public GuessingEventArgs(int type) : base(type)
		{
		}

		public virtual int Guessing
		{
			get	{ return guessing_;			}
			set	{ this.guessing_ = value;	}
		}

		private int guessing_;
		
		
		/// <summary>This should NOT be called from anyone other than ParserEventSupport! 
		/// </summary>
		public virtual void  setValues(int type, int guessing)
		{
			setValues(type);
			this.Guessing = guessing;
		}
	}
}