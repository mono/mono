'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 22
REM ExpectedError: BC30043
REM ErrorMessage: 'MyBase' is valid only within an instance method

Imports System

Module MethodDeclarationA
	Class A
		Public i as Integer		
	end Class
	Class B
		Inherits A
		Sub A1()
			MyBase.i = 10
		End Sub
		Shared Sub A()
			MyBase.i = 10
		End SUb
	end Class
	Sub Main()
	End Sub
End Module
 
