REM LineNo: 5
REM ExpectedError: BC31411
REM ErrorMessage: 'C1' must be declared 'MustInherit' because it contains methods declared 'MustOverride'.

Class C1
    Public MustOverride Function F1()
End Class

Module MustInheritC3
    Sub Main()
    End Sub
End Module


