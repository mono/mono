REM LineNo: 13
REM ExpectedError: BC30424
REM ErrorMessage: Constants must be an intrinsic or enumerated type, not a class, structure, or array type.

REM LineNo: 13
REM ExpectedError: BC30059
REM ErrorMessage: Constant expression is required.

Class C
End Class

Module ConstantC3
    Const a As C = New C()
    Sub main()
    End Sub
End Module
