'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.


REM LineNo: 17
REM ExpectedError: BC30401
REM ErrorMessage: 'Cfun' cannot implement 'fun' because there is no matching sub on interface 'A'.

Interface A
	Sub fun(ByVal a As Integer)
End Interface

Class B
	Implements A
	Sub Cfun(ByVal a As String) Implements A.fun
	End Sub
End Class

Module InterfaceI
	Sub Main()	
	End Sub
End Module
