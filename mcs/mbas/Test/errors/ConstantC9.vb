REM LineNo: 18
REM ExpectedError: BC30424
REM ErrorMessage: Constants must be an intrinsic or enumerated type, not a class, structure, or array type.

REM LineNo: 18
REM ExpectedError: BC30445
REM ErrorMessage: Const declaration cannot have an array initializer.

REM LineNo: 18
REM ExpectedError: BC30424
REM ErrorMessage: Constants must be an intrinsic or enumerated type, not a class, structure, or array type.

REM LineNo: 18
REM ExpectedError: BC30445
REM ErrorMessage: Const declaration cannot have an array initializer.

Module Constant
    Const d() As Long = {1, 2}, e() As Long = {1, 2}
    Sub main()
    End Sub
End Module
