'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionDecimaltoString
	Function fun(Byval i as String)
		if i <> "10.5" then
			Throw new System.Exception("Implicit Conversion of Decimal to String not working. Expected 10.5 but got " &i)
		End if
	End Function
	Sub Main()
		Dim i as Decimal = 10.5
		fun(i)
		
	End Sub
End Module
