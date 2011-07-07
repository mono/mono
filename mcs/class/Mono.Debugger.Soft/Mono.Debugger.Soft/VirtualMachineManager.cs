using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;

namespace Mono.Debugger.Soft
{
	public class LaunchOptions {
		public string AgentArgs {
			get; set;
		}

		public bool Valgrind {
			get; set;
		}
		
		public ProcessLauncher CustomProcessLauncher {
			get; set;
		}

		public TargetProcessLauncher CustomTargetProcessLauncher {
			get; set;
		}

		public delegate Process ProcessLauncher (ProcessStartInfo info);
		public delegate ITargetProcess TargetProcessLauncher (ProcessStartInfo info);
	}

	public class VirtualMachineManager
	{
		private delegate VirtualMachine LaunchCallback (ITargetProcess p, ProcessStartInfo info, Socket socket);
		private delegate VirtualMachine ListenCallback (Socket dbg_sock, Socket con_sock); 
		private delegate VirtualMachine ConnectCallback (Socket dbg_sock, Socket con_sock, IPEndPoint dbg_ep, IPEndPoint con_ep); 

		internal VirtualMachineManager () {
		}

		public static VirtualMachine LaunchInternal (Process p, ProcessStartInfo info, Socket socket)
		{
			return LaunchInternal (new ProcessWrapper (p), info, socket);
		}
			
		public static VirtualMachine LaunchInternal (ITargetProcess p, ProcessStartInfo info, Socket socket) {
			Socket accepted = null;
			try {
				accepted = socket.Accept ();
			} catch (Exception) {
				throw;
			}

			Connection conn = new Connection (accepted);

			VirtualMachine vm = new VirtualMachine (p, conn);

			if (info.RedirectStandardOutput)
				vm.StandardOutput = p.StandardOutput;
			
			if (info.RedirectStandardError)
				vm.StandardError = p.StandardError;

			conn.EventHandler = new EventHandler (vm);

			vm.connect ();

			return vm;
		}

		public static IAsyncResult BeginLaunch (ProcessStartInfo info, AsyncCallback callback)
		{
			return BeginLaunch (info, callback, null);
		}

		public static IAsyncResult BeginLaunch (ProcessStartInfo info, AsyncCallback callback, LaunchOptions options)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			Socket socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.Bind (new IPEndPoint (IPAddress.Loopback, 0));
			socket.Listen (1000);
			IPEndPoint ep = (IPEndPoint) socket.LocalEndPoint;

			// We need to inject our arguments into the psi
			info.Arguments = string.Format ("{0} --debug --debugger-agent=transport=dt_socket,address={1}:{2}{3} {4}", 
								options == null || !options.Valgrind ? "" : info.FileName,
								ep.Address,
								ep.Port,
								options == null || options.AgentArgs == null ? "" : "," + options.AgentArgs,
								info.Arguments);

			if (options != null && options.Valgrind)
				info.FileName = "valgrind";
				
			ITargetProcess p;
			if (options != null && options.CustomProcessLauncher != null)
				p = new ProcessWrapper (options.CustomProcessLauncher (info));
			else if (options != null && options.CustomTargetProcessLauncher != null)
				p = options.CustomTargetProcessLauncher (info);
			else
				p = new ProcessWrapper (Process.Start (info));
			
			p.Exited += delegate (object sender, EventArgs eargs) {
				socket.Close ();
			};

			LaunchCallback c = new LaunchCallback (LaunchInternal);
			return c.BeginInvoke (p, info, socket, callback, socket);
		}

		public static VirtualMachine EndLaunch (IAsyncResult asyncResult) {
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			if (!asyncResult.IsCompleted)
				asyncResult.AsyncWaitHandle.WaitOne ();

			AsyncResult result = (AsyncResult) asyncResult;
			LaunchCallback cb = (LaunchCallback) result.AsyncDelegate;
			return cb.EndInvoke (asyncResult);
		}

		public static VirtualMachine Launch (ProcessStartInfo info)
		{
			return Launch (info, null);
		}

		public static VirtualMachine Launch (ProcessStartInfo info, LaunchOptions options)
		{
			return EndLaunch (BeginLaunch (info, null, options));
		}

		public static VirtualMachine Launch (string[] args)
		{
			return Launch (args, null);
		}

		public static VirtualMachine Launch (string[] args, LaunchOptions options)
		{
			ProcessStartInfo pi = new ProcessStartInfo ("mono");
			pi.Arguments = String.Join (" ", args);

			return Launch (pi, options);
		}
			
		public static VirtualMachine ListenInternal (Socket dbg_sock, Socket con_sock) {
			Socket con_acc = null;
			Socket dbg_acc = null;

			if (con_sock != null) {
				try {
					con_acc = con_sock.Accept ();
				} catch (Exception) {
					try {
						dbg_sock.Close ();
					} catch {}
					throw;
				}
			}
						
			try {
				dbg_acc = dbg_sock.Accept ();
			} catch (Exception) {
				if (con_sock != null) {
					try {
						con_sock.Close ();
						con_acc.Close ();
					} catch {}
				}
				throw;
			}

			if (con_sock != null) {
				if (con_sock.Connected)
					con_sock.Disconnect (false);
				con_sock.Close ();
			}

			if (dbg_sock.Connected)
				dbg_sock.Disconnect (false);
			dbg_sock.Close ();

			Connection conn = new Connection (dbg_acc);

			VirtualMachine vm = new VirtualMachine (null, conn);

			if (con_acc != null) {
				vm.StandardOutput = new StreamReader (new NetworkStream (con_acc));
				vm.StandardError = null;
			}

			conn.EventHandler = new EventHandler (vm);

			vm.connect ();

			return vm;
		}

		public static IAsyncResult BeginListen (IPEndPoint dbg_ep, AsyncCallback callback) {
			return BeginListen (dbg_ep, null, callback);
		}
		
		public static IAsyncResult BeginListen (IPEndPoint dbg_ep, IPEndPoint con_ep, AsyncCallback callback)
		{
			int dbg_port, con_port;
			return BeginListen (dbg_ep, con_ep, callback, out dbg_port, out con_port);
		}

		public static IAsyncResult BeginListen (IPEndPoint dbg_ep, IPEndPoint con_ep, AsyncCallback callback,
			out int dbg_port, out int con_port)
		{
			dbg_port = con_port = 0;
			
			Socket dbg_sock = null;
			Socket con_sock = null;

			dbg_sock = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			dbg_sock.Bind (dbg_ep);
			dbg_sock.Listen (1000);
			dbg_port = ((IPEndPoint) dbg_sock.LocalEndPoint).Port;

			if (con_ep != null) {
				con_sock = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				con_sock.Bind (con_ep);
				con_sock.Listen (1000);
				con_port = ((IPEndPoint) con_sock.LocalEndPoint).Port;
			}
			
			ListenCallback c = new ListenCallback (ListenInternal);
			return c.BeginInvoke (dbg_sock, con_sock, callback, con_sock ?? dbg_sock);
		}

		public static VirtualMachine EndListen (IAsyncResult asyncResult) {
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			if (!asyncResult.IsCompleted)
				asyncResult.AsyncWaitHandle.WaitOne ();

			AsyncResult result = (AsyncResult) asyncResult;
			ListenCallback cb = (ListenCallback) result.AsyncDelegate;
			return cb.EndInvoke (asyncResult);
		}

		public static VirtualMachine Listen (IPEndPoint dbg_ep)
		{
			return Listen (dbg_ep, null);
		}

		public static VirtualMachine Listen (IPEndPoint dbg_ep, IPEndPoint con_ep)
		{
			return EndListen (BeginListen (dbg_ep, con_ep, null));
		}

		/*
		 * Connect to a virtual machine listening at the specified address.
		 */
		public static VirtualMachine Connect (IPEndPoint endpoint) {
			return Connect (endpoint, null);
		}

		public static VirtualMachine Connect (IPEndPoint endpoint, IPEndPoint consoleEndpoint) { 
			if (endpoint == null)
				throw new ArgumentNullException ("endpoint");

			return EndConnect (BeginConnect (endpoint, consoleEndpoint, null));
		}

		public static VirtualMachine ConnectInternal (Socket dbg_sock, Socket con_sock, IPEndPoint dbg_ep, IPEndPoint con_ep) {
			if (con_sock != null) {
				try {
					con_sock.Connect (con_ep);
				} catch (Exception) {
					try {
						dbg_sock.Close ();
					} catch {}
					throw;
				}
			}
						
			try {
				dbg_sock.Connect (dbg_ep);
			} catch (Exception) {
				if (con_sock != null) {
					try {
						con_sock.Close ();
					} catch {}
				}
				throw;
			}

			Connection conn = new Connection (dbg_sock);

			VirtualMachine vm = new VirtualMachine (null, conn);

			if (con_sock != null) {
				vm.StandardOutput = new StreamReader (new NetworkStream (con_sock));
				vm.StandardError = null;
			}

			conn.EventHandler = new EventHandler (vm);

			vm.connect ();

			return vm;
		}

		public static IAsyncResult BeginConnect (IPEndPoint dbg_ep, AsyncCallback callback) {
			return BeginConnect (dbg_ep, null, callback);
		}

		public static IAsyncResult BeginConnect (IPEndPoint dbg_ep, IPEndPoint con_ep, AsyncCallback callback) {
			Socket dbg_sock = null;
			Socket con_sock = null;

			dbg_sock = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			if (con_ep != null) {
				con_sock = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			}
			
			ConnectCallback c = new ConnectCallback (ConnectInternal);
			return c.BeginInvoke (dbg_sock, con_sock, dbg_ep, con_ep, callback, con_sock ?? dbg_sock);
		}

		public static VirtualMachine EndConnect (IAsyncResult asyncResult) {
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			if (!asyncResult.IsCompleted)
				asyncResult.AsyncWaitHandle.WaitOne ();

			AsyncResult result = (AsyncResult) asyncResult;
			ConnectCallback cb = (ConnectCallback) result.AsyncDelegate;
			return cb.EndInvoke (asyncResult);
		}

		public static void CancelConnection (IAsyncResult asyncResult)
		{
			((Socket)asyncResult.AsyncState).Close ();
		}
	}
}
