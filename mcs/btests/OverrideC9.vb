REM LineNo: 12
REM ExpectedError: BC31408
REM ErrorMessage: 'Overrides' and 'Shadows' cannot be combined.

Class A
    Public Overridable Sub F1()
    End Sub
End Class

Class B
    Inherits A
    Public Overrides Shadows Sub F1()
    End Sub
End Class


Module OverrideC1
    Sub Main()
    End Sub
End Module
