Imports System
Module ConditionalCompilation
	Sub Main()
		Dim value As Integer
		'Testing whitespaces between #If
                Try                                                                                                             
                	#	 If True
                        	value=50
	                #End   If
        	        If value<>50
                	        Throw New Exception("#A1-Conditional Compilation:Failed ")
	                End If
        	Catch e As Exception
			Console.WriteLine(e.Message)
		End Try                                                                                                                     
                                                     
      End Sub


#Const A = True
#Const B = False
#If A Then
    Sub Z()
    End Sub
#Else
    Sub W()
    End Sub
#End If
#If B Then
    Sub X()
    End Sub
#Else
    Sub Y()
    End Sub
#End If

End Module

