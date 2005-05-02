'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 14
REM ExpectedError: BC30177
REM ErrorMessage: Only one of 'NotOverridable', 'MustOverride', or 'Overridable' can be specified.

Imports System

Module MethodDeclarationA
	Class C
		MustOverride NotOverridable Sub A()
		End Sub
	End Class
	Sub Main()
	End Sub
End Module
 
