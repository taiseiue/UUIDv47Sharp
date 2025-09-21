using System.Runtime.CompilerServices;

namespace UUIDv47Sharp;

/// <summary>
/// Represents a 128-bit UUID (Universally Unique Identifier).
/// </summary>
public readonly struct Uuid : IEquatable<Uuid>, ISpanFormattable
{
    private readonly byte _b00, _b01, _b02, _b03, _b04, _b05, _b06, _b07;
    private readonly byte _b08, _b09, _b10, _b11, _b12, _b13, _b14, _b15;

    /// <summary>
    /// An empty UUID (all bits zero).
    /// </summary>
    public static readonly Uuid Empty = default;

    /// <summary>
    /// Creates a new UUID from a 16-byte array.
    /// </summary>
    /// <param name="bytes">The 16-byte array representing the UUID.</param>
    /// <exception cref="ArgumentException">Thrown when the byte array is not 16 bytes long.</exception>
    public Uuid(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length != 16) throw new ArgumentException("UUID must be 16 bytes.", nameof(bytes));
        _b00 = bytes[0]; _b01 = bytes[1]; _b02 = bytes[2]; _b03 = bytes[3];
        _b04 = bytes[4]; _b05 = bytes[5]; _b06 = bytes[6]; _b07 = bytes[7];
        _b08 = bytes[8]; _b09 = bytes[9]; _b10 = bytes[10]; _b11 = bytes[11];
        _b12 = bytes[12]; _b13 = bytes[13]; _b14 = bytes[14]; _b15 = bytes[15];
    }

    /// <summary>
    /// Copies the UUID bytes into the provided destination span.
    /// </summary>
    /// <param name="destination">The destination span to copy the UUID bytes into.</param>
    /// <exception cref="ArgumentException">Thrown when the destination span is not large enough.</exception>
    public void CopyTo(Span<byte> destination)
    {
        if (destination.Length < 16) throw new ArgumentException("Destination too small.", nameof(destination));
        destination[0] = _b00; destination[1] = _b01; destination[2] = _b02; destination[3] = _b03;
        destination[4] = _b04; destination[5] = _b05; destination[6] = _b06; destination[7] = _b07;
        destination[8] = _b08; destination[9] = _b09; destination[10] = _b10; destination[11] = _b11;
        destination[12] = _b12; destination[13] = _b13; destination[14] = _b14; destination[15] = _b15;
    }

    /// <summary>
    /// Returns a new byte array containing the UUID bytes.
    /// </summary>
    /// <returns>A new byte array containing the UUID bytes.</returns>
    public byte[] ToByteArray()
    {
        var arr = new byte[16];
        CopyTo(arr);
        return arr;
    }

    /// <summary>
    /// Tries to parse a UUID from a string in either hyphenated (36 chars) or compact (32 chars) format.
    /// </summary>
    /// <param name="text">The input string to parse.</param>
    /// <param name="uuid">The parsed UUID.</param>
    /// <returns>True if the parse was successful, false otherwise.</returns>
    public static bool TryParse(string? text, out Uuid uuid)
        => text is null ? (uuid = default, false).Item2 : TryParse(text.AsSpan(), out uuid);

    /// <summary>
    /// Tries to parse a UUID from a ReadOnlySpan<char> in either hyphenated (36 chars) or compact (32 chars) format.
    /// </summary>
    /// <param name="text">The input span to parse.</param>
    /// <param name="uuid">The parsed UUID.</param>
    /// <returns>True if the parse was successful, false otherwise.</returns>
    public static bool TryParse(ReadOnlySpan<char> text, out Uuid uuid)
    {
        if (text.Length == 36)
        {
            // Hyphen positions
            if (text[8] != '-' || text[13] != '-' || text[18] != '-' || text[23] != '-')
            { uuid = default; return false; }
            Span<char> hexSpan = stackalloc char[32];
            text.Slice(0, 8).CopyTo(hexSpan[..8]);
            text.Slice(9, 4).CopyTo(hexSpan.Slice(8, 4));
            text.Slice(14, 4).CopyTo(hexSpan.Slice(12, 4));
            text.Slice(19, 4).CopyTo(hexSpan.Slice(16, 4));
            text.Slice(24, 12).CopyTo(hexSpan.Slice(20, 12));
            return DecodeHex32(hexSpan, out uuid);
        }
        if (text.Length == 32)
        {
            return DecodeHex32(text, out uuid);
        }
        uuid = default;
        return false;

        static bool DecodeHex32(ReadOnlySpan<char> hex, out Uuid value)
        {
            Span<byte> raw = stackalloc byte[16];
            for (int i = 0; i < 16; i++)
            {
                int hi = FromHex(hex[i * 2]);
                int lo = FromHex(hex[i * 2 + 1]);
                if ((hi | lo) < 0)
                { value = default; return false; }
                raw[i] = (byte)((hi << 4) | lo);
            }
            value = new Uuid(raw); return true;
        }
        static int FromHex(char c)
        {
            if ((uint)(c - '0') <= 9) return c - '0';
            c = (char)(c | 0x20); // to lower
            if ((uint)(c - 'a') <= 5) return c - 'a' + 10;
            return -1;
        }
    }

    /// <summary>
    /// Parses a UUID from a string in either hyphenated (36 chars) or compact (32 chars) format.
    /// </summary>
    /// <param name="text">The input string to parse.</param>
    /// <returns>The parsed UUID.</returns>
    /// <exception cref="FormatException">Thrown when the input string is not a valid UUID format.</exception>
    public static Uuid Parse(string text)
    {
        if (!TryParse(text, out var u)) throw new FormatException("Invalid UUID format.");
        return u;
    }

    /// <summary>
    /// Returns the string representation of the UUID in hyphenated format.
    /// </summary>
    /// <returns>The hyphenated string representation of the UUID.</returns>
    public override string ToString() => ToString(null, null);

    /// <summary>
    /// Formats the UUID into the provided destination span according to the specified format.
    /// </summary>
    /// <param name="destination">The span to write the formatted UUID to.</param>
    /// <param name="charsWritten">The number of characters written to the destination span.</param>
    /// <param name="format">The format specifier.</param>
    /// <param name="provider">The format provider.</param>
    /// <returns>True if the formatting was successful, false otherwise.</returns>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null)
    {
        char f = (format.Length == 0) ? 'D' : (format.Length == 1 ? format[0] : '\0');
        var self = this;
        if (f == 'D' || f == 'd')
        {
            if (destination.Length < 36) { charsWritten = 0; return false; }
            WriteHyphenated(destination);
            charsWritten = 36; return true;
        }
        else if (f == 'N' || f == 'n')
        {
            if (destination.Length < 32) { charsWritten = 0; return false; }
            WriteCompact(destination);
            charsWritten = 32; return true;
        }
        charsWritten = 0; return false;

        void WriteHyphenated(Span<char> dest)
        {
            Span<byte> bytesLocal = stackalloc byte[16];
            self.CopyTo(bytesLocal);
            const string hex = "0123456789abcdef";
            int j = 0;
            for (int i = 0; i < 16; i++)
            {
                if (i == 4 || i == 6 || i == 8 || i == 10)
                    dest[j++] = '-';
                dest[j++] = hex[(bytesLocal[i] >> 4) & 0xF];
                dest[j++] = hex[bytesLocal[i] & 0xF];
            }
        }
        void WriteCompact(Span<char> dest)
        {
            Span<byte> bytesLocal = stackalloc byte[16];
            self.CopyTo(bytesLocal);
            const string hex = "0123456789abcdef";
            int j = 0;
            for (int i = 0; i < 16; i++)
            {
                dest[j++] = hex[(bytesLocal[i] >> 4) & 0xF];
                dest[j++] = hex[bytesLocal[i] & 0xF];
            }
        }
    }

    /// <summary>
    /// Returns the string representation of the UUID according to the specified format and format provider.
    /// </summary>
    /// <param name="format">The format specifier.</param>
    /// <param name="formatProvider">The format provider.</param>
    /// <returns>The string representation of the UUID.</returns>
    /// <exception cref="FormatException">Thrown when the format is invalid.</exception>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        Span<char> buffer = stackalloc char[36];
        if (!TryFormat(buffer, out int written, format, formatProvider))
            throw new FormatException("Unsupported format specifier.");
        return new string(buffer[..written]);
    }
    /// <summary>
    /// Determines whether the specified UUID is equal to the current UUID.
    /// </summary>
    /// <param name="other">The UUID to compare with the current UUID.</param>
    /// <returns>True if the specified UUID is equal to the current UUID; otherwise, false.</returns>
    public bool Equals(Uuid other)
    {
        return _b00 == other._b00 && _b01 == other._b01 && _b02 == other._b02 && _b03 == other._b03 &&
               _b04 == other._b04 && _b05 == other._b05 && _b06 == other._b06 && _b07 == other._b07 &&
               _b08 == other._b08 && _b09 == other._b09 && _b10 == other._b10 && _b11 == other._b11 &&
               _b12 == other._b12 && _b13 == other._b13 && _b14 == other._b14 && _b15 == other._b15;
    }
    /// <summary>
    /// Determines whether the specified object is equal to the current UUID.
    /// </summary>
    /// <param name="obj">The object to compare with the current UUID.</param>
    /// <returns>True if the specified object is equal to the current UUID; otherwise, false.</returns>
    public override bool Equals(object? obj) => obj is Uuid u && Equals(u);
    /// <summary>
    /// Returns a hash code for the UUID.
    /// </summary>
    /// <returns>A hash code for the UUID.</returns>
    public override int GetHashCode()
    {
        // Fold into two ulongs for a stable hash
        Span<byte> bytes = stackalloc byte[16];
        CopyTo(bytes);
        ulong p0 = Unsafe.ReadUnaligned<ulong>(ref bytes[0]);
        ulong p1 = Unsafe.ReadUnaligned<ulong>(ref bytes[8]);
        return HashCode.Combine(p0, p1);
    }

    public static bool operator ==(Uuid left, Uuid right) => left.Equals(right);
    public static bool operator !=(Uuid left, Uuid right) => !left.Equals(right);

    internal byte GetByte(int index) => index switch
    {
        0 => _b00,
        1 => _b01,
        2 => _b02,
        3 => _b03,
        4 => _b04,
        5 => _b05,
        6 => _b06,
        7 => _b07,
        8 => _b08,
        9 => _b09,
        10 => _b10,
        11 => _b11,
        12 => _b12,
        13 => _b13,
        14 => _b14,
        15 => _b15,
        _ => throw new ArgumentOutOfRangeException(nameof(index))
    };

    internal Uuid WithByte(int index, byte value)
    {
        Span<byte> tmp = stackalloc byte[16];
        CopyTo(tmp);
        tmp[index] = value;
        return new Uuid(tmp);
    }

    internal Uuid WithFirstBytes(ReadOnlySpan<byte> src, int count)
    {
        Span<byte> tmp = stackalloc byte[16];
        CopyTo(tmp);
        src.Slice(0, count).CopyTo(tmp);
        return new Uuid(tmp);
    }

    internal Uuid Mutate(Action<Span<byte>> mutator)
    {
        Span<byte> tmp = stackalloc byte[16];
        CopyTo(tmp);
        mutator(tmp);
        return new Uuid(tmp);
    }
}
