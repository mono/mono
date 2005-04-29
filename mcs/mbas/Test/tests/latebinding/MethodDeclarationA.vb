'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System

Module MethodDeclarationA
Class C
	Function A(i as Integer)As Integer			 
	End Function
	Function AB()As Integer			 
		return 10
	End Function
End Class
	Sub Main()
		Dim o As Object = new C()
		o.A (o.AB())
	End Sub
End Module
 
