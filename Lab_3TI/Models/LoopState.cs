using System.Collections.Generic;

namespace LoopVerification.Models
{
    public class LoopState
    {
        public int Index { get; set; } // j
        public int Result { get; set; } // res
        public int Steps { get; set; }
        public bool IsRunning { get; set; }
        public int VariantValue { get; set; }
        public bool InvariantBefore { get; set; }
        public bool InvariantAfter { get; set; }
        public List<int> FoundElements { get; set; } = new List<int>();

        public LoopState()
        {
            Index = 0;
            Result = 0;
            Steps = 0;
            IsRunning = false;
            VariantValue = 0;
            InvariantBefore = false;
            InvariantAfter = false;

        }
    }
}