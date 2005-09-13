
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Data.OleDb;

namespace Mono.Data.GDA.Test
{
	public class TestGDA
	{
		private IntPtr m_gdaClient = IntPtr.Zero;
		private IntPtr m_gdaConnection = IntPtr.Zero;
		
		static void Main (string[] args)
		{
			TestGDA test = new TestGDA ();
			
			/* initialization */
			libgda.gda_init ("TestGDA#", "0.1", args.Length, args);
			test.m_gdaClient = libgda.gda_client_new ();

			/* open connection */
			test.m_gdaConnection = libgda.gda_client_open_connection (
				test.m_gdaClient,
				"PostgreSQL",
				"", "");
			if (test.m_gdaConnection != IntPtr.Zero) {
				System.Console.Write ("Connection successful!");

				/* close connection */
				libgda.gda_connection_close (test.m_gdaConnection);
			}
		}       
	}
}
