'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 19
REM ExpectedError: BC30149
REM ErrorMessage: 'B' must implement 'Sub fun([a As Integer = 10])' for interface 'A'.

REM LineNo: 20
REM ExpectedError: BC30401
REM ErrorMessage: 'Cfun' cannot implement 'fun' because there is no matching sub on interface 'A'.

Interface A	
	Sub fun(ByVal Optional a As Integer=10)
End Interface

Class B
	Implements A
	Sub Cfun(ByVal Optional a As Integer=20) Implements A.fun
	End Sub	
End Class

Module InterfaceI
	Sub Main()	
	End Sub
End Module
