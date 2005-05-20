'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Class C
	Public Function fun(i as integer, Optional a1 as Char = "c", Optional j as Integer=30) As Integer
		if a1="c" and i=2 and j=40
			return 10
		End if
		return 11	
	End Function
End Class

Module M
        Sub Main()
		   dim o as Object = new C()
		   dim a as integer = o.fun(j:= 40,i := 2 )
		   if a<>10 then
			throw new System.Exception("#A1 - Binding not proper")
		   end if
        End Sub
End Module
