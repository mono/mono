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

                public StringWriter() {
                        internalString = new StringBuilder();
                }

                public StringWriter( IFormatProvider formatProvider ) {
                        internalFormatProvider = formatProvider;
                }

                public StringWriter( StringBuilder sb ) {
                        internalString = sb;
                }

                public StringWriter( StringBuilder sb, IFormatProvider formatProvider ) {
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
                }

                protected override void Dispose (bool disposing)
		{
			// MS.NET doesn't clear internal buffer.
			// internalString = null;
			base.Dispose (disposing);
		}

                public virtual StringBuilder GetStringBuilder() {
                        return internalString;
                }

                public override string ToString() {
                        return internalString.ToString();
                }

                public override void Write( char value ) {
                        internalString.Append( value );
                }

                public override void Write( string value ) {
                        internalString.Append( value );
                }

                public override void Write( char[] buffer, int index, int count ) {
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
                        
                        
