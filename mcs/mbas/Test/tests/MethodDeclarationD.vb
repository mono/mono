'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System

Module MethodDeclarationA
	Function A(ByRef i as Integer)As Integer			 
		i = 19
	End Function
	Sub Main()
		A(10)
	End Sub
End Module
 
