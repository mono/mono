Module IntegerLiteralTestC1
    Sub Main()
        Try
            Dim i As Integer
            i = &H2G
        Catch e As Exception
            Console.WriteLine(e.Message)
        End Try
    End Sub
End Module
