'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module ImpConversionSingletoDouble
	Function fun(Byval i as Double)
		if i <> 10.5 then
			Throw new System.Exception("Implicit Conversion of Single to Double not working. Expected 10.5 but got " &i)
		End if
	End Function
	Sub Main()
		Dim i as Single = 10.5
		fun(i)
		
	End Sub
End Module
