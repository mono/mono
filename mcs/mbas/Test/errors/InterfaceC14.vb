'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2005 Novell, Inc.

REM LineNo: 17
REM ExpectedError: BC31035
REM ErrorMessage: Interface 'A' is not implemented by this class.

Imports System

Interface A
	Function A1()
End Interface

Class B
	Public Function A1() implements A.A1
	End Function
End Class

Module A2
	Sub Main()
		Dim x as A= new B()
		x.A1()	
	End Sub
End Module
