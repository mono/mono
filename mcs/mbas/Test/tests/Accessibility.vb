Imports System
Class C1
	Public a As Integer
	Private b As Integer
	Friend c As Integer
	Protected d As Integer
	Public Sub S1() ' All data members of the class should be accessible
		a=10
		b=20
		c=30
		d=40
		S2()
	End Sub
	Private Sub S2()
	End Sub
End Class
Class C2
	Inherits C1
	Public Sub DS1() 'All data members except private members should be accessible
		a=100
		c=300
		d=400
        End Sub
End Class
Class C3
	Public Sub S1() 'All public and friend members should be accessible
		Dim myC As New C1()
		myC.a=1000
		myC.c=3000
	End Sub
End Class
Module Accessibility
	Sub Main()
  		 Dim myC1 As New C1()
		 myC1.S1()
 		 
		 Dim myC2 As New C2()
		 myC2.DS1()
		 Dim myC3 As New C3()
	         myC3.S1()
	End Sub
End Module
