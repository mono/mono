'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 14
REM ExpectedError: BC30059
REM ErrorMessage: Constant expression is required.

Imports System

Module MethodDeclarationA
	Public i as integer = 0
	Sub A1(Optional ByVal j as Integer = i)			
	End Sub
	Sub Main()
	End Sub
End Module
 
