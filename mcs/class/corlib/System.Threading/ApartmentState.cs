//
// System.Threading.ApartmentState.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Threading
{
	public enum ApartmentState {
		STA = 0,
		MTA = 1,
		Unknown = 2
	}
}
