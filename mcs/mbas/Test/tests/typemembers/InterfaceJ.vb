'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'Tries to declare vaiables of a Interface using the Class which implements it...

Interface A
	Sub fun()
End Interface

Class C
	Implements A
	Sub Cfun() Implements A.fun
	End Sub
End Class

Module InterfaceI
	Sub Main()
		Dim a as A = New C
	End Sub
End Module
