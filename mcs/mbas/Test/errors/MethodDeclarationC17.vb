'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 13
REM ExpectedError: BC30202
REM ErrorMessage: 'Optional' expected.

Imports System

Module MethodDeclarationA
	Sub A1(Optional ByVal i as Integer = 20, ByVal j as Integer)			
	End Sub
	Sub Main()
	End Sub
End Module
 
