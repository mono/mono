// 
// System.Web.Services.Protocols.XmlReturnWriter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.IO;
using System.Web;

namespace System.Web.Services.Protocols {
	internal class XmlReturnWriter : MimeReturnWriter {

		#region Methods

		[MonoTODO]
                public override object GetInitializer (LogicalMethodInfo methodInfo)
		{
			throw new NotImplementedException ();
		}

                [MonoTODO]
                public override object[] GetInitializers (LogicalMethodInfo[] methodInfos)
                {
                        throw new NotImplementedException ();
                }

		[MonoTODO]
                public override void Initialize (object initializer) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Write (HttpResponse response, Stream outputStream, object returnValue)
		{
			//serializer.Serialize (textWriter, o)
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
