//
// System.IO.TextReader
//
// Author: Marcin Szczepanski (marcins@zipworld.com.au)
//
// TODO: Implement the Thread Safe stuff
//

using System;

namespace System.IO {

	[Serializable]
	public abstract class TextReader : MarshalByRefObject, IDisposable {
		
		protected TextReader() { }
		
		public static readonly TextReader Null;
		
		public virtual void Close()
		{ 
			Dispose(true);
		}

		void System.IDisposable.Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose( bool disposing )
		{
			return;
		}
		
		public virtual int Peek()
		{
			return -1;
		}
		
		public virtual int Read()
		{
			return -1;
		}
		
		public virtual int Read (char[] buffer, int index, int count)
		{
			int c, i;
			
			for (i = 0; i < count; i++) {
				if ((c = Read ()) == -1)
					return i;
				buffer [index + i] = (char)c;
			}
			
			return i;
		}
		
		public virtual int ReadBlock (char [] buffer, int index, int count)
		{
			int read_count = 0;
			do {
				read_count = Read (buffer, index, count);
				index += read_count;
				count -= read_count;
			} while (read_count != 0 && count > 0);

			return read_count;
		}

		public virtual string ReadLine()
		{ 
			return String.Empty;
		}

		public virtual string ReadToEnd()
		{ 
			return String.Empty;
		}

		[MonoTODO]
		public static TextReader Synchronized( TextReader reader )
		{
                        // TODO: Implement
			return Null;
		}	
	}
}
