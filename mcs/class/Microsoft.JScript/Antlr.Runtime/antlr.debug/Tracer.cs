namespace antlr.debug
{
	using System;
	
	public class Tracer : TraceListenerBase, TraceListener
	{
		protected string indentString = "";
		// TBD: should be StringBuffer
		
		
		protected internal virtual void  dedent()
		{
			if (indentString.Length < 2)
				indentString = "";
			else
				indentString = indentString.Substring(2);
		}
		public override void  enterRule(object source, TraceEventArgs e)
		{
			System.Console.Out.WriteLine(indentString + e);
			indent();
		}
		public override void  exitRule(object source, TraceEventArgs e)
		{
			dedent();
			System.Console.Out.WriteLine(indentString + e);
		}
		protected internal virtual void  indent()
		{
			indentString += "  ";
		}
	}
}