# SampleAPI Solution (.slnx)

## プロジェクト概要

SampleAPIは、.NET 10を使用したクリーンアーキテクチャベースのWebAPIソリューションです。**Visual Studio 2026の新しい.slnx形式**を採用し、レイヤー分離、依存性注入、セキュリティ、ロギング、データアクセスなどのベストプラクティスを実装しています。

## ソリューション形式について

このプロジェクトは**.slnx形式**（XML-based Solution Format）を使用しています。

### .slnx形式の利点

* ✅ **XMLベース** - 人間が読みやすく編集しやすい
* ✅ **シンプルな構造** - 従来の.slnより簡潔
* ✅ **Gitフレンドリー** - マージコンフリクトが少ない
* ✅ **将来性** - Microsoftが推奨する新形式

### 要件

* **Visual Studio 2022 17.10以降** または **Visual Studio 2026**
* 古いバージョンのVisual Studioでは開けません

## アーキテクチャ

このソリューションは4つのプロジェクトで構成されています:

```
SampleAPI.Solution/
├── SampleAPI.slnx                      # XMLベースのソリューションファイル
├── SampleAPI/                          # Web API プロジェクト
├── SampleAPI.ApplicationCore/          # ビジネスロジック層
├── SampleAPI.Common/                   # 共通ユーティリティ
└── SampleAPI.Infrastructure/           # データアクセス層
```

### プロジェクト詳細

#### 1. SampleAPI (Web API層)

APIエンドポイントとHTTPリクエストハンドリングを担当します。

**構造:**

```
SampleAPI/
├── Areas/
│   └── V1/
│       └── Controllers/          # APIコントローラー (バージョン別)
├── Handlers/
│   ├── AuthenticationHandler.cs # APIキー認証ハンドラー
│   └── GlobalExceptionHandler.cs # グローバル例外処理
├── Interfaces/                   # サービスインターフェース定義
├── Models/                       # リクエスト/レスポンスモデル
├── Services/                     # ビジネスロジック実装
├── Properties/
│   └── launchSettings.json       # 起動設定
├── appsettings.json              # 共通設定
├── appsettings.{Environment}.json # 環境別設定
├── nlog.config                   # NLogロギング設定
└── Program.cs                    # アプリケーションエントリーポイント
```

**主要機能:**

* RESTful APIエンドポイント
* Swagger/OpenAPI ドキュメンテーション
* APIキーベース認証 (Authorizationヘッダー)
* グローバル例外ハンドリング
* 環境別設定管理 (Local/Development/Pre/Live)
* 統合ロギング (NLog)

**エンドポイント例:**

* `GET /api/v1/users` - 全ユーザー取得
* `GET /api/v1/users/{id}` - ユーザー詳細取得
* `POST /api/v1/users` - ユーザー作成
* `PUT /api/v1/users/{id}` - ユーザー更新
* `DELETE /api/v1/users/{id}` - ユーザー削除

#### 2. SampleAPI.ApplicationCore (アプリケーションコア層)

ドメインモデル、インターフェース、設定クラスを定義します。

**構造:**

```
SampleAPI.ApplicationCore/
├── Interfaces/
│   ├── IUserService.cs           # ビジネスロジックインターフェース
│   └── IUserRepository.cs        # データリポジトリインターフェース
├── Models/
│   └── User.cs                   # ドメインモデル
└── Configurations/
    └── AppSettings.cs            # アプリケーション設定クラス
```

#### 3. SampleAPI.Common (共通ライブラリ層)

全プロジェクトで共有する共通機能を提供します。

**構造:**

```
SampleAPI.Common/
└── Logging/
    ├── ILoggerService.cs         # ロギングインターフェース
    └── LoggerService.cs          # NLogロギング実装
```

#### 4. SampleAPI.Infrastructure (インフラストラクチャ層)

データアクセス、外部API接続、AWS統合を担当します。

**構造:**

```
SampleAPI.Infrastructure/
├── Data/
│   ├── DapperRepository.cs           # Dapper SELECT操作用
│   ├── StoredProcedureExecutor.cs    # ストアドプロシージャ実行用
│   └── UserRepository.cs             # ユーザーデータリポジトリ
├── ExternalApi/
│   └── ExternalApiClient.cs          # 外部API接続クライアント
└── Configurations/
    └── SecretsManagerService.cs      # AWS Secrets Manager連携
```

## セットアップ手順

### 前提条件

* **.NET 10 SDK**
* **Visual Studio 2022 17.10以降** または **Visual Studio 2026**
* SQL Server (2019以降推奨)
* AWS CLI (本番環境のみ)

### 1. ソリューションを開く

Visual Studio 2026で `SampleAPI.slnx` を開きます。

```bash
# または コマンドラインから
cd SampleAPI.Solution
code .  # VS Codeの場合
```

### 2. データベースセットアップ

```sql
-- データベース作成
CREATE DATABASE SampleDB_Local;
GO

USE SampleDB_Local;
GO

-- テーブル作成
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL
);
GO

-- ストアドプロシージャ作成
CREATE PROCEDURE sp_InsertUser
    @Username NVARCHAR(50),
    @Email NVARCHAR(100),
    @NewId INT OUTPUT
AS
BEGIN
    INSERT INTO Users (Username, Email, CreatedAt)
    VALUES (@Username, @Email, GETUTCDATE());
    
    SET @NewId = SCOPE_IDENTITY();
END
GO

CREATE PROCEDURE sp_UpdateUser
    @Id INT,
    @Username NVARCHAR(50),
    @Email NVARCHAR(100)
AS
BEGIN
    UPDATE Users
    SET Username = @Username,
        Email = @Email,
        UpdatedAt = GETUTCDATE()
    WHERE Id = @Id;
END
GO

CREATE PROCEDURE sp_DeleteUser
    @Id INT
AS
BEGIN
    DELETE FROM Users WHERE Id = @Id;
END
GO
```

### 3. 設定ファイル編集

`appsettings.Local.json` を編集してデータベース接続文字列を設定:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SampleDB_Local;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  },
  "Api": {
    "Authentication": {
      "ValidApiKeys": [
        "local-test-key-12345"
      ]
    }
  }
}
```

### 4. 依存関係の復元とビルド

```bash
dotnet restore
dotnet build
```

### 5. 実行

Visual Studioで `Local` プロファイルを選択して実行、またはコマンドラインから:

```bash
cd SampleAPI
dotnet run --environment Local
```

### 6. Swaggerで動作確認

ブラウザで以下にアクセス:

```
https://localhost:7001
```

## APIの使用方法

### 認証

全エンドポイントでAuthorizationヘッダーが必要です:

```
Authorization: Bearer local-test-key-12345
```

### Swaggerでのテスト

1. https://localhost:7001 にアクセス
2. 「Authorize」ボタンをクリック
3. `Bearer local-test-key-12345` を入力
4. エンドポイントをテスト

### curlでのテスト

```bash
# ユーザー一覧取得
curl -X GET "https://localhost:7001/api/v1/users" \
     -H "Authorization: Bearer local-test-key-12345"

# ユーザー作成
curl -X POST "https://localhost:7001/api/v1/users" \
     -H "Authorization: Bearer local-test-key-12345" \
     -H "Content-Type: application/json" \
     -d '{
       "username": "testuser",
       "email": "test@example.com"
     }'
```

## ソリューション名の変更手順

このテンプレートを別のプロジェクト名で使用する場合の手順です。

### 例: `SampleAPI` → `NXJpki` に変更する場合

#### 1. ソリューションファイルの変更

```bash
mv SampleAPI.slnx NXJpki.slnx
```

#### 2. プロジェクトのディレクトリ名を変更

```bash
mv SampleAPI NXJpki
mv SampleAPI.ApplicationCore NXJpki.ApplicationCore
mv SampleAPI.Common NXJpki.Common
mv SampleAPI.Infrastructure NXJpki.Infrastructure
```

#### 3. 各プロジェクトファイル（.csproj）の名前を変更

```bash
mv NXJpki/SampleAPI.csproj NXJpki/NXJpki.csproj
mv NXJpki.ApplicationCore/SampleAPI.ApplicationCore.csproj NXJpki.ApplicationCore/NXJpki.ApplicationCore.csproj
mv NXJpki.Common/SampleAPI.Common.csproj NXJpki.Common/NXJpki.Common.csproj
mv NXJpki.Infrastructure/SampleAPI.Infrastructure.csproj NXJpki.Infrastructure/NXJpki.Infrastructure.csproj
```

#### 4. .slnxファイル内のプロジェクト参照を更新

`NXJpki.slnx` を開いて、以下のように変更:

**変更前:**
```xml
<Project Path="SampleAPI\SampleAPI.csproj" />
<Project Path="SampleAPI.ApplicationCore\SampleAPI.ApplicationCore.csproj" />
<Project Path="SampleAPI.Common\SampleAPI.Common.csproj" />
<Project Path="SampleAPI.Infrastructure\SampleAPI.Infrastructure.csproj" />
```

**変更後:**
```xml
<Project Path="NXJpki\NXJpki.csproj" />
<Project Path="NXJpki.ApplicationCore\NXJpki.ApplicationCore.csproj" />
<Project Path="NXJpki.Common\NXJpki.Common.csproj" />
<Project Path="NXJpki.Infrastructure\NXJpki.Infrastructure.csproj" />
```

#### 5. 各.csprojファイル内のプロジェクト参照を更新

**NXJpki/NXJpki.csproj:**
```xml
<ItemGroup>
  <ProjectReference Include="..\NXJpki.ApplicationCore\NXJpki.ApplicationCore.csproj" />
  <ProjectReference Include="..\NXJpki.Common\NXJpki.Common.csproj" />
  <ProjectReference Include="..\NXJpki.Infrastructure\NXJpki.Infrastructure.csproj" />
</ItemGroup>
```

**NXJpki.Infrastructure/NXJpki.Infrastructure.csproj:**
```xml
<ItemGroup>
  <ProjectReference Include="..\NXJpki.ApplicationCore\NXJpki.ApplicationCore.csproj" />
  <ProjectReference Include="..\NXJpki.Common\NXJpki.Common.csproj" />
</ItemGroup>
```

#### 6. 名前空間（Namespace）の一括置換

全ファイルで名前空間を検索・置換:

```bash
# Linuxの場合
find . -type f -name "*.cs" -exec sed -i 's/namespace SampleAPI/namespace NXJpki/g' {} +
find . -type f -name "*.cs" -exec sed -i 's/using SampleAPI/using NXJpki/g' {} +

# Windowsの場合（PowerShell）
Get-ChildItem -Recurse -Filter *.cs | ForEach-Object {
    (Get-Content $_.FullName) -replace 'namespace SampleAPI', 'namespace NXJpki' | Set-Content $_.FullName
    (Get-Content $_.FullName) -replace 'using SampleAPI', 'using NXJpki' | Set-Content $_.FullName
}
```

または、Visual Studioの「フォルダーを指定して置換」機能を使用:
- `Ctrl + Shift + H`
- 検索: `SampleAPI`
- 置換: `NXJpki`
- 対象: `*.cs`

#### 7. appsettings.jsonファイルの確認

必要に応じて、以下のファイルのプロジェクト固有の設定を更新:
- `appsettings.json`
- `appsettings.Local.json`
- `appsettings.Development.json`
- `appsettings.Pre.json`
- `appsettings.Live.json`

#### 8. nlog.configの確認

`nlog.config` のログファイルパスを確認:

```xml
<target name="file" xsi:type="File" fileName="logs/nxjpki-${shortdate}.log" />
```

#### 9. ビルドとテスト

```bash
dotnet restore
dotnet build
dotnet run --project NXJpki --environment Local
```

#### 10. データベースオブジェクトの更新（必要に応じて）

ストアドプロシージャやテーブル名に `SampleAPI` プレフィックスがある場合は更新してください。

### チェックリスト

変更後、以下を確認してください:

- [ ] ソリューションファイル名が変更されている
- [ ] 4つのプロジェクトディレクトリ名が変更されている
- [ ] 4つの.csprojファイル名が変更されている
- [ ] .slnx内のプロジェクトパスが更新されている
- [ ] 各.csprojのProjectReference参照が更新されている
- [ ] すべての.csファイルの名前空間が更新されている
- [ ] appsettings系ファイルの設定が確認されている
- [ ] nlog.configのログファイル名が確認されている
- [ ] `dotnet build` が成功する
- [ ] アプリケーションが正常に起動する

## 環境管理

4つの環境をサポート:

| 環境 | 用途 | Secrets Manager | Swagger |
| --- | --- | --- | --- |
| **Local** | ローカル開発 | 使用しない | 有効 |
| **Development** | 開発環境 | 使用する | 有効 |
| **Pre** | ステージング | 使用する | 無効 |
| **Live** | 本番環境 | 使用する | 無効 |

## AWS Secrets Manager設定 (本番環境)

```bash
aws secretsmanager create-secret \
    --name sampleapi/live/database/connectionstring \
    --description "SampleAPI Live Database Connection String" \
    --secret-string '{"connectionString":"Server=xxx;Database=xxx;User Id=xxx;Password=xxx;"}' \
    --region ap-northeast-1
```

## プロジェクトの特徴

✅ **.slnx形式** - 最新のXMLベースソリューション  
✅ **クリーンアーキテクチャ** - レイヤー分離設計  
✅ **Dapper + ストアドプロシージャ** - 高速データアクセス  
✅ **APIキー認証** - セキュアなエンドポイント  
✅ **Swagger統合** - 自動APIドキュメント  
✅ **NLogロギング** - 統合ロギング  
✅ **環境別設定** - 4環境対応  
✅ **AWS Secrets Manager** - セキュアな接続文字列管理

## ライセンス

このプロジェクトはサンプルコードです。自由に使用・修正できます。

## 変更履歴

### v1.0.0 (2026-01-15)

* 初回リリース (.slnx形式)
* 基本的なCRUD API実装
* 認証/認可機能
* ロギング統合
* AWS Secrets Manager統合
* 環境別設定管理
