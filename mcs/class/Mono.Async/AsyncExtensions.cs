//
// AsyncExtensions.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright 2011 Xamarin, Inc (http://www.xamarin.com)
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

using System.IO;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

public static class AsyncExtensions
{
	public static Task<Socket> AcceptSocketAsync (this TcpListener source)
	{
		return Task<Socket>.Factory.FromAsync (source.BeginAcceptSocket, source.EndAcceptSocket, null);
	}

	public static Task<TcpClient> AcceptTcpClientAsync(this TcpListener source)
	{
		return Task<TcpClient>.Factory.FromAsync (source.BeginAcceptTcpClient, source.EndAcceptTcpClient, null);
	}

	public static Task ConnectAsync (this TcpClient source, IPAddress address, int port)
	{
		return Task.Factory.FromAsync (source.BeginConnect, source.EndConnect, address, port, null);
	}

	public static Task ConnectAsync (this TcpClient source, IPAddress[] ipAddresses, int port)
	{
		return Task.Factory.FromAsync (source.BeginConnect, source.EndConnect, ipAddresses, port, null);
	}

	public static Task ConnectAsync (this TcpClient source, string hostname, int port)
	{
		return Task.Factory.FromAsync (source.BeginConnect, source.EndConnect, hostname, port, null);
	}

}
