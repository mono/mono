//
// ChineseLunisolarCalendar.cs
//
// Author
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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

#if NET_2_0

namespace System.Globalization {

using System;
using System.Runtime.InteropServices;

[Serializable]
public class ChineseLunisolarCalendar : EastAsianLunisolarCalendar {
	internal static readonly CCEastAsianLunisolarEraHandler era_handler;

	static ChineseLunisolarCalendar ()
	{
		era_handler = new CCEastAsianLunisolarEraHandler ();
		era_handler.appendEra (ChineseEra, CCFixed.FromDateTime (new DateTime (1, 1, 1)));
	}

	public const int ChineseEra = 1;

	[MonoTODO]
	public ChineseLunisolarCalendar ()
		: base (era_handler)
	{
	}

	[ComVisible (false)]
	public override int [] Eras {
		get {
			return (int []) era_handler.Eras.Clone();
		}
	}

	[ComVisible (false)]
	public override int GetEra (DateTime time) {
		// M_CheckDateTime not needed, because EraYear does the
		// right thing.
		int rd = CCFixed.FromDateTime(time);
		int era;
		era_handler.EraYear(out era, rd);
		return era;
	}

	static DateTime ChineseMin = new DateTime (1901, 2, 19);
	// looks like this valus is related to Lunar equation; Current
	// epact compulation is valid only from 1900 to 2100.
	static DateTime ChineseMax = new DateTime (2101, 1, 28, 23, 59, 59, 999);

	[ComVisible (false)]
	public override DateTime MinSupportedDateTime {
		get {
			return ChineseMin;
		}
	}

	[ComVisible (false)]
	public override DateTime MaxSupportedDateTime {
		get {
			return ChineseMax;
		}
	}
}

}
#endif
