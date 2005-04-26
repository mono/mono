'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Class C
	Public Function fun(i as integer, ParamArray a() as Long)
		return 10
	End Function
	Public Function fun(ParamArray a() as Long)
		return 20
	End Function
End Class

Module M
        Sub Main()
		   dim o as Object = new C()
		   dim a as integer = o.fun(1,2,3)
		   if a<>10 then
			throw new System.Exception("#A1 - Binding not proper")
		   end if
        End Sub
End Module
