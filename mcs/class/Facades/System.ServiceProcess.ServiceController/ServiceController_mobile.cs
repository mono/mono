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
		public bool CanPauseAndContinue
		{
			get
			{
				throw new PlatformNotSupportedException ();
			}
		}

		public bool CanShutdown
		{
			get
			{
				throw new PlatformNotSupportedException ();
			}
		}

		public bool CanStop
		{
			get
			{
				throw new PlatformNotSupportedException ();
			}
		}

		public ServiceController[] DependentServices
		{
			get
			{
				throw new PlatformNotSupportedException ();
			}
		}

		public string DisplayName
		{
			get
			{
				throw new PlatformNotSupportedException ();
			}
		}

		public string MachineName
		{
			get
			{
				throw new PlatformNotSupportedException ();
			}
		}

		public SafeHandle ServiceHandle
		{
			get
			{
				throw new PlatformNotSupportedException ();
			}
		}

		public string ServiceName
		{
			get
			{
				throw new PlatformNotSupportedException ();
			}
		}

		public ServiceController[] ServicesDependedOn
		{
			get
			{
				throw new PlatformNotSupportedException ();
			}
		}

		public ServiceType ServiceType
		{
			get
			{
				throw new PlatformNotSupportedException ();
			}
		}

		public ServiceStartMode StartType
		{
			get
			{
				throw new PlatformNotSupportedException ();
			}
		}

		public ServiceControllerStatus Status
		{
			get
			{
				throw new PlatformNotSupportedException ();
			}
		}

		public ServiceController (string name)
		{
			throw new PlatformNotSupportedException ();
		}

		public ServiceController (string name, string machineName)
		{
			throw new PlatformNotSupportedException ();
		}

		public void Continue ()
		{
			throw new PlatformNotSupportedException ();
		}

		public void Dispose ()
		{
			throw new PlatformNotSupportedException ();
		}

		protected virtual void Dispose (bool disposing)
		{
			throw new PlatformNotSupportedException ();
		}

		public static ServiceController[] GetDevices ()
		{
			throw new PlatformNotSupportedException ();
		}

		public static ServiceController[] GetDevices (string machineName)
		{
			throw new PlatformNotSupportedException ();
		}

		public static ServiceController[] GetServices ()
		{
			throw new PlatformNotSupportedException ();
		}

		public static ServiceController[] GetServices (string machineName)
		{
			throw new PlatformNotSupportedException ();
		}

		public void Pause ()
		{
			throw new PlatformNotSupportedException ();
		}

		public void Refresh ()
		{
			throw new PlatformNotSupportedException ();
		}

		public void Start ()
		{
			throw new PlatformNotSupportedException ();
		}

		public void Start (string[] args)
		{
			throw new PlatformNotSupportedException ();
		}

		public void Stop ()
		{
			throw new PlatformNotSupportedException ();
		}

		public void WaitForStatus (ServiceControllerStatus desiredStatus)
		{
			throw new PlatformNotSupportedException ();
		}

		public void WaitForStatus (ServiceControllerStatus desiredStatus, TimeSpan timeout)
		{
			throw new PlatformNotSupportedException ();
		}
	}
}

#endif