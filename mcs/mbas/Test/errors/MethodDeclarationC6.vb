'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 21
REM ExpectedError: BC30730
REM ErrorMessage: Methods declared 'Overrides' cannot be declared 'Overridable' because they are implicitly overridable.

Imports System

Module MethodDeclarationA
	Class A
		Overridable Sub A()
		End SUb
	end Class
	Class C
		Inherits A
		NotOverridable Overrides Sub A()
		End Sub
		Overridable Overrides Sub A1()
		End Sub
	End Class
	Sub Main()
	End Sub
End Module
 
