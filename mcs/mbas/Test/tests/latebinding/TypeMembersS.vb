'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System

Class C
	 Public a as Integer() = {1,2,3}
End Class

Class B
	Function fun(ByRef i() as Integer, j as integer)
		i(j) = 0
	End Function
End Class

Module M
	Sub Main()
		dim o as object = new C()
		dim o1 as object = new B()
		o1.fun(o.a, 1)
		if o.a(1) then
			throw new System.Exception("LateBinding Not Working")
		end if
	End Sub
End Module
