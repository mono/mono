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
		Dim i as String
		A(i)
		if i<>"19" then
			Throw new System.Exception("ByRef not working")
		End if	
	End Sub
End Module
 
