namespace antlr.debug
{
	using System;
	
	public interface NewLineListener : Listener
	{
		void hitNewLine(object source, NewLineEventArgs e);
	}
}