REM LineNo: 9
REM ExpectedError: BC30299
REM ErrorMessage: 'C2' cannot inherit from class 'C1' because 'C1' is declared 'NotInheritable'.

NotInheritable Class C1
End Class

Class C2
    Inherits C1
End Class

Module Module1
    Sub Main()
    End Sub
End Module

