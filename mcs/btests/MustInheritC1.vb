REM LineNo: 15
REM ExpectedError: BC30376
REM ErrorMessage: 'New' cannot be used on class 'C1' because it contains a 'MustOverride' member that has not been overridden.

MustInherit Class C1
    Public Function F1()
        Dim a As Integer = 10
    End Function

    Public MustOverride Function F2()
End Class

Module MustInheritC1
    Sub Main()
        Dim x As C1 = New C1()
    End Sub
End Module
