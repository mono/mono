'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ExpConversionShorttoDoubleA
	Sub Main()
		Dim a as Short = 123 
		Dim b as Double
		b = CDbl(a)
		if b <> 123
			Throw new System.Exception("Short to Double Conversion is not working properly. Expected 123 but got " &b)
		End if	
	End Sub
End Module
