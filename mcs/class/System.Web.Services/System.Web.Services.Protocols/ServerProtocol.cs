// 
// System.Web.Services.Protocols.ServerProtocol.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;

namespace System.Web.Services.Protocols {
	[MonoTODO ("Figure out what thsi class does.")]
	internal abstract class ServerProtocol {

		#region Constructors

		protected ServerProtocol ()
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region Properties

		public virtual bool IsOneWay {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public virtual LogicalMethodInfo MethodInfo {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion

		#region Methods

		[MonoTODO]
		public virtual void CreateServerInstance ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void DisposeServerInstance ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Initialize ()
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}
