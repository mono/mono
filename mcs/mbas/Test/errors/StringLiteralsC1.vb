REM LineNo: 7
REM ExpectedError: BC30205
REM ErrorMessage: End of statement expected.

Module StringLiteralsC1
    Sub main()
        Dim x As String = "b"b"
    End Sub
End Module
