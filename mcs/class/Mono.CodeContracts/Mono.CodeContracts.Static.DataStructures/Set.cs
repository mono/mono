using System;
using System.Collections;
using System.Collections.Generic;

namespace Mono.CodeContracts.Static.DataStructures
{
		internal class Set<T> : IMutableSet<T>, IReadonlySet<T>, IEnumerable<T>, IEnumerable
		{
				private static int idCount;
				private HashSet<T> data;
				private int id;

				public int Count
				{
						get
						{
								return this.data.Count;
						}
				}

				public bool IsEmpty
				{
						get
						{
								return this.Count == 0;
						}
				}

				public bool IsSingleton
				{
						get
						{
								return this.Count == 1;
						}
				}

				public bool IsReadOnly
				{
						get
						{
								return false;
						}
				}

				public static Set<T> Empty
				{
						get
						{
								return new Set<T>(0);
						}
				}

				bool IReadonlySet<T>.IsEmpty
				{
						get
						{
								return this.IsEmpty;
						}
				}

				bool IReadonlySet<T>.IsSingleton
				{
						get
						{
								return this.IsSingleton;
						}
				}

				static Set()
				{
				}

				public Set()
				{
						this.data = new HashSet<T>();
						this.id = Set<T>.idCount++;
				}

				public Set(IEqualityComparer<T> comparer)
				{
						this.data = new HashSet<T>(comparer);
						this.id = Set<T>.idCount++;
				}

				public Set(int capacity)
				{
						this.data = new HashSet<T>();
						this.id = Set<T>.idCount++;
				}

				public Set(T singleton)
				{
						this.data = new HashSet<T>();
						this.Add(singleton);
						this.id = Set<T>.idCount++;
				}

				public Set(Set<T> original)
				{
						this.data = new HashSet<T>((IEnumerable<T>)original.data , original.data.Comparer);
						this.id = Set<T>.idCount++;
				}

				public Set(IEnumerable<T> original)
				{
						this.data = new HashSet<T>();
						this.AddRange(original);
						this.id = Set<T>.idCount++;
				}

				public Set(IEnumerable<T> original, IEnumerable<T> original2)
				{
						this.data = new HashSet<T>();
						this.AddRange(original);
						if (!object.ReferenceEquals((object)original, (object)original2))
								this.AddRange(original2);
						this.id = Set<T>.idCount++;
				}

				private Set(HashSet<T> set)
				{
						this.data = set;
						this.id = Set<T>.idCount++;
				}

				public static Set<T> operator |(Set<T> a, Set<T> b)
				{
						HashSet<T> set = new HashSet<T>((IEnumerable<T>)a.data);
						set.UnionWith((IEnumerable<T>)b.data);
						return new Set<T>(set);
				}

				public static Set<T> operator &(Set<T> a, Set<T> b)
				{
						if (a.Count > b.Count)
								return Set<T>.IntersectionInternal(b, a);
						else
								return Set<T>.IntersectionInternal(a, b);
				}

				public static Set<T> operator -(Set<T> a, Set<T> b)
				{
						Set<T> set = new Set<T>();
						foreach (T a1 in a)
						{
								if (!b.Contains(a1))
										set.Add(a1);
						}
						return set;
				}

				public static Set<T> operator ^(Set<T> a, Set<T> b)
				{
						Set<T> set = new Set<T>();
						foreach (T a1 in a)
						{
								if (!b.Contains(a1))
										set.Add(a1);
						}
						foreach (T a1 in b)
						{
								if (!a.Contains(a1))
										set.Add(a1);
						}
						return set;
				}

				public static bool operator <=(Set<T> a, Set<T> b)
				{
						foreach (T a1 in a)
						{
								if (!b.Contains(a1))
										return false;
						}
						return true;
				}

				public static bool operator <(Set<T> a, Set<T> b)
				{
						if (a.Count < b.Count)
								return a <= b;
						else
								return false;
				}

				public static bool operator >(Set<T> a, Set<T> b)
				{
						return b < a;
				}

				public static bool operator >=(Set<T> a, Set<T> b)
				{
						return b <= a;
				}

				private void ObjectInvariant()
				{
				}

				public bool AddQ(T a)
				{
						if (this.data.Contains(a))
								return false;
						this.data.Add(a);
						return true;
				}

				public bool Add(T a)
				{
						return this.data.Add(a);
				}

				public void AddRange(IEnumerable<T> range)
				{
						foreach (T a in range)
								this.Add(a);
				}

				public Set<U> ConvertAll<U>(Converter<T, U> converter)
				{
						Set<U> set = new Set<U>(this.Count);
						foreach (T input in this)
								set.Add(converter(input));
						return set;
				}

				public bool TrueForAll(Predicate<T> predicate)
				{
						foreach (T obj in this)
						{
								if (!predicate(obj))
										return false;
						}
						return true;
				}

				public Set<T> FindAll(Predicate<T> predicate)
				{
						return this.FindAllInternal<Set<T>>(predicate, new Set<T>());
				}

				protected R FindAllInternal<R>(Predicate<T> predicate, R result) where R : IMutableSet<T>
				{
						foreach (T element in this)
						{
								if (predicate(element))
										result.Add(element);
						}
						return result;
				}

				public bool Exists(Predicate<T> predicate)
				{
						foreach (T obj in this)
						{
								if (predicate(obj))
										return true;
						}
						return false;
				}

				public void ForEach(Action<T> action)
				{
						foreach (T obj in this)
								action(obj);
				}

				public T PickAnElement()
				{
						HashSet<T>.Enumerator enumerator = this.data.GetEnumerator();
						enumerator.MoveNext();
						return enumerator.Current;
				}

				public void Clear()
				{
						this.data.Clear();
				}

				public List<T> ToList()
				{
						List<T> list = new List<T>(this.Count);
						foreach (T obj in this)
								list.Add(obj);
						return list;
				}

				public bool Contains(T a)
				{
						return this.data.Contains(a);
				}

				public bool IsSubset(Set<T> s)
				{
						if (this.Count > s.Count)
								return false;
						else
								return this.data.IsSubsetOf((IEnumerable<T>)s.data);
				}

				public bool Remove(T a)
				{
						return this.data.Remove(a);
				}

				public IEnumerator<T> GetEnumerator()
				{
						return (IEnumerator<T>)this.data.GetEnumerator();
				}

				public Set<T> Union(Set<T> b)
				{
						if (this.Count == 0)
								return b;
						if (b.Count == 0)
								return this;
						else
								return this | b;
				}

				private static Set<T> IntersectionInternal(Set<T> a, Set<T> b)
				{
						HashSet<T> set = new HashSet<T>((IEnumerable<T>)a.data);
						set.IntersectWith((IEnumerable<T>)b.data);
						return new Set<T>(set);
				}

				public Set<T> Intersection(Set<T> b)
				{
						if (b.Count == 0)
								return b;
						if (this.Count == 0)
								return this;
						else
								return this & b;
				}

				public Set<T> Intersection(IEnumerable<T> xs)
				{
						if (this.Count == 0)
								return new Set<T>(xs);
						Set<T> set = new Set<T>();
						foreach (T a in xs)
						{
								if (this.Contains(a))
										set.Add(a);
						}
						return set;
				}

				public Set<T> Difference(IEnumerable<T> b)
				{
						return this - new Set<T>(b);
				}

				public Set<T> Difference(Set<T> s)
				{
						return this - s;
				}

				public Set<T> SymmetricDifference(IEnumerable<T> b)
				{
						return this ^ new Set<T>(b);
				}

				public override bool Equals(object obj)
				{
						if (obj == null)
								return false;
						if (object.ReferenceEquals((object)this, obj))
								return true;
						Set<T> set1 = this;
						Set<T> set2 = obj as Set<T>;
						if (object.Equals((object)set2, (object)null) || set1.Count != set2.Count)
								return false;
						foreach (T a in set1)
						{
								if (!set2.Contains(a))
										return false;
						}
						return true;
				}

				public override int GetHashCode()
				{
						int num = 0;
						foreach (T obj in this.data)
								num ^= obj.GetHashCode();
						return num;
				}

				IEnumerator IEnumerable.GetEnumerator()
				{
						return this.data.GetEnumerator();
				}

				public void CopyTo(T[] array, int index)
				{
						this.data.CopyTo(array, index);
				}

				public override string ToString()
				{
						return ((object)(this.Count.ToString() + " elements")).ToString();
				}

				IMutableSet<U> IReadonlySet<T>.ConvertAll<U>(Converter<T, U> converter)
				{
						return (IMutableSet<U>)this.ConvertAll<U>(converter);
				}

				IMutableSet<T> IReadonlySet<T>.Difference(IEnumerable<T> b)
				{
						return (IMutableSet<T>)this.Difference(b);
				}

				IMutableSet<T> IReadonlySet<T>.FindAll(Predicate<T> predicate)
				{
						return (IMutableSet<T>)this.FindAll(predicate);
				}

				void IReadonlySet<T>.ForEach(Action<T> action)
				{
						this.ForEach(action);
				}

				bool IReadonlySet<T>.Exists(Predicate<T> predicate)
				{
						return this.Exists(predicate);
				}

				IMutableSet<T> IReadonlySet<T>.Intersection(IReadonlySet<T> b)
				{
						Set<T> b1 = b as Set<T>;
						if (b1 != null)
								return (IMutableSet<T>)this.Intersection(b1);
						else
								return (IMutableSet<T>)this.Intersection(new Set<T>((IEnumerable<T>)b));
				}

				bool IReadonlySet<T>.IsSubset(IReadonlySet<T> s)
				{
						Set<T> s1 = s as Set<T>;
						if (s1 != null)
								return this.IsSubset(s1);
						else
								return this.IsSubset(new Set<T>((IEnumerable<T>)s));
				}

				T IReadonlySet<T>.PickAnElement()
				{
						return this.PickAnElement();
				}

				bool IReadonlySet<T>.TrueForAll(Predicate<T> predicate)
				{
						return this.TrueForAll(predicate);
				}

				IMutableSet<T> IReadonlySet<T>.Union(IReadonlySet<T> b)
				{
						Set<T> b1 = b as Set<T>;
						if (b1 != null)
								return (IMutableSet<T>)this.Union(b1);
						else
								return (IMutableSet<T>)this.Union(new Set<T>((IEnumerable<T>)b));
				}
		}
}