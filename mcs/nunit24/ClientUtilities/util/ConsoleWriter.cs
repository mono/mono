// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

namespace NUnit.Util
{
	using System;
	using System.IO;
	using System.Text;

	/// <summary>
	/// Class used for receiving console output from the running test and displaying it.
	/// </summary>
	public class ConsoleWriter : TextWriter
	{
		#region Private Fields

		private TextWriter console;

		#endregion

		#region Constructors
    			
		public ConsoleWriter(TextWriter console)
		{
			this.console = console;
		}

		#endregion
    			
		#region TextWriter Overrides

		public override void Close()
		{
			//console.Close ();
		}

		public override void Flush()
		{
			console.Flush ();
		}


		public override void Write(char c)
		{
			console.Write(c);
		}

		public override void Write(String s)
		{
			console.Write(s);
		}

		public override void WriteLine(string s)
		{
			console.WriteLine(s);
		}

		public override Encoding Encoding
		{
			get { return Encoding.Default; }
		}

		public override Object InitializeLifetimeService()
		{
			return null;
		}

		#endregion
	}
}
