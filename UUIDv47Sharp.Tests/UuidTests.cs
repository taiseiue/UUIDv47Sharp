using System;
using UUIDv47Sharp;

public class UuidTests
{
    [Fact]
    public void Parse_D_Format_Roundtrip()
    {
        var bytes = new byte[16];
        new Random(123).NextBytes(bytes);
        var u = new Uuid(bytes);
        var str = u.ToString(); // default 'D'
        var parsed = Uuid.Parse(str);
        Assert.Equal(u, parsed);
        Assert.Equal(str, parsed.ToString());
    }

    [Fact]
    public void Parse_N_Format_Roundtrip()
    {
        var bytes = new byte[16];
        new Random(456).NextBytes(bytes);
        var u = new Uuid(bytes);
        var strN = u.ToString("N", null);
        Assert.Equal(32, strN.Length);
        Assert.True(Uuid.TryParse(strN, out var parsed));
        Assert.Equal(u, parsed);
        Assert.Equal(strN, parsed.ToString("N", null));
    }

    [Fact]
    public void TryParse_Invalid_ReturnsFalse()
    {
        Assert.False(Uuid.TryParse("", out _));
        Assert.False(Uuid.TryParse("not-a-uuid", out _));
        // wrong hyphen positions
        Assert.False(Uuid.TryParse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee".Replace('-', ':'), out _));
        // wrong length
        Assert.False(Uuid.TryParse(new string('a', 31), out _));
        Assert.False(Uuid.TryParse(new string('a', 33), out _));
    }

    [Fact]
    public void Parse_Invalid_Throws()
    {
        Assert.Throws<FormatException>(() => Uuid.Parse("abc"));
    }

    [Fact]
    public void TryFormat_BufferTooSmall_Fails()
    {
        var u = new Uuid(new byte[16]);
        Span<char> buf = stackalloc char[10];
        Assert.False(u.TryFormat(buf, out _)); // default D needs 36
        Assert.False(u.TryFormat(buf, out _, "N")); // N needs 32
    }

    [Fact]
    public void Equality_And_HashCode()
    {
        var bytes = new byte[16];
        new Random(789).NextBytes(bytes);
        var u1 = new Uuid(bytes);
        var u2 = new Uuid(bytes);
        Assert.True(u1 == u2);
        Assert.True(u1.Equals(u2));
        Assert.Equal(u1.GetHashCode(), u2.GetHashCode());
        bytes[0] ^= 0xFF;
        var u3 = new Uuid(bytes);
        Assert.NotEqual(u1, u3);
        Assert.True(u1 != u3);
    }

    [Fact]
    public void ToGuid_ToUuid_Roundtrip()
    {
        var bytes = new byte[16];
        new Random(321).NextBytes(bytes);
        var u = new Uuid(bytes);
        Guid g = u.ToGuid();
        var back = g.ToUuid();
        Assert.Equal(u, back);
    }

    [Fact]
    public void Empty_Is_Default()
    {
        Assert.Equal(default, Uuid.Empty);
        Assert.Equal(new Uuid(new byte[16]), Uuid.Empty);
    }
}
