public class App {
  public static void Main() {
    FP.appendArrays(new int[] {1, 2}, new int[] {3, 4});
  }
}

class FP {
    public static T[] appendArrays<T>(params T[][] arrays) {
      int length = 0;
      foreach (T[] array in arrays)
        length += array.Length;
      T[] result = new T[length];
      int k = 0;
      foreach (T[] array in arrays)
        foreach (T obj in array) {
          result[k] = obj;
          k++;
        }
      return result;
    }
}
