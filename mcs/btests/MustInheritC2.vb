REM LineNo: 9
REM ExpectedError: BC30610
REM ErrorMessage: Class 'C2' must either be declared 'MustInherit' or override the following inherited 'MustOverride' member(s): Public MustOverride Function F1() As Object.

MustInherit Class C1
    Public MustOverride Function F1()
End Class

Class C2
    Inherits C1
End Class

Module MustInheritC2
    Sub Main()
    End Sub
End Module
