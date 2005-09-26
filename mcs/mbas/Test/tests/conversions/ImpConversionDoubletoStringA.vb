'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System.Threading
Imports System.Globalization

Module ImpConversionDoubletoStringA
	Sub Main()
			Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US")

			Dim a as Double= 123.052
			Dim b as String= a
			if b <> "123.052"
				Throw new System.Exception("Conversion of Double to String not working. Expected 123.052 but got " &b) 
			End if		
	End Sub
End Module
