'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 26
REM ExpectedError: BC30455
REM ErrorMessage: Argument not specified for parameter 'a1' of 'Public Function fun(i As Integer, a1 As Long) As Object'.

REM LineNo: 26
REM ExpectedError: BC30272
REM ErrorMessage: 'a' is not a parameter of 'Public Function fun(i As Integer, a1 As Long) As Object'.

Class C
	Public Function fun(i as integer, a1 as Long)
		if i=2 and a1=1 then
			return 10
		end if
		return 11
	End Function
End Class

Module M
        Sub Main()
		   dim o as C = new C()
		   dim a as integer = o.fun(a := 1, i := 2)
        End Sub
End Module
