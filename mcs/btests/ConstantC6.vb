REM LineNo: 42
REM ExpectedError: BC30424
REM ErrorMessage: Constants must be an intrinsic or enumerated type, not a class, structure, or array type.

REM LineNo: 42
REM ExpectedError: BC30445
REM ErrorMessage: Const declaration cannot have an array initializer.

REM LineNo: 43
REM ExpectedError: BC30424
REM ErrorMessage: Constants must be an intrinsic or enumerated type, not a class, structure, or array type.

REM LineNo: 43
REM ExpectedError: BC30672
REM ErrorMessage: Explicit initialization is not permitted for arrays declared with explicit bounds.

REM LineNo: 43
REM ExpectedError: BC30445
REM ErrorMessage: Const declaration cannot have an array initializer.

REM LineNo: 44
REM ExpectedError: BC30424
REM ErrorMessage: Constants must be an intrinsic or enumerated type, not a class, structure, or array type.

REM LineNo: 45
REM ExpectedError: BC30424
REM ErrorMessage: Constants must be an intrinsic or enumerated type, not a class, structure, or array type.

REM LineNo: 45
REM ExpectedError: BC30445
REM ErrorMessage: Const declaration cannot have an array initializer.

REM LineNo: 45
REM ExpectedError: BC30424
REM ErrorMessage: Constants must be an intrinsic or enumerated type, not a class, structure, or array type.

REM LineNo: 45
REM ExpectedError: BC30445
REM ErrorMessage: Const declaration cannot have an array initializer.

Module Constant
    Const a() As Integer = {1, 2}
    Const b(2) As Long = {1, 2}
    Const c() As String
    Const d() As Long = {1, 2}, e() As Long = {1, 2}
    Sub main()
    End Sub
End Module
