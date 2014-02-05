using System.IO;

namespace Mono.XBuild.Utilities
{
	public class OutWriter
	{
		TextWriter writer;

		public OutWriter (TextWriter writer)
		{
			this.writer = writer;
		}

		public void WriteOut (object sender, string s)
		{
			writer.Write (s);
		}

		public static ProcessEventHandler GetWriteHandler (TextWriter tw)
		{
			return tw != null ? new ProcessEventHandler(new OutWriter (tw).WriteOut) : null;
		}
	}
}
