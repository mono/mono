REM LineNo: 14
REM ExpectedError: BC30610
REM ErrorMessage: Class 'C3' must either be declared 'MustInherit' or override the following inherited 'MustOverride' member(s): Public MustOverride Function F1() As Integer.

REM LineNo: 16
REM ExpectedError: BC31404
REM ErrorMessage: 'Public Function F1() As Integer' cannot shadow a method declared 'MustOverride'.

MustInherit Class C1
    Public MustOverride Function F1() As Integer
End Class

'Omitting keyword 'overrides' in a class that inherits the mustinherit class
Class C3
    Inherits C1
        Public Function F1() As Integer
        End Function
End Class

Module MustInheritC2
    Sub Main()
    End Sub
End Module


