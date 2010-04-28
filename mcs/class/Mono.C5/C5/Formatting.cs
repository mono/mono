/*
 Copyright (c) 2003-2006 Niels Kokholm and Peter Sestoft
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
*/

using C5;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Text;

namespace C5
{
  /// <summary>
  /// <i>(Describe usage of "L:300" format string.)</i>
  /// </summary>
  public interface IShowable : IFormattable
  {
    //TODO: wonder if we should use TextWriters instead of StringBuilders?
    /// <summary>
    /// Format <code>this</code> using at most approximately <code>rest</code> chars and 
    /// append the result, possibly truncated, to stringbuilder.
    /// Subtract the actual number of used chars from <code>rest</code>.
    /// </summary>
    /// <param name="stringbuilder"></param>
    /// <param name="rest"></param>
    /// <param name="formatProvider"></param>
    /// <returns>True if the appended formatted string was complete (not truncated).</returns>
    bool Show(StringBuilder stringbuilder, ref int rest, IFormatProvider formatProvider);
  }
  // ------------------------------------------------------------

  // Static helper methods for Showing collections 

  /// <summary>
  /// 
  /// </summary>
  public static class Showing
  {
    /// <summary>
    /// Show  <code>Object obj</code> by appending it to <code>stringbuilder</code>
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="stringbuilder"></param>
    /// <param name="rest"></param>
    /// <param name="formatProvider"></param>
    /// <returns>True if <code>obj</code> was shown completely.</returns>
    public static bool Show(Object obj, StringBuilder stringbuilder, ref int rest, IFormatProvider formatProvider)
    {
      IShowable showable;
      if (rest <= 0)
        return false;
      else if ((showable = obj as IShowable) != null)
        return showable.Show(stringbuilder, ref rest, formatProvider);
      int oldLength = stringbuilder.Length;
      stringbuilder.AppendFormat(formatProvider, "{0}", obj);
      rest -= (stringbuilder.Length - oldLength);
      return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="showable"></param>
    /// <param name="format"></param>
    /// <param name="formatProvider"></param>
    /// <returns></returns>
    public static String ShowString(IShowable showable, String format, IFormatProvider formatProvider)
    {
      int rest = maxLength(format);
      StringBuilder sb = new StringBuilder();
      showable.Show(sb, ref rest, formatProvider);
      return sb.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    static int maxLength(String format)
    {
      //TODO: validate format string
      if (format == null)
        return 80;
      if (format.Length > 1 && format.StartsWith("L"))
      {
        return int.Parse(format.Substring(1));
      }
      else
        return int.MaxValue;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="stringbuilder"></param>
    /// <param name="rest"></param>
    /// <param name="formatProvider"></param>
    /// <returns>True if collection was shown completely</returns>
    public static bool ShowCollectionValue<T>(ICollectionValue<T> items, StringBuilder stringbuilder, ref int rest, IFormatProvider formatProvider)
    {
      string startdelim = "{ ", enddelim = " }";
      bool showIndexes = false;
      bool showMultiplicities = false;
      //TODO: do not test here at run time, but select code at compile time
      //      perhaps by delivering the print type to this metod
      IList<T> list;
      ICollection<T> coll = items as ICollection<T>;
      if ((list = items as IList<T>) != null)
      {
        startdelim = "[ ";
        enddelim = " ]";
        //TODO: should have been (items as IIndexed<T>).IndexingSpeed
        showIndexes = list.IndexingSpeed == Speed.Constant;
      }
      else if (coll != null)
      {
        if (coll.AllowsDuplicates)
        {
          startdelim = "{{ ";
          enddelim = " }}";
          if (coll.DuplicatesByCounting)
            showMultiplicities = true;
        }
      }

      stringbuilder.Append(startdelim);
      rest -= 2 * startdelim.Length;
      bool first = true;
      bool complete = true;
      int index = 0;

      if (showMultiplicities)
      {
        foreach (KeyValuePair<T, int> p in coll.ItemMultiplicities())
        {
          complete = false;
          if (rest <= 0)
            break;
          if (first)
            first = false;
          else
          {
            stringbuilder.Append(", ");
            rest -= 2;
          }
          if (complete = Showing.Show(p.Key, stringbuilder, ref rest, formatProvider))
          {
            string multiplicityString = string.Format("(*{0})", p.Value);
            stringbuilder.Append(multiplicityString);
            rest -= multiplicityString.Length;
          }
        }
      }
      else
      {
        foreach (T x in items)
        {
          complete = false;
          if (rest <= 0)
            break;
          if (first)
            first = false;
          else
          {
            stringbuilder.Append(", ");
            rest -= 2;
          }
          if (showIndexes)
          {
            string indexString = string.Format("{0}:", index++);
            stringbuilder.Append(indexString);
            rest -= indexString.Length;
          }
          complete = Showing.Show(x, stringbuilder, ref rest, formatProvider);
        }
      }
      if (!complete)
      {
        stringbuilder.Append("...");
        rest -= 3;
      }
      stringbuilder.Append(enddelim);
      return complete;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    /// 
    /// <param name="dictionary"></param>
    /// <param name="stringbuilder"></param>
    /// <param name="formatProvider"></param>
    /// <param name="rest"></param>
    /// <returns></returns>
    public static bool ShowDictionary<K, V>(IDictionary<K, V> dictionary, StringBuilder stringbuilder, ref int rest, IFormatProvider formatProvider)
    {
      bool sorted = dictionary is ISortedDictionary<K, V>;
      stringbuilder.Append(sorted ? "[ " : "{ ");
      rest -= 4;				   // Account for "( " and " )"
      bool first = true;
      bool complete = true;

      foreach (KeyValuePair<K, V> p in dictionary)
      {
        complete = false;
        if (rest <= 0)
          break;
        if (first)
          first = false;
        else
        {
          stringbuilder.Append(", ");
          rest -= 2;
        }
        complete = Showing.Show(p, stringbuilder, ref rest, formatProvider);
      }
      if (!complete)
      {
        stringbuilder.Append("...");
        rest -= 3;
      }
      stringbuilder.Append(sorted ? " ]" : " }");
      return complete;
    }
  }
}