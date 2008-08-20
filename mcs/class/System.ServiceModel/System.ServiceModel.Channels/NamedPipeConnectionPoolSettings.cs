//
// NamedPipeConnectionPoolSettings.cs
//
// Authors:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc.  http://www.novell.com
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
using System.Collections.Generic;
using System.Net;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

namespace System.ServiceModel.Channels
{
	public sealed class NamedPipeConnectionPoolSettings
	{
		internal NamedPipeConnectionPoolSettings ()
		{
		}

		string group_name = "default";
		TimeSpan idle_timeout = TimeSpan.FromSeconds (120);
		int max_conn = 10;

		internal void CopyPropertiesFrom (NamedPipeConnectionPoolSettings other)
		{
			group_name = other.group_name;
			idle_timeout = other.idle_timeout;
			max_conn = other.max_conn;
		}

		[MonoTODO]
		public string GroupName {
			get { return group_name; }
			set { group_name = value; }
		}

		[MonoTODO]
		public TimeSpan IdleTimeout {
			get { return idle_timeout; }
			set { idle_timeout = value; }
		}

		[MonoTODO]
		public int MaxOutboundConnectionsPerEndpoint {
			get { return max_conn; }
			set { max_conn = value; }
		}
	}
}
