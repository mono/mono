//
// System.IO.StringWriter
//
// Authors
//	Marcin Szczepanski (marcins@zipworld.com.au)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004 Novell (http://www.novell.com)
//

using System.Globalization;
using System.Text;

namespace System.IO {

	[Serializable]
        public class StringWriter : TextWriter {
                
                private StringBuilder internalString;
		private bool disposed = false;

                public StringWriter () : this (new StringBuilder ())
		{
		}

                public StringWriter (IFormatProvider formatProvider) :
			this (new StringBuilder (), formatProvider)
		{
		}

                public StringWriter (StringBuilder sb) :
			this (sb, null)
		{
		}

                public StringWriter (StringBuilder sb, IFormatProvider formatProvider)
		{
			if (sb == null)
				throw new ArgumentNullException ("sb");

			internalString = sb;
			internalFormatProvider = formatProvider;
		}

                public override System.Text.Encoding Encoding {
                        get {
                                return System.Text.Encoding.Unicode;
                        }
		}

                public override void Close ()
		{
			Dispose (true);
			disposed = true;
                }

                protected override void Dispose (bool disposing)
		{
			// MS.NET doesn't clear internal buffer.
			// internalString = null;
			base.Dispose (disposing);
			disposed = true;
		}

                public virtual StringBuilder GetStringBuilder ()
		{
			return internalString;
		}

                public override string ToString ()
		{
			return internalString.ToString ();
                }

                public override void Write (char value)
		{
			if (disposed) {
				throw new ObjectDisposedException ("StringReader", 
					Locale.GetText ("Cannot write to a closed StringWriter"));
			}

                        internalString.Append (value);
		}

                public override void Write (string value)
		{
			if (disposed) {
				throw new ObjectDisposedException ("StringReader", 
					Locale.GetText ("Cannot write to a closed StringWriter"));
			}

			internalString.Append (value);
		}

                public override void Write (char[] buffer, int index, int count)
		{
			if (disposed) {
				throw new ObjectDisposedException ("StringReader", 
					Locale.GetText ("Cannot write to a closed StringWriter"));
			}
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index", "< 0");
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "< 0");
			// re-ordered to avoid possible integer overflow
                        if (index > buffer.Length - count)
				throw new ArgumentException ("index + count > buffer.Length");

			internalString.Append (buffer, index, count);
		}
	}
}
              
