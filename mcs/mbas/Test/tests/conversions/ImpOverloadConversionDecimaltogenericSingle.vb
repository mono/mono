'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'Expected to call single 

Module ImpConversionDecimaltoSingle
	Function fun(Byval i as Single)	
		return 1
	End Function
	Function fun(Byval i as Double)
		return 2
	End Function
	Sub Main()
		Dim j as Integer
		Dim i as Decimal = 10
		j=fun(i)
		if j <> 1 then
			Throw new System.Exception("Implicit Conversion not working. Expected 1 but got " &j)
		End if
	
	End Sub
End Module
