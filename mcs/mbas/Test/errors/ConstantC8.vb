REM LineNo: 6
REM ExpectedError: BC30424
REM ErrorMessage: Constants must be an intrinsic or enumerated type, not a class, structure, or array type.

Module Constant
    Const c() As String
    Sub main()
    End Sub
End Module
