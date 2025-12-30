namespace UUIDv47Sharp;

/// <summary>
/// Extension methods for UUIDv47.
/// </summary>
public static class Uuid47Extensions
{
    /// <summary>
    /// Encode (v7 -> facade).
    /// </summary>
    /// <param name="v7">The original v7 UUID.</param>
    /// <param name="key">The key to use for encoding.</param>
    /// <returns>The encoded facade UUID.</returns>
    public static Uuid ToFacadeV4(this Uuid v7, Key key) => Uuid47Codec.Encode(v7, key);
    /// <summary>
    /// Decode (facade -> v7).
    /// </summary>
    /// <param name="facade">The facade UUID to decode.</param>
    /// <param name="key">The key to use for decoding.</param>
    /// <returns>The decoded v7 UUID.</returns>
    public static Uuid FromFacadeV4(this Uuid facade, Key key) => Uuid47Codec.Decode(facade, key);
    /// <summary>
    /// Convert to System.Guid.
    /// </summary>
    /// <param name="uuid">The UUID to convert.</param>
    /// <returns>The converted System.Guid.</returns>
    public static Guid ToGuid(this Uuid uuid)
    {
        Span<byte> bytes = stackalloc byte[16];
        uuid.CopyTo(bytes);
        return new Guid(bytes, bigEndian: true);
    }
    /// <summary>
    /// Convert to UUID from System.Guid.
    /// </summary>
    /// <param name="guid">The GUID to convert.</param>
    /// <returns>The converted UUID.</returns>
    public static Uuid ToUuid(this Guid guid)
    {
        Span<byte> bytes = stackalloc byte[16];
        guid.TryWriteBytes(bytes, bigEndian: true, out var written);
        return new Uuid(bytes);
    }
}
