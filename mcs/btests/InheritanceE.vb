Public Class C1
    Public Overridable Sub F1()
    End Sub
End Class

Public Class C2
    Inherits C1

    Public Overrides Sub F1()
    End Sub
End Class

Module InheritanceE
    Sub Main()
	dim b as New C1()
	b.F1()

        Dim d As C2 = New C2()
	d.F1()
    End Sub
End Module



