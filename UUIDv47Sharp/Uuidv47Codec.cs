using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace UUIDv47Sharp;

/// <summary>
/// Codec for encoding/decoding UUIDv7 to/from UUIDv4-looking facades using SipHash-2-4.
/// </summary>
public static class Uuid47Codec
{
    private const ulong Mask48Bits = 0x0000FFFFFFFFFFFFUL;

    /// <summary>
    /// Encode a UUIDv7 into a UUIDv4-looking facade.
    /// </summary>
    /// <param name="v7">The original UUIDv7.</param>
    /// <param name="key">The key to use for encoding.</param>
    /// <returns>The encoded UUIDv4-looking facade.</returns>
    public static Uuid Encode(Uuid v7, Key key)
    {
        ulong ts48 = Read48BigEndian(v7);
        ulong mask48 = ComputeTimestampMask(key, v7);
        ulong enc = ts48 ^ mask48;

        return Write48BigEndian(v7, enc)
            .Pipe(u => SetVersion(u, 4))
            .Pipe(SetVariantRfc4122);
    }

    /// <summary>
    /// Decode a UUIDv4-looking facade back into the original UUIDv7.
    /// </summary>
    /// <param name="facade">The facade UUID to decode.</param>
    /// <param name="key">The key to use for decoding.</param>
    /// <returns>The decoded UUIDv7.</returns>
    public static Uuid Decode(Uuid facade, Key key)
    {
        // Encode で XOR された 48bit を復号して元の v7 を再構成する
        ulong enc = Read48BigEndian(facade);
        ulong mask48 = ComputeTimestampMask(key, facade); // facade から計算しても元と同じマスク
        ulong ts = enc ^ mask48;

        return Write48BigEndian(facade, ts)
            .Pipe(u => SetVersion(u, 7))
            .Pipe(SetVariantRfc4122);
    }

    /// <summary>
    /// Reads a 48-bit value (v48) big-endian from bytes 0..5.
    /// </summary>
    /// <param name="u">The UUID to read from.</param>
    /// <returns>The 48-bit value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Read48BigEndian(Uuid u) =>
        ((ulong)u.GetByte(0) << 40) |
        ((ulong)u.GetByte(1) << 32) |
        ((ulong)u.GetByte(2) << 24) |
        ((ulong)u.GetByte(3) << 16) |
        ((ulong)u.GetByte(4) << 8) |
        u.GetByte(5);

    /// <summary>
    /// Writes a 48-bit value (v48) big-endian into bytes 0..5 (keeping remaining bytes).
    /// </summary>
    private static Uuid Write48BigEndian(Uuid original, ulong v48) => original.Mutate(span =>
    {
        span[0] = (byte)(v48 >> 40);
        span[1] = (byte)(v48 >> 32);
        span[2] = (byte)(v48 >> 24);
        span[3] = (byte)(v48 >> 16);
        span[4] = (byte)(v48 >> 8);
        span[5] = (byte)(v48);
    });

    /// <summary>
    /// Builds the SipHash input from a UUIDv7 (or facade) by extracting relevant bytes.
    /// The input is 10 bytes: the lower nibble of byte 6, all of byte 7, the lower 6 bits of byte 8,
    /// and all of bytes 9-15.
    /// This excludes the version and variant bits to ensure stability between v7 and facade.
    /// </summary>
    /// <param name="u">The UUID to read from.</param>
    /// <param name="dest">The destination span to write the SipHash input to.</param>
    /// <exception cref="ArgumentException">Thrown when the destination span is too small.</exception>
    private static void BuildSipInputFromV7(Uuid u, Span<byte> dest)
    {
        if (dest.Length < 10) throw new ArgumentException("Destination must be at least 10 bytes.", nameof(dest));
        // version 上位 / variant 上位 2bit を除いたランダム部でマスク用ハッシュを安定化
        dest[0] = (byte)(u.GetByte(6) & 0x0F);   // 低位のみ
        dest[1] = u.GetByte(7);
        dest[2] = (byte)(u.GetByte(8) & 0x3F);   // 下位 6bit
        for (int i = 0; i < 7; i++)
            dest[3 + i] = u.GetByte(9 + i);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Uuid SetVersion(Uuid u, int version) => u.Mutate(span =>
    {
        span[6] = (byte)((span[6] & 0x0F) | ((version & 0x0F) << 4));
    });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Uuid SetVariantRfc4122(Uuid u) => u.Mutate(span =>
    {
        span[8] = (byte)((span[8] & 0x3F) | 0x80);
    });

    private static ulong ComputeTimestampMask(Key key, Uuid source)
    {
        Span<byte> sipInput = stackalloc byte[10];
        BuildSipInputFromV7(source, sipInput);
        return SipHash24(key.K0, key.K1, sipInput) & Mask48Bits;
    }

#if DEBUG

    public static ulong __Test_ComputeTimestampMask(Key key, Uuid source) => ComputeTimestampMask(key, source);
#endif

    private static TOut Pipe<TIn, TOut>(this TIn input, Func<TIn, TOut> fn) => fn(input);

    #region SipHash-2-4 Implementation
    /// <summary>
    /// Computes the SipHash-2-4 of the given data using the provided 128-bit key (k0, k1).
    /// </summary>
    /// <param name="k0">The first 64 bits of the key.</param>
    /// <param name="k1">The second 64 bits of the key.</param>
    /// <param name="data">The data to hash.</param>
    /// <returns>The 64-bit SipHash value.</returns>
    private static ulong SipHash24(ulong k0, ulong k1, ReadOnlySpan<byte> data)
    {
        ulong v0 = 0x736f6d6570736575UL ^ k0;
        ulong v1 = 0x646f72616e646f6dUL ^ k1;
        ulong v2 = 0x6c7967656e657261UL ^ k0;
        ulong v3 = 0x7465646279746573UL ^ k1;

        int len = data.Length;
        int fullBlocks = len & ~0x7; // 8バイト境界まで

        for (int offset = 0; offset < fullBlocks; offset += 8)
        {
            ulong m = BinaryPrimitives.ReadUInt64LittleEndian(data.Slice(offset, 8));
            v3 ^= m;
            SipRound(ref v0, ref v1, ref v2, ref v3);
            SipRound(ref v0, ref v1, ref v2, ref v3);
            v0 ^= m;
        }

        ulong b = (ulong)len << 56;
        int leftover = len - fullBlocks;
        if (leftover != 0)
        {
            ReadOnlySpan<byte> tail = data.Slice(fullBlocks);
            for (int i = 0; i < leftover; i++)
                b |= (ulong)tail[i] << (8 * i);
        }

        v3 ^= b; SipRound(ref v0, ref v1, ref v2, ref v3); SipRound(ref v0, ref v1, ref v2, ref v3); v0 ^= b;
        v2 ^= 0xff;
        SipRound(ref v0, ref v1, ref v2, ref v3);
        SipRound(ref v0, ref v1, ref v2, ref v3);
        SipRound(ref v0, ref v1, ref v2, ref v3);
        SipRound(ref v0, ref v1, ref v2, ref v3);
        return v0 ^ v1 ^ v2 ^ v3;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void SipRound(ref ulong v0, ref ulong v1, ref ulong v2, ref ulong v3)
        {
            v0 += v1; v1 = RotL(v1, 13); v1 ^= v0; v0 = RotL(v0, 32);
            v2 += v3; v3 = RotL(v3, 16); v3 ^= v2;
            v0 += v3; v3 = RotL(v3, 21); v3 ^= v0;
            v2 += v1; v1 = RotL(v1, 17); v1 ^= v2; v2 = RotL(v2, 32);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ulong RotL(ulong x, int b) => (x << b) | (x >> (64 - b));
    }
    #endregion
}
