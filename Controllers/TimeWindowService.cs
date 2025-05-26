namespace ServiceHub.Controllers
{
    public class TimeWindowService
    {
        private readonly List<TimeSpan> _runTimes;
        private readonly List<TimeSpan> _transferTimes;

        public TimeWindowService(IConfiguration configuration)
        {
            _runTimes = configuration.GetValue<string>("RunTimes")
            .Split(',')
            .Select(TimeSpan.Parse)
            .ToList();

            _transferTimes = configuration.GetValue<string>("TransferRunTimes")
                .Split(',')
                .Select(TimeSpan.Parse)
                .ToList();
        }

        public bool IsTransferWindowOpen()
        {
            var now = DateTime.Now.TimeOfDay;

            // Find the next runtime after each transfer time
            foreach (var transferTime in _transferTimes)
            {
                var nextRuntime = _runTimes.FirstOrDefault(r => r > transferTime);
                var windowEnd = nextRuntime != default ? nextRuntime : _runTimes.First().Add(TimeSpan.FromDays(1));

                if (now >= transferTime && now < windowEnd)
                {
                    return true;
                }
            }

            return false;
        }

        public string GetTransferWindowMessage()
        {
            var windows = new List<string>();

            foreach (var transferTime in _transferTimes)
            {
                var nextRuntime = _runTimes.FirstOrDefault(r => r > transferTime);
                var windowEnd = nextRuntime != default ? nextRuntime : _runTimes.First().Add(TimeSpan.FromDays(1));

                windows.Add($"{transferTime:hh\\:mm} to {windowEnd:hh\\:mm}");
            }

            return $"Transfer allowed during: {string.Join(", ", windows)}";
        }

        public TimeSpan? GetNextWindowChange()
        {
            var now = DateTime.Now.TimeOfDay;

            // If currently in a transfer window, return time until it ends
            foreach (var transferTime in _transferTimes)
            {
                var nextRuntime = _runTimes.FirstOrDefault(r => r > transferTime);
                var windowEnd = nextRuntime != default ? nextRuntime : _runTimes.First().Add(TimeSpan.FromDays(1));

                if (now >= transferTime && now < windowEnd)
                {
                    return windowEnd - now;
                }
            }

            // Otherwise find when next transfer window starts
            var nextTransfer = _transferTimes.FirstOrDefault(t => t > now);
            if (nextTransfer != default)
            {
                return nextTransfer - now;
            }

            // If no more today, return time until first transfer tomorrow
            if (_transferTimes.Any())
            {
                return _transferTimes.First().Add(TimeSpan.FromDays(1)) - now;
            }

            return null;
        }
        public List<TimeSpan> GetRunTimes() => _runTimes;
        public List<TimeSpan> GetTransferTimes() => _transferTimes;

    }
}
