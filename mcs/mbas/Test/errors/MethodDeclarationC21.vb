'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 16
REM ExpectedError: BC30566
REM ErrorMessage:  Array initializer has too many dimensions.

Imports System

Module MethodDeclarationA
	Sub A1(ParamArray ByVal args() as Integer)			
	End Sub
	Sub Main()
		A1(New Integer(){{1,1},2,3})
	End Sub
End Module
 
