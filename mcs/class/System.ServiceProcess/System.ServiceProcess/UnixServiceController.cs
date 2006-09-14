//
// System.ServiceProcess.UnixServiceController
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
	internal class UnixServiceController : ServiceControllerImpl
	{
		public UnixServiceController (ServiceController serviceController)
			: base (serviceController)
		{
		}

		public override bool CanPauseAndContinue {
			get {
				throw new NotImplementedException ();
			}
		}

		public override bool CanShutdown {
			get {
				throw new NotImplementedException ();
			}
		}

		public override bool CanStop {
			get {
				throw new NotImplementedException ();
			}
		}

		public override ServiceController [] DependentServices {
			get {
				throw new NotImplementedException ();
			}
		}

		public override string DisplayName {
			get {
				throw new NotImplementedException ();
			}
		}

		public override string ServiceName {
			get {
				throw new NotImplementedException ();
			}
		}

		public override ServiceController [] ServicesDependedOn {
			get {
				throw new NotImplementedException ();
			}
		}

		public override ServiceType ServiceType {
			get {
				throw new NotImplementedException ();
			}
		}

		public override ServiceControllerStatus Status {
			get {
				throw new NotImplementedException ();
			}
		}

		public override void Close ()
		{
			throw new NotImplementedException ();
		}

		public override void Continue ()
		{
			throw new NotImplementedException ();
		}

		public override void Dispose (bool disposing)
		{
		}

		public override void ExecuteCommand (int command)
		{
			throw new NotImplementedException ();
		}

		public override ServiceController [] GetDevices ()
		{
			throw new NotImplementedException ();
		}

		public override ServiceController [] GetServices ()
		{
			throw new NotImplementedException ();
		}

		public override void Pause ()
		{
			throw new NotImplementedException ();
		}

		public override void Refresh ()
		{
			throw new NotImplementedException ();
		}

		public override void Start (string [] args)
		{
			throw new NotImplementedException ();
		}

		public override void Stop ()
		{
			throw new NotImplementedException ();
		}
	}
}
