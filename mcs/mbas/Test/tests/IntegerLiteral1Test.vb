imports System

Module IntegerLiteralTest
    Sub Main()
        Try
            Dim i As Integer
            Dim l As Long
            Dim s As Short

            l = 20L
            s = 20S
            i = 20I

        Catch e As Exception
            Console.WriteLine(e.Message)
        End Try
    End Sub
End Module
