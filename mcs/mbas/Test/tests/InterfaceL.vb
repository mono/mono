'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

'To Check if derived class need not implement the already existing Implementation

Interface A
	Sub fun(ByVal a As Integer)
End Interface

Interface AB
	Inherits A
	Sub fun1(ByVal a As Integer)
End Interface

Class B
	Implements A
	Sub Cfun(ByVal a As Integer) Implements A.fun
	End Sub
End Class

Class BC
	Inherits B
	Implements AB
	Sub Cfun1(ByVal a As Integer) Implements AB.fun1
	End Sub
End Class

Module InterfaceI
	Sub Main()	
	End Sub
End Module
