using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Adsophic.Common.Util.DataStructures
{
    public class ThreadSafeStack<T> 
    {
        private Stack<T> _stack = new Stack<T>();
        private object _lock = new object(); 

        public void Push(T obj)
        {
            lock (_lock)
            {
                _stack.Push(obj); 
            }
        }

        public T Pop()
        {
            lock (_lock)
            {
                return _stack.Pop(); 
            }
        }

        public bool IsEmpty { get { return _stack.Count == 0; } } 
    }
}
