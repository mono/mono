'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Class C
	Public Function fun(i as integer, a as Long)
		if i=2 and a=1 then
			return 10
		end if
		return 11
	End Function
End Class

Module M
        Sub Main()
		   dim o as Object = new C()
		   dim a as integer = o.fun(a := 1, i := 2)
		   if a<>10 then
			throw new System.Exception("#A1 - Binding not proper")
		   end if
        End Sub
End Module
