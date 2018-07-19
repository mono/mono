using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using NUnitLite.Runner;
using NUnit.Framework.Internal;
using Mono.Security.Interface;

    class TcpWriter : TextWriter
    {
        private string hostName;
        private int port;

        private TcpClient client;
        private NetworkStream stream;
        private StreamWriter writer;

        public TcpWriter(string hostName, int port)
        {
            this.hostName = hostName;
            this.port = port;
            this.client = new TcpClient(hostName, port);
            this.stream = client.GetStream();
            this.writer = new StreamWriter(stream);
        }

        public override void Write(char value)
        {
            writer.Write(value);
        }

        public override void Write(string value)
        {
            writer.Write(value);
        }

        public override void WriteLine(string value)
        {
            writer.WriteLine(value);
            writer.Flush();
        }

        public override System.Text.Encoding Encoding
        {
            get { return System.Text.Encoding.Default; }
        }
    }

public class TestRunner
{
	public static int Main(string[] args) {
		TextUI runner;

		// Make sure the TLS subsystem including the DependencyInjector is initialized.
		// This would normally happen on system startup in
		// `xamarin-macios/src/ObjcRuntime/Runtime.cs`.
		MonoTlsProviderFactory.Initialize ();

		// First argument is the connection string
		if (args [0].StartsWith ("tcp:")) {
			var parts = args [0].Split (':');
			if (parts.Length != 3)
				throw new Exception ();
			string host = parts [1];
			string port = parts [2];
			args = args.Skip (1).ToArray ();

			Console.WriteLine ($"Connecting to harness at {host}:{port}.");
			runner = new TextUI (new TcpWriter (host, Int32.Parse (port)));
		} else {
			runner = new TextUI ();
		}
		runner.Execute (args);
            
		return (runner.Failure ? 1 : 0);
    }
}
