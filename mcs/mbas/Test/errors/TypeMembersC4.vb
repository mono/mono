'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 19
REM ExpectedError: BC30241
REM ErrorMessage:  Named argument expected.

Class C
	Public Function fun(i as integer, a1 as Char)
			return 10
	End Function
End Class

Module M
        Sub Main()
		   dim o as C = new C()
		   dim a as integer = o.fun(a1 := "s", 2)
        End Sub
End Module
