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

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Microsoft.VisualBasic.CompilerServices
{

	[MonoTODO]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[StructLayout(LayoutKind.Auto)] 
	public class ProjectData {

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
 

		
		/*
		[MonoTODO]
		public static void EndApp()
		{
			//FIXME
		}
		*/

		/*
		[MonoTODO]
		protected static void Finalize()
		{
			//FIXME
		}
		*/

		

	}
}
