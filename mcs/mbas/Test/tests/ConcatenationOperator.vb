Imports System

Module ConcatenationOperator
    Sub main()
        Dim a As String = "Hello "
        Dim b As String = "World"

        Dim c As String = a & b
        If c <> "Hello World" Then
            Console.WriteLine("#A1-Concatenation Failed")
        End If

        c = a & CInt(123)
        If c <> "Hello 123" Then
            Console.WriteLine("#A2-Concatenation Failed")
        End If

        c = a & Nothing
        If c <> "Hello " Then
            Console.WriteLine("#A3-Concatenation Failed")
        End If

        c = Nothing & a
        If c <> "Hello " Then
            Console.WriteLine("#A4-Concatenation Failed")
        End If

        c = a & CDec(123.23)
        If c <> "Hello 123.23" Then
            Console.WriteLine("#A5-Concatenation Failed")
        End If

        c = a & CBool(123)
        If c <> "Hello True" Then
            Console.WriteLine("#A6-Concatenation Failed")
        End If

    End Sub

End Module