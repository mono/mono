// 
// System.Web.Services.Protocols.MimeParameterWriter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.IO;
using System.Net;
using System.Text;
using System.Web.Services;

namespace System.Web.Services.Protocols {
	public abstract class MimeParameterWriter : MimeFormatter {

		#region Constructors

		[MonoTODO]
		protected MimeParameterWriter () 
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Properties 

		public virtual Encoding RequestEncoding {
			[MonoTODO]
			get { throw new NotImplementedException (); }
			[MonoTODO]
			set { throw new NotImplementedException (); }
		}

		public virtual bool UsesWriteRequest {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public virtual string GetRequestUrl (string url, object[] parameters)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void InitializeRequest (WebRequest request, object[] values)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void WriteRequest (Stream requestStream, object[] values)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
