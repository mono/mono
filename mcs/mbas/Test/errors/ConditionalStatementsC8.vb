REM LineNo: 12
REM ExpectedError: BC30201
REM ErrorMessage: Expression expected

Imports System

Module ConditionalStatementsC8

    Sub Main()

        Dim i As Integer = 0
        Select 
		Case 0 To 
                    i = 10
                Case Else
                    i = 11
        End Select   

    End Sub

End Module