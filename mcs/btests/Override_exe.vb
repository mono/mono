Imports System
Imports NSOverride


Class D
	Inherits B2

	Public Overrides Sub S1(i as integer)
	End Sub

	Public Overrides Sub S2(i as integer, j as string)
	End Sub

	Public Overrides readonly Property SS(i as integer, y as string) As Integer
		get
		End Get
	End Property

	Public Overrides writeonly Property SS1(i as integer, y as string) as Integer
		set (Value As Integer)
		End Set
	End Property

	Public Overrides Property SS2(i as integer, y as string) as Integer
		get
		End Get
		set (Value As Integer)
		End Set
	End Property
End Class

Module M
	Sub main
	End Sub
End Module
