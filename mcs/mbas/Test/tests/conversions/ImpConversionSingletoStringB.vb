'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System.Threading
Imports System.Globalization

Module ImpConversionSingletoStringC
	Sub Main()
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US")

			Dim a as Single = 123.90
			Dim b as String = a + "123"
			if b <> "246.900001525879"
				Throw new System.Exception("Concat of Single & String not working. Expected 246.900001525879 but got " &b) 
			End if		
	End Sub
End Module



