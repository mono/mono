REM LineNo: 4
REM ExpectedError: BC30420
REM ErrorMessage: 'Sub Main' was not found in 'NameSpaceA'.

Namespace ns1
    Class c1
        Public a As Integer = 5
    End Class
End Namespace

Namespace ns2
    Class c2
        Public b As Integer
    End Class
End Namespace