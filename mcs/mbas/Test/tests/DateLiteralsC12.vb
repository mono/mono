' This test case runs on .Net 1.1 as well
' Though the vb spec says the date format should be as follows
' #[Whitespace+]DateOrTime[Whitespace+]#

Module DateLiterals
    Sub Main()
        Dim d As Date
	
	d = #01/01/2004 5:05:07PM#
   End Sub
End Module

	
	
