//
// driver.cs: Guides the compilation process through the different phases.
//
// Author: 
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
// (C) 2005, Novell Inc. (http://www.novell.com)
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
using System.IO;
using Microsoft.Vsa;
using Microsoft.JScript;
using Microsoft.JScript.Vsa;

class Driver {		
	public static void Main (string [] args) {
	
		if (args.Length < 1) {
			Console.WriteLine ("Usage: mjs filename.js");
			Environment.Exit (0);
		}

		VsaEngine engine = new VsaEngine ();
		engine.InitVsaEngine ("mjs:com.mono-project", new MonoEngineSite ());

		foreach (string fn in args) {
			IVsaCodeItem item = (IVsaCodeItem) engine.Items.CreateItem (fn, VsaItemType.Code, VsaItemFlag.None);
			item.SourceText = GetCodeFromFile (fn);
		}
		engine.Compile ();
	}

	static string GetCodeFromFile (string fn)
	{
		try {
			StreamReader reader = new StreamReader (fn);
			return reader.ReadToEnd ();
		} catch (FileNotFoundException) {
			throw new JScriptException (JSError.FileNotFound);
		} catch (ArgumentNullException) {
			throw new JScriptException (JSError.FileNotFound);
		} catch (ArgumentException) {
			throw new JScriptException (JSError.FileNotFound);
		} catch (IOException) {
			throw new JScriptException (JSError.NoError);
		} catch (OutOfMemoryException) {
			throw new JScriptException (JSError.OutOfMemory);
		}
	}
}

class MonoEngineSite : IVsaSite {
	public void GetCompiledState (out byte [] pe, out byte [] debugInfo)
	{
		throw new NotImplementedException ();
	}

	public object GetEventSourceInstance (string itemName, string eventSourceName)
	{
		throw new NotImplementedException ();
	}

	public object GetGlobalInstance (string name)
	{
		throw new NotImplementedException ();
	}

	public void Notify (string notify, object info)
	{
		throw new NotImplementedException ();
	}

	public bool OnCompilerError (IVsaError error)
	{
		throw new NotImplementedException ();
	}
}
