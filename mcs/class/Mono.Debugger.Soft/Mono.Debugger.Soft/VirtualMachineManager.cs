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

		public static IAsyncResult BeginLaunch (ProcessStartInfo info, AsyncCallback callback, LaunchOptions options = null) {
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

			AsyncResult async = (AsyncResult) asyncResult;
			LaunchCallback cb = (LaunchCallback) async.AsyncDelegate;
			return cb.EndInvoke (asyncResult);
		}

		public static VirtualMachine Launch (ProcessStartInfo info, LaunchOptions options = null) {
			return EndLaunch (BeginLaunch (info, null, options));
		}

		public static VirtualMachine Launch (string[] args, LaunchOptions options = null) {
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

		public static IAsyncResult BeginListen (IPEndPoint dbg_ep, IPEndPoint con_ep, AsyncCallback callback) {
			Socket dbg_sock = null;
			Socket con_sock = null;

			dbg_sock = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			dbg_sock.Bind (dbg_ep);
			dbg_sock.Listen (1000);

			if (con_ep != null) {
				con_sock = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				con_sock.Bind (con_ep);
				con_sock.Listen (1000);
			}
			
			ListenCallback c = new ListenCallback (ListenInternal);
			return c.BeginInvoke (dbg_sock, con_sock, callback, con_sock ?? dbg_sock);
		}

		public static VirtualMachine EndListen (IAsyncResult asyncResult) {
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			if (!asyncResult.IsCompleted)
				asyncResult.AsyncWaitHandle.WaitOne ();

			AsyncResult async = (AsyncResult) asyncResult;
			ListenCallback cb = (ListenCallback) async.AsyncDelegate;
			return cb.EndInvoke (asyncResult);
		}

		public static VirtualMachine Listen (IPEndPoint dbg_ep, IPEndPoint con_ep = null) { 
			return EndListen (BeginListen (dbg_ep, con_ep, null));
		}

		/*
		 * Connect to a virtual machine listening at the specified address.
		 */
		public static VirtualMachine Connect (IPEndPoint endpoint) {
			if (endpoint == null)
				throw new ArgumentNullException ("endpoint");

			Socket socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.Connect (endpoint);

			Connection conn = new Connection (socket);

			VirtualMachine vm = new VirtualMachine (null, conn);

			conn.EventHandler = new EventHandler (vm);

			vm.connect ();

			return vm;
		}
	}
}
