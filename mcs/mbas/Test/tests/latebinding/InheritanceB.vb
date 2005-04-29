Imports System

Public Class C1
	Public Overridable Sub F1(ByVal name As String)
		Dim t As Type = GetType (C1)
		if t.name <> name Then
			throw new Exception ("#A1, Should not some here")
		End If
	End Sub
End Class

Public Class C2
	Inherits C1

	Public Overrides Sub F1(ByVal name As String)
		Dim t As Type = GetType (C2)
		if t.name <> name Then
			throw new Exception ("#A2, Should not some here")
		End If
	End Sub
End Class

Module InheritanceE
	Sub Main()
		dim b as object = New C1()
		b.F1("C1")
	
		Dim d As object = New C2()
		d.F1("C2")
	End Sub
End Module



