REM LineNo: 4
REM ExpectedError: BC30420
REM ErrorMessage: 'Sub Main' was not found in 'Event_dll'.

Imports System
              
NameSpace NSEvent
                                                                  
Class C
	                                                                                                                            
	Delegate Sub EvtHan(i as integer, y as string)
        Public Event E as EvtHan
                                                                                
        Public Sub S()
                RaiseEvent E (10, "abc")
        End Sub
End Class

End NameSpace
