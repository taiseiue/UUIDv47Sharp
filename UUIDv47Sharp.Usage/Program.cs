using UUIDv47Sharp;

var key = new Key(0x0123456789abcdef, 0xfedcba9876543210);
// Or generate a random key:
// var key = Key.NewRandom();

// Parse a UUIDv7
// (e.g., from your database)
var v7 = Uuid.Parse("018f2d9f-9a2a-7def-8c3f-7b1a2c4d5e6f");

// Encode to facade (v4-like) for external use
var facade = Uuid47Codec.Encode(v7, key);
Console.WriteLine($"ExternalID: {facade}");
// Output: External ID: 2463c780-7fca-4def-8c3f-7b1a2c4d5e6f

// Decode back to original v7 for internal use
var decoded = Uuid47Codec.Decode(facade, key);
Console.WriteLine($"InternalID: {decoded}");
// Output: Internal ID: 018f2d9f-9a2a-7def-8c3f-7b1a2c4d5e6f
