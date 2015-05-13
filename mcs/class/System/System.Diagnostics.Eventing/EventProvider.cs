namespace System.Diagnostics.Eventing
{
	public class EventProvider
	{
		public virtual void Dispose()
		{

		}
		
		public virtual void Close()
		{
		}
		
		public System.Boolean IsEnabled ()
		{
			return false;
		}
		
		public System.Boolean IsEnabled(System.Byte level,System.Int64 keywords)
			                                                                                   
		{
			return false;
		}
		
		public static System.Diagnostics.Eventing.EventProvider.WriteEventErrorCode GetLastWriteEventError()
		{
			return null;
		}
		
		public System.Boolean WriteMessageEvent(System.String eventMessage,System.Byte eventLevel,System.Int64 eventKeywords)
			//QC   Intervals :(System.String) System.Diagnostics.Eventing.EventProvider.WriteMessageEvent.eventMessage Interval   0 :         null : null        
			//QC             :                                                                                         Interval   1 :          new : new         
			//QC   Intervals :(System.Byte) System.Diagnostics.Eventing.EventProvider.WriteMessageEvent.eventLevel Interval   0 :         null : null        
			//QC             :                                                                                     Interval   1 :          new : new         
			//QC   Intervals :(System.Int64) System.Diagnostics.Eventing.EventProvider.WriteMessageEvent.eventKeywords Interval   0 :         null : null        
			//QC             :                                                                                         Interval   1 :          new : new         
		{
			return true;
		}
		
		public System.Boolean WriteMessageEvent(System.String eventMessage)
			//QC   Intervals :(System.String) System.Diagnostics.Eventing.EventProvider.WriteMessageEvent.eventMessage Interval   0 :         null : null        
			//QC             :                                                                                         Interval   1 :          new : new         
		{
			return true;
		}
		
		public System.Boolean WriteEvent(System.Diagnostics.Eventing.EventDescriptor eventDescriptor,System.Object[] eventPayload)
			//QC   Intervals :(System.Diagnostics.Eventing.EventDescriptor) System.Diagnostics.Eventing.EventProvider.WriteEvent.eventDescriptor Interval   0 :         null : null        
			//QC             :                                                                                                                   Interval   1 :          new : new         
			//QC   Intervals :(System.Object[]) System.Diagnostics.Eventing.EventProvider.WriteEvent.eventPayload Interval   0 :         null : null        
			//QC             :                                                                                    Interval   1 :          new : new         
		{
			return true;
		}

		public System.Boolean WriteEvent(ref System.Diagnostics.Eventing.EventDescriptor eventDescriptor, int dataCount, IntPtr ppCall)
			//QC   Intervals :(System.Diagnostics.Eventing.EventDescriptor) System.Diagnostics.Eventing.EventProvider.WriteEvent.eventDescriptor Interval   0 :         null : null        
			//QC             :                                                                                                                   Interval   1 :          new : new         
			//QC   Intervals :(System.Object[]) System.Diagnostics.Eventing.EventProvider.WriteEvent.eventPayload Interval   0 :         null : null        
			//QC             :                                                                                    Interval   1 :          new : new         
		{
			return true;
		}
		
		public System.Boolean WriteEvent(System.Diagnostics.Eventing.EventDescriptor eventDescriptor,System.String data)
			//QC   Intervals :(System.Diagnostics.Eventing.EventDescriptor) System.Diagnostics.Eventing.EventProvider.WriteEvent.eventDescriptor Interval   0 :         null : null        
			//QC             :                                                                                                                   Interval   1 :          new : new         
			//QC   Intervals :(System.String) System.Diagnostics.Eventing.EventProvider.WriteEvent.data Interval   0 :         null : null        
			//QC             :                                                                          Interval   1 :          new : new         
		{
			return true;
		}
		
		public System.Boolean WriteTransferEvent(System.Diagnostics.Eventing.EventDescriptor eventDescriptor,System.Guid relatedActivityId,System.Object[] eventPayload)
			//QC   Intervals :(System.Diagnostics.Eventing.EventDescriptor) System.Diagnostics.Eventing.EventProvider.WriteTransferEvent.eventDescriptor Interval   0 :         null : null        
			//QC             :                                                                                                                           Interval   1 :          new : new         
			//QC   Intervals :(System.Guid) System.Diagnostics.Eventing.EventProvider.WriteTransferEvent.relatedActivityId Interval   0 :         null : null        
			//QC             :                                                                                             Interval   1 :          new : new         
			//QC   Intervals :(System.Object[]) System.Diagnostics.Eventing.EventProvider.WriteTransferEvent.eventPayload Interval   0 :         null : null        
			//QC             :                                                                                            Interval   1 :          new : new         
		{
			return true;
		}
		
		public static void SetActivityId(System.Guid id)
			//QC   Intervals :(System.Guid) System.Diagnostics.Eventing.EventProvider.SetActivityId.id Interval   0 :         null : null        
			//QC             :                                                                         Interval   1 :          new : new         
		{
		}
		
		public static System.Guid CreateActivityId()
		{
			return Guid.NewGuid ();
		}
		
		public EventProvider(System.Guid providerGuid)
		{

		}
		
		public class WriteEventErrorCode
		{

		}
		
	}
	
}