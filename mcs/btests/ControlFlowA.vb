Imports System

Module ControlFlowA
    Dim result As Integer

    Sub F1()
	Dim i As Integer
        For i = 0 To 4
            Stop
            result += i
            If i = 3 Then
                Exit Sub
            End If 
        Next i
        result = 4
    End Sub

    Sub main()

        F1()
        If result <> 6 Then
            Throw New Exception("#CFA1 - Exit Statement failed")
        End If
        Console.WriteLine(result)
        Try
            End
            Console.WriteLine("After Stop Statement")
        Catch e As Exception
            Console.WriteLine(e.Message)
        Finally
            Throw New Exception("#CFA2 - End Statement failed")
        End Try

    End Sub

End Module
