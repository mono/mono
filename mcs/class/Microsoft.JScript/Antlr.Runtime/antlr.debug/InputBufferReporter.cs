namespace antlr.debug
{
	using System;
	
	public class InputBufferReporter : InputBufferListenerBase, InputBufferListener
	{
		public virtual void inputBufferChanged(object source, InputBufferEventArgs e)
		{
			System.Console.Out.WriteLine(e);
		}

		/// <summary> charBufferConsume method comment.
		/// </summary>
		public override void  inputBufferConsume(object source, InputBufferEventArgs e)
		{
			System.Console.Out.WriteLine(e);
		}

		/// <summary> charBufferLA method comment.
		/// </summary>
		public override void  inputBufferLA(object source, InputBufferEventArgs e)
		{
			System.Console.Out.WriteLine(e);
		}

		public override void  inputBufferMark(object source, InputBufferEventArgs e)
		{
			System.Console.Out.WriteLine(e);
		}

		public override void  inputBufferRewind(object source, InputBufferEventArgs e)
		{
			System.Console.Out.WriteLine(e);
		}
	}
}