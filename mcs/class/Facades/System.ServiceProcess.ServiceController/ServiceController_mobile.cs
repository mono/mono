//
// ServiceController_mobile.cs
//
// Author:
//   Alexander Köplinger (alexander.koeplinger@xamarin.com)
//
// (C) 2016 Xamarin, Inc.
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

#if MOBILE || XAMMAC_4_5

using System;
using System.Runtime.InteropServices;

namespace System.ServiceProcess
{
	public class ServiceController : IDisposable
	{
		[MonoTODO]
		public bool CanPauseAndContinue
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool CanShutdown
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public bool CanStop
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public ServiceController[] DependentServices
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string DisplayName
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string MachineName
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public SafeHandle ServiceHandle
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string ServiceName
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public ServiceController[] ServicesDependedOn
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public ServiceType ServiceType
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public ServiceStartMode StartType
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public ServiceControllerStatus Status
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public ServiceController (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ServiceController (string name, string machineName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Continue ()
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
		public static ServiceController[] GetDevices ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ServiceController[] GetDevices (string machineName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ServiceController[] GetServices ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static ServiceController[] GetServices (string machineName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Pause ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Refresh ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Start ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Start (string[] args)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Stop ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void WaitForStatus (ServiceControllerStatus desiredStatus)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void WaitForStatus (ServiceControllerStatus desiredStatus, TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif