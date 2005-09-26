'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System.Threading
Imports System.Globalization

Module ImpConversionStringtoDoubleD
	Sub Main()
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US")

			Dim a as String = "12.9"
			Dim b as Double = a + 123
			if b <> 135.9
				Throw new System.Exception("Concat of String & Double not working. Expected  135.9 but got " &b) 
			End if		
	End Sub
End Module

