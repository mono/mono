'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 13
REM ExpectedError: BC32024
REM ErrorMessage: Default values cannot be supplied for parameters that are not declared 'Optional'.

Imports System

Module MethodDeclarationA
	Sub A1(ByVal i as Integer = 20)			
	End Sub
	Sub Main()
	End Sub
End Module
 
