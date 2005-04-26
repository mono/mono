'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Class C1
	Public Function fun(byref a as integer) as String
		a = a mod 2
	End function
End Class

Class C
	Inherits C1
End Class

Module M
        Sub Main()
		   dim o as Object = new C()
		   dim a as Double = 1.33234 
		   o.fun(a)
		   if a<>1 then
			throw new System.Exception("#A1 - Binding not proper")
		   end if
        End Sub
End Module
