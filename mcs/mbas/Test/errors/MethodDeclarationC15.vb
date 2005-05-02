'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 13
REM ExpectedError: BC30812
REM ErrorMessage: Optional parameters must specify a default value.

Imports System

Module MethodDeclarationA
	Sub A1(Optional ByVal i as Integer )			
		i = 0
	End Sub
	Sub Main()
	End Sub
End Module
 
