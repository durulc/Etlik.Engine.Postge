using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RTLS.Manager
{
    public class FixedSizedQueue<T>
    {
        private object LOCK = new object();
        ConcurrentQueue<T> queue;

        public int MaxSize { get; set; }

        public FixedSizedQueue(int maxSize)
        {
            this.MaxSize = maxSize;            
        }

        public T[] SizedQueue(IEnumerable<T> items = null)
        {           
            if (items == null)
            {
                queue = new ConcurrentQueue<T>();
            }
            else
            {
                queue = new ConcurrentQueue<T>(items);
                EnsureLimitConstraint();
            }
            return queue.ToArray();
        }

        public void Enqueue(T obj)
        {
            queue.Enqueue(obj);
            EnsureLimitConstraint();
        }

        private void EnsureLimitConstraint()
        {
            if (queue.Count > MaxSize)
            {
                lock (LOCK)
                {
                    T overflow;
                    while (queue.Count > MaxSize)
                    {
                        queue.TryDequeue(out overflow);
                    }
                }
            }
        }

        public T[] GetSnapshot()
        {
            return queue.ToArray();
        }
    }
}
