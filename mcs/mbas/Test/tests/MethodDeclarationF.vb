'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System

Module MethodDeclarationA
	Sub A1 ( ByVal ParamArray j() as Date )			
		Dim i as Date
		For each i in j
		Next i
	End Sub
	Sub Main()
		Dim ar as Date() = {}
		A1(ar) 
	End Sub
End Module
 
