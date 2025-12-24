using System;
using UUIDv47Sharp;

public class Uuid47CodecTests
{
    private static Uuid MakeV7Like(ulong unixMillis, ReadOnlySpan<byte> random) // random len must be 10
    {
        // v7 layout: 48 bits unix ts ms, 4 bits version, 12 bits rand, 2 bits variant, 62 bits rand
        if (random.Length != 10) throw new ArgumentException();
        Span<byte> b = stackalloc byte[16];
        b[0] = (byte)(unixMillis >> 40);
        b[1] = (byte)(unixMillis >> 32);
        b[2] = (byte)(unixMillis >> 24);
        b[3] = (byte)(unixMillis >> 16);
        b[4] = (byte)(unixMillis >> 8);
        b[5] = (byte)(unixMillis);
        // bytes[6] high nibble = version 7
        b[6] = (byte)(((7 & 0xF) << 4) | (random[0] & 0x0F));
        b[7] = random[1];
        // byte8: variant 10xxxxxx
        b[8] = (byte)(0x80 | (random[2] & 0x3F));
        // remaining 7 bytes
        for (int i = 0; i < 7; i++) b[9 + i] = random[3 + i];
        return new Uuid(b);
    }

    [Fact]
    public void EncodeDecode_Roundtrip_PreservesOriginal()
    {
        var key = new Key(0x1122334455667788UL, 0x99AABBCCDDEEFF00UL);
        var rand = new byte[10];
        new Random(1).NextBytes(rand);
        ulong ts = 0x0000_1234_5678UL; // 48bit
        var v7 = MakeV7Like(ts, rand);
        var facade = Uuid47Codec.Encode(v7, key);
        // facade version should be 4
        Assert.Equal('4', facade.ToString()[14]); // position of version nibble in canonical D format (8-4-4-4-12) -> char index 14
        var decoded = Uuid47Codec.Decode(facade, key);
        Assert.Equal(v7, decoded);
        // decoded version nibble '7'
        Assert.Equal('7', decoded.ToString()[14]);
    }

    [Fact]
    public void Encode_ChangesTimestampField()
    {
        var key = new Key(1, 2);
        var rand = new byte[10];
        new Random(2).NextBytes(rand);
        ulong ts = 0x0000_00AA_BBCCUL;
        var v7 = MakeV7Like(ts, rand);
        var facade = v7.ToFacadeV4(key);
        // timestamp bytes differ (0..5)
        Span<byte> a = stackalloc byte[16];
        Span<byte> b = stackalloc byte[16];
        v7.CopyTo(a);
        facade.CopyTo(b);
        bool anyDiff = false;
        for (int i = 0; i < 6; i++) if (a[i] != b[i]) { anyDiff = true; break; }
        Assert.True(anyDiff);
    }

    [Fact]
    public void Decode_WithDifferentKey_FailsToRecover()
    {
        var k1 = new Key(10, 20);
        var k2 = new Key(30, 40);
        var rand = new byte[10];
        new Random(3).NextBytes(rand);
        ulong ts = 0x0000_0F0F_F0F0UL;
        var original = MakeV7Like(ts, rand);
        var facade = original.ToFacadeV4(k1);
        var decodedWrong = facade.FromFacadeV4(k2);
        Assert.NotEqual(original, decodedWrong);
    }

    [Fact]
    public void MultipleRandom_Roundtrips()
    {
        var key = Key.NewRandom();
        var rng = new Random(99);
        for (int i = 0; i < 100; i++)
        {
            ulong ts = (ulong)rng.NextInt64(0, 1L << 48);
            var rand = new byte[10]; rng.NextBytes(rand);
            var v7 = MakeV7Like(ts, rand);
            var facade = v7.ToFacadeV4(key);
            var back = facade.FromFacadeV4(key);
            Assert.Equal(v7, back);
        }
    }

    [Fact]
    public void EncodeDecode_WithGuidV7_Roundtrip()
    {
        var originalV7 = Guid.CreateVersion7();
        var uuid = originalV7.ToUuid();
        var key = new Key(0x0123456789ABCDEF, 0xFEDCBA9876543210);

        var masked = Uuid47Codec.Encode(uuid, key);
        var decoded = Uuid47Codec.Decode(masked, key).ToGuid();

        Assert.Equal(originalV7, decoded);
    }
}
