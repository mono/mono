namespace antlr.debug
{
	using System;
	
	public class NewLineEventArgs : ANTLREventArgs
	{
		public NewLineEventArgs()
		{
		}
		public NewLineEventArgs(int line)
		{
			Line = line;
		}

		public virtual int Line
		{
			get	{ return this.line_; }
			set	{ this.line_ = value; }
		}

		private int line_;
		
		
		/// <summary>This should NOT be called from anyone other than ParserEventSupport! 
		/// </summary>
		public override string ToString()
		{
			return "NewLineEvent [" + line_ + "]";
		}
	}
}