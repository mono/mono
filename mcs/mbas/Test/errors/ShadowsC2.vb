REM LineNo: 16
REM ExpectedError: BC31408
REM ErrorMessage: 'Overloads' and 'Shadows' cannot be combined.

Class B
    Function F()
    End Function

    Function F(ByVal i As Integer)
    End Function
End Class

Class D
    Inherits B

    Overloads Shadows Function F()
    End Function
End Class

Module ShadowsC2
    Sub Main()
    End Sub
End Module
