Public Class C1
End Class

Public Class C2
    Inherits C1
End Class

Module InheritanceB
    Sub Main()
        Dim b As C1 = New C2()
        Dim d As C2 = New C2()
    End Sub
End Module
