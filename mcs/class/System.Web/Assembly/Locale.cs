//
// Locale.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2001 - 2003 Ximian, Inc (http://www.ximian.com)
//

internal sealed class Locale {

	private Locale ()
	{
	}

	/// <summary>
	///   Returns the translated message for the current locale
	/// </summary>
	public static string GetText (string msg)
	{
		return msg;
	}
}
