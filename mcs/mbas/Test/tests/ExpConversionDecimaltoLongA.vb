'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ExpConversionDecimaltoLongA
	Sub Main()
		Dim a as Decimal = 123.501
		Dim b as Long = CLng(a)
		if b<>124 then 
			Throw New System.Exception("Explicit Conversion of Long to Single has Failed. Expected 123.5 but got " &b)
		End if		
	End Sub
End Module
