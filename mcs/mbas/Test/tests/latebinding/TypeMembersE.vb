'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Class C
	 Sub fun(ByRef a as Long)	
		   	throw new System.Exception("#A1 - Binding not working")		   
	 End Sub
	 Sub fun(ByRef a as Integer)	
		   a = a + 10
		   if a<>20 then
			throw new System.Exception("#A1 - Binding not working")
		   end if
	 End Sub
End Class

Module M	 
        Sub Main()
		   dim o as Object = new C()
		   o.fun(10)   'Constant value passed            		
        End Sub
End Module

