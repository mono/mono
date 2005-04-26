'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 23
REM ExpectedError: BC30272
REM ErrorMessage:  Argument not specified for parameter 'i' of 'Public Function fun(i As Integer, a1 As Char) As Object'.

REM LineNo: 23
REM ExpectedError: BC30455
REM ErrorMessage:  'j' is not a parameter of 'Public Function fun(i As Integer, a1 As Char) As Object'.

Class C
	Public Function fun(i as integer, a1 as Char)
			return 10
	End Function
End Class

Module M
        Sub Main()
		   dim o as C = new C()
		   dim a as integer = o.fun(a1 := "s", j := 3)
        End Sub
End Module
