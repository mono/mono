//
// System.IO.TextWriter
//
// Author: Marcin Szczepanski (marcins@zipworld.com.au)
//
// TODO: Implement the Thread Safe stuff
//

using System;

namespace System.IO {
	public abstract class TextReader : MarshalByRefObject, IDisposable {

		protected TextReader() { }
		
		public static readonly TextReader Null;
		
		public virtual void Close() { 
			Dispose(true);
		}

		void System.IDisposable.Dispose() {
			Dispose(true);
		}

		protected virtual void Dispose( bool disposing ) {
			return;
		}
		
		public virtual int Peek() {
			return -1;
		}
		
		public virtual int Read() {
			return -1;
		}
		
	
		// LAMESPEC:  The Beta2 docs say this should be Read( out char[] ...
		// whereas the MS implementation is just Read( char[] ... )
		// Not sure which one is right, we'll see in Beta3 :)

		public virtual int Read( char[] buffer, int index, int count ) { 
			return 1;
		}
		
		public virtual int ReadBlock( char[] buffer, int index, int count ) { 
			return 1;
		}

		public virtual string ReadLine() { 
			return String.Empty;
		}

		public virtual string ReadToEnd() { 
			return String.Empty;
		}
		
		public static TextReader Synchronised( TextReader reader ) {
                        // TODO: Implement
			return Null;
		}	
	}
}
