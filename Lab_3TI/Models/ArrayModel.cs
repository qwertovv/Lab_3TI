using System;

namespace LoopVerification.Models
{
    public class ArrayModel
    {
        public int[] Values { get; set; }
        public int Length => Values?.Length ?? 0;

        public ArrayModel(int size = 10, int minValue = -10, int maxValue = 10)
        {
            Values = new int[size];
            Random random = new Random();
            for (int i = 0; i < size; i++)
            {
                Values[i] = random.Next(minValue, maxValue + 1);
            }
        }
    }
}