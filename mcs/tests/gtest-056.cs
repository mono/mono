//-- ex-gen-logger
//-- ex-gen-struct-pair
//-- ex-gen-logging-pairs
// 1.2 alpha

using System;

public class Log<T> {
  private const int SIZE = 5;
  private static int instanceCount = 0;
  private int count = 0;
  private T[] log = new T[SIZE];
  public Log() { instanceCount++; }
  public static int InstanceCount { get { return instanceCount; } }
  public void Add(T msg) { log[count++ % SIZE] = msg; }
  public int Count { get { return count; } }
  public T Last {
    get { // Return the last log entry, or null if nothing logged yet
      return count==0 ? default(T) : log[(count-1)%SIZE];
    }
    set { // Update the last log entry, or create one if nothing logged yet 
      if (count==0)
        log[count++] = value;
      else
        log[(count-1)%SIZE] = value;
    }
  }    
  public T[] All {
    get {
      int size = Math.Min(count, SIZE);
      T[] res = new T[size];
      for (int i=0; i<size; i++)
        res[i] = log[(count-size+i) % SIZE];
      return res;
    }
  }
}

class TestLog {
    public static void Main(String[] args) {
      Log<String> log1 = new Log<String>();
      log1.Add("Reboot");
      log1.Add("Coffee");
      Log<DateTime> log2 = new Log<DateTime>();
      log2.Add(DateTime.Now);
      log2.Add(DateTime.Now.AddHours(1));
      DateTime[] dts = log2.All;
      // Printing both logs:
      foreach (String s in log1.All) 
	Console.Write("{0}   ", s);
      Console.WriteLine();
      foreach (DateTime dt in dts) 
	Console.Write("{0}   ", dt);
      Console.WriteLine();
  }
}
