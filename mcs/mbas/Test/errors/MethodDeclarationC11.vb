'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 14
REM ExpectedError: BC30209
REM ErrorMessage: Option Strict On requires all variable declarations to have an 'As' clause.

Option Strict
Imports System

Module MethodDeclarationA
	Sub A(i)			
	End Sub
	Sub Main()
	End Sub
End Module
 
