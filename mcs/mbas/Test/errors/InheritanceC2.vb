REM LineNo: 10
REM ExpectedError: BC30683
REM ErrorMessage: 'Inherits' statement must precede all declarations in a class.

Public Class C1
End Class

Public Class C2
    Dim a As String
    Inherits C1
End Class

Module InheritanceC2
    Sub Main()
    End Sub
End Module
