REM LineNo: 6
REM ExpectedError: BC30284
REM ErrorMessage: sub 'F1' cannot be declared 'Overrides' because it does not override a sub in a base class.

Class D
    Public Overrides Sub F1()
    End Sub
End Class


Module OverrideC1
    Sub Main()
    End Sub
End Module
