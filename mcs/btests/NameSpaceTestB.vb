REM LineNo: 7
REM ExpectedError: BC30002
REM ErrorMessage: Type 'ns1.c1' is not defined.

Module NSB
    Sub Main()
        Dim c1 As ns1.c1 = New ns1.c1()
    End Sub
End Module
