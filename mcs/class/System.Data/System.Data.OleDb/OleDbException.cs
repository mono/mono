//
// System.Data.OleDb.OleDbException
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Data.OleDb
{
	[Serializable]
#if NET_2_0
	public sealed class OleDbException : DbException
#else
	public sealed class OleDbException : ExternalException
#endif
	{
		private OleDbConnection connection;

		#region Constructors

		internal OleDbException (OleDbConnection cnc)
		{
			connection = cnc;
		}
		
		#endregion // Constructors
		
		#region Properties

		[TypeConverterAttribute (typeof (OleDbException.ErrorCodeConverter))]
		public override int ErrorCode {
			get {
				GdaList glist;
				IntPtr errors;

				errors = libgda.gda_connection_get_errors (connection.GdaConnection);
				if (errors != IntPtr.Zero) {
					glist = (GdaList) Marshal.PtrToStructure (errors, typeof (GdaList));
					return (int) libgda.gda_error_get_number (glist.data);
				}

				return -1;
			}
		}
		
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Content)]
		public OleDbErrorCollection Errors {
			get {
				GdaList glist;
				IntPtr errors;
				OleDbErrorCollection col = new OleDbErrorCollection ();
				
				errors = libgda.gda_connection_get_errors (connection.GdaConnection);
				if (errors != IntPtr.Zero) {
					glist = (GdaList) Marshal.PtrToStructure (errors, typeof (GdaList));
					while (glist != null) {
						col.Add (new OleDbError (
								 libgda.gda_error_get_description (glist.data),
								 (int) libgda.gda_error_get_number (glist.data),
								 libgda.gda_error_get_source (glist.data),
								 libgda.gda_error_get_sqlstate (glist.data)));
						glist = (GdaList) Marshal.PtrToStructure (glist.next,
											  typeof (GdaList));
					}
				}

				return col;
			}
		}

		new public string Message {
			get {
				GdaList glist;
				IntPtr errors;
				string msg = "";

				errors = libgda.gda_connection_get_errors (connection.GdaConnection);
				if (errors != IntPtr.Zero) {
					glist = (GdaList) Marshal.PtrToStructure (errors, typeof (GdaList));
					while (glist != null) {
						msg = msg + ";" +  libgda.gda_error_get_description (glist.data);
						glist = (GdaList) Marshal.PtrToStructure (glist.next,
											  typeof (GdaList));
					}

					return msg;
				}

				return null;
			}
		}

		new public string Source {
			get {
				GdaList glist;
				IntPtr errors;

				errors = libgda.gda_connection_get_errors (connection.GdaConnection);
				if (errors != IntPtr.Zero) {
					glist = (GdaList) Marshal.PtrToStructure (errors, typeof (GdaList));
					return libgda.gda_error_get_source (glist.data);
				}

				return null;
			}
		}

		#endregion // Properties

		#region Methods

		public override void GetObjectData (SerializationInfo si, StreamingContext context)
		{
			if (si == null)
				throw new ArgumentNullException ("si");

			si.AddValue ("connection", connection);
			base.GetObjectData (si, context);
			throw new NotImplementedException ();
		}

		#endregion // Methods

		internal sealed class ErrorCodeConverter : Int32Converter
		{
			[MonoTODO]
			public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
				return base.ConvertTo (context, culture, value, destinationType);
			}
		}
	}
}
