using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsCommon
{
    public static class TimeFn
    {
        public static int ExponentialTimeoutFn(int numErrors, int poolInterval, int maxMinutes = 5)
        {
            int MAX_BACKOFF = Math.Min(numErrors, 10); // 17 minutes max
            var exponentialTimeoutMs = (int)(Math.Pow(2, MAX_BACKOFF) - 1) * 1000;
            return poolInterval + exponentialTimeoutMs;
        }
    }
}
