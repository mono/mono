Imports System
Module ConditionalCompilation
	Sub Main()
		Dim value As Integer
		Try

			'Testing #If,#Elseif, #Else and #End If block
                                                                                                                             
        	        #If False
                	          Throw New Exception("#A1-Conditional Compilation :Failed")
	                #ElseIf True
        	                value=30
                	#Else
                        	  Throw New Exception("#A2-Conditional Compilation :Failed")
	                #End If
                                                                                                                            
        	        If value<>30 Then
                	        Throw New Exception("#A3-Conditional Compilation:Failed ")
	                End If
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
                                                                                                                             
                                                                                                                             
                'Testing nested #If and #End If Block
                Try                                                                                                             
	                #If True
        	                #If True
			                 value=40
	                        #End If
        	        #End If

                	If value<>40
        	               	Throw New Exception("#A4-Conditional Compilation:Failed ")
	                End If
		Catch e As Exception
			Console.WriteLine(e.Message)
		End Try
	End Sub
End Module
