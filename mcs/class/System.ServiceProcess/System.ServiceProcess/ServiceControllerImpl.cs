//
// System.ServiceProcess.ServiceControllerImpl
//
// Author:
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

namespace System.ServiceProcess
{
	internal abstract class ServiceControllerImpl
	{
		protected ServiceControllerImpl (ServiceController serviceController)
		{
			_serviceController = serviceController;
		}

		protected ServiceController ServiceController {
			get {
				return _serviceController;
			}
		}

		public abstract bool CanPauseAndContinue {
			get;
		}

		public abstract bool CanShutdown {
			get;
		}

		public abstract bool CanStop {
			get;
		}

		public abstract ServiceController [] DependentServices {
			get;
		}

		public abstract string DisplayName {
			get;
		}

		public abstract string ServiceName {
			get;
		}

		public abstract ServiceController [] ServicesDependedOn {
			get;
		}

		public abstract ServiceType ServiceType {
			get;
		}

		public abstract ServiceControllerStatus Status
		{
			get;
		}

		public abstract void Close ();

		public abstract void Continue ();

		public abstract void Dispose (bool disposing);

		public abstract void ExecuteCommand (int command);

		public abstract ServiceController [] GetDevices ();

		public abstract ServiceController [] GetServices ();

		public abstract void Pause ();

		public abstract void Refresh ();

		public abstract void Start (string [] args);

		public abstract void Stop ();

		private readonly ServiceController _serviceController;
	}
}
