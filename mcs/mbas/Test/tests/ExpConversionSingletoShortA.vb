'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ExpConversionSingletoShortA
	Sub Main()
		Dim a as Single = 123.501 
		Dim b as Short
		b = CShort(a)
		if b <> 124
			Throw new System.Exception("Single to Short Conversion is not working properly. Expected 124 but got " &b)
		End if	
	End Sub
End Module
