REM LineNo: 13
REM ExpectedError: BC30610
REM ErrorMessage: Class 'D' must either be declared 'MustInherit' or override the following inherited 'MustOverride' member(s): Public MustOverride Function F() As Object.

REM LineNo: 16
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
