using System;
using System.Runtime.CompilerServices;
using System.Net.Http;
using WebAssembly.Net.Http.HttpClient;

[assembly: InternalsVisibleTo ("System.Net.Http, PublicKey=002400000480000094000000060200000024000052534131000400000100010007d1fa57c4aed9f0a32e84aa0faefd0de9e8fd6aec8f87fb03766c834c99921eb23be79ad9d5dcc1dd9ad236132102900b723cf980957fc4e177108fc607774f29e8320e92ea05ece4e821c0a5efe8f1645c4c0c93c1ab99285d622caa652c1dfad63d745d6f2de5f17e5eaf0fc4963d261c8a12436518206dc093344d5ad293")]

namespace WebAssembly {
	internal class RuntimeOptions {
		internal static object GetHttpMessageHandler ()
		{
			var handler = GetCustomHttpMessageHandler ();
			if (handler != null)
				return handler;
			return new WasmHttpMessageHandler ();
		}

		static object GetCustomHttpMessageHandler ()
		{
			string envvar = Environment.GetEnvironmentVariable ("WASM_HTTP_CLIENT_HANDLER_TYPE")?.Trim ();
			Type handlerType = null;
			if (!String.IsNullOrEmpty (envvar))
				handlerType = Type.GetType (envvar, false);
			else
				return null;
			if (handlerType == null)
				return null;
			return Activator.CreateInstance (handlerType);
		}
	}
}