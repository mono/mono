Imports System
Module ConditionalCompilation
	Sub Main()
		Dim value As Integer
		Try
			'Testing #If,#Elseif, #Else and #End If block - Variation 1
                                                                                                                             
        	        #If False
                	          Throw New Exception("#B11-Conditional Compilation :Failed")
	                #ElseIf True
        	                value=10
                	#Else
                        	  Throw New Exception("#B12-Conditional Compilation :Failed")
	                #End If
                                                                                                                            
        	        If value<>10 Then
                	        Throw New Exception("#B13-Conditional Compilation:Failed ")
	                End If
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try

		Try
			'Testing #If,#Elseif, #Else and #End If block - Variation 2
                                                                                                                             
        	        #If True
				value=20
	                #ElseIf True
				Throw New Exception("#B21-Conditional Compilation :Failed")
                	#Else
                        	  Throw New Exception("#B22-Conditional Compilation :Failed")
	                #End If
                                                                                                                            
        	        If value<>20 Then
                	        Throw New Exception("#B23-Conditional Compilation:Failed ")
	                End If
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try

		Try
			'Testing #If,#Elseif, #Else and #End If block - Variation 3
                                                                                                                             
        	        #If False
				Throw New Exception("#B31-Conditional Compilation :Failed")
	                #ElseIf False
				Throw New Exception("#B32-Conditional Compilation :Failed")
                	#Else
				value=30
	                #End If
                                                                                                                            
        	        If value<>30 Then
                	        Throw New Exception("#B33-Conditional Compilation:Failed ")
	                End If
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
                       
		Try
			'Testing #If,#Elseif, #Else and #End If block - Variation 4

			value=40                                                                                                                             
        	        #If False
				Throw New Exception("#B41-Conditional Compilation :Failed")
	                #ElseIf False
				Throw New Exception("#B42-Conditional Compilation :Failed")
	                #ElseIf True
				value=40
                	#Else
				Throw New Exception("#B42-Conditional Compilation :Failed")
	                #End If
                                                                                                                            
        	        If value<>40 Then
                	        Throw New Exception("#B33-Conditional Compilation:Failed ")
	                End If
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try


		Try
			'Testing #If,#Elseif and #End If block - Variation 5
                                                         
			value=50
                                                                    
        	        #If False
				Throw New Exception("#B51-Conditional Compilation :Failed")
	                #ElseIf False
				Throw New Exception("#B52-Conditional Compilation :Failed")
                	#ElseIf False
				Throw New Exception("#B53-Conditional Compilation :Failed")
	                #End If
                                                                                                                            
        	        If value<>50 Then
                	        Throw New Exception("#B54-Conditional Compilation:Failed ")
	                End If
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try                                                                                                      

		Try
			'Testing #If,#Elseif and #End If block - Variation 6
                                                         
        	        #If False
				Throw New Exception("#B61-Conditional Compilation :Failed")
	                #ElseIf True
				value=60
                	#ElseIf False
				Throw New Exception("#B63-Conditional Compilation :Failed")
	                #End If
                                                                                                                            
        	        If value<>60 Then
                	        Throw New Exception("#B64-Conditional Compilation:Failed ")
	                End If
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try                                                                                                      
	End Sub
End Module
