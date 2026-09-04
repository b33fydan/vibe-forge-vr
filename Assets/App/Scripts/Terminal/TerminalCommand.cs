using System;

namespace VibeForge.Terminal
{
    /// <summary>
    /// A parsed terminal input line. v0 splits on whitespace with no quoting;
    /// the command name is matched case-insensitively, arguments keep casing.
    /// </summary>
    public readonly struct TerminalCommand : IEquatable<TerminalCommand>
    {
        public static readonly TerminalCommand Empty = new TerminalCommand(null, null, null);

        public TerminalCommand(string raw, string name, string[] arguments)
        {
            Raw = raw;
            Name = name;
            Arguments = arguments ?? Array.Empty<string>();
        }

        public string Raw { get; }
        public string Name { get; }
        public string[] Arguments { get; }

        public bool IsEmpty => string.IsNullOrEmpty(Name);

        public bool Equals(TerminalCommand other)
        {
            return Raw == other.Raw && Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return obj is TerminalCommand other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Raw != null ? Raw.GetHashCode() : 0) * 397) ^
                       (Name != null ? Name.GetHashCode() : 0);
            }
        }
    }
}
