using System;
using System.Runtime.InteropServices;

public class Test 
{
 [DllImport ("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
 public static extern int MessageBoxA (int hWnd, string pText, string pCaption, int uType);

	static public void Main()
	{
		MessageBoxA(0, "Calling winelib from Mono", "TEST", 0);
	}
}

