Class C1
	Public Overridable Sub S(byVal a As Integer)
	End Sub
End Class
Class C2
   Inherits C1
	Public Overrides Sub S()
	End Sub
	Private Overrides Sub S(byVal a As Integer)
	End Sub
	Public Overrides Sub S(byRef a As Integer)
	End Sub
	Public Overrides Sub S(byVal a As String)
	End Sub
End Class
Module OverrideC2
	Sub Main()
	End Sub
End Module
