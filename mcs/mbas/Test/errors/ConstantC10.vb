REM LineNo: 13
REM ExpectedError: BC30424
REM ErrorMessage: Constants must be an intrinsic or enumerated type, not a class, structure, or array type.

REM LineNo: 13
REM ExpectedError: BC30438
REM ErrorMessage: Constants must have a value.

Class C
End Class

Module ConstantC3
    Const a As C 
    Sub main()
    End Sub
End Module
