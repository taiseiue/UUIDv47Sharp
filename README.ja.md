# UUIDv47Sharp
このライブラリは、時刻情報を含みソート可能なUUIDv7と、その時刻情報を暗号化してランダムに見えるUUID(facade)とを相互に変換する機能を提供します。

これにより、データベース内部では効率的なUUIDv7を使いつつ、外部APIでは生成時刻などの情報が推測できないIDを公開でき、プライバシーとパフォーマンスを両立させます。

このライブラリは、[stateless-me/uuidv47](https://github.com/stateless-me/uuidv47)をC#/.NETエコシステムに移植したものです。

|       |        |
|-------|--------|
|[English](./README.md)|Japanese|

## What's this?
このライブラリは、UUIDv7と、UUIDv4のようなランダムな外観を持つID(facade)との間で、決定的(deterministic)かつ逆変換が可能(reversible)な変換方法を提供します。

この仕組みは、UUIDv7のタイムスタンプ部分のみを対象にXORマスクを適用することで、時刻情報を隠蔽するものです。このXORマスクは、UUID自身のランダムビット部分に紐付けられた、可逆暗号であるSipHash-2-4ストリームを用いて生成されます。このアプローチにより、変換後のIDは時刻情報が秘匿されつつも、元のUUIDv7と1対1で対応するため、いつでも相互に変換することが可能です。

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

## Principles
- **ランダムビットの保持:** UUIDv7が持つ74ビットのランダム部分は、変換後も完全に保持され、変更されません。
- **タイムスタンプのマスキング:** 48ビットのタイムスタンプ部分は、SipHash-2-4から生成されるキーストリームとのXOR演算によってマスク（暗号化）されます。
- **キーの導出:** マスキングに使用するキーは、UUID自身のランダムビット部分から決定論的に導出されます。
- **RFC準拠:** 変換前後のIDはどちらも、UUIDの仕様に準拠するため、適切なバージョンビットとバリアントビットを維持します。

上記のアプローチにより、この変換は以下の優れた特徴を持ちます。

- **決定論的 (Deterministic):** 同じUUIDv7を入力すれば、常に全く同じfacadeが出力されます。
- **可逆性 (Reversible):** facadeは、秘密鍵を用いることで、元のUUIDv7へと逆変換できます。
- **安全性 (Secure):** SipHash-2-4アルゴリズムにより、facadeから秘密鍵や元のタイムスタンプを推測することに対する暗号学的な安全性が提供されます。

## Security Tips
- 変更に使用したKeyは厳重に管理してください。このKeyはUUIDv7へと逆変換する時に使用します。
- 生成されたUUIDv4風の結果はタイムスタンプのみを暗号化しており、ランダムビット部分は保護していません。
- UUIDv7の生成には、十分に安全性を備えたアルゴリズムおよびライブラリを使用してください。

## Security
このソフトウェアでセキュリティ上の脆弱性を発見された場合でも、IssueやPull Requestを作成**しないで**ください。 代わりに、以下のいずれかの方法でご報告ください。

- GitHubの[Security Advisory](https://github.com/taiseiue/UUIDv47Sharp/security/advisories)に投稿する
- taiseiue@wsnet.jp 宛に直接連絡する（PGP鍵は[OpenPGP](https://keys.openpgp.org/search?q=0D2E1F9F051058B2B360B34DA25AD3BFB865EC1E)から入手できます）

セキュリティ脆弱性に関するIssueが作成いただいた場合、その内容は把握しますが、該当のIssueは削除します。

## Credits
これは、Stateless Limited社が開発した優れたUUID生成ライブラリである[stateless-me/uuidv47](https://github.com/stateless-me/uuidv47)のC#による実装です。
また、uuidv47のGo言語版実装である[n2p5/uuid47](https://github.com/n2p5/uuid47)の実装を参考にしました。

## License

このソフトウェアは、[The MIT License](./LICENSE)のもとで公開します。

Copyright (c) 2025 Taisei Uemura  
Released under the [MIT license](./LICENSE)
