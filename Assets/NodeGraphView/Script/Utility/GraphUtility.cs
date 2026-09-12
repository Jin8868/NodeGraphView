using System;

namespace NodeGraphView
{
    public partial class GraphUtility
    {
        private static readonly DateTime Epoch = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private const int SequenceBits = 12;
        private const long SequenceMask = (1L << SequenceBits) - 1;
        private static readonly object IdLock = new object();
        private static long LastTimestamp = -1;
        private static long Sequence;

        public static long GenerateID()
        {
            long timestamp = GetCurrentTimestamp();

            if (timestamp < 0)
            {
                throw new InvalidOperationException("Current time cannot be earlier than 2025-01-01 00:00:00 UTC.");
            }

            lock (IdLock)
            {
                if (timestamp < LastTimestamp)
                {
                    timestamp = LastTimestamp;
                }

                if (timestamp == LastTimestamp)
                {
                    Sequence = (Sequence + 1) & SequenceMask;
                    if (Sequence == 0)
                    {
                        timestamp = WaitNextTimestamp(LastTimestamp);
                    }
                }
                else
                {
                    Sequence = 0;
                }

                LastTimestamp = timestamp;
                return (timestamp << SequenceBits) | Sequence;
            }
        }

        private static long GetCurrentTimestamp()
        {
            return (long)(DateTime.UtcNow - Epoch).TotalMilliseconds;
        }

        private static long WaitNextTimestamp(long lastTimestamp)
        {
            long timestamp = GetCurrentTimestamp();
            while (timestamp <= lastTimestamp)
            {
                timestamp = GetCurrentTimestamp();
            }

            return timestamp;
        }
    }
}
