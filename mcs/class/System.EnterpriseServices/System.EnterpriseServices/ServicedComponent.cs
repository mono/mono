// 
// System.EnterpriseServices.ServicedComponent.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

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
