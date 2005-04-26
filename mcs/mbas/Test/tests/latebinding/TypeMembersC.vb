'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Class C
	 Sub fun(ByRef a as Long)	
		   a = a + 20
	 End Sub
	 Sub fun(ByRef a as Integer)	
		   a = a + 10
	 End Sub
End Class

Module M	 
        Sub Main()
		   dim o as Object = new C()
		   dim a as Integer = 10
		   o.fun(a)               		
		   if a<>20 then
			throw new System.Exception("#A1 - Binding not working")
		   end if
        End Sub
End Module
