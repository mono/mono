imports System

Module IntegerLiteralTest
    Sub Main()
        Try
            Dim i As Integer
            Dim l As Long
            Dim s As Short

            i = 20
            i = System.Int32.MaxValue
            i = System.Int32.MinValue

            l = (System.Int32.MaxValue)
            l = l + 100
            l = System.Int64.MaxValue
            l = System.Int64.MinValue
        Catch e As Exception
            Console.WriteLine(e.Message)
        End Try
    End Sub
End Module
