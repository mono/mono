'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 19
REM ExpectedError: BC30149
REM ErrorMessage: 'B' must implement 'Sub fun(ParamArray a() As Integer)' for interface 'A'.

REM LineNo: 20
REM ExpectedError: BC30401
REM ErrorMessage: 'Cfun' cannot implement 'fun' because there is no matching sub on interface 'A'.

Interface A
	Sub fun(ByVal Paramarray a() As Integer)
End Interface

Class B
	Implements A
	Sub Cfun(ByVal a() As Integer) Implements A.fun
	End Sub
End Class

Module InterfaceI
	Sub Main()	
	End Sub
End Module
