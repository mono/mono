'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionDecimaltoLong
	Function fun(Byval i as Long)
		if i <> 10 then
			Throw new System.Exception("Implicit Conversion of Decimal to Long not working. Expected 10 but got " &i)
		End if
	End Function
	Sub Main()
		Dim i as Decimal = 10
		fun(i)
		
	End Sub
End Module
