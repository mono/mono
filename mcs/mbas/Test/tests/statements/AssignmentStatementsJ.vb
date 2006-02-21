Imports System
Imports Microsoft.VisualBasic

Module AssignmentStatementsC
	Class Obj
		Dim _prop as String
		Public Property prop() As String
			Set
				_prop = Value
			End Set
			Get
				Return _prop
			End Get
		End Property
	End Class

	Sub Main()
		Dim foo As New Obj
		foo.prop = "Hello"
		foo.prop += "World"

        	If foo.prop <> "HelloWorld" Then
            		Throw New Exception("#ASC5 - Assignment Statement failed")
        	End If
	End Sub
End Module
