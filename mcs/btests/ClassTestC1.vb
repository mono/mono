REM LineNo: 11
REM ExpectedError: BC30689
REM ErrorMessage: Statement cannot appear outside of a method body.

REM LineNo: 12
REM ExpectedError: BC30087
REM ErrorMessage: 'End If' must be preceded by a matching 'If'.

Imports System
Class C1
	If True Then	
	End If
End Class
Module ClassTest
	Sub Main()
	End Sub
End Module
