'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'Simple Check if Interfaces within interfaces are allowed
Interface A
	Interface AB
		Sub fun()
	End Interface
End Interface

Class C
	Implements A.AB	
	Sub Cfun() Implements A.AB.fun
	End Sub
End Class

Module InterfaceI
	Sub Main()
	End Sub
End Module

