//
// LdapDirectoryIdentifier.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.
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

namespace System.DirectoryServices.Protocols
{
	public class LdapDirectoryIdentifier : DirectoryIdentifier
	{
		public LdapDirectoryIdentifier (string server)
		{
			Servers = new string [] {server};
		}

		public LdapDirectoryIdentifier (string server, int portNumber)
			: this (server)
		{
			PortNumber = portNumber;
		}

		public LdapDirectoryIdentifier (string server, bool fullyQualifiedDnsHostName, bool connectionless)
			: this (server)
		{
			FullyQualifiedDnsHostName = fullyQualifiedDnsHostName;
			Connectionless = connectionless;
		}

		public LdapDirectoryIdentifier (string [] servers, bool fullyQualifiedDnsHostName, bool connectionless)
		{
			Servers = servers;
			FullyQualifiedDnsHostName = fullyQualifiedDnsHostName;
			Connectionless = connectionless;
		}

		public LdapDirectoryIdentifier (string server, int portNumber, bool fullyQualifiedDnsHostName, bool connectionless)
			: this (new string [] {server}, portNumber, fullyQualifiedDnsHostName, connectionless)
		{
		}

		public LdapDirectoryIdentifier (string [] servers, int portNumber, bool fullyQualifiedDnsHostName, bool connectionless)
			: this (servers, fullyQualifiedDnsHostName, connectionless)
		{
			PortNumber = portNumber;
		}

		public bool Connectionless { get; private set; }
		public bool FullyQualifiedDnsHostName { get; private set; }
		public int PortNumber { get; private set; }
		public string [] Servers { get; private set; }
	}
}
