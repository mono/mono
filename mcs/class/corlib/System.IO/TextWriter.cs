//
// System.IO.TextWriter
//
// Author: Marcin Szczepanski (marcins@zipworld.com.au)
//
// TODO: Implement the Thread Safe stuff

using System.Text;

namespace System.IO {

	[Serializable]
	public abstract class TextWriter : MarshalByRefObject, IDisposable {
                
                protected TextWriter() {
			CoreNewLine = "\n".ToCharArray ();
		}
                
                protected TextWriter( IFormatProvider formatProvider ) {
                        internalFormatProvider = formatProvider;
                }

                protected char[] CoreNewLine;

                internal IFormatProvider internalFormatProvider;

                public static readonly TextWriter Null = new NullTextWriter ();

                public abstract Encoding Encoding { get; }

                public virtual IFormatProvider FormatProvider { 
                        get {
                                return internalFormatProvider;
                        } 
                }

                public virtual string NewLine { 
                        get {
                                return new String(CoreNewLine);
                        }
                        
                        set {
                                CoreNewLine = value.ToCharArray();
                        }
                }

                public virtual void Close () { 
                        Dispose (true);
                }

                protected virtual void Dispose (bool disposing) { }
                
		void System.IDisposable.Dispose () {
			Dispose (true);
		}


                public virtual void Flush()
		{
			// do nothing
		}

		[MonoTODO]
                public static TextWriter Synchronized (TextWriter writer)
		{
                        // TODO: Implement.

                        return Null;
                }

                public virtual void Write (bool value)
		{
			Write (value.ToString ());
		}
		
                public virtual void Write (char value)
		{
			Write (value.ToString (internalFormatProvider));
		}

                public virtual void Write (char[] value)
		{
			if (value != null)
				Write (new String (value));
		}
		
                public virtual void Write (decimal value)
		{
			Write (value.ToString (internalFormatProvider));
		}
		
                public virtual void Write (double value)
		{
			Write (value.ToString (internalFormatProvider));
		}

                public virtual void Write (int value)
		{
			Write (value.ToString (internalFormatProvider));
		}
		
                public virtual void Write (long value)
		{
			Write (value.ToString (internalFormatProvider));
		}
		
                public virtual void Write (object value)
		{
			if (value != null)
				Write (value.ToString ());
		}
		
                public virtual void Write (float value)
		{
			Write (value.ToString (internalFormatProvider));
		}
		
                public virtual void Write (string value)
		{
			// do nothing
		}
		
		[CLSCompliant(false)]
                public virtual void Write (uint value)
		{
			Write (value.ToString (internalFormatProvider));
		}
		
		[CLSCompliant(false)]
                public virtual void Write (ulong value)
		{
			Write (value.ToString (internalFormatProvider));
		}
		
                public virtual void Write (string format, object arg0)
		{
			Write (String.Format (format, arg0));
		}
		
                public virtual void Write (string format, params object[] arg)
		{
			Write (String.Format (format, arg));
		}
		
                public virtual void Write (char[] buffer, int index, int count)
		{
			Write (new String (buffer, index, count));
		}
		
                public virtual void Write (string format, object arg0, object arg1)
		{
			Write (String.Format (format, arg0, arg1));
		}
		
                public virtual void Write (string format, object arg0, object arg1, object arg2 )
		{
			Write (String.Format (format, arg0, arg1, arg2));
		}
                
                public virtual void WriteLine ()
		{
			Write (NewLine);
		}
		
                public virtual void WriteLine (bool value)
		{
			Write (value);
			WriteLine();
		}
		
                public virtual void WriteLine (char value)
		{
			Write (value);
			WriteLine();
		}
		
                public virtual void WriteLine (char[] value)
		{
			Write (value);
			WriteLine();
		}
		
                public virtual void WriteLine (decimal value)
		{
			Write (value);
			WriteLine();
		}
		
                public virtual void WriteLine (double value)
		{
			Write (value);
			WriteLine();
		}
		
                public virtual void WriteLine (int value)
		{
			Write (value);
			WriteLine();
		}
		
                public virtual void WriteLine (long value)
		{
			Write (value);
			WriteLine();
		}
		
                public virtual void WriteLine (object value)
		{
			Write (value);
			WriteLine();
		}
		
                public virtual void WriteLine (float value)
		{
			Write (value);
			WriteLine();
		}
		
                public virtual void WriteLine (string value)
		{
			Write (value);
			WriteLine();
		}
		
		[CLSCompliant(false)]
                public virtual void WriteLine (uint value)
		{
			Write (value);
			WriteLine();
		}
		
		[CLSCompliant(false)]
                public virtual void WriteLine (ulong value)
		{
			Write (value);
			WriteLine();
		}
		
                public virtual void WriteLine (string format, object arg0)
		{
			Write (format, arg0);
			WriteLine();
		}
		
                public virtual void WriteLine (string format, params object[] arg)
		{
			Write (format, arg);
			WriteLine();
		}
		
                public virtual void WriteLine (char[] buffer, int index, int count)
		{
			Write (buffer, index, count);
			WriteLine();
		}
		
                public virtual void WriteLine (string format, object arg0, object arg1)
		{
			Write (format, arg0, arg1);
			WriteLine();
		}
		
                public virtual void WriteLine (string format, object arg0, object arg1, object arg2)
		{
			Write (format, arg0, arg1, arg2);
			WriteLine();
		}

		//
		// Null version of the TextWriter, for the `Null' instance variable
		//
		sealed class NullTextWriter : TextWriter {
			public override Encoding Encoding {
				get {
					return Encoding.Default;
				}
			}
			
			public override void Write (string s)
			{
			}
		}
        }
}





