Module CharacterLiterals1
    Sub Main()
        Dim c As Char
        c = "x"c

        Dim a As String = "X"
        If a <> c Then
            Throw New System.Exception("a is not same as c")
        End If
    End Sub
End Module
