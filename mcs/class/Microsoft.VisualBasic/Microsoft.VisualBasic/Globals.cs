//
// Globals.cs
//
// Author:
//	Chris J Breisch (cjbreisch@altavista.net) 
//	Dennis Hayes (dennish@raytek.com)
//	
// (C) 2002 Chris J Breisch
// 
//
 /*
  * Copyright (c) 2002-2003 Mainsoft Corporation.
  * Copyright (C) 2004 Novell, Inc (http://www.novell.com)
  *
  * Permission is hereby granted, free of charge, to any person obtaining a
  * copy of this software and associated documentation files (the "Software"),
  * to deal in the Software without restriction, including without limitation
  * the rights to use, copy, modify, merge, publish, distribute, sublicense,
  * and/or sell copies of the Software, and to permit persons to whom the
  * Software is furnished to do so, subject to the following conditions:
  * 
  * The above copyright notice and this permission notice shall be included in
  * all copies or substantial portions of the Software.
  * 
  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
  * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
  * DEALINGS IN THE SOFTWARE.
  */

using System;
using Microsoft.VisualBasic.CompilerServices;

namespace Microsoft.VisualBasic {

	[StandardModule]
	sealed public class Globals	{
		
		// prevent public constructor
		private Globals ()
		{
		}

		public static string ScriptEngine 
		{
			get { return "VB"; }
		}

		public static int ScriptEngineMajorVersion 
		{
			get { return 7; }
		}

		public static int ScriptEngineMinorVersion 
		{
			get { return 0; }
		}

		public static int ScriptEngineBuildVersion
		{
			get { return 0; }
		}
	}
}

