REM LineNo: 16
REM ExpectedError: BC30730
REM ErrorMessage: Methods declared 'Overrides' cannot be declared 'Overridable' because they are implicitly overridable.

REM LineNo: 16
REM ExpectedError: BC40005
REM ErrorMessage: sub 'F1' shadows an overridable method in a base class. To override the base method, this method must be declared 'Overrides'.

Class A
    Public Overridable Sub F1()
    End Sub
End Class

Class B
    Inherits A
    Public Overrides Overridable Sub F1()
    End Sub
End Class


Module OverrideC1
    Sub Main()
    End Sub
End Module
