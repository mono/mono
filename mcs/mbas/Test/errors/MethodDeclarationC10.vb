'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 23
REM ExpectedError: BC30501
REM ErrorMessage: 'Shared' cannot be combined with 'NotOverridable' on a method declaration.

REM LineNo: 23
REM ExpectedError: BC30501
REM ErrorMessage: 'Shared' cannot be combined with 'Overrides' on a method declaration.

Imports System

Module MethodDeclarationA
	Class A
		Overridable Sub A()			
		End Sub
	end Class
	Class AB
		Inherits A
		Shared NotOverridable Overrides Sub A()			
		End SUb
	end Class
	Sub Main()
	End Sub
End Module
 
