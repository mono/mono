namespace antlr.debug
{
	using System;
	

	public interface Listener
	{
		void  doneParsing	(object source, TraceEventArgs e);
		void  refresh		();
	}
}