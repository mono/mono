'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System.Threading
Imports System.Globalization

Module ExpConversionDatetoStringA
	Sub Main()
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US")

			Dim a as Date= "1/1/0001 12:00:00 AM"
			Dim b as String= a.toString()
			if b <> "1/1/0001 12:00:00 AM"
				Throw new System.Exception("Conversion of Date to String not working. Expected 1/1/0001 12:00:00 AM but got " &b) 
			End if		
	End Sub
End Module
