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
	/// <summary>
	/// FIXME: Summary description for ProjectData.
	/// </summary>
	
	[MonoTODO]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[StructLayout(LayoutKind.Auto)] 
	public class ProjectData{

		internal static System.Exception projectError;
		internal static int erl;
		internal static Microsoft.VisualBasic.ErrObject pErr;

		internal static Microsoft.VisualBasic.ErrObject Err 
		{
			get
			{
				if (pErr==null)
					pErr=new ErrObject();

				return pErr;
			}
			set
			{
				pErr = value;
			}
		}

		/// <summary>
		/// FIXME: Summary description for ClearProjectError
		/// </summary>
		public static void ClearProjectError()
		{
			projectError = null;
			erl = 0;
		}

		/// <summary>
		/// FIXME: Summary description for SetProjectError
		/// </summary>
		/// <param name="ex">FIXME: Required. Summary description for ex</param>
		[MonoTODO]
		public static void SetProjectError(System.Exception ex)
		{
			SetProjectError(ex, 0);
		}

		/// <summary>
		/// FIXME: Summary description for SetProjectError
		/// </summary>
		/// <param name="ex">FIXME: Required. Summary description for ex</param>
		/// <param name="lErl">FIXME: Required. Summary description for lErl</param>
		[MonoTODO]
		public static void SetProjectError(System.Exception ex, int lErl)
		{
			projectError = ex;
			erl = lErl;
			Err.SetException (ex);
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
