using System.Collections.Generic;

namespace VibeForge.Terminal
{
    /// <summary>
    /// In-memory command history for the current terminal session.
    /// Bounded, never persisted: relaunching the app starts fresh.
    /// </summary>
    public sealed class TerminalHistory
    {
        public const int DefaultCapacity = 128;

        readonly List<string> _entries = new List<string>();
        readonly int _capacity;

        public TerminalHistory(int capacity = DefaultCapacity)
        {
            _capacity = capacity > 0 ? capacity : DefaultCapacity;
        }

        public int Count => _entries.Count;

        public IReadOnlyList<string> Entries => _entries;

        public void Add(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            _entries.Add(line);
            while (_entries.Count > _capacity)
            {
                _entries.RemoveAt(0);
            }
        }

        public void Clear()
        {
            _entries.Clear();
        }
    }
}
