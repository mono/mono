REM LineNo: 14
REM ExpectedError: BC30074
REM ErrorMessage: Constant cannot be the target of an assignment.

Imports System

Module M
	Public Enum E1 As Long
		A = 2
		B
	End Enum

   Sub Main()
	E1.A = 5
	
    End Sub

	

End Module
