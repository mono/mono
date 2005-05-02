'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 13
REM ExpectedError: BC30529
REM ErrorMessage: All parameters must be explicitly typed if any are.

Imports System

Module MethodDeclarationA
	Sub A(i as Integer, j)			
	End Sub
	Sub Main()
	End Sub
End Module
 
