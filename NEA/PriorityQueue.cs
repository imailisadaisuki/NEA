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
        private List<double> priorities = new List<double>();
        public void Enqueue(T item, double priority)
        {
            items.Add(item);
            priorities.Add(priority);
        }
        public T Dequeue()
        {
            if (items.Count == 0)
                throw new InvalidOperationException("Queue is empty");
            int highestPriorityLocation = priorities.IndexOf(priorities.Min());
            T temp = items[highestPriorityLocation];
            items.RemoveAt(highestPriorityLocation);
            priorities.RemoveAt(highestPriorityLocation);
            return temp;
        }
        public bool IsEmpty() => items.Count == 0;
        public int Count() => items.Count;
    }
}