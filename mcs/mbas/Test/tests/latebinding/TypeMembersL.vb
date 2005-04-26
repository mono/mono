'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Class C1
	Public fun as Integer = 10
End Class

Class C
	Public fun as Integer = 20
End Class

Module M
        Sub Main()
		   dim o as Object = new C()
		   if o.fun<>20 then
			throw new System.Exception("#A1 - Binding not proper")
		   end if
		   o = new C1()
		   if o.fun<>10 then
			throw new System.Exception("#A2 - Binding not proper")
		   end if
        End Sub
End Module
