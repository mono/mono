Imports System
Module IntegerLiteralTest4
    Sub Main()
        Try
            Dim l As Integer
            l = System.Int32.MinValue
            l = l - 1
        Catch e As Exception
            Console.WriteLine(e.Message)
        End Try
    End Sub
End Module
