'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ExpConversionDecimaltoDoubleA
	Sub Main()
		Dim a as Decimal = 123.5
		Dim b as Double = CDbl(a)
		if b<>123.5 then 
			Throw New System.Exception("Explicit Conversion of Long to Double has Failed. Expected 123.5 but got " &b)
		End if		
	End Sub
End Module
