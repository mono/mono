'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System.Threading
Imports System.Globalization

Module ImpConversionBooleantoDoubleC
	Sub Main()
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US")

			Dim a as Boolean = True
			Dim b as Double = 111.9 + a
			if b <> "110.9"
				Throw new System.Exception("Addition of Boolean & Double not working. Expected 110 but got " &b) 
			End if		
	End Sub
End Module
