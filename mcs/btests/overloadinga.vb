'Testing overloading in different classes
Class B
    Public Function F()
    End Function

    Public Function F(ByVal i As Integer)
    End Function

    Public Function F(ByVal s As String)
    End Function

    Public overridable Function F1()
    End Function

    Public Function F1(ByVal i As Integer)
    End Function
End Class

Class D
    Inherits B

    Public Function F()
    End Function

    Public overloads Function F(ByVal s as Integer)
    End Function

    Public overloads Function F(ByVal s As String)
    End Function

    Public Function F1()
    End Function
End Class

Module OverloadingA
    Sub Main()
        Dim x As D = New D()

        x.F()
        x.F(10)
        x.F1(20)

	Dim x1 As B=New B()
	x1.F(20)
	x1.F1()
    End Sub

End Module
