REM LineNo: 14
REM ExpectedError: BC30205
REM ErrorMessage: End of statement expected

Imports System

Module ConditionalStatementsC11

    Sub Main()

        Dim i As Integer = 0

        Select i 
		Case 0 To 2  	i++

                Case Is < 2, 3, 6 To 7
			i = i + 2
        End Select

    End Sub

End Module