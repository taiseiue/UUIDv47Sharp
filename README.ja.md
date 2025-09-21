# UUIDv47Sharp
[stateless-me/uuidv47](https://github.com/stateless-me/uuidv47)をC#/.NETエコシステムに移植したものです。このライブラリを使用すると、データベース内で時系列順に並べられたUUIDv7を保存すると同時に、外部APIに対してはUUIDv4のような形式のUUIDを公開できます。

|       |        |
|-------|--------|
|[English](./README.md)|Japanese|

## What's this?

UUIDv47Sharpは、UUIDv7とUUIDv4のような形式のUUID間の、決定論的かつ逆変換が可能な変換方法を提供します。これは、UUIDv7のタイムスタンプ部分のみを、UUID自身のランダムビットに紐付けられた可逆暗号化されたShiHash-4-4ストリームを用いてXORマスクすることで実現しています。

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
- 変更に使用したKeyは厳重に管理してください。このKeyはUUIDv7へと逆変換する時に使用します。
- 生成されたUUIDv4風の結果はタイムスタンプのみを暗号化しており、ランダムビット部分は保護していません。
- UUIDv7の生成には、十分に安全性を備えたアルゴリズムおよびライブラリを使用してください。

## Security
このソフトウェアでセキュリティ上の脆弱性を発見された場合は、IssueやPull Requestを作成**しないで**ください。 代わりに、以下のいずれかの方法でご報告ください。

- GitHubの[Security Advisory](https://github.com/taiseiue/UUIDv47Sharp/security/advisories)に投稿する
- taiseiue@wsnet.jp 宛に直接連絡する（PGP鍵は[OpenPGP](https://keys.openpgp.org/search?q=0D2E1F9F051058B2B360B34DA25AD3BFB865EC1E)から入手できます）

セキュリティ脆弱性に関するIssueが作成された場合、その内容は受理しますが、該当のIssueは削除します。

## Credits
これは、Stateless Limited社が開発した優れたUUID生成ライブラリである[stateless-me/uuidv47](https://github.com/stateless-me/uuidv47)のC#版実装です。
また、uuidv47のGo言語版実装である[n2p5/uuid47](https://github.com/n2p5/uuid47)の実装を参考にしました。

## License

このソフトウェアは、[The MIT License](./LICENSE)のもとで公開します。

Copyright (c) 2025 Taisei Uemura  
Released under the [MIT license](./LICENSE)
