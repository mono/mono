'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 16
REM ExpectedError: BC31405
REM ErrorMessage: Optional parameters cannot have structure types.

Imports System

Module MethodDeclarationA
	Structure A
		Dim i as Integer
	End Structure
	Sub A1(Optional ByVal i as A = 0)			
	End Sub
	Sub Main()
	End Sub
End Module
 
