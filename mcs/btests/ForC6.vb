REM LineNo: 11,15
REM ExpectedError: BC30201
REM ErrorMessage: Expression expected

Imports System

Module ForC6

    Sub main()
        Dim i As Integer
        For i = 0 To
            Console.WriteLine("Hello World")
        Next

        For i = 0 To 10 Step 
            Console.WriteLine("Hello World 1")
        Next
    End Sub

End Module