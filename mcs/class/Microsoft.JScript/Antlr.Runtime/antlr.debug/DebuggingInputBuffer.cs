namespace antlr.debug
{
	using System;
	using ArrayList	= System.Collections.ArrayList;
	
	public class DebuggingInputBuffer : InputBuffer
	{
		public virtual ArrayList InputBufferListeners
		{
			get { return inputBufferEventSupport.InputBufferListeners; }
		}
		public virtual bool DebugMode
		{
			set	{ debugMode = value;	}
		}

		private InputBuffer buffer;
		private InputBufferEventSupport inputBufferEventSupport;
		private bool debugMode = true;
		
		
		public DebuggingInputBuffer(InputBuffer buffer)
		{
			this.buffer = buffer;
			inputBufferEventSupport = new InputBufferEventSupport(this);
		}
		public virtual void  addInputBufferListener(InputBufferListener l)
		{
			inputBufferEventSupport.addInputBufferListener(l);
		}
		public override void  consume()
		{
			char la = ' ';
			try
			{
				la = buffer.LA(1);
			}
			catch (CharStreamException)
			{
			} // vaporize it...
			buffer.consume();
			if (debugMode)
				inputBufferEventSupport.fireConsume(la);
		}
		public override void  fill(int a)
		{
			buffer.fill(a);
		}
		public virtual bool isDebugMode()
		{
			return debugMode;
		}
		public override bool isMarked()
		{
			return buffer.isMarked();
		}
		public override char LA(int i)
		{
			char la = buffer.LA(i);
			if (debugMode)
				inputBufferEventSupport.fireLA(la, i);
			return la;
		}
		public override int mark()
		{
			int m = buffer.mark();
			inputBufferEventSupport.fireMark(m);
			return m;
		}
		public virtual void  removeInputBufferListener(InputBufferListener l)
		{
			if (inputBufferEventSupport != null)
				inputBufferEventSupport.removeInputBufferListener(l);
		}
		public override void  rewind(int mark)
		{
			buffer.rewind(mark);
			inputBufferEventSupport.fireRewind(mark);
		}
	}
}