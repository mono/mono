namespace antlr.debug
{
	using System;
	using ArrayList	= System.Collections.ArrayList;

	public class InputBufferEventSupport
	{
		public virtual ArrayList InputBufferListeners
		{
			get
			{
				return inputBufferListeners;
			}
			
		}
		private object source;
		private ArrayList inputBufferListeners;
		private InputBufferEventArgs inputBufferEvent;
		protected internal const int CONSUME = 0;
		protected internal const int LA = 1;
		protected internal const int MARK = 2;
		protected internal const int REWIND = 3;
		
		
		public InputBufferEventSupport(object source)
		{
			inputBufferEvent = new InputBufferEventArgs();
			this.source = source;
		}
		public virtual void  addInputBufferListener(InputBufferListener l)
		{
			if (inputBufferListeners == null)
				inputBufferListeners = new ArrayList();
			inputBufferListeners.Add(l);
		}
		public virtual void  fireConsume(char c)
		{
			inputBufferEvent.setValues(InputBufferEventArgs.CONSUME, c, 0);
			fireEvents(CONSUME, inputBufferListeners);
		}
		public virtual void  fireEvent(int type, Listener l)
		{
			switch (type)
			{
				case CONSUME: 
					((InputBufferListener) l).inputBufferConsume(source, inputBufferEvent); break;
				
				case LA: 
					((InputBufferListener) l).inputBufferLA(source, inputBufferEvent); break;
				
				case MARK: 
					((InputBufferListener) l).inputBufferMark(source, inputBufferEvent); break;
				
				case REWIND: 
					((InputBufferListener) l).inputBufferRewind(source, inputBufferEvent); break;
				
				default: 
					throw new System.ArgumentException("bad type " + type + " for fireEvent()");
				
			}
		}
		public virtual void  fireEvents(int type, ArrayList listeners)
		{
			ArrayList targets = null;
			Listener l = null;
			
			lock(this)
			{
				if (listeners == null)
					return ;
				targets = (ArrayList) listeners.Clone();
			}
			
			if (targets != null)
				 for (int i = 0; i < targets.Count; i++)
				{
					l = (Listener) targets[i];
					fireEvent(type, l);
				}
		}
		public virtual void  fireLA(char c, int la)
		{
			inputBufferEvent.setValues(InputBufferEventArgs.LA, c, la);
			fireEvents(LA, inputBufferListeners);
		}
		public virtual void  fireMark(int pos)
		{
			inputBufferEvent.setValues(InputBufferEventArgs.MARK, ' ', pos);
			fireEvents(MARK, inputBufferListeners);
		}
		public virtual void  fireRewind(int pos)
		{
			inputBufferEvent.setValues(InputBufferEventArgs.REWIND, ' ', pos);
			fireEvents(REWIND, inputBufferListeners);
		}
		protected internal virtual void  refresh(ArrayList listeners)
		{
			ArrayList v;
			lock(listeners)
			{
				v = (ArrayList) listeners.Clone();
			}
			if (v != null)
				 for (int i = 0; i < v.Count; i++)
					((Listener) v[i]).refresh();
		}
		public virtual void  refreshListeners()
		{
			refresh(inputBufferListeners);
		}
		public virtual void  removeInputBufferListener(InputBufferListener l)
		{
			if (inputBufferListeners != null)
			{
				ArrayList temp_arraylist;
				object temp_object;
				temp_arraylist = inputBufferListeners;
				temp_object = l;
				temp_arraylist.Contains(temp_object);
				temp_arraylist.Remove(temp_object);
			}
		}
	}
}