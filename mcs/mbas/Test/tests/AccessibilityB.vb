Imports System
Class C1
	Public a As Integer=20
	Private b As Integer=30
	Friend c As Integer=40
	Protected d As Integer=50
	Public Sub S1() ' All data members of the class should be accessible
		Try

			If a<>20 Then
				Throw New Exception("#A1-Accessibility:Failed-error accessing value of public data member frm same class")
			End If
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try

		Try
			If b<>30 Then
				Throw New Exception("#A2-Accessibility:Failed-error accessing value of private data  member from same class")
			End If
  	        Catch e As Exception
                        Console.WriteLine(e.Message)
                End Try

   		Try

			If c<>40 Then	
				Throw New Exception("#A3-Accessibility:Failed-error accessing value of friend data  member from same class")
			End If
	        Catch e As Exception
                        Console.WriteLine(e.Message)
                End Try
		
		Try   	
			If d<>50 Then
				Throw New Exception("#A4-Accessibility:Failed-error accessing value of protected data  member from same class")
			End If
	        Catch e As Exception
                        Console.WriteLine(e.Message)
                End Try
 
 		S2()
	End Sub
	Private Sub S2()
	End Sub
End Class
Class C2
	Inherits C1
	Public Sub DS1() 'All data members except private members should be accessible
		Try
			If a<>20 Then
        	                Throw New Exception("#A5-Accessibility:Failed-error accessing value of public data  member from derived class")
                	End If
  	        Catch e As Exception
                        Console.WriteLine(e.Message)
                End Try

		Try
 			If c<>40 Then
        	                Throw New Exception("#A6-Accessibility:Failed-error accessing value of friend data  member from derived class")
                	End If
		 Catch e As Exception
                        Console.WriteLine(e.Message)
                End Try
		
		Try
		        If d<>50 Then
        	                Throw New Exception("#A7-Accessibility:Failed-error accessing value of protected data  member from derived")
                	End If
		Catch e As Exception
                        Console.WriteLine(e.Message)
                End Try

        End Sub
End Class
Class C3
	Public Sub S1() 'All public and friend members should be accessible
		Dim myC As New C1()
		Try
			If myC.a<>20 Then
        	                Throw New Exception("#A8-Accessibility:Failed-error accessing value of public data  member from another class")
                	End If
		Catch e As Exception
                        Console.WriteLine(e.Message)
                End Try

		Try
	                If myC.c<>40 Then 
        	                Throw New Exception("#A9-Accessibility:Failed-error accessing value of friend data  member from another class")
                	End If
                Catch e As Exception
                        Console.WriteLine(e.Message)
                End Try

	End Sub
End Class
Module Accessibility
	Sub Main()
  		 Dim myC1 As New C1()
		 myC1.S1()
		 Try
			 If myC1.a<>20 Then
				Throw New Exception("#A10-Accessibility:Failed-error accessing value of  public data member form another module")
			 End If
		 Catch e As Exception
                        Console.WriteLine(e.Message)
                 End Try
 
		 Dim myC2 As New C2()
		 myC2.DS1()
		 Dim myC3 As New C3()
	         myC3.S1()
	End Sub
End Module
