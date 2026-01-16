# SampleAPI Solution

## 概要
このソリューションは、.NET 10を使用した4層アーキテクチャのRESTful APIプロジェクトです。クリーンアーキテクチャの原則に従い、各層が明確な責任を持つように設計されています。

## プロジェクト構成

### 1. SampleAPI (プレゼンテーション層)
- **役割**: APIエンドポイントの公開とHTTPリクエスト/レスポンスの処理
- **主要コンポーネント**:
  - `Areas/`: コントローラーの配置（機能別にグループ化）
  - `Models/`: DTOとリクエスト/レスポンスモデル
  - `Services/`: ビジネスロジックの実装
  - `Interfaces/`: サービスインターフェース
  - `Handlers/`: 認証ハンドラーとグローバル例外ハンドラー
- **セキュリティ**: Authorizationヘッダーによる認証機構
- **ドキュメント**: Swagger/OpenAPI統合

### 2. SampleAPI.ApplicationCore (アプリケーション層)
- **役割**: ビジネスロジックのインターフェース定義とアプリケーション全体の契約
- **主要コンポーネント**:
  - `Interfaces/`: サービスとリポジトリのインターフェース定義
  - `Models/`: ドメインモデルとエンティティ
  - `Configurations/`: アプリケーション設定の定義

### 3. SampleAPI.Common (共通ライブラリ層)
- **役割**: 全プロジェクトで共有される汎用的な機能の提供
- **主要コンポーネント**:
  - `Logging/`: NLogベースの共通ロギング機構
  - `Extensions/`: 拡張メソッド
  - `Helpers/`: ヘルパークラス

### 4. SampleAPI.Infrastructure (インフラストラクチャ層)
- **役割**: 外部リソースへのアクセスとデータ永続化
- **主要コンポーネント**:
  - `Data/`: データアクセス層（Dapper使用）
  - `ExternalApi/`: 外部API接続クライアント
  - `Configurations/`: インフラ設定管理
- **データアクセス**:
  - 参照: Dapper ORM
  - 更新/追加: ストアドプロシージャ
  - 接続文字列: AWS Secrets Manager

## 環境構成

プロジェクトは以下の4つの環境をサポートしています：

1. **Local**: ローカル開発環境
2. **Development**: 開発環境
3. **Pre**: プレプロダクション環境
4. **Live**: 本番環境

各環境の設定は `appsettings.{Environment}.json` で管理されます。

## 技術スタック

- **.NET**: 10
- **API Framework**: ASP.NET Core Web API
- **ORM (Read)**: Dapper
- **Database**: ストアドプロシージャ経由でのCUD操作
- **Logging**: NLog
- **Documentation**: Swagger/OpenAPI
- **Secret Management**: AWS Secrets Manager
- **CI/CD**: Jenkins

## アーキテクチャ原則

### 依存関係の方向
```
SampleAPI
    ↓
SampleAPI.ApplicationCore
    ↓
SampleAPI.Infrastructure
    ↓
SampleAPI.Common
```

### レイヤー責務

1. **プレゼンテーション層 (SampleAPI)**
   - HTTPリクエスト/レスポンス処理
   - 入力検証
   - 認証・認可
   - ルーティング

2. **アプリケーション層 (ApplicationCore)**
   - ビジネスロジックの調整
   - トランザクション管理
   - ドメインルールの実装

3. **インフラストラクチャ層 (Infrastructure)**
   - データ永続化
   - 外部サービス統合
   - キャッシング

4. **共通ライブラリ層 (Common)**
   - ロギング
   - ユーティリティ
   - 共通拡張メソッド

## セキュリティ

- **認証**: Authorization HeaderベースのJWT認証
- **認可**: ロールベースアクセス制御
- **接続文字列管理**: AWS Secrets Managerによる暗号化保管
- **例外処理**: グローバル例外ハンドラーによる一元管理

## データアクセスパターン

### 読み取り操作
```csharp
// Dapperを使用した軽量なデータ読み取り
var results = await dapperHelper.QueryAsync<Model>(sql, parameters);
```

### 書き込み操作
```csharp
// ストアドプロシージャを使用した安全な更新
await procedureHelper.ExecuteProcedureAsync("sp_UpdateData", parameters);
```

## ロギング

NLogを使用した構造化ロギング:
- アプリケーションログ
- エラーログ
- パフォーマンスログ
- 監査ログ

## Swagger統合

すべてのAPIエンドポイントは以下で確認可能:
- Local: `https://localhost:5001/swagger`
- Development: `https://dev-api.example.com/swagger`

## デプロイメント

### Jenkins Pipeline
各環境へのデプロイはJenkinsパイプラインを通じて自動化されています:

1. ビルド
2. ユニットテスト実行
3. パッケージング
4. 環境別設定の適用
5. デプロイ

### 環境変数
- `ASPNETCORE_ENVIRONMENT`: 実行環境の指定
- `AWS_REGION`: AWS Secrets Managerのリージョン
- `SECRET_NAME`: シークレット名

## 開発ガイドライン

### 新しいエンドポイントの追加

1. `SampleAPI.ApplicationCore` にインターフェースとモデルを定義
2. `SampleAPI` にサービスとコントローラーを実装
3. 必要に応じて `SampleAPI.Infrastructure` にリポジトリを追加
4. Swaggerアノテーションを追加してドキュメント化

### 命名規則

- **Controllers**: `{Feature}Controller` (例: `UserController`)
- **Services**: `{Feature}Service` (例: `UserService`)
- **Interfaces**: `I{Name}` (例: `IUserService`)
- **Models**: PascalCase (例: `UserModel`)
- **ファイル**: 1クラス = 1ファイル

## トラブルシューティング

### よくある問題

1. **DB接続エラー**
   - AWS Secrets Managerの設定を確認
   - ネットワーク接続を確認

2. **認証エラー**
   - Authorizationヘッダーの形式を確認
   - トークンの有効期限を確認

## ライセンス

内部使用のみ

## 連絡先

プロジェクトに関する質問は開発チームまでお問い合わせください。
