REM LineNo: 9
REM ExpectedError: BC30439
REM ErrorMessage: Constant expression not representable in type 'Integer'.

Imports System

Module M
	Public Enum E1
		A =  System.Int64.MaxValue
		B 
	End Enum

   Sub Main()
	Dim i as integer = E1.A
    End Sub
End Module

