REM LineNo: 6
REM ExpectedError: BC30672
REM ErrorMessage: Explicit initialization is not permitted for arrays declared with explicit bounds.

Module VariableC3
    Dim j(5) As Integer = {1, 2, 3, 4, 5}

    Sub main()
    End Sub
End Module
