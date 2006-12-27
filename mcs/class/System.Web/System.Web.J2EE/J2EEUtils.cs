//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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
using System.Web.Util;
using System.IO;
using vmw.@internal.io;
using vmw.common;

namespace System.Web.J2EE
{
	internal sealed class J2EEUtils
	{
		public J2EEUtils()
		{
		}

		public static int RunProc(string[] cmd)
		{	
			java.lang.Runtime rt = java.lang.Runtime.getRuntime();
			java.lang.Process proc = rt.exec(cmd);
			
			StreamGobbler errorGobbler = new 
				StreamGobbler(proc.getErrorStream(), "ERROR");            
          
			StreamGobbler outputGobbler = new 
				StreamGobbler(proc.getInputStream(), "OUTPUT");
                
			errorGobbler.start();
			outputGobbler.start();
                             
			int exitVal = proc.waitFor();
			return exitVal;	
		}
	}

	public class StreamGobbler : java.lang.Thread
	{
		java.io.InputStream _is;
		String _type;
    
		public StreamGobbler(java.io.InputStream ins, String type)
		{
			this._is = ins;
			this._type = type;
		}
    
		public override void run()
		{
			try
			{
				java.io.InputStreamReader isr = new java.io.InputStreamReader(_is);
				java.io.BufferedReader br = new java.io.BufferedReader(isr);
				String line=null;
				while ( (line = br.readLine()) != null)
				{
#if DEBUG
					Console.WriteLine(_type + ">" + line); 
#endif
				}
			} 
			catch (Exception ex)
			{
#if DEBUG
				Console.WriteLine(ex);
#endif
			}
		}
	}
}

#if !CODEDOM_SUPPORT
//stubs for CodeDom symbols
namespace System.CodeDom
{
	public class CodeObject
	{
		protected CodeObject () { }

		public System.Collections.IDictionary UserData { get { throw new NotSupportedException (); } }
	}

	public class CodeExpression : CodeObject
	{
		protected CodeExpression () { }
	}
}
#endif
