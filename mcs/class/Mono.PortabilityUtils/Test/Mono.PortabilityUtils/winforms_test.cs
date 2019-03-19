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
// Copyright (c) 2019 AxxonSoft.
//
// Authors:
//	Nikita Voronchev <nikita.voronchev@ru.axxonsoft.com>
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Mono.PortabilityUtils;

namespace MonoTests.Mono.PortabilityUtils
{
	public static class WinFormsTestProgram
	{
		public static int Main (string[] args)
		{
			var command = args[0];

			switch (command)
			{
				case "RegisterWindowMessage": {
					var msgName = args[1];
					int msgId = WinForms.RegisterWindowMessage (msgName);
					PrintResult("{0}", msgId);
					return 0; }

				case "EnumTopLevelWindows": {
					var param = new IntPtr (Convert.ToInt32 (args[1]));
					var windows = new List<WinFormsTestHelpers.OpenedWindow> ();
					WinForms.EnumTopLevelWindows ( (hwnd, lParam) => {
						windows.Add (new WinFormsTestHelpers.OpenedWindow (hwnd, lParam));
						return true;
					}, param);
					PrintResult (string.Join (" ", windows));
					return 0; }

				case "PostMessage": {
					var hwnd = new IntPtr (Convert.ToInt32 (args[1]));
					var msgId = Convert.ToInt32 (args[2]);
					var wParam = new IntPtr (Convert.ToInt32 (args[3]));
					var lParam = new IntPtr (Convert.ToInt32 (args[4]));
					bool postRet = WinForms.PostMessage (hwnd, msgId, wParam, lParam);
					PrintResult (postRet ? "0" : "1");
					return 0; }

				default:
					return 1;
			}
		}

		public static void PrintResult (string format, params object[] args)
		{
			string msg = WinFormsTestHelpers.ProcessSdtCommunication.SerializeOutputData (format, args);
			Console.WriteLine (msg);
		}
	}
}