Imports System
Module ExternalDirectives
	Sub Main()
	    #ExternalSource(“/home/test.aspx”,30)
                #ExternalSource(“/home/test.aspx”,30)
                      Console.WriteLine(“In test.aspx”)
               #End ExternalSource
	   #End ExternalSource

	   #ExternalSource("/home/test.aspx")
	   #End ExternalSource
  	   #ExternalSource(30)
           #End ExternalSource

	End Sub
End Module
