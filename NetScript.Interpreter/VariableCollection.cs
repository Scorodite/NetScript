using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetScript.Interpreter
{
    public class VariableCollection : IDictionary<string, object>
    {
        public VariableCollection Parent { get; set; }
        private Dictionary<string, VariableInfo> Content { get; }

        public VariableCollection()
        {
            Content = new();
        }

        #region IDictionary
        public object this[string key] { get => Get(key); set => Set(key, value); }

        public ICollection<string> Keys => Content.Keys;

        public ICollection<object> Values => (from v in Content.Values select v.Value).ToArray();

        public int Count => Content.Count;

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly => false;

        public void Add(string key, object value) =>
            Add(key, value, true);

        public void Add(string key, object value, bool mutable)
        {
            if (Content.TryGetValue(key, out var info) && !info.Mutable)
            {
                throw new Exception($"Tried to change constant variable {key}");
            }
            Content[key] = new(value, mutable);
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            if (!Content.TryGetValue(item.Key, out var info) || info.Mutable)
            {
                Content[item.Key] = new(item.Value);
            }
        }

        public void Clear()
        {
            foreach (string name in Content.Keys.ToArray())
            {
                if (Content[name].Mutable)
                {
                    Content.Remove(name);
                }
            }
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            return Content.TryGetValue(item.Key, out var info) && info.Value == item.Value;
        }

        public bool ContainsKey(string key)
        {
            return Content.ContainsKey(key);
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            foreach ((string name, VariableInfo info) in Content)
            {
                yield return new KeyValuePair<string, object>(name, info.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Remove(string key)
        {
            if (Content.TryGetValue(key, out var info) && info.Mutable)
            {
                Content.Remove(key);
                return true;
            }
            return false;
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            if (Content.TryGetValue(item.Key, out var info) && info.Value == item.Value && info.Mutable)
            {
                Content.Remove(item.Key);
                return true;
            }
            return false;
        }

        public bool TryGetValue(string key, out object value)
        {
            if (Content.TryGetValue(key, out var info))
            {
                value = info.Value;
                return true;
            }
            value = null;
            return false;
        }
        #endregion

        public void Set(string key, object value)
        {
            if (Content.TryGetValue(key, out var info) && info.Mutable)
            {
                Content[key] = new(value);
            }
            else if (Parent is not null)
            {
                Parent.Set(key, value);
            }
            else
            {
                throw new Exception($"Variable {key} does not exist");
            }
        }

        public object Get(string key)
        {
            if (Content.TryGetValue(key, out var info))
            {
                return info.Value;
            }
            else if (Parent is not null)
            {
                return Parent.Get(key);
            }
            else
            {
                throw new Exception($"Variable {key} does not exist");
            }
        }
    }

    public struct VariableInfo
    {
        public object Value;
        public bool Mutable;

        public VariableInfo(object val)
        {
            Value = val;
            Mutable = true;
        }

        public VariableInfo(object val, bool mutable)
        {
            Value = val;
            Mutable = mutable;
        }
    }
}
