Imports System

NameSpace NSOverride
	Public Class B1
		Public Overridable readonly Property SS(i as integer, y as string) as Integer
			get
			End Get
		End Property
	
		Public Overridable writeonly Property SS1(i as integer, y as string) as Integer
			set (Value As Integer)
			End Set
		End Property

		Public Overridable Property SS2(i as integer, y as string) as Integer
			get
			End Get
			set (Value As Integer)
			End Set
		End Property
		
		Public Overridable Sub S2(i as integer, j as string)
		End Sub
	End Class
	

	Public Class B2
		Inherits B1
		
		Public Overridable Sub S1(i as integer)
		End Sub
	End Class
End NameSpace
