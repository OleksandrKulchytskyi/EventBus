using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;

namespace EventBus.Extensions
{
	internal class DynamicDictionary : DynamicObject, IDictionary<string, object>
	{
		private readonly IDictionary<string, object> _container;

		public DynamicDictionary(IDictionary<string, object> obj)
		{
			_container = obj;
		}

		public object this[string key]
		{
			get
			{
				object result;
				_container.TryGetValue(key, out result);
				return Wrap(result);
			}
			set
			{
				_container[key] = Unwrap(value);
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "The compiler generates calls to invoke this")]
		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			result = this[binder.Name];
			return true;
		}

		[SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "The compiler generates calls to invoke this")]
		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			this[binder.Name] = value;
			return true;
		}

		public static object Wrap(object value)
		{
			var obj = value as IDictionary<string, object>;
			if (obj != null)
			{
				return new DynamicDictionary(obj);
			}

			return value;
		}

		public static object Unwrap(object value)
		{
			var dictWrapper = value as DynamicDictionary;
			if (dictWrapper != null)
			{
				return dictWrapper._container;
			}

			return value;
		}

		public void Add(string key, object value)
		{
			_container.Add(key, value);
		}

		public bool ContainsKey(string key)
		{
			return _container.ContainsKey(key);
		}

		public ICollection<string> Keys
		{
			get { return _container.Keys; }
		}

		public bool Remove(string key)
		{
			return _container.Remove(key);
		}

		public bool TryGetValue(string key, out object value)
		{
			return _container.TryGetValue(key, out value);
		}

		public ICollection<object> Values
		{
			get { return _container.Values; }
		}

		public void Add(KeyValuePair<string, object> item)
		{
			_container.Add(item);
		}

		public void Clear()
		{
			_container.Clear();
		}

		public bool Contains(KeyValuePair<string, object> item)
		{
			return _container.Contains(item);
		}

		public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			_container.CopyTo(array, arrayIndex);
		}

		public int Count
		{
			get { return _container.Count; }
		}

		public bool IsReadOnly
		{
			get { return _container.IsReadOnly; }
		}

		public bool Remove(KeyValuePair<string, object> item)
		{
			return _container.Remove(item);
		}

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return _container.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}