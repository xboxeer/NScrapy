# NScrapy
![buildpass](https://img.shields.io/badge/build-pass-blue.svg) ![license](https://img.shields.io/badge/License-Apache2.0-yellowgreen.svg) ![netversion](https://img.shields.io/badge/.netcore-2.0-lightgrey.svg) ![release](https://img.shields.io/badge/release-v0.1-blue.svg)

## 简介 / Introduction

NScrapy 是基于 .NET Core 异步编程框架和 Redis 内存存储的开源分布式爬虫框架。NScrapy 的整体思想源于知名的 Python 爬虫框架 Scrapy，整体写法也接近于 Scrapy，支持分布式部署、丰富的中间件扩展以及流式（Fluent）API。

NScrapy is a distributed spider framework based on .NET Core async programming and Redis. Inspired by the famous Python framework Scrapy, NScrapy offers a similar development experience with support for distributed deployment, middleware extensions, and a Fluent API.

---

## CLI 工具 / CLI Tool

### 安装 / Installation

```bash
git clone https://github.com/xboxeer/NScrapy.git
cd NScrapy
dotnet build ./NScrapy.Cli/NScrapy.Cli.csproj
dotnet tool install --global --add-path ./NScrapy.Cli/bin/Debug/net10.0/publish
```

### nscrapy new — 创建爬虫项目

创建新的爬虫项目模板：

```bash
nscrapy new <SpiderName> [options]
```

| 参数/选项 | 说明 |
|---|---|
| `<SpiderName>` | 爬虫名称（PascalCase，必填） |
| `-t, --type <TYPE>` | 模板类型：`basic`（默认）或 `distributed` |
| `-o, --output <PATH>` | 输出目录（默认 `.`） |
| `--force` | 覆盖已有文件 |

**示例：**

```bash
# 创建基础爬虫
nscrapy new MySpider

# 在指定目录创建
nscrapy new MySpider -o ./spiders

# 创建分布式爬虫
nscrapy new MyDistributedSpider -t distributed

# 覆盖已存在的项目
nscrapy new MySpider --force
```

### nscrapy run — 运行爬虫

运行爬虫程序：

```bash
nscrapy run [spider-name] [options]
```

| 选项 | 说明 |
|---|---|
| `--role <ROLE>` | 运行角色：`single`（默认）、`spider`、`downloader` |
| `--distributed` | 启用分布式模式 |
| `--redis <HOST:PORT>` | Redis 连接地址 |
| `--redis-password <PWD>` | Redis 密码 |
| `--redis-ssl` | 使用 SSL 连接 Redis |
| `--receiver-queue <NAME>` | 请求队列名称 |
| `--response-queue <NAME>` | 响应队列名称 |
| `--concurrency <N>` | 并发请求数 |
| `--delay <MS>` | 请求间隔（毫秒） |
| `-c, --config <PATH>` | 配置文件路径 |

**示例：**

```bash
# 本地单节点运行
nscrapy run MySpider

# 分布式模式运行
nscrapy run MySpider --role spider --distributed --redis localhost:6379

# 带 Redis 认证和 SSL
nscrapy run MySpider --redis localhost:6380 --redis-password secret --redis-ssl

# 调整并发和请求间隔
nscrapy run MySpider --concurrency 10 --delay 100

# 运行下载器节点
nscrapy run --role downloader --redis localhost:6379

# 使用自定义配置
nscrapy run MySpider -c ./config.json
```

### 配置优先级

配置按以下优先级生效（高 → 低）：

1. **CLI 参数** — 命令行直接指定
2. **环境变量** — `NSCRAPY_*` 前缀（如 `NSCRAPY_REDIS_HOST`）
3. **配置文件** — JSON 配置文件

### 项目结构

`nscrapy new` 生成的结构：

```
MySpider/
├── Spiders/
│   └── MySpiderSpider.cs    # 爬虫逻辑
├── Items/
│   └── MySpiderItem.cs      # 数据模型
├── Pipelines/
│   └── MySpiderPipeline.cs  # 数据处理管道
├── Program.cs               # 入口
├── MySpider.csproj
└── appsettings.json         # 配置
```

---

## 快速开始 / Quick Start

### 通过 CLI 创建爬虫（推荐）

```bash
nscrapy new MySpider
cd MySpider
nscrapy run MySpider
```

### 通过 NuGet 集成

```bash
dotnet add package NScrapy.Infra
dotnet add package NScrapy.Scheduler
```

使用 Fluent API 编写爬虫：

```csharp
NScrapy.CreateSpider("MySpider")
    .StartUrl("https://example.com")
    .OnResponse(r => {
        Console.WriteLine(r.CssSelector("title::text").Extract());
    })
    .AddPipeline<MyPipeline>()
    .Configure(o => o.Concurrency = 5)
    .Run();
```

> 💡 更多 API 用法参见下方「编程指南」章节。

---

## Docker 部署 / Docker Deployment

### 简介 / Overview

NScrapy 支持通过 Docker Compose 进行容器化部署，提供三种模式：

NScrapy supports containerized deployment via Docker Compose with three modes:

| 模式 / Mode | 适用场景 / Use Case | Redis |
|---|---|---|
| **local-only** | 单节点，无需 Redis / Single-node, no Redis | 无 / None (InMemoryScheduler) |
| **local-redis** | 同主机多工作节点 / Multi-worker on same host | 本地 Redis 容器 / Local Redis container |
| **managed-redis** | 云端/生产部署 / Cloud / production deploy | 外部 Redis / External Redis |

### 前置条件 / Prerequisites

- Docker & Docker Compose v2+
- .NET 10.0（用于本地开发构建 / for local development/builds）

### 快速启动 / Quick Start

**1. 克隆项目 / Clone the project**

```bash
git clone https://github.com/your-org/NScrapy.git
cd NScrapy
```

**2. 创建 `.env` 配置文件 / Create a `.env` config file**

```bash
# 托管 Redis 模式示例（如 Azure Redis Cache）
# Example for managed-redis mode (Azure Redis Cache)
cat > .env << 'EOF'
REDIS_ENABLED=true
REDIS_HOST=your-redis.redis.cache.windows.net
REDIS_PORT=6380
REDIS_PASSWORD=your-access-key-here
REDIS_USESSL=true
EOF
```

**3. 选择模式并启动 / Choose your mode and start**

```bash
# 模式 1：本地仅限（无 Redis）
# Mode 1: Local only (no Redis)
docker-compose up -d

# 模式 2：本地 + 本地 Redis 容器
# Mode 2: Local + local Redis container
docker-compose --profile local-redis up -d

# 模式 3：托管 Redis（需先取消 .env 中 Redis 主机注释）
# Mode 3: Managed Redis (uncomment Redis host in .env first)
docker-compose up -d
```

**4. 扩缩容 Spider 和 Downloader / Scale Spider and Downloader workers**

```bash
# 运行 3 个 Spider 实例和 4 个 Downloader 工作进程
# Run 3 spider instances and 4 downloader workers
docker-compose up -d --scale spider=3 --scale downloader=4
```

### 配置说明 / Configuration

所有设置通过环境变量控制。JSON 配置文件（`appsettings.spider.json`、`appsettings.downloader.json`）作为默认值，运行时会由环境变量覆盖。

All settings are controlled via environment variables. The JSON config files serve as defaults; environment variables override them at runtime.

| 变量 / Variable | 说明 / Description | 默认值 / Default |
|---|---|---|
| `REDIS_ENABLED` | 启用 Redis 调度器 / Enable Redis scheduler | `false` |
| `REDIS_HOST` | Redis 主机名 / Redis hostname | `redis` |
| `REDIS_PORT` | Redis 端口 / Redis port | `6379` |
| `REDIS_PASSWORD` | Redis 密码 / Redis password | _(empty)_ |
| `REDIS_USESSL` | 使用 TLS/SSL | `false` |
| `SCHEDULER_TYPE` | 调度器类名 / Scheduler class name | `InMemoryScheduler` |

### 项目结构 / Project Structure

```
NScrapy/
├── NScrapy.Cli/                # CLI 工具（nscrapy new / nscrapy run）
├── NScrapy.Core/               # 核心抽象层
├── NScrapy.Engine/             # 爬虫引擎
├── NScrapy.Infra/              # 基础设施（HTTP、解析器等）
├── NScrapy.Scheduler/          # 调度器（InMemory / Redis）
├── NScrapy.Spider/             # Spider 抽象
├── NScrapy.Downloader/         # Downloader 抽象
├── docker-compose.yml          # Docker 部署配置（3 种模式）
├── Dockerfile.Spider           # Spider 镜像构建
├── Dockerfile.Downloader        # Downloader 镜像构建
├── appsettings.spider.json     # Spider 默认配置
└── appsettings.downloader.json # Downloader 默认配置
```

### 手动构建镜像 / Building Images Manually

```bash
# 构建 Spider 镜像
docker build -f Dockerfile.Spider -t nscrapy-spider .

# 构建 Downloader 镜像
docker build -f Dockerfile.Downloader -t nscrapy-downloader .

# 运行 spider 节点
docker run nscrapy-spider run MySpider --role spider --distributed

# 运行 downloader 节点
docker run nscrapy-downloader run --role downloader
```

### 架构 / Architecture

```
                    ┌─────────────────┐
                    │  Spider(s)      │  ← 调度请求 / Schedules requests
                    │  (Crawler logic) │
                    └────────┬────────┘
                             │ Redis queues
                    ┌─────────▼────────┐
                    │  Downloader(s)  │  ← HTTP 工作器 / HTTP workers
                    │  (Workers)       │
                    └─────────────────┘
```

---

## 编程指南 / Programming Guide

### Fluent API（推荐方式 / Recommended Way）

NScrapy 提供流式 API，让爬虫编写更加简洁直观。通过 `NScrapy.CreateSpider()` 可以链式调用配置爬虫行为，支持本地运行和分布式运行两种模式。

NScrapy provides a Fluent API for writing spiders in a concise and intuitive chain-call style. Use `NScrapy.CreateSpider()` to configure and run spiders, supporting both local and distributed modes.

#### 本地爬虫 / Local Spider

```csharp
// Local spider / 本地爬虫
NScrapy.CreateSpider("MySpider")
    .StartUrl("https://example.com")
    .OnResponse(r => {
        Console.WriteLine(r.CssSelector("title::text").Extract());
    })
    .AddPipeline<MyPipeline>()
    .Configure(o => o.Concurrency = 5)
    .Run();
```

#### 分布式爬虫 / Distributed Spider

```csharp
// Distributed spider / 分布式爬虫
NScrapy.CreateSpider("MySpider")
    .StartUrl("https://example.com")
    .OnResponse(r => { /* parse / 解析 */ })
    .Distributed(d => d
        .UseRedis("localhost:6379")
        .ReceiverQueue("nscrapy:requests")
        .ResponseQueue("nscrapy:responses")
    )
    .Run();
```

#### SpiderOptions 配置项 / Configuration Options

| 选项 / Option | 类型 / Type | 说明 / Description |
|---|---|---|
| `Concurrency` | `int` | 并发请求数（默认 1）/ Concurrent requests, default 1 |
| `DelayMs` | `int` | 请求间隔毫秒（默认 0）/ Delay between requests (ms), default 0 |
| `MaxRetries` | `int` | 最大重试次数（默认 3）/ Max retry count, default 3 |
| `TimeoutMs` | `int` | 请求超时毫秒（默认 30000）/ Request timeout (ms), default 30000 |
| `UserAgents` | `List<string>` | User-Agent 列表，随机选用 / User-Agent list, randomly selected |

```csharp
NScrapy.CreateSpider("MySpider")
    .StartUrl("https://example.com")
    .OnResponse(r => { /* parse / 解析 */ })
    .Configure(o => {
        o.Concurrency = 5;
        o.DelayMs = 1000;
        o.MaxRetries = 3;
        o.TimeoutMs = 15000;
        o.UserAgents = new List<string> {
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36"
        };
    })
    .Run();
```

#### JsRenderMiddleware（JS 渲染中间件）

启用 JavaScript 渲染支持（用于抓取 SPA 页面的内容）：

Enable JavaScript rendering support (for crawling SPA pages):

```csharp
NScrapy.CreateSpider("MySpider")
    .StartUrl("https://example.com")
    .OnResponse(r => { /* parse / 解析 */ })
    .AddDownloaderMiddleware<JsRenderMiddleware>()
    .Run();
```

> ⚠️ 注意：`JsRenderMiddleware` 目前为存根实现，功能开发中。
> ⚠️ Note: `JsRenderMiddleware` is currently a stub implementation; full functionality is under development.

---

## 分布式配置 / Distributed Configuration

### Redis 配置 / Redis Configuration

#### Spider 端 / Spider Side

修改项目中的 `appsettings.json`：

Modify `appsettings.json` in your project:

```json
{
  "Scheduler": {
    "SchedulerType": "NScrapy.Scheduler.RedisExt.RedisScheduler"
  },
  "Scheduler.RedisExt": {
    "RedisServer": "192.168.0.106",
    "RedisPort": "6379",
    "ReceiverQueue": "NScrapy.Downloader",
    "ResponseQueue": "NScrapy.ResponseQueue"
  }
}
```

#### Downloader 端 / Downloader Side

修改 `NScrapy.DownloaderShell.dll` 同层目录中的 `appsettings.json`：

Modify `appsettings.json` under `NScrapy.DownloaderShell.dll` directory:

```json
{
  "Scheduler": {
    "SchedulerType": "NScrapy.Scheduler.RedisExt.RedisScheduler"
  },
  "Scheduler.RedisExt": {
    "RedisServer": "192.168.0.106",
    "RedisPort": "6379",
    "ReceiverQueue": "NScrapy.Downloader",
    "ResponseQueue": "NScrapy.ResponseQueue"
  }
}
```

单独运行 DownloaderShell：
Run DownloaderShell individually:

```bash
dotnet /path/to/NScrapy.DownloaderShell.dll
```

### 状态更新中间件 / Status Updater Middleware

如果需要将 Downloader 状态更新到 Redis（用于监控），可以在 `appsettings.json` 中添加：

If you want to update Downloader status to Redis (for monitoring), add to `appsettings.json`:

```json
"DownloaderMiddlewares": [
  { "Middleware": "NScrapy.DownloaderShell.StatusUpdaterMiddleware" }
]
```

> 💡 [NScrapyWebConsole](https://github.com/xboxeer/NScrapyWebConsole) 会从 Redis 中读取 Downloader 状态数据。
> 💡 [NScrapyWebConsole](https://github.com/xboxeer/NScrapyWebConsole) reads Downloader status from Redis.

### MongoDB Pipeline

将爬取的数据存储到 MongoDB：

Save crawled data to MongoDB:

```csharp
public class MongoItemPipeline : IPipeline<JobItem>
{
    private MongoClient client = new MongoClient("mongodb://localhost:27017");
    public async void ProcessItem(JobItem item, ISpider spider)
    {
        var db = client.GetDatabase("NScrapy");
        var collection = db.GetCollection<JobItem>("JobItem");
        await collection.InsertOneAsync(item);
    }
}
```

添加到 `appsettings.json`：
Add to `appsettings.json`:

```json
"Pipelines": [
  { "Pipeline": "NScrapy.Project.MongoItemPipeline" }
]
```

### CSV Pipeline

将数据保存为 CSV 文件：

Save data to CSV file:

```csharp
public class CSVItemPipeline : IPipeline<JobItem>
{
    private string startTime = DateTime.Now.ToString("yyyyMMddhhmm");

    public void ProcessItem(JobItem item, ISpider spider)
    {
        var info = $"{item.Title},{item.Firm},{item.Salary},{item.Time},{System.Environment.NewLine}";
        Console.WriteLine(info);
        File.AppendAllText($"output-{startTime}.csv", info, Encoding.UTF8);
    }
}
```

添加到 `appsettings.json`：
Add to `appsettings.json`:

```json
"Pipelines": [
  { "Pipeline": "NScrapy.Project.MongoItemPipeline" },
  { "Pipeline": "NScrapy.Project.CSVItemPipeline" }
]
```

---

## 旧版 API / Legacy API

> 📄 旧版类继承 API 文档已移至 [LEGACY.md](./LEGACY.md)。
> For legacy class-inheritance API documentation, see [LEGACY.md](./LEGACY.md).
