'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

Imports System

Interface A
	Function A1()
End Interface

Class B
	Implements A
	Public Function A1() implements A.A1
	End Function
End Class

Module A2
	Sub Main()
		Dim x as A= new B()
		x.A1()
	End Sub
End Module
