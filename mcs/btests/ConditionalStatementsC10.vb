REM LineNo: 14
REM ExpectedError: BC30095
REM ErrorMessage: 'Select Case' must end with a matching 'End Select'.

Imports System

Module ConditionalStatementsC10

    Sub Main()

        Dim i As Integer = 0

        Select case i 
		case 0 To 1
			Console.WriteLine("i is either 0 or 1")
      
    End Sub

End Module