'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 13
REM ExpectedError: BC30641
REM ErrorMessage: 'ByVal' and 'ByRef' cannot be combined.

Imports System

Module MethodDeclarationA
	Sub A(ByVal ByRef i as Integer)			
	End Sub
	Sub Main()
	End Sub
End Module
 
