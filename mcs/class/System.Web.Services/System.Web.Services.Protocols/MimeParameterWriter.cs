// 
// System.Web.Services.Protocols.MimeParameterWriter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
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

		protected MimeParameterWriter () 
		{
		}
		
		#endregion // Constructors

		#region Properties 

		public virtual Encoding RequestEncoding {
			get { return null; }
			set { ; }
		}

		public virtual bool UsesWriteRequest {
			get { return false; }
		}

		#endregion // Properties

		#region Methods

		public virtual string GetRequestUrl (string url, object[] parameters)
		{
			return url;
		}

		public virtual void InitializeRequest (WebRequest request, object[] values)
		{
		}

		public virtual void WriteRequest (Stream requestStream, object[] values)
		{
		}

		#endregion // Methods
	}
}
