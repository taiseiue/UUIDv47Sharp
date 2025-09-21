using System.Security.Cryptography;

namespace UUIDv47Sharp;

/// <summary>
/// Represents a 128-bit SipHash key.
/// </summary>
public readonly struct Key
{
    public ulong K0 { get; }
    public ulong K1 { get; }

    /// <summary>
    /// Creates a new Key from two 64-bit unsigned integers.
    /// </summary>
    /// <param name="k0">The first 64-bit word of the key.</param>
    /// <param name="k1">The second 64-bit word of the key.</param>
    public Key(ulong k0, ulong k1)
    {
        K0 = k0;
        K1 = k1;
    }

    /// <summary>
    /// Generates a new random Key using a cryptographically secure random number generator.
    /// </summary>
    /// <returns>A new random Key.</returns>
    public static Key NewRandom()
    {
        Span<byte> buf = stackalloc byte[16];
        RandomNumberGenerator.Fill(buf);
        ulong k0 = BitConverter.ToUInt64(buf[..8]);
        ulong k1 = BitConverter.ToUInt64(buf[8..]);
        return new Key(k0, k1);
    }
}