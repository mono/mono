REM LineNo: 8
REM ExpectedError: BC30004
REM ErrorMessage: Character constant must contain exactly one character.

Module CharacterLiteralsC2
    Sub Main()
        Dim ch As Char
        ch = "xc"c
    End Sub
End Module
