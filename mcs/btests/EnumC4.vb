REM LineNo: 10
REM ExpectedError: BC30500
REM ErrorMessage: Constant 'A' cannot depend on its own value.

Imports System

Module M
	Public Enum E1 As Long
		A = B
		B
	End Enum

   Sub Main()
    End Sub
End Module


