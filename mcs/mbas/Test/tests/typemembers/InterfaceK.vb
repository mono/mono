'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'To Check if MustOverridable methods can Implement the interface functions

Interface A
	Sub fun(ByVal a As Integer)
	Sub bun(ByVal a As Integer)
End Interface

MustInherit Class B
	Implements A
	Sub Cfun(ByVal a As Integer) Implements A.fun
	End Sub
	MustOverride Sub Cbun(ByVal a As Integer) Implements A.bun
End Class

Class C
	Inherits B
	Overrides Sub Cbun(ByVal a As Integer)
	End Sub
End Class

Module InterfaceI
	Sub Main()	
	End Sub
End Module
