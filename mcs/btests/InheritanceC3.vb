REM LineNo: 12
REM ExpectedError: BC30121
REM ErrorMessage: 'Inherits' can appear only once within a 'Class' statement and can only specify one class.

Public Class C1
End Class

Public Class C2
End Class

Public Class C3
    Inherits C1, C2
End Class

Module InheritanceC3
    Sub Main()
    End Sub
End Module
