# SampleAPI

SampleAPIは、レイヤードアーキテクチャ、JWT Bearer認証、DapperによるSQL Serverアクセス、ストアドプロシージャ、NLog、Swagger、AWS Secrets Manager連携を使用した .NET 10 Web API サンプルです。

このソリューションは、XMLベースの `.slnx` 形式を使用しています。

## 要件

- .NET 10 SDK
- Visual Studio 2022 17.10以降、Visual Studio 2026、Rider、または VS Code
- ローカルDB検証用の SQL Server 2019以降
- AWS Secrets ManagerからDBシークレットを読み取る非Local環境ではAWS認証情報

## ソリューション構成

```text
SampleAPI/
├── SampleAPI.slnx
├── Database/
│   └── InitializeDatabase.sql
├── SampleAPI/
│   ├── Areas/V1/Controllers/UserController.cs
│   ├── Handlers/GlobalExceptionHandler.cs
│   ├── Interfaces/IUserService.cs
│   ├── Models/
│   ├── Services/UserService.cs
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Local.json
│   ├── appsettings.Development.json
│   ├── appsettings.Pre.json
│   ├── appsettings.Live.json
│   └── nlog.config
├── SampleAPI.ApplicationCore/
│   ├── Configurations/
│   ├── Interfaces/IUserRepository.cs
│   └── Models/User.cs
├── SampleAPI.Common/
│   ├── Extensions/
│   ├── Helpers/
│   └── Logging/
└── SampleAPI.Infrastructure/
    ├── Configurations/SecretsManagerHelper.cs
    ├── Data/
    │   ├── DapperHelper.cs
    │   ├── ProcedureHelper.cs
    │   └── UserRepository.cs
    └── ExternalApi/
        ├── ExternalApiClient.cs
        └── IExternalApiClient.cs
```

## アーキテクチャ

このソリューションは4つのプロジェクトに分かれています。

- `SampleAPI`: プレゼンテーション層です。コントローラー、DTO、サービス実装、ミドルウェア登録、Swagger、JWT設定、ヘルスチェックエンドポイント、アプリケーション起動処理を含みます。
- `SampleAPI.ApplicationCore`: アプリケーションの契約とドメインモデルを定義します。
- `SampleAPI.Infrastructure`: SQL Serverアクセス、ストアドプロシージャ実行、外部HTTP APIクライアント、AWS Secrets Manager連携を担当します。
- `SampleAPI.Common`: ロギング、ヘルパー、拡張メソッドなどの共通機能を提供します。

## 設定

`appsettings.json` には共通設定を配置しています。`JwtSettings:SecretKey` は意図的に空にしており、環境別設定または環境変数から指定する必要があります。

`appsettings.Local.json` には、ローカル開発用のJWTシークレットとSQL Server接続文字列の例を配置しています。

`Development`、`Pre`、`Live` では、まずAWS Secrets ManagerからDB接続文字列を読み取ります。Secrets Managerの読み取りに失敗した場合は、設定ファイル上の接続文字列にフォールバックします。

環境変数名は、ASP.NET Coreの設定規約に合わせて次の形式を推奨します。

```bash
JwtSettings__SecretKey="replace-with-secure-secret"
ConnectionStrings__DefaultConnection="Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;"
```

## ローカルDBセットアップ

まずローカルDBを作成します。

```sql
CREATE DATABASE SampleDB_Local;
GO

USE SampleDB_Local;
GO
```

その後、次のスクリプトを実行します。

```text
Database/InitializeDatabase.sql
```

このスクリプトは以下を作成します。

- `Users` テーブル
- `sp_CreateUser`
- `sp_UpdateUser`
- `sp_DeleteUser`
- サンプルユーザー

## ビルド

リポジトリルートから実行します。

```bash
dotnet restore SampleAPI.slnx
dotnet build SampleAPI.slnx
```

## ローカル実行

起動プロファイルを使用する場合:

```bash
dotnet run --project SampleAPI/SampleAPI.csproj --launch-profile http
```

明示的に指定する場合:

```bash
ASPNETCORE_ENVIRONMENT=Local dotnet run --project SampleAPI/SampleAPI.csproj --urls http://127.0.0.1:5000
```

Swaggerは `Local` と `Development` 環境で有効です。

```text
http://localhost:5000/swagger
```

## ヘルスチェック

ヘルスチェックエンドポイントは匿名アクセス可能で、JWT認証は不要です。

```bash
curl http://localhost:5000/health
curl http://localhost:5000/api/v1/health
```

レスポンス例:

```json
{
  "status": "Healthy",
  "timestamp": "2026-05-03T00:00:00Z"
}
```

## APIエンドポイント

ユーザー関連エンドポイントでは、有効なJWT Bearerトークンが必要です。

```text
Authorization: Bearer {jwt-token}
```

利用可能なエンドポイント:

- `GET /api/v1/user`
- `GET /api/v1/user/{id}`
- `POST /api/v1/user`
- `PUT /api/v1/user/{id}`
- `DELETE /api/v1/user/{id}`

ユーザー作成リクエスト:

```json
{
  "username": "testuser",
  "email": "test@example.com",
  "fullName": "Test User",
  "phoneNumber": "090-1234-5678",
  "password": "Password123!"
}
```

パスワードは、リポジトリへ渡される前に ASP.NET Core の `IPasswordHasher<User>` でハッシュ化されます。

## 認証

このアプリケーションは、`Program.cs` で設定された ASP.NET Core のJWT Bearer認証を使用します。

カスタムのサンプル認証ハンドラーはありません。呼び出し元は、`JwtSettings:SecretKey` で署名され、次の値と一致する実際のJWTを指定する必要があります。

- `JwtSettings:Issuer`
- `JwtSettings:Audience`

SwaggerにはBearerセキュリティ定義が設定されているため、有効なJWTがあればSwagger UIから認証付きAPIをテストできます。

## データアクセス

読み取り処理は、`DapperHelper` 経由のDapper SQLクエリを使用します。

書き込み処理は、`ProcedureHelper` 経由でストアドプロシージャを使用します。

- 作成: `sp_CreateUser`
- 更新: `sp_UpdateUser`
- 削除: `sp_DeleteUser`

DB接続文字列は、DI構築時にブロックせず、クエリまたはプロシージャ実行時に非同期で解決されます。

## 外部APIクライアント

`ExternalApiClient` はtyped HTTP clientとして登録されています。

```csharp
builder.Services.AddHttpClient<IExternalApiClient, ExternalApiClient>();
```

これにより、`HttpClient` を手動生成せず、ASP.NET Coreに基盤となるハンドラー管理を任せられます。

## CI/CD

`Jenkinsfile` は `.slnx` ソリューションファイルを使用します。

```bash
dotnet restore SampleAPI.slnx
dotnet build SampleAPI.slnx --configuration Release --no-restore
dotnet test SampleAPI.slnx --configuration Release --no-build
```

パイプラインのスモークテストでは、次のエンドポイントを呼び出します。

- `/health`
- `/api/v1/health`

## 環境

| 環境 | 用途 | Swagger | DBシークレット取得元 |
| --- | --- | --- | --- |
| Local | ローカル開発 | 有効 | `appsettings.Local.json` |
| Development | 共有開発環境 | 有効 | AWS Secrets Manager、その後フォールバック設定 |
| Pre | ステージング | 無効 | AWS Secrets Manager、その後フォールバック設定 |
| Live | 本番 | 無効 | AWS Secrets Manager、その後フォールバック設定 |

## プロジェクトの特徴

- .NET 10
- `.slnx` ソリューション形式
- レイヤードアーキテクチャ
- JWT Bearer認証
- Swagger/OpenAPI
- グローバル例外ハンドリング
- NLogロギング
- Dapperによる読み取り
- ストアドプロシージャによる書き込み
- AWS Secrets Manager対応
- typed `HttpClientFactory` 外部APIクライアント
- デプロイ確認用の匿名ヘルスチェックエンドポイント

## 現在の注意点

- 現時点ではテストプロジェクトは含まれていません。
- APIはJWTを検証しますが、ログインまたはトークン発行エンドポイントはまだ提供していません。
- 本番用のJWTシークレットは、ソース管理対象外で設定してください。
