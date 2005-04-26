'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Class C
	Public Function fun()
		return 10
	End function
	Private Function fun(a as integer)
		return 20
	End function
End Class

Module M
        Sub Main()
		   dim o as Object = new C()
		   if o.fun()<>10 then
			throw new System.Exception("#A1 - Binding not proper")
		   end if
        End Sub
End Module
