//
// ProjectData.cs
//
// Authors:
//   Martin Adoue (martin@cwanet.com)
//   Chris J Breisch (cjbreisch@altavista.net)
//   Francesco Delfino (pluto@tipic.com)
//
// (C) 2002 Ximian Inc.
//     2002 Tipic, Inc. (http://www.tipic.com)
//

//
// Copyright (c) 2002-2003 Mainsoft Corporation.
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
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Microsoft.VisualBasic.CompilerServices
{

	[MonoTODO]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[StructLayout(LayoutKind.Auto)] 
	public sealed class ProjectData {

		private ProjectData () {}

		internal static int erl;
		internal static Microsoft.VisualBasic.ErrObject pErr = new ErrObject();

		internal static System.Exception projectError;


		public static void ClearProjectError ()
		{
			pErr.Clear();
		}

		internal static Microsoft.VisualBasic.ErrObject Err 
		{
			get {
				return pErr;
			}
		}

		public static Exception CreateProjectError(int hr)
		{
			pErr.Clear();
			return pErr.CreateException(hr, VBUtils.GetResourceString(pErr.MapErrorNumber(hr)));
		}

		public static void SetProjectError(Exception ex)
		{
			pErr.CaptureException(ex);
		}

		public static void SetProjectError(Exception ex, int lErl)
		{
			pErr.CaptureException(ex, lErl);
		}
 
		public static void EndApp()
		{
			//FileSystem.CloseAllFiles(Assembly.GetCallingAssembly());
			Environment.Exit(0);
		}

		[MonoTODO]
		protected static void Finalize()
		{
			throw new NotImplementedException();
		}
	}
}
