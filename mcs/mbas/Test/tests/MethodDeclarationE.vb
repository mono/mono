'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

'To Check if only those parameters that follow Optional must be Optional

Imports System

Module MethodDeclarationA
	Sub A1(ByVal i as Integer, Optional ByVal j as Integer =10)			
	End Sub
	Sub Main()
	End Sub
End Module
 
