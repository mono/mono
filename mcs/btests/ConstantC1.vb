REM LineNo: 14
REM ExpectedError: BC30424
REM ErrorMessage: Constants must be an intrinsic or enumerated type, not a class, structure, or array type.

REM LineNo: 14
REM ExpectedError: BC30672
REM ErrorMessage: Explicit initialization is not permitted for arrays declared with explicit bounds.

REM LineNo: 14
REM ExpectedError: BC30445
REM ErrorMessage: Const declaration cannot have an array initializer.

Module ConstantC1
    Const a(2) As Integer = {1, 2}
    Sub main()
    End Sub
End Module
