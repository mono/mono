REM LineNo: 5
REM ExpectedError: BC31408
REM ErrorMessage: 'MustInherit' and 'NotInheritable' cannot be combined.

MustInherit NotInheritable Class C1
End Class

Module Module1
    Sub Main()
    End Sub
End Module
