Class B
    Overridable Function F() As Integer
        Return 5
    End Function
End Class

Class D
    Inherits B

    Overrides Function F() As Integer
        ' you should be able to access 
        ' the members of base class 
        ' using 'MyBase' as follows
        MyBase.F()

        Return 10
    End Function
End Class

Module OverrideA
    Sub Main()
        Dim x As B

        x = New B()
        If x.F() <> 5 Then
            Throw New System.Exception("#A1, unexpected result from base class")
        End If

        x = New D()
        If x.F() <> 10 Then
            Throw New System.Exception("#A2, unexpected result from derived class")
        End If
    End Sub
End Module
