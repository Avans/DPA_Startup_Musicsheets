using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Interfaces
{
    public class ViewManagerPool : IViewManagerPool
    {
        private readonly Dictionary<Type, IViewManager> _pool;

        public ViewManagerPool()
        {
            _pool = new Dictionary<Type, IViewManager>();
        }

        public T GetInstance<T>()
        {
            if (!_pool.TryGetValue(typeof(T), out var instance))
            {
                instance = (IViewManager)Activator.CreateInstance(typeof(T));
                _pool.Add(typeof(T), instance);
            }

            return (T)instance;
        }

        public IEnumerator<IViewManager> GetEnumerator()
        {
            return _pool.Select(v => v.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
