'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ExpConversionInttoDecimalA
	Sub Main()
		Dim a as Integer = 123 
		Dim b as Decimal
		b = CDec(a)
		if b <> 123
			Throw new System.Exception("Int to Decimal Conversion is not working properly. Expected 123 but got " &b)
		End if	
	End Sub
End Module
