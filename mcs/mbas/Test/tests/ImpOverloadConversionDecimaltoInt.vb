'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionDecimaltoInt
	Function fun(Byval i as Integer)
		if i <> 10 then
			Throw new System.Exception("Implicit Conversion of Decimal to Int not working. Expected 10 but got " &i)
		End if
	End Function
	Sub Main()
		Dim i as Decimal = 10
		fun(i)
		
	End Sub
End Module
