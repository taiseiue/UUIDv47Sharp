#if DEBUG
using System;
using UUIDv47Sharp;

public class SipHashMaskTests
{
    private static Uuid MakeV7(ulong ts48, byte[] rand10)
    {
        if (rand10.Length != 10) throw new ArgumentException();
        Span<byte> b = stackalloc byte[16];
        b[0] = (byte)(ts48 >> 40);
        b[1] = (byte)(ts48 >> 32);
        b[2] = (byte)(ts48 >> 24);
        b[3] = (byte)(ts48 >> 16);
        b[4] = (byte)(ts48 >> 8);
        b[5] = (byte)ts48;
        b[6] = (byte)(((7 & 0xF) << 4) | (rand10[0] & 0x0F));
        b[7] = rand10[1];
        b[8] = (byte)(0x80 | (rand10[2] & 0x3F));
        for (int i = 0; i < 7; i++) b[9 + i] = rand10[3 + i];
        return new Uuid(b);
    }

    [Fact]
    public void Mask_Is_Stable_For_Fixed_Input()
    {
        var key = new Key(0x0123456789ABCDEFUL, 0xFEDCBA9876543210UL);
        byte[] rand = new byte[10];
        new Random(42).NextBytes(rand);
        ulong ts = 0x0000_1122_3344UL; // 48bit
        var v7 = MakeV7(ts, rand);
        ulong mask1 = Uuid47Codec.__Test_ComputeTimestampMask(key, v7);
        ulong mask2 = Uuid47Codec.__Test_ComputeTimestampMask(key, v7); // 再計算
        Assert.Equal(mask1, mask2);
        // 48bit 以外は 0 のはず
        Assert.Equal(0UL, mask1 & ~0xFFFFFFFFFFFFUL);
    }

    [Fact]
    public void Encode_Uses_Computed_Mask()
    {
        var key = new Key(0x0F0E0D0C0B0A0908UL, 0x0706050403020100UL);
        var rand = new byte[10]; new Random(7).NextBytes(rand);
        ulong ts = 0x0000_0AAA_BBBBUL;
        var v7 = MakeV7(ts, rand);
        ulong mask = Uuid47Codec.__Test_ComputeTimestampMask(key, v7);
        var facade = Uuid47Codec.Encode(v7, key);

        // 復号で元に戻ることは既存テストが担保、ここでは timestamp フィールドが ts ^ mask なっていることを直接検証
        Span<byte> orig = stackalloc byte[16];
        Span<byte> f = stackalloc byte[16];
        v7.CopyTo(orig); facade.CopyTo(f);
        ulong encTs = ((ulong)f[0] << 40) | ((ulong)f[1] << 32) | ((ulong)f[2] << 24) | ((ulong)f[3] << 16) | ((ulong)f[4] << 8) | f[5];
        ulong expected = ts ^ mask;
        Assert.Equal(expected, encTs);
    }

    [Fact]
    public void Changing_Random_Part_Changes_Mask()
    {
        var key = new Key(111, 222);
        ulong ts = 0x123456789ABUL; // 48bit
        byte[] r1 = new byte[10];
        byte[] r2 = new byte[10];
        var rand = new Random(100);
        rand.NextBytes(r1);
        rand.NextBytes(r2);
        var v7a = MakeV7(ts, r1);
        var v7b = MakeV7(ts, r2);
        ulong m1 = Uuid47Codec.__Test_ComputeTimestampMask(key, v7a);
        ulong m2 = Uuid47Codec.__Test_ComputeTimestampMask(key, v7b);
        Assert.NotEqual(m1, m2); // ランダム部が異なればマスクも異なる想定
    }
}
#endif
