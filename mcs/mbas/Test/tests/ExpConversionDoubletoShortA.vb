'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ExpConversionDoubletoShortA
	Sub Main()
		Dim a as Double = 123.501 
		Dim b as Short
		b = CShort(a)
		if b <> 124
			Throw new System.Exception("Double to Short Conversion is not working properly. Expected 124 but got " &b)
		End if	
	End Sub
End Module
