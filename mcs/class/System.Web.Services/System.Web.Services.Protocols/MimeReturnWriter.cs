// 
// System.Web.Services.Protocols.MimeReturnWriter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.IO;
using System.Web;

namespace System.Web.Services.Protocols {
	internal abstract class MimeReturnWriter : MimeFormatter {

		#region Methods 

		public abstract void Write (HttpResponse response, Stream outputStream, object returnValue);

		#endregion // Methods
	}
}
