namespace antlr.debug
{
	using System;
	
	public abstract class ANTLREventArgs : EventArgs
	{
		public ANTLREventArgs()
		{
		}
		public ANTLREventArgs(int type)
		{
			this.Type = type;
		}
	
		public virtual int Type
		{
			get
			{
				return this.type_;
			}
			set
			{
				this.type_ = value;
			}
		}

		internal void setValues(int type)
		{
			this.Type = type;
		}

		/// <summary>
		/// Event type.
		/// </summary>
		private int type_;
	}
}