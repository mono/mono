REM LineNo: 13
REM ExpectedError: BC31086
REM ErrorMessage: 'F1' overrides a sub in the base class 'B' that is not declared 'Overridable'.

Class B
    Public Sub F1()
    End Sub
End Class

Class D
    Inherits B

    Public Overrides Sub F1()
    End Sub
End Class


Module OverrideC1
    Sub Main()
    End Sub
End Module
