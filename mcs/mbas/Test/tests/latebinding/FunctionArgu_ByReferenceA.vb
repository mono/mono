Imports System
Module APR_1_0_0
	Class C
		Function F(ByRef p As Integer) as Integer
		p += 1
		return p
		End Function
	End Class

	Sub Main()
		Dim obj As Object = new C()
		Dim a As Integer = 1
		Dim b as Integer = 0
		b=obj.F(a)
		if (b<>a)
			Throw New System.Exception("#A1, Unexcepted Behaviour in Arguments_ByReferenceA.vb")
		end if
	End Sub 
End Module
