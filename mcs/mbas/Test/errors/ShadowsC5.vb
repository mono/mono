REM LineNo: 12
REM ExpectedError: BC31404
REM ErrorMessage: 'Public Shadows Function F() As Object' cannot shadow a method declared 'MustOverride'.

MustInherit Class B
    MustOverride Function F()
End Class

Class D
    Inherits B

    Shadows Function F()
    End Function
End Class

Module ShadowsC3
    Sub Main()
    End Sub
End Module
