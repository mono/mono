Imports System
Module ExternalSourceDirective
	Sub Main()
	    #ExternalSource(“/home/test.aspx”,30)
            	    Console.WriteLine(“In test.aspx”)
	    #End ExternalSource
	End Sub
	#ExternalSource(“/home/test.aspx”,30)
		Sub S()
	        End Sub
	#End ExternalSource
End Module
