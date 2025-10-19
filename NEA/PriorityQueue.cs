using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NEA
{
    internal class PriorityQueue<T>
    // reference: https://youtu.be/XycnarZEBvQ?si=XrVtKtf5JrTj3sCE
    {
        private List<(T item, double priority)> heap = new List<(T, double)>();
        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (heap[index].priority >= heap[parent].priority)
                    break;
                (heap[index], heap[parent]) = (heap[parent], heap[index]); // swap
                index = parent;
            }
        }
        private void HeapifyDown(int index)
        {
            int size = heap.Count;
            while (true)
            {
                int left = 2 * index + 1;
                int right = 2 * index + 2;
                int smallest = index;
                if (left < size && heap[left].priority > heap[smallest].priority)
                    smallest = left;
                if (right < size && heap[right].priority < heap[smallest].priority)
                    smallest = right;
                if (smallest == index)
                    break;
                (heap[index], heap[smallest]) = (heap[smallest], heap[index]);
                index = smallest;
            }
        }
        public void Enqueue(T item, double priority)
        {
            heap.Add((item, priority));
            HeapifyUp(heap.Count - 1);
        }
        public T Dequeue()
        {
            if (IsEmpty())
                throw new InvalidOperationException("Priority queue is empty.");
            T item = heap[0].item;
            heap[0] = heap[heap.Count - 1];
            heap.RemoveAt(heap.Count - 1);
            if (!IsEmpty())
                HeapifyDown(0);
            return item;
        }
        public bool IsEmpty() => heap.Count == 0;
        public int Count() => heap.Count;
    }
}