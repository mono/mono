'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System

Module MethodDeclarationA
	Function A(ByRef i as Integer)As Integer			 
		i = 10
	End Function
	Function AB()As Integer			 
		return 10
	End Function
	Sub Main()
		A(AB())
	End Sub
End Module
 
