REM LineNo: 13
REM ExpectedError: BC30058
REM ErrorMessage: Statements and labels are not valid between 'Select Case' and first 'Case'

Imports System

Module ConditionalStatementsC15

    Sub Main()
	Dim i As Integer = 0
	Select Case i
		 
			Console.WriteLine("Hello World")
		Case >= 2
			Console.WriteLine("Compile time error")        
	end select
    End Sub

End Module