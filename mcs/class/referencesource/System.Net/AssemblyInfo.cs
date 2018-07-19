//------------------------------------------------------------------------------
// <copyright file="Peer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
using System;
using System.Security.Permissions;
using System.Security;
using System.Runtime.CompilerServices;

// These types exist in System.Net.dll in Silverlight but in System.dll in the Framework.
// Forward them so people can build portable libraries without changing assembly references.
[assembly: TypeForwardedToAttribute(typeof(System.Net.Cookie))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.CookieCollection))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.CookieContainer))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.CookieException))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.DownloadProgressChangedEventArgs))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.DownloadProgressChangedEventHandler))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.DownloadStringCompletedEventArgs))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.DownloadStringCompletedEventHandler))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.HttpRequestHeader))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.HttpStatusCode))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.HttpWebRequest))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.HttpWebResponse))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.ICredentials))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.IWebRequestCreate))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.NetworkCredential))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.OpenReadCompletedEventArgs))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.OpenReadCompletedEventHandler))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.OpenWriteCompletedEventArgs))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.OpenWriteCompletedEventHandler))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.ProtocolViolationException))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.UploadProgressChangedEventArgs))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.UploadProgressChangedEventHandler))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.UploadStringCompletedEventArgs))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.UploadStringCompletedEventHandler))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.WebClient))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.WebException))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.WebExceptionStatus))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.WebHeaderCollection))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.WebRequest))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.WebResponse))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.NetworkInformation.NetworkAddressChangedEventHandler))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.NetworkInformation.NetworkChange))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.NetworkInformation.NetworkInterface))]

[assembly: TypeForwardedToAttribute(typeof(System.Net.DnsEndPoint))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.EndPoint))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.IPAddress))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.IPEndPoint))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.SocketAddress))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.Sockets.AddressFamily))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.Sockets.ProtocolType))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.Sockets.Socket))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.Sockets.SocketAsyncEventArgs))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.Sockets.SocketAsyncOperation))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.Sockets.SocketError))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.Sockets.SocketException))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.Sockets.SocketShutdown))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.Sockets.SocketType))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.WriteStreamClosedEventArgs))]
[assembly: TypeForwardedToAttribute(typeof(System.Net.WriteStreamClosedEventHandler))]


[assembly: SecurityCritical]
#pragma warning disable 618
[assembly:SecurityPermission(SecurityAction.RequestMinimum, Execution = true)]
#pragma warning restore 618
namespace System.Net.PeerToPeer
{
}
