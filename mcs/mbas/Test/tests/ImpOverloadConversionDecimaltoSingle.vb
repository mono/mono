'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionDecimaltoSingle
	Function fun(Byval i as Single)
		if i <> 10.5 then
			Throw new System.Exception("Implicit Conversion of Decimal to Single not working. Expected 10.5 but got " &i)
		End if
	End Function
	Sub Main()
		Dim i as Decimal = 10.5
		fun(i)
		
	End Sub
End Module
