Imports System

Module ConditionalStatementsC

    Sub Main()

        Dim i As Integer
        Dim arr(10) As Integer

        For i = 0 To 10

            Select Case i
                Case 0 To 4
                'Case 0 
                    'arr(i) = 1
		Console.WriteLine("MANJU i is {0}",i)
            End Select

        Next


    End Sub

End Module
