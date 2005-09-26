'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System.Threading
Imports System.Globalization

Module ExpConversionSingletoStringA
	Sub Main()
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US")

			Dim a as Single= 123.052
			Dim b as String= a.toString()
			if b <> "123.052"
				Throw new System.Exception("Conversion of Single to String not working. Expected 123.052 but got " &b) 
			End if		
	End Sub
End Module
