' Author:
'   Maverson Eduardo Schulze Rosa (maverson@gmail.com)
'
' GrupoTIC - UFPR - Federal University of Paraná

Module Test
Public i as integer
	Sub Main()
    		Try
			i = 5 
			out_of_block_backward_label:
			i+=2
			If ( i <> 9) Then
				goto out_of_block_backward_label
			Else
				Goto out_of_try_forward_label
 			End If	
		Finally
		    i+=1 
		End Try
		out_of_try_forward_label:
		if i<>10 then
			Throw new System.Exception("Finally block not working... Expected 10 but got "&i)
		End if
	End Sub
End Module
