'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Class C
	 writeonly Property fun(a1 as Integer) as Integer
		set(a as Integer)	
		   if a<>30 then
			throw new System.Exception("#A1 - Latebinding not working. a = "  &a)
		   end if
		end set
	 End Property
	 writeonly Property fun(a as Long) as Long
		set(a1 as Long)	
		   if a1<>20 then
			throw new System.Exception("#A1 - Latebinding not working. a1 = "  &a)
		   end if
		end set
	 End Property
End Class

Module M	 
        Sub Main()
		   dim a as integer = 30
		   dim a1 as long = 20
		   dim o as Object = new C()
		   o.fun(a) = a
		   o.fun(a1) = a1
        End Sub
End Module

