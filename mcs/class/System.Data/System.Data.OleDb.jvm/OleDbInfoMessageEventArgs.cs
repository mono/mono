//
// System.Data.OleDb.OleDbInfoMessageEventArgs
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
//

using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	public sealed class OleDbInfoMessageEventArgs : EventArgs
	{
		private OleDbErrorCollection errors;
		internal OleDbInfoMessageEventArgs(OleDbErrorCollection errors)
		{
			this.errors = errors;
		}
		#region Properties

		public int ErrorCode {
			[MonoTODO]
			get { return errors[0].NativeError; }
		}

		public OleDbErrorCollection Errors {
			[MonoTODO]
			get { return errors; }
		}

		public string Message {
			[MonoTODO]
			get { return errors[0].Message; }
		}

		public string Source {
			[MonoTODO]
			get { return errors[0].Source;}
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override string ToString ()
		{
			return errors[0].Message;
		}

		#endregion // Methods
	}
}
