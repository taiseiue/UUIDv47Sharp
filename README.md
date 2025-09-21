# UUIDv47Sharp
This is a port of [stateless-me/uuidv47](https://github.com/stateless-me/uuidv47) to the C#/.NET ecosystem. With this library, you can store UUIDv7s in a database in time-ordered sequence while simultaneously exposing UUIDs in a format similar to UUIDv4 to external APIs.

|       |        |
|-------|--------|
|English|[Japanese](./README.ja.md)|

## What's this?

This provides a deterministic and reversible conversion method between UUID formats like UUIDv7 and UUIDv4. This is achieved by XOR-masking only the timestamp portion of UUIDv7 using a reversible encrypted ShiHash-4-4 stream that is tied to the UUID's own random bits.


## Usage

```cs
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
```

## Security Tips
- Carefully safeguard the key used for modification. This key will be used when reversing the conversion to UUIDv7.
- The generated UUIDv4-like output only encrypts the timestamp portion; the random bits remain unprotected.
- For UUIDv7 generation, use a sufficiently secure algorithm and library.

## Security
If you discover any security vulnerabilities in this software, please **DO NOT** create an issue or pull request. Instead, please report it using one of the following methods:

- Submit a report to our [Security Advisory](https://github.com/taiseiue/UUIDv47Sharp/security/advisories) page on GitHub
- Contact us directly at taiseiue@wsnet.jp (you can obtain our PGP key from [OpenPGP](https://keys.openpgp.org/search?q=0D2E1F9F051058B2B360B34DA25AD3BFB865EC1E))

If an issue related to a security vulnerability is created, we will accept the report but subsequently delete the associated issue.

## Credits
This is a C# implementation of the highly efficient UUID generation library [stateless-me/uuidv47](https://github.com/stateless-me/uuidv47) developed by Stateless Limited.
We also referenced the Go language implementation [n2p5/uuid47](https://github.com/n2p5/uuid47) of uuidv47 for implementation guidance.

## License
This software is released under the [The MIT License](./LICENSE).

Copyright (c) 2025 Taisei Uemura  
Released under the [MIT license](./LICENSE)
