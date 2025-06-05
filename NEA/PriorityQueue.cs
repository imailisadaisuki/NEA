using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NEA
{
    internal class PriorityQueue<T>
    {
        private List<T> items = new List<T>();
        private List<int> priorities = new List<int>();
        public void Enqueue(T item, int priority)
        {
            items.Add(item);
            priorities.Add(priority);
        }
        public T Dequeue()
        {
            if (items.Count == 0)
                throw new InvalidOperationException("Queue is empty");
            int highestPriority = priorities.Min();
            items.RemoveAt(highestPriority);
            priorities.RemoveAt(highestPriority);
            return items[highestPriority];
        }
        public bool IsEmpty() => items.Count == 0;
        public int Count() => items.Count;
    }

}
