REM LineNo: 10
REM ExpectedError: BC30439
REM ErrorMessage: Constant expression not representable in type 'Integer'.

Imports System

Module M
	Public Enum E1
		A =  System.Int32.MaxValue
		B 
	End Enum

   Sub Main()
	Dim i as integer = E1.A
    End Sub
End Module

