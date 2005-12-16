using System;

namespace Cairo
{
	public class FontFace
	{
		IntPtr handle;

		public FontFace (IntPtr handle)
		{
			this.handle = handle;
		}

		public IntPtr Handle {
			get { return handle; }
		}
	}
}

