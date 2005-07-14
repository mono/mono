using System;
using System.Collections;
using System.Windows;
using System.Windows.Serialization;

namespace Xaml.TestVocab.Console {
	public delegate void SomethingHappenedHandler();
	public class ConsoleApp : IAddChild {
		private ArrayList actions = new ArrayList();

		public event SomethingHappenedHandler SomethingHappened;
		
		public void AddText(string Text)
		{
			actions.Add(new ConsoleWriter(
						new ConsoleValueString(Text)));
		}

		public void AddChild(object Value)
		{
			if (Value is IConsoleAction)
				actions.Add(Value);
			else
				throw new Exception(Value.ToString() + " is not a console action");
		}

		public void Run()
		{
			foreach (IConsoleAction action in actions) {
				int reps = GetRepetitions((DependencyObject)action);
				for (int i = 0; i < reps; i++) {
					SomethingHappenedHandler s = SomethingHappened;
					if (s != null)
						s();
					action.Run();
				}
			}
		}


		public static readonly DependencyProperty RepetitionsProperty = DependencyProperty.RegisterAttached("Repetitions", typeof(int), typeof(ConsoleApp), new PropertyMetadata(1));
		public static void SetRepetitions(DependencyObject obj, int number)
		{
			obj.SetValue(RepetitionsProperty, number);
		}
		public static int GetRepetitions(DependencyObject obj)
		{
			return (int)obj.GetValue(RepetitionsProperty);
		}
	}
}
