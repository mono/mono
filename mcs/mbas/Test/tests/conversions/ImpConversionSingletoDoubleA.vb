'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionSingletoDoubleA
	Sub Main()
		Dim a as Single = 123.5 
		Dim b as Double
		b = a
		if b <> 123.5
			Throw new System.Exception("Single to Double Conversion is not working properly. Expected 123.5 but got " &b)
		End if	
	End Sub
End Module
