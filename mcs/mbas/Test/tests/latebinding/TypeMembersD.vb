'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Class C
	 Sub fun(ByRef a as Long)	
		   a = a + 10
	 End Sub
End Class

Module M	 
        Sub Main()
		   dim o as Object = new C()
		   const a as Integer = 10
		   o.fun(a)               		
		   if a<>10 then
			throw new System.Exception("#A1 - ByRef not working")
		   end if
        End Sub
End Module
