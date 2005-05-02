'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 15
REM ExpectedError: BC30429
REM ErrorMessage: 'End Sub' must be preceded by a matching 'Sub'.

Imports System

Module MethodDeclarationA
	MustInherit Class C
		MustOverride Sub A()
		End Sub
	End Class
	Sub Main()
	End Sub
End Module
 
