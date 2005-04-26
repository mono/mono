'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Class C
	 Sub fun(ByRef a as Long, Byref a1 as Integer)	
		   a = a + 10
		   a1 = a1 + 20
	 End Sub
	 Sub fun(ByRef a as Integer, Byref a1 as Long)	
		   a = a + 20
		   a1 = a1 + 10
	 End Sub
End Class

Module M	 
        Sub Main()
		   dim a as integer = 10
		   dim a1 as long = 10
		   dim o as Object = new C()
		   o.fun(a,a1)               		
		   if a<>30 then
			throw new System.Exception("#A1 - Latebinding not working. a = "  &a)
		   end if
		   if a1<>20 then
			throw new System.Exception("#A1 - Latebinding not working. a1 = "  &a)
		   end if
        End Sub
End Module

