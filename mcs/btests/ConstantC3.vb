REM LineNo: 9
REM ExpectedError: BC30059
REM ErrorMessage: Constant expression is required.

Class C
End Class

Module ConstantC3
    Const a As Integer = New Integer(10)
    Sub main()
    End Sub
End Module
