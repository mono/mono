Imports System

Module M1
    Function Main() As Integer

        Dim x As Integer = 7
        Dim y As Integer = 2

        y += 3
        x = y + 10
        If y <> 5 Then
            Return 1
        End If
        If x <> 15 Then
            Return 2
        End If

        x += 9
        If x <> 24 Then
            Return 3
        End If

        Dim c As Byte = 3
        Dim d As Byte = 5
        d ^= c
        Console.WriteLine(d)

        Dim s As Short = 5
        Dim i As Integer = 400001
        s   <<= i
        Console.WriteLine(s)

    End Function

End Module
