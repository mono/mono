'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'Expected to call double

Module ImpConversionSingletoSingle
	Function fun(Byval i as Decimal)	
		return 1
	End Function
	Function fun(Byval i as Double)
		return 2
	End Function
	Sub Main()
		Dim j as Integer
		Dim i as single = 10
		j=fun(i)
		if j <> 2 then
			Throw new System.Exception("Implicit Conversion not working. Expected 1 but got " &j)
		End if
	
	End Sub
End Module
