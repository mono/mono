REM LineNo: 14
REM ExpectedError: BC30456
REM ErrorMessage: 'c' is not a member of 'M.E1'.

Imports System

Module M
	Public Enum E1 As Long
		A = 2
		B
	End Enum

   Sub Main()
	Dim i as long = E1.c
    End Sub
End Module
