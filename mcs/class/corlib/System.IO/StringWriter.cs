//
// System.IO.StringWriter
//
// Author: Marcin Szczepanski (marcins@zipworld.com.au)
//

using System.Text;

namespace System.IO {
		[Serializable]
        public class StringWriter : TextWriter {
                
                private StringBuilder internalString;
		private bool disposed = false;

                public StringWriter() : this (new StringBuilder ()) {
			
                }

                public StringWriter( IFormatProvider formatProvider ) : this (new StringBuilder (), formatProvider)  {

                }

                public StringWriter( StringBuilder sb ) : this (sb, null) {

                }

                public StringWriter( StringBuilder sb, IFormatProvider formatProvider ) {
			
			if (sb == null)
				throw new ArgumentNullException ();

                        internalString = sb;
                        internalFormatProvider = formatProvider;
                }

                public override System.Text.Encoding Encoding {
                        get {
                                return System.Text.Encoding.Unicode;
                        }
                }

                public override void Close() {
                        Dispose( true );
			disposed = true;
                }

                protected override void Dispose (bool disposing)
		{
			// MS.NET doesn't clear internal buffer.
			// internalString = null;
			base.Dispose (disposing);
			disposed = true;
		}

                public virtual StringBuilder GetStringBuilder() {
                        return internalString;
                }

                public override string ToString() {
                        return internalString.ToString();
                }

                public override void Write( char value ) {

			if (disposed) 
				throw new ObjectDisposedException ("StringWriter", "Cannot write to a closed StringWriter");

                        internalString.Append( value );
                }

                public override void Write( string value ) {

			if (disposed) 
				throw new ObjectDisposedException ("StringWriter", "Cannot write to a closed StringWriter");

                        internalString.Append( value );
                }

                public override void Write( char[] buffer, int index, int count ) {

			if (disposed) 
				throw new ObjectDisposedException ("StringReader", "Cannot write to a closed StringWriter");

                        if( buffer == null ) {
                                throw new ArgumentNullException();
                        } else if( index < 0 || count < 0 ) {
                                throw new ArgumentOutOfRangeException();
                        } else if( index > buffer.Length || index + count > buffer.Length ) {
                                throw new ArgumentException();
                        }
                        
                        char[] writeBuffer = new char[ count ];

                        Array.Copy( buffer, index, writeBuffer, 0, count );

                        internalString.Append( writeBuffer );
                }
                          
        }
}
                        
                        
