//
// System.IO.TextWriter
//
// Author: Marcin Szczepanski (marcins@zipworld.com.au)
//
// TODO: Implement the Thread Safe stuff

namespace System.IO {
        public abstract class TextWriter : MarshalByRefObject, IDisposable {
                
                protected TextWriter() { }
                
                protected TextWriter( IFormatProvider formatProvider ) {
                        InternalFormatProvider = formatProvider;
                }

                protected char[] coreNewLine;

                protected IFormatProvider InternalFormatProvider;

                public static readonly TextWriter Null;

                public abstract System.Text.Encoding Encoding { get; }

                public virtual IFormatProvider FormatProvider { 
                        get {
                                return InternalFormatProvider;
                        } 
                }

                public virtual string NewLine { 
                        get {
                                return new String(coreNewLine);
                        }
                        
                        set {
                                coreNewLine = value.ToCharArray();
                        }
                }

                public virtual void Close() { 
                        Dispose( true );
                }

                protected virtual void Dispose( bool disposing ) { }
                
		void System.IDisposable.Dispose() {
			Dispose(true);
		}


                protected virtual void Flush() { }
                
                public static TextWriter Synchronised( TextWriter writer ) {
                        // TODO: Implement.

                        return Null;
                }

                public virtual void Write( bool value ) { }
                public virtual void Write( char value ) { }
                public virtual void Write( char[] value ) { }
                public virtual void Write( decimal value ) { }
                public virtual void Write( double value ) { }
                public virtual void Write( int value ) { }
                public virtual void Write( long value ) { }
                public virtual void Write( object value ) { }
                public virtual void Write( float value ) { }
                public virtual void Write( string value ) { }
                public virtual void Write( uint value ) { }
                public virtual void Write( ulong value ) { }
                public virtual void Write( string format, object arg0 ) { }
                public virtual void Write( string format, params object[] arg ) { }
                public virtual void Write( char[] buffer, int index, int count ) { }
                public virtual void Write( string format, object arg0, object arg1 ) { }
                public virtual void Write( string format, object arg0, object arg1, object arg2 ) { }
                
                public virtual void WriteLine() { }
                public virtual void WriteLine( bool value ) { }
                public virtual void WriteLine( char value ) { }
                public virtual void WriteLine( char[] value ) { }
                public virtual void WriteLine( decimal value ) { }
                public virtual void WriteLine( double value ) { }
                public virtual void WriteLine( int value ) { }
                public virtual void WriteLine( long value ) { }
                public virtual void WriteLine( object value ) { }
                public virtual void WriteLine( float value ) { }
                public virtual void WriteLine( string value ) { }
                public virtual void WriteLine( uint value ) { }
                public virtual void WriteLine( ulong value ) { }
                public virtual void WriteLine( string format, object arg0 ) { }
                public virtual void WriteLine( string format, params object[] arg ) { }
                public virtual void WriteLine( char[] buffer, int index, int count ) { }
                public virtual void WriteLine( string format, object arg0, object arg1 ) { }
                public virtual void WriteLine( string format, object arg0, object arg1, object arg2 ) { }
                

        }
}





