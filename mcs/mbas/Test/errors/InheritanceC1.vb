REM LineNo: 10
REM ExpectedError: BC30256
REM ErrorMessage: Class 'C2' cannot inherit from itself:

Public Class C1
    Inherits C2
End Class

Public Class C2
    Inherits C1
End Class

Module InheritanceC1
    Sub Main()
    End Sub
End Module
