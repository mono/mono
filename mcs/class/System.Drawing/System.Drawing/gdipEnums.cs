//
// System.Drawing.gdipEnums.cs
//
// Author: 
// Alexandre Pigolkine (pigolkine@gmx.de)
//
using System;

namespace System.Drawing {
	/// <summary>
	/// GDI+ API enumerations
	/// </summary>
	
	#region Status
	internal enum Status {
	    Ok = 0,
	    GenericError = 1,
	    InvalidParameter = 2,
	    OutOfMemory = 3,
	    ObjectBusy = 4,
	    InsufficientBuffer = 5,
	    NotImplemented = 6,
	    Win32Error = 7,
	    WrongState = 8,
	    Aborted = 9,
	    FileNotFound = 10,
	    ValueOverflow = 11,
	    AccessDenied = 12,
	    UnknownImageFormat = 13,
	    FontFamilyNotFound = 14,
	    FontStyleNotFound = 15,
	    NotTrueTypeFont = 16,
	    UnsupportedGdiplusVersion = 17,
	    GdiplusNotInitialized = 18,
	    PropertyNotFound = 19,
	    PropertyNotSupported = 20
	}
	#endregion
	
	#region Unit
	internal enum Unit
	{
	    UnitWorld		= 0,
	    UnitDisplay 	= 1,
	    UnitPixel 		= 2,
	    UnitPoint		= 3,
	    UnitInch		= 4,
	    UnitDocument	= 5,
	    UnitMillimeter	= 6
	};
	#endregion
	
}
