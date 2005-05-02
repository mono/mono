Imports System

Module ConcatenationOperator
    Sub main()
        Dim a As String = "Hello "
        Dim b As String = "World"

        Dim c As String = a & b
        If c <> "Hello World" Then
            Throw new System.Exception("#A1-Concatenation Failed")
        End If

        c = a & CInt(123)
        If c <> "Hello 123" Then
            Throw new System.Exception("#A2-Concatenation Failed")
        End If

        c = a & Nothing
        If c <> "Hello " Then
            Throw new System.Exception("#A3-Concatenation Failed")
        End If

        c = Nothing & a
        If c <> "Hello " Then
            Throw new System.Exception("#A4-Concatenation Failed")
        End If

        c = a & CDec(123.23)
        If c <> "Hello 123.23" Then
            Throw new System.Exception("#A5-Concatenation Failed")
        End If

        c = a & CBool(123)
        If c <> "Hello True" Then
            Throw new System.Exception("#A6-Concatenation Failed")
        End If

    End Sub

End Module