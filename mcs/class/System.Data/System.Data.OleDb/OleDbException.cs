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

using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Data.OleDb
{
	[Serializable]
	public sealed class OleDbException : ExternalException
	{
		private OleDbConnection connection;
		
		#region Constructors

		internal OleDbException (OleDbConnection cnc)
		{
			connection = cnc;
		}
		
		#endregion // Constructors
		
		#region Properties

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

		public override string Message {	
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

		public override string Source {	
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

		[MonoTODO]
		public override void GetObjectData (SerializationInfo si, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
