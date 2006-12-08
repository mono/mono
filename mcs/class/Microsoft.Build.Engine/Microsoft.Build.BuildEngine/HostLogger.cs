//
// HostLogger.cs:
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
// 
// (C) 2005 Marek Sieradzki
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

#if NET_2_0

using System;
using Microsoft.Build.Framework;

namespace Microsoft.Build.BuildEngine {

	[MonoTODO]
	public class HostLogger : ILogger, IHostLogger {
		
		IHostFeedback	hostFeedbackObject;
		string		parameters;
		LoggerVerbosity	verbosity;
	
		public HostLogger ()
		{
		}

		public void Initialize (IEventSource eventSource)
		{
		}

		public void Shutdown ()
		{
		}

		public IHostFeedback HostFeedbackObject {
			get {
				return hostFeedbackObject;
			}
			set {
				hostFeedbackObject = value;
			}
		}

		public string Parameters {
			get {
				return parameters;
			}
			set {
				parameters = value;
			}
		}

		public LoggerVerbosity Verbosity {
			get {
				return verbosity;
			}
			set {
				verbosity = value;
			}
		}
	}
}

#endif
