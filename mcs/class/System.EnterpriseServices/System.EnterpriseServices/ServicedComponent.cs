// 
// System.EnterpriseServices.ServicedComponent.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;

namespace System.EnterpriseServices {
	[Serializable]
	public abstract class ServicedComponent : ContextBoundObject, IDisposable, IRemoteDispatch, IServicedComponentInfo {

		#region Constructors

		public ServicedComponent ()
		{
		}

		#endregion

		#region Methods

		[MonoTODO]
		protected internal virtual void Activate ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual bool CanBePooled ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual void Construct (string s)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual void Deactivate ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void DisposeObject (ServicedComponent sc)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		string IRemoteDispatch.RemoteDispatchAutoDone (string s)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		string IRemoteDispatch.RemoteDispatchNotAutoDone (string s)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IServicedComponentInfo.GetComponentInfo (ref int infoMask, out string[] infoArray)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
