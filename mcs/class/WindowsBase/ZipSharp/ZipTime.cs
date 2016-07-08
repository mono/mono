// ZipTime.cs created with MonoDevelop
// User: alan at 11:56 13/10/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Runtime.InteropServices;

namespace zipsharp
{
	[StructLayoutAttribute (LayoutKind.Sequential)]
	struct ZipTime
	{
		uint second;
		uint minute;
		uint hour;
		uint day;
		uint month;
		uint year;

		public ZipTime (DateTime time)
		{
			second = (uint) time.Second;
			minute = (uint) time.Minute;
			hour = (uint) time.Hour;
			day = (uint) time.Day;
			month = (uint) time.Month - 1;
			year = (uint) time.Year;
		}

		public DateTime Date
		{
			get { return new DateTime ((int) year, (int) month + 1, (int) day, (int) hour, (int) minute, (int) second); }
		}
	}
}
