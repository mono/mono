'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Class C
	Public Function fun(Byval i as integer, Optional Byval a1 as Char = "d", Optional Byval j as Integer=30) As Integer
		if a1="c" and i=2 and j=30
			return 10
		End if
		return 11	
	End Function
End Class

Module M
Public i as integer
        Sub Main()
		   dim o as Object = new C()
		   dim a as integer = o.fun(i := 2.321, a1 := "caa")
		   if a<>10 or i=2 then
			throw new System.Exception("#A1 - Binding not proper")
		   end if
        End Sub
End Module
