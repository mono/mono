// 
// System.Web.Services.Protocols.HttpServerProtocol.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.IO;
using System.Web.Services;

namespace System.Web.Services.Protocols {
	[MonoTODO ("Figure out what this class does.")]
	internal abstract class HttpServerProtocol : ServerProtocol {

		#region Constructors

		[MonoTODO ("Is the bool parameter the one way?")]
		protected HttpServerProtocol (bool isOneWay)
		{
			throw new NotImplementedException ();
		}

		#endregion // Constructors

		#region Properties

		[MonoTODO]
		public override bool IsOneWay {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override LogicalMethodInfo MethodInfo {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public static bool AreUrlParametersSupported (LogicalMethodInfo methodInfo)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool Initialize () 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override object[] ReadParameters ()
		{
			throw new NotImplementedException ();
		}
                
		[MonoTODO]
		public override bool WriteException (Exception e, Stream outputStream)
                {
                        throw new NotImplementedException ();
                }

		[MonoTODO]
                public override void WriteReturns (object[] returnValues, Stream outputStream)
                {
			//xmlReturnWriter.Write (Response, outputStream, returnValue);
                        throw new NotImplementedException ();
                }

		#endregion // Methods
	}
}
