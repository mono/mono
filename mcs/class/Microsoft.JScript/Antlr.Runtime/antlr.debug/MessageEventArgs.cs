namespace antlr.debug
{
	using System;
	
	public class MessageEventArgs : ANTLREventArgs
	{
		public MessageEventArgs()
		{
		}
		public MessageEventArgs(int type, string text)
		{
			setValues(type, text);
		}

		public virtual string Text
		{
			get	{ return text_;			}
			set	{ this.text_ = value;	}
			
		}

		private string text_;

		public static int WARNING = 0;
		public static int ERROR = 1;
		
		
		/// <summary>This should NOT be called from anyone other than ParserEventSupport! 
		/// </summary>
		internal void  setValues(int type, string text)
		{
			setValues(type);
			this.Text   = text;
		}

		public override string ToString()
		{
			return "ParserMessageEvent [" + (Type == WARNING?"warning,":"error,") + Text + "]";
		}
	}
}