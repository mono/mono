'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Module stopstmt
	Function fun()
		return 10
	End Function
	Function fun1()
		return "Hello"
	End Function
	Sub Main()
		Dim i as Integer = fun()
		Dim s as String = fun1()
		if i<>10 or s<>"Hello" then
			Throw new System.Exception("Return not working")
		End if
	End Sub
End Module
