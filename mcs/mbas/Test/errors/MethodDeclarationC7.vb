'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 19
REM ExpectedError: BC30043
REM ErrorMessage: 'Me' is valid only within an instance method

Imports System

Module MethodDeclarationA
	Class A
		Public i as Integer
		Sub A1()
			Me.i = 10
		End Sub
		Shared Sub A()
			Me.i = 10
		End SUb
	end Class
	Sub Main()
	End Sub
End Module
 
