'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 25
REM ExpectedError: BC30519
REM ErrorMessage:  Overload resolution failed because no accessible 'fun' can be called without a narrowing conversion

Module ImpConversionDecimaltoInt
	Function fun(Byval i as Integer)	
		return 1
	End Function
	Function fun(Byval i as Short)
		return 2
	End Function
	Function fun(Byval i as Long)
		return 3
	End Function
	Function fun(Byval i as Byte)
		return 4
	End Function
	Sub Main()
		Dim i as Decimal = 10
		i=fun(i)
		if i <> 10 then
			Throw new System.Exception("Implicit Conversion of Decimal to Int not working. Expected 10 but got " &i)
		End if
	
	End Sub
End Module
