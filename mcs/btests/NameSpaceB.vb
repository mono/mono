REM LineNo: 11
REM ExpectedError: BC30002
REM ErrorMessage: Type 'ns1.c1' is not defined.

REM LineNo: 12
REM ExpectedError: BC30002
REM ErrorMessage: Type 'ns2.c2' is not defined.

Module NSB
    Sub Main()
        Dim c1 As ns1.c1 = New ns1.c1()
        Dim c2 As ns2.c2 = New ns2.c2()
        If c1.a <> 5 Then
            Throw New System.Exception("value of ns1.c1.a got changed")
        End If
    End Sub
End Module
