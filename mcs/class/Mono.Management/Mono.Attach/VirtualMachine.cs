using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;

using Mono.Unix;
using Mono.Unix.Native;

namespace Mono.Attach
{
	/*
	 * Represents a running mono virtual machine.
	 */
	public class VirtualMachine {

		long pid;

		public VirtualMachine (long pid) {
			// FIXME: Check for unix
			this.pid = pid;
		}

		public long Pid {
			get {
				return pid;
			}
		}

		public bool IsCurrent {
			get {
				return pid == Syscall.getpid ();
			}
		}

		public string[] GetCommandLine () {
			return File.OpenText ("/proc/" + pid + "/cmdline").ReadToEnd ().Split ('\0');
		}

		public string GetWorkingDirectory () {
			return UnixPath.ReadLink ("/proc/" + pid + "/cwd");
		}

		/*
		 * Return the list of running mono vms owned by the current user. The 
		 * result includes the current vm too.
		 */
		public static List<VirtualMachine> GetVirtualMachines () {
			PerformanceCounterCategory p = new PerformanceCounterCategory (".NET CLR JIT");
			string[] machines = p.GetInstanceNames ();

			var res = new List<VirtualMachine> ();

			foreach (string s in machines) {
				// The names are in the form 'pid/name'
				int pos = s.IndexOf ('/');
				if (pos != -1)
					res.Add (new VirtualMachine (Int32.Parse (s.Substring (0, pos))));
			}
			return res;
		}

		/*
		 * Loads the specific agent assembly into this vm.
		 */
		public void Attach (string agent, string args) {
			string user = UnixUserInfo.GetRealUser ().UserName;

			// Check whenever the attach socket exists
			string socket_file = "/tmp/mono-" + user + "/.mono-" + pid;

			if (!File.Exists (socket_file)) {
				string trigger_file = "/tmp/.mono_attach_pid" + pid;
				FileStream trigger = null;

				try {
					trigger = File.Create (trigger_file);
					trigger.Close ();

					// Ask the vm to start the attach mechanism
					Syscall.kill ((int)pid, Signum.SIGQUIT);

					// Wait for the socket file to materialize
					int i;
					for (i = 0; i < 10; ++i) {
						if (File.Exists (socket_file))
							break;
						Thread.Sleep (100);
					}

					if (i == 10)
						throw new Exception (String.Format ("Runtime failed to create attach socket '{0}'.", socket_file));
				} finally {
					File.Delete (trigger_file);
				}
			}

			/* 
			 * We communicate with the agent inside the runtime using a simlified
			 * version of the .net remoting protocol.
			 */

			string path = "/tmp/mono-" + user + "/.mono-" + pid;

			UnixClient client = new UnixClient (path);

			NetworkStream stream = client.GetStream ();

			// Compose payload
			MemoryStream ms = new MemoryStream ();
			BinaryWriter writer = new BinaryWriter (ms);
			write_string (writer, "attach");
			write_string (writer, agent);
			write_string (writer, args);

			// Write header
			byte[] magic = new byte [] { (byte)'M', (byte)'O', (byte)'N', (byte)'O', 1, 0 };
			stream.Write (magic, 0, magic.Length);

			// Write payload length
			new BinaryWriter (stream).Write ((int)ms.Length);

			// Write payload
			stream.Write (ms.GetBuffer (), 0, (int)ms.Length);
		}

		enum PrimitiveType : byte {
			PRIM_TYPE_NULL = 17,
			PRIM_TYPE_STRING = 18
		};

		void write_string (BinaryWriter writer, string s) {
			if (s == null)
				writer.Write ((sbyte)PrimitiveType.PRIM_TYPE_NULL);
			else {
				writer.Write ((sbyte)PrimitiveType.PRIM_TYPE_STRING);
				writer.Write (s);
				writer.Write ((byte)0);
			}
		}

		public override string ToString () {
			return "VirtualMachine (pid=" + pid + ")";
		}
	}
}
