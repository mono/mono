'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System

Class C
	 Public a as Integer() = {1,2,3}
End Class

Module M
	Sub Main()
		dim o as object = new C()
		o.a(1) = 0
		if o.a(1) then
			throw new System.Exception("LateBinding Not Working")
		end if
	End Sub
End Module
