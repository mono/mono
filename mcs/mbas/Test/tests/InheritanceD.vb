Interface IBase
    Function F(ByVal i As Integer)
End Interface

Interface ILeft
    Inherits IBase
End Interface

Interface IRight
    Inherits IBase
End Interface

Interface IDerived
    Inherits ILeft, IRight
End Interface

Class D
    Implements IDerived

    Function F(ByVal i As Integer) Implements IDerived.F
    End Function
End Class

Module InheritanceD
    Sub Main()
    End Sub
End Module