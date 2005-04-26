'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 22
REM ExpectedError: BC32007
REM ErrorMessage: 'Integer' values cannot be converted to 'Char'.

Class C
	Public Function fun(i as integer, a1 as Char)
		if i=2 and a1=1 then
			return 10
		end if
		return 11
	End Function
End Class

Module M
        Sub Main()
		   dim o as C = new C()
		   dim a as integer = o.fun(a1 := 1, i := 2)
	   End Sub
End Module
