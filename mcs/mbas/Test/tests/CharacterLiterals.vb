Module CharacterLiterals
    Sub Main()
        Dim c As Char
        c = "x"

        c = "X"

        Dim a As String = "X"c
        If a <> c Then
            Throw New System.Exception("a is not same as c")
        End If

        'the outcome should be "x"
        c = """x"""
    End Sub

End Module
