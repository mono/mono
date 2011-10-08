//
// Mono.Net.Dns.SimpleResolverEventArgs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo.mono@gmail.com)
//
// Copyright 2011 Gonzalo Paniagua Javier
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
using System;
using System.Net;
using System.Threading;

namespace Mono.Net.Dns {
	class SimpleResolverEventArgs : EventArgs {
		public event EventHandler<SimpleResolverEventArgs> Completed;

		public SimpleResolverEventArgs ()
		{
		}

		public ResolverError ResolverError { get; set; }
		public string ErrorMessage { get; set; }
		public ResolverAsyncOperation LastOperation;
		public string HostName { get; set; }
		public IPHostEntry HostEntry { get; internal set; }
		public object UserToken { get; set; }
		internal ushort QueryID;
		internal ushort Retries;
		internal Timer Timer;
		internal IPAddress PTRAddress;

		internal void Reset (ResolverAsyncOperation op)
		{
			ResolverError = 0;
			ErrorMessage = null;
			HostEntry = null;
			LastOperation = op;
			QueryID = 0;
			Retries = 0;
			PTRAddress = null;
		}

		protected internal void OnCompleted (object sender)
		{
			var handler = Completed;
			if (handler != null)
				handler (sender, this);
		}
	}
}

