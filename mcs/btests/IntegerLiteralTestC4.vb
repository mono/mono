Module IntegerLiteralTestC4
    Sub Main()
        Try
            Dim i As Int16
            i = A
        Catch e As Exception
            Console.WriteLine(e.Message)
        End Try
    End Sub
End Module
