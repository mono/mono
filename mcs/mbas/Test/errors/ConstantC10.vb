REM LineNo: 9
REM ExpectedError: BC30438
REM ErrorMessage: Constants must have a value.

Class C
End Class

Module ConstantC3
    Const a As C 
    Sub main()
    End Sub
End Module
