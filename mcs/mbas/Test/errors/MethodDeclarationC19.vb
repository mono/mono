'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 13
REM ExpectedError: BC30060
REM ErrorMessage:Conversion from 'String' to 'Integer' cannot occur in a constant expression.

Imports System

Module MethodDeclarationA
	Sub A1(Optional ByVal j as Integer = "Hello")			
	End Sub
	Sub Main()
	End Sub
End Module
 
