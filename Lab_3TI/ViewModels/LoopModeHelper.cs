using LoopVerification.Models;
using System.Collections.Generic;

namespace LoopVerification.ViewModels
{
    public static class LoopModeHelper
    {
        public static List<LoopMode> LoopModes { get; } = new List<LoopMode>
        {
            LoopMode.PrefixSum,
            LoopMode.CountGreaterThanT,
            LoopMode.PrefixMax
        };
    }
}