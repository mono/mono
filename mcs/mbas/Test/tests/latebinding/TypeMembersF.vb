'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Class C
	 Sub fun(ByRef a as Long)	
		   a = a + 10
	 End Sub
	 Sub fun(ByRef a as Integer)	
		   a = a + 20
	 End Sub
	 Sub fun(ByRef a as Decimal)	
		   a = a + 30
	 End Sub
End Class

Module M	 
        Sub Main()
		   dim a as integer = 10
		   dim a1 as long = 10
		   dim a2 as Decimal = 10
		   dim o as Object = new C()
		   o.fun(a)               		
		   o.fun(a1)               		
		   o.fun(a2)               		
		   if a<>30 then
			throw new System.Exception("#A1 - Latebinding not working. a = "  &a)
		   end if
		   if a1<>20 then
			throw new System.Exception("#A1 - Latebinding not working. a1 = "  &a)
		   end if
		   if a2<>40 then
			throw new System.Exception("#A1 - Latebinding not working. a2 = "  &a)
		   end if
        End Sub
End Module

