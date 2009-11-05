using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Mono.Debugger
{
	public class LaunchOptions {
		public string Runtime {
			get; set;
		}

		public bool RedirectStandardOutput {
			get; set;
		}

		public bool RedirectStandardError {
			get; set;
		}

		public string AgentArgs {
			get; set;
		}

		public bool Valgrind {
			get; set;
		}
	}

	public class VirtualMachineManager
	{
		internal VirtualMachineManager () {
		}

		/*
		 * Launch a new virtual machine with the provided arguments.
		 */
		public static VirtualMachine Launch (string[] args, LaunchOptions options = null) {
			if (args == null)
				throw new ArgumentNullException ("args");
			Socket socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.Bind (new IPEndPoint (IPAddress.Loopback, 0));
			socket.Listen (1000);

			IPEndPoint endPoint = (IPEndPoint)socket.LocalEndPoint;
			string addressString = endPoint.Address.ToString () + ":" + endPoint.Port.ToString ();

			string runtime = "mono";
			bool valgrind = options != null && options.Valgrind;

			string extra_args = "";

			if (options != null && options.Runtime != null)
				runtime = options.Runtime;
			if (options != null && options.AgentArgs != null)
				extra_args = "," + options.AgentArgs;

			string agent_args = "--debug --debugger-agent=transport=dt_socket,address=" + addressString + extra_args;
			if (valgrind)
				agent_args = runtime + " " + agent_args;

			ProcessStartInfo start_info = new ProcessStartInfo ();
			start_info.FileName = valgrind ? "valgrind" : runtime;
			start_info.Arguments = agent_args + " " + String.Join (" ", args);
			start_info.UseShellExecute = false;
			if (options != null && options.RedirectStandardOutput)
				start_info.RedirectStandardOutput = true;
			if (options != null && options.RedirectStandardError)
				start_info.RedirectStandardError = true;
			Process p = Process.Start (start_info);
			bool exited = false;
			/* Handle the debuggee exiting so we don't block in Accept () forever */
			p.Exited += delegate (object sender, EventArgs eargs) {
				exited = true;
				socket.Shutdown (SocketShutdown.Both);
			};

			/* 
			 * Wait until we are connected to the debuggee so the caller gets 
			 * back a fully usable vm object.
			 * FIXME: This might block forever.
			 */
			Socket accepted = null;
			try {
				accepted = socket.Accept ();
			} catch (SocketException) {
				if (exited)
					throw new IOException ("Debuggee process exited.");
				else
					throw;
			}

			Connection conn = new Connection (accepted);

			VirtualMachine vm = new VirtualMachine (p, conn);

			if (options != null && options.RedirectStandardOutput)
				vm.StandardOutput = p.StandardOutput;
			
			if (options != null && options.RedirectStandardError)
				vm.StandardError = p.StandardError;

			conn.EventHandler = new EventHandler (vm);

			vm.connect ();

			return vm;
		}

		/*
		 * Wait for a virtual machine to connect at the specified address.
		 */
		public static VirtualMachine Listen (IPAddress address, int debugger_port, int console_port) {
			if (address == null)
				throw new ArgumentNullException ("address");

			IPEndPoint dbg_ep = new IPEndPoint (address, debugger_port);
			IPEndPoint con_ep = new IPEndPoint (address, console_port);

			Socket con_sock = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			Socket dbg_sock = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			con_sock.Bind (con_ep);
			dbg_sock.Bind (dbg_ep);
			con_sock.Listen (1000);
			dbg_sock.Listen (1000);

			return Listen (con_sock, dbg_sock);
		}

		public static VirtualMachine Listen (Socket con_sock, Socket dbg_sock) {
			/* 
			 * FIXME: This might block forever.
			 */
			Socket con_accepted = con_sock.Accept ();
			Socket dbg_accepted = dbg_sock.Accept ();

			con_sock.Disconnect (false);
			dbg_sock.Disconnect (false);
			con_sock.Close ();
			dbg_sock.Close ();

			Connection conn = new Connection (dbg_accepted);

			VirtualMachine vm = new VirtualMachine (null, conn);

			vm.StandardOutput = new StreamReader (new NetworkStream (con_accepted));
			vm.StandardError = null;
			conn.EventHandler = new EventHandler (vm);

			vm.connect ();

			return vm;
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
