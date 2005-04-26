'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Class C1
	Public Function fun() as String
		return fun
	End function
End Class
Class C
	Inherits C1
End Class

Module M
        Sub Main()
		   dim o as Object = new C()
		   dim a as Integer = o.fun()
		   if a<>0 then
			throw new System.Exception("#A1 - Binding not proper")
		   end if
        End Sub
End Module
