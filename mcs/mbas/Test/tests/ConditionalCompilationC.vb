#If True
Imports System
Module ConditionalCompilation
	Sub Main()
		Dim value As Integer
		Try
		'Testing nested #If,#Elseif, #Else and #End If block 

		#If False          
			Throw New Exception("#C01-Conditional Compilation :Failed")

        	        #If False
                	          Throw New Exception("#C02-Conditional Compilation :Failed")
	                #ElseIf True
        	                value=10
                	#Else
                        	  Throw New Exception("#C03-Conditional Compilation :Failed")
	                #End If

			Throw New Exception("#C04-Conditional Compilation :Failed")
		#ElseIf True
        	        #If True
				value=20
	                #ElseIf True
				Throw New Exception("#C05-Conditional Compilation :Failed")
                	#Else
                        	  Throw New Exception("#C06-Conditional Compilation :Failed")
	                #End If
                                                                                                                            
        	        If value<>20 Then
                	        Throw New Exception("#C07-Conditional Compilation:Failed ")
	                End If

		#ElseIf True
        	        #If False
				Throw New Exception("#C08-Conditional Compilation :Failed")
	                #ElseIf False
				Throw New Exception("#C09-Conditional Compilation :Failed")
                	#Else
				value=30
	                #End If
                                                                                                                            
        	        If value<>30 Then
                	        Throw New Exception("#C10-Conditional Compilation:Failed ")
	                End If
		#Else
        	        #If False
				Throw New Exception("#C11-Conditional Compilation :Failed")
	                #ElseIf True
				Throw New Exception("#C12-Conditional Compilation :Failed")
                	#ElseIf False
				Throw New Exception("#C13-Conditional Compilation :Failed")
	                #End If

               	        Throw New Exception("#C14-Conditional Compilation:Failed ")
		#End If
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try                                                                                                      
	End Sub
End Module
#End If