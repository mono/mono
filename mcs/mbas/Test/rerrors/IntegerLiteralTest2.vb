Imports System
Module IntegerLiteralTest2
    Sub Main()
        Try
            Dim i As Integer
            i = System.Int32.MinValue
            i = i - 1
        Catch e As Exception
            Console.WriteLine(e.Message)
        End Try
    End Sub
End Module
