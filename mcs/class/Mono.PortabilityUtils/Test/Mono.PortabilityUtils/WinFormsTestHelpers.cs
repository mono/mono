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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MonoTests.Mono.PortabilityUtils
{
    internal class WinFormsTestHelpers
	{
		public static uint RunProcessAndGetMsgId (ProcessStartInfo pInfo)
		{
			string msgId = RunProcessAndGetData (pInfo);
			return Convert.ToUInt32 (msgId);
		}

		public static OpenedWindow[] RunProcessAndGetWindows (int lParamInt)
		{
			var pInfo = GetProcessStartInfo (string.Format ("EnumTopLevelWindows {0}", lParamInt));
			string windows = RunProcessAndGetData (pInfo);
			return windows.Split ()
				.Where (w => !string.IsNullOrEmpty(w))
				.Select (w => new OpenedWindow (w))
				.ToArray ();
		}

		public static bool RunProcessAndPostMessage (IntPtr hwnd, int msgId, IntPtr wParam, IntPtr lParam)
		{
			var pInfo = GetProcessStartInfo (string.Format ("PostMessage {0} {1} {2} {3}", hwnd, msgId, wParam, lParam));
			string postRetCode = RunProcessAndGetData (pInfo);
            bool postRet = Convert.ToInt32 (postRetCode) == 0 ? true : false;
			return postRet;
		}

		public static ProcessStartInfo GetProcessStartInfo (string cmdArgsFormat, params string[] a)
		{
			// The `winforms_test.exe` was already made by the `Makefile`.
			var cmdArgs = string.Format(cmdArgsFormat, a);
			return new ProcessStartInfo ("make") {
				Arguments = string.Format("run-winforms_test.exe-from-test args=\"{0}\"", cmdArgs),
				RedirectStandardOutput = true,
				UseShellExecute = false
			};
		}

		public static string RunProcessAndGetData (ProcessStartInfo pInfo)
		{
			string stdOut;
			using (Process testProcess = new Process())
			{
				testProcess.StartInfo = pInfo;
				testProcess.Start();
				stdOut = testProcess.StandardOutput.ReadToEnd().Trim();
				testProcess.WaitForExit();
			}
			return ProcessSdtCommunication.DeserializeFirstOutputData (stdOut);
		}

        public class ProcessSdtCommunication
        {
            private const string STD_MSG_FORMAT = "TEST_PROGRAM_RESULT:>{0}<:";
            private static readonly string STD_MSG_REGEX = string.Format (STD_MSG_FORMAT, "(.*)");

            public static string SerializeOutputData (string format, params object[] args)
            {
                var s = string.Format (format, args);
                return string.Format (STD_MSG_FORMAT, s);
            }

            public static string DeserializeFirstOutputData (string serialized)
            {
                MatchCollection matches = Regex.Matches (serialized, STD_MSG_REGEX);
                string data = matches[0].Groups[1].Value;
                return data;
            }
        }

		public class OpenedWindow
		{
			public IntPtr Hwnd;
			public IntPtr LParam;

			public OpenedWindow (string strWindowDesc)
			{
				DeserializeHwndAndLParam (strWindowDesc, out Hwnd, out LParam);
			}

			public OpenedWindow (IntPtr hwnd, IntPtr lParam)
			{
                Hwnd = hwnd;
				LParam = lParam;
			}

			public override string ToString()
			{
				return SerializeHwndAndLParam ();
			}

            public string SerializeHwndAndLParam ()
            {
                return string.Format("{0}-{1}", Hwnd.ToString("X"), LParam.ToString("X"));
            }

            public static void DeserializeHwndAndLParam (string strWindowDesc, out IntPtr hwnd, out IntPtr lParam)
            {
                var winDesc = strWindowDesc.Split ('-');
                hwnd = new IntPtr (Convert.ToInt32 (winDesc [0], 16));
                lParam = new IntPtr (Convert.ToInt32 (winDesc [1], 16));
            }
		}
	}
}