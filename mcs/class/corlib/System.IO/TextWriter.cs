//
// System.IO.TextWriter
//
// Authors:
//   Marcin Szczepanski (marcins@zipworld.com.au)
//   Miguel de Icaza (miguel@gnome.org)
//

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

                public static TextWriter Synchronized (TextWriter writer)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer is null");

			if (writer is SynchronizedWriter)
				return writer;
			
			return new SynchronizedWriter (writer);
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

	//
	// Sychronized version of the TextWriter.
	//
	[Serializable]
	internal class SynchronizedWriter : TextWriter {
		private TextWriter writer;

		public SynchronizedWriter (TextWriter writer)
		{
			this.writer = writer;
		}

		public override void Close ()
		{
			lock (this){
				writer.Close ();
			}
		}

		public override void Flush ()
		{
			lock (this){
				writer.Flush ();
			}
		}

#region Write methods
		public override void Write (bool value)
		{
			lock (this){
				writer.Write (value);
			}
		}
		
		public override void Write (char value)
		{
			lock (this){
				writer.Write (value);
			}
		}

		public override void Write (char [] value)
		{
			lock (this){
				writer.Write (value);
			}
		}

		public override void Write (Decimal value)
		{
			lock (this){
				writer.Write (value);
			}
		}

		public override void Write (int value)
		{
			lock (this){
				writer.Write (value);
			}
		}

		public override void Write (long value)
		{
			lock (this){
				writer.Write (value);
			}
		}
		
		public override void Write (object value)
		{
			lock (this){
				writer.Write (value);
			}
		}

		public override void Write (float value)
		{
			lock (this){
				writer.Write (value);
			}
		}
		
		public override void Write (string value)
		{
			lock (this){
				writer.Write (value);
			}
		}
		
		[CLSCompliant(false)]
		public override void Write (uint value)
		{
			lock (this){
				writer.Write (value);
			}
		}
		
		[CLSCompliant(false)]
		public override void Write (ulong value)
		{
			lock (this){
				writer.Write (value);
			}
		}
		
		public override void Write (string format, object value)
		{
			lock (this){
				writer.Write (format, value);
			}
		}
		
		public override void Write (string format, object [] value)
		{
			lock (this){
				writer.Write (format, value);
			}
		}

		public override void Write (char [] buffer, int index, int count)
		{
			lock (this){
				writer.Write (buffer, index, count);
			}
		}
		
		public override void Write (string format, object arg0, object arg1)
		{
			lock (this){
				writer.Write (format, arg0, arg1);
			}
		}
		
		public override void Write (string format, object arg0, object arg1, object arg2)
		{
			lock (this){
				writer.Write (format, arg0, arg1, arg2);
			}
		}
#endregion
#region WriteLine methods
		public override void WriteLine ()
		{
			lock (this){
				writer.WriteLine ();
			}
		}

		public override void WriteLine (bool value)
		{
			lock (this){
				writer.WriteLine (value);
			}
		}

		public override void WriteLine (char value)
		{
			lock (this){
				writer.WriteLine (value);
			}
		}

		public override void WriteLine (char [] value)
		{
			lock (this){
				writer.WriteLine (value);
			}
		}

		public override void WriteLine (Decimal value)
		{
			lock (this){
				writer.WriteLine (value);
			}
		}

		public override void WriteLine (double value)
		{
			lock (this){
				writer.WriteLine (value);
			}
		}

		public override void WriteLine (int value)
		{
			lock (this){
				writer.WriteLine (value);
			}
		}

		public override void WriteLine (long value)
		{
			lock (this){
				writer.WriteLine (value);
			}
		}

		public override void WriteLine (object value)
		{
			lock (this){
				writer.WriteLine (value);
			}
		}

		public override void WriteLine (float value)
		{
			lock (this){
				writer.WriteLine (value);
			}
		}

		public override void WriteLine (string value)
		{
			lock (this){
				writer.WriteLine (value);
			}
		}

		[CLSCompliant(false)]
		public override void WriteLine (uint value)
		{
			lock (this){
				writer.WriteLine (value);
			}
		}

		[CLSCompliant(false)]
		public override void WriteLine (ulong value)
		{
			lock (this){
				writer.WriteLine (value);
			}
		}

		public override void WriteLine (string format, object value)
		{
			lock (this){
				writer.WriteLine (format, value);
			}
		}

		public override void WriteLine (string format, object [] value)
		{
			lock (this){
				writer.WriteLine (format, value);
			}
		}

		public override void WriteLine (char [] buffer, int index, int count)
		{
			lock (this){
				writer.WriteLine (buffer, index, count);
			}
		}
		
		public override void WriteLine (string format, object arg0, object arg1)
		{
			lock (this){
				writer.WriteLine (format, arg0, arg1);
			}
		}

		public override void WriteLine (string format, object arg0, object arg1, object arg2)
		{
			lock (this){
				writer.WriteLine (format, arg0, arg1, arg2);
			}
		}
#endregion
		
		public override Encoding Encoding {
			get {
				lock (this){
					return writer.Encoding;
				}
			}
		}

		public override IFormatProvider FormatProvider {
			get {
				lock (this){
					return writer.FormatProvider;
				}
			}
		}

		public override string NewLine {
			get {
				lock (this){
					return writer.NewLine;
				}
			}

			set {
				lock (this){
					writer.NewLine = value;
				}
			}
		}
	}
}





