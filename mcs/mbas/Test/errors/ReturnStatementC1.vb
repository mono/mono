'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 12
REM ExpectedError: BC30311
REM ErrorMessage: Value of type 'Integer' cannot be converted to 'Date'.

Module retstmt
	Function fun() As Date
		return 10
	End Function
	Sub Main()
		Dim s as String = fun()
		if s<>10 then
			Throw new System.Exception("Return not working")
		End if
	End Sub
End Module 
