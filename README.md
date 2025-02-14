# JS4399 For C#

此项目是一个可以自动本地免实名注册 4399 账号，并登录到 4399 我的世界启动器的程序，采用 C# 编写。

## 使用方法

### 1. 添加 JS4399 命名空间

```csharp
using JS4399MC;
```

### 2. 创建 JS4399 对象

```csharp
JS4399 js4399 = new JS4399(new JS4399HttpConfig{});
```

如果需要自定义网络代理和 HTTP 请求超时时间，可以如下配置：

```csharp
JS4399 js4399 = new JS4399(new JS4399HttpConfig
{
    Proxy = new WebProxy("http://127.0.0.1:8080"),
    Timeout = TimeSpan.FromSeconds(10) // 默认为10秒
});
```

### 3. 定义验证码处理异步函数

在注册或登录时，需要提供一个验证码处理函数。如果需要进行验证码识别，验证码图片的 Base64 编码会传入此函数，函数需要返回验证码字符串。

本项目的示例代码中自带4399识别库，为“雨落”识别库。

```csharp
Func<string, Task<string>> captchaFunc = async (base64) =>
{
    string code = Ocr.GetOcrResult(base64);
    return code;
};
```

## 注册

使用 `JS4399.JS4399RegisterAsync` 方法进行自动注册。

示例：

```csharp
JS4399Result registerResult = await js4399.JS4399RegisterAsync(null, captchaFunc);
```

`JS4399Result` 对象包含以下信息：
- `Success` 表示是否注册成功。
- `Message` 表示注册失败或成功的原因。
- 如果注册成功，`Username` 和 `Password` 表示注册的用户名和密码。

### 自定义用户名和密码

可以传递一个包含用户名和密码的字典：

示例：

```csharp
JS4399Result registerResult = await js4399.JS4399RegisterAsync(new Dictionary<string, object>
{
    { "username", "username123" },
    { "password", "12345678" }
}, captchaFunc);
```

## 登录

使用 `JS4399.JS4399LoginAsync` 方法进行自动登录。

示例：

```csharp
JS4399Result loginResult = await js4399.JS4399LoginAsync(new Dictionary<string, object>
{
    { "username", "username123" },
    { "password", "12345678" }
}, captchaFunc);
```

`JS4399Result` 对象包含以下信息：
- `Success` 表示是否注册成功。
- `Message` 表示注册失败或成功的原因。
- 如果登录成功，`SauthJson` 和 `SauthJsonValue` 表示登录信息。
    - `SauthJson` 是一个仅有 `sauth_json` 一个成员的 JSON 对象文本。
    - `SauthJsonValue` 是 `SauthJson` 中 `sauth_json` 的值，是一个拥有多个成员的 JSON 对象文本。
