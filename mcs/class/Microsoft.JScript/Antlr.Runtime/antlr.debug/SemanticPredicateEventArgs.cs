namespace antlr.debug
{
	using System;
	
	public class SemanticPredicateEventArgs : GuessingEventArgs
	{
		public SemanticPredicateEventArgs()
		{
		}
		public SemanticPredicateEventArgs(int type) : base(type)
		{
		}

		public virtual int Condition
		{
			get	{ return this.condition_;	}
			set	{ this.condition_ = value;	}
		}

		public virtual bool Result
		{
			get	{ return this.result_;	}
			set	{ this.result_ = value;	}
		}

		public const int VALIDATING = 0;
		public const int PREDICTING = 1;

		private int condition_;
		private bool result_;
		
		
		/// <summary>This should NOT be called from anyone other than ParserEventSupport! 
		/// </summary>
		internal void  setValues(int type, int condition, bool result, int guessing)
		{
			base.setValues(type, guessing);
			this.Condition	= condition;
			this.Result		= result;
		}

		public override string ToString()
		{
			return "SemanticPredicateEvent [" + Condition + "," + Result + "," + Guessing + "]";
		}
	}
}