REM LineNo: 14
REM ExpectedError: BC32006
REM ErrorMessage: 'char' values cannot be converted to 'Integer'.

Imports System

Module ConditionalStatementsC14

    Sub Main()
	Dim i As Integer = 0
	Select Case i
		Case 1
			Console.WriteLine("Hello World")
		Case >= "E"c
			Console.WriteLine("Compile time error")        
	end select
    End Sub

End Module