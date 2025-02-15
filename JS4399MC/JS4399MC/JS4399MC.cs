using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using JS4399MC;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JS4399MC
{
    public class JS4399HttpConfig
    {
        public WebProxy Proxy { get; set; } = null;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
    }


    public class JS4399Result
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string Username { get; set; }
        public string Password { get; set; }
        public string SauthJson { get; set; }
        public string SauthJsonValue { get; set; }
    }

    public class JS4399
    {
        //private readonly HttpClient _httpClient;
        private const string _userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:134.0) Gecko/20100101 Firefox/134.0";
        private static readonly Random _random = new Random();
        private static JS4399HttpConfig _httpConfig;

        public JS4399(JS4399HttpConfig httpConfig = null)
        {
            _httpConfig = httpConfig ?? new JS4399HttpConfig();
        }

        public async Task<JS4399Result> JS4399RegisterAsync(Dictionary<string, object> registerConfig = null, Func<string, Task<string>> captchaFunctionAsync = null)
        {
            var result = new JS4399Result();
            try
            {
                var username = registerConfig?.ContainsKey("username") == true ?
                    registerConfig["username"].ToString() : GenerateRandomString();

                var password = registerConfig?.ContainsKey("password") == true ?
                    registerConfig["password"].ToString() : GenerateRandomString();

                var captchaID = "captchaReqb3d25c6d6a4" + GenerateRandomString(8, new Dictionary<string, object> { { "numbers", true } });

                // Get captcha
                var httpClient = new HttpClient(new HttpClientHandler
                {
                    Proxy = _httpConfig.Proxy,
                    UseProxy = _httpConfig.Proxy != null
                });
                httpClient.DefaultRequestHeaders.Add("User-Agent", _userAgent);

                var captchaUrl = $"https://ptlogin.4399.com/ptlogin/captcha.do?xx=1&captchaId={captchaID}";
                var request = new HttpRequestMessage(HttpMethod.Get, captchaUrl);

                var captchaResponse = await httpClient.SendAsync(request);
                if (!captchaResponse.IsSuccessStatusCode)
                {
                    result.Message = "访问验证码接口失败";
                    return result;
                }

                var bytes = await captchaResponse.Content.ReadAsByteArrayAsync();
                var base64String = Convert.ToBase64String(bytes);
                var captcha = await captchaFunctionAsync(base64String);

                // Generate ID card
                var idCard = "110108" + GetRandomDate("19700101", "20041231") +
                           GenerateRandomString(3, new Dictionary<string, object> { { "numbers", true } });
                idCard += GetIDCardLastCode(idCard);

                // Generate name
                var name = GenerateRandomString(1, new Dictionary<string, object> {
                    { "custom", "李王张刘陈杨赵黄周吴徐孙胡朱高林何郭马罗梁宋郑谢韩唐冯于董萧程曹袁邓许傅沈曾彭吕苏卢蒋蔡贾丁魏薛叶阎余潘杜戴夏钟汪田任姜范方石姚谭廖邹熊金陆郝孔白崔康毛邱秦江史顾侯邵孟龙万段漕钱汤尹黎易常武乔贺赖龚文" }
                }) + GenerateRandomString(2, new Dictionary<string, object> { { "chinese", true } });

                // Build register URL
                var registerUrl = $"https://ptlogin.4399.com/ptlogin/register.do?" +
                    $"postLoginHandler=default&displayMode=popup&appId=www_home&gameId=&" +
                    $"cid=&externalLogin=qq&aid=&ref=&css=&redirectUrl=&regMode=reg_normal&" +
                    $"sessionId={captchaID}&regIdcard=true&noEmail=false&crossDomainIFrame=&" +
                    $"crossDomainUrl=&mainDivId=popup_reg_div&showRegInfo=true&includeFcmInfo=false&" +
                    $"expandFcmInput=true&fcmFakeValidate=true&userNameLabel=4399%E7%94%A8%E6%88%B7%E5%90%8D&" +
                    $"username={username}&password={password}&" +
                    $"realname={HttpUtility.UrlEncode(name)}&idcard={idCard}&" +
                    $"email={GenerateRandomString(9, new Dictionary<string, object> { { "numbers", true } })}@qq.com&" +
                    $"reg_eula_agree=on&inputCaptcha={captcha}";
                request = new HttpRequestMessage(HttpMethod.Get, registerUrl);
                var registerResponse = await httpClient.SendAsync(request);
                if (!registerResponse.IsSuccessStatusCode)
                {
                    result.Message = "访问注册接口失败";
                    return result;
                }

                var responseText = await registerResponse.Content.ReadAsStringAsync();
                if (responseText.Contains("验证码错误"))
                {
                    result.Message = "验证码错误";
                    return result;
                }
                if (responseText.Contains("用户名格式错误"))
                {
                    result.Message = "用户名格式错误";
                    return result;
                }
                if (responseText.Contains("用户名已被注册"))
                {
                    result.Message = "用户名已被注册";
                    return result;
                }
                if (!responseText.Contains("请一定记住您注册的用户名和密码"))
                {
                    result.Message = "未知错误";
                    return result;
                }
                result.Success = true;
                result.Message = "注册成功";
                result.Username = username;
                result.Password = password;
            }catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }

        public async Task<JS4399Result> JS4399LoginAsync(Dictionary<string, object> loginConfig, Func<string, Task<string>> captchaFunctionAsync)
        {
            var result = new JS4399Result();
            try
            {
                var username = loginConfig["username"].ToString();
                var password = loginConfig["password"].ToString();
                var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

                HttpClient httpClient = new HttpClient(new HttpClientHandler
                {
                    Proxy = _httpConfig.Proxy,
                    UseProxy = _httpConfig.Proxy != null
                });
                httpClient.DefaultRequestHeaders.Add("User-Agent", _userAgent);



                // Check captcha need
                var verifyUrl = $"https://ptlogin.4399.com/ptlogin/verify.do?" +
                    $"username={username}&appId=kid_wdsj&t={currentTime}&inputWidth=iptw2&v=1";

                var verifyResponse = await httpClient.GetAsync(verifyUrl);
                if (!verifyResponse.IsSuccessStatusCode)
                {
                    result.Message = "访问验证接口失败";
                    return result;
                }

                var verifyText = await verifyResponse.Content.ReadAsStringAsync();
                string captcha = null;
                string captchaID = null;

                if (verifyText != "0")
                {
                    captchaID = GetBetweenStrings(verifyText, "captchaId=", "'");
                    if (string.IsNullOrEmpty(captchaID))
                    {
                        result.Message = "获取captchaID失败";
                        return result;
                    }

                    var captchaResult = await httpClient.GetAsync($"https://ptlogin.4399.com/ptlogin/captcha.do?captchaId={captchaID}");
                    if (!captchaResult.IsSuccessStatusCode)
                    {
                        result.Message = "获取验证码失败";
                        return result;
                    }

                    var bytes = await captchaResult.Content.ReadAsByteArrayAsync();
                    var base64String = Convert.ToBase64String(bytes);
                    captcha = await captchaFunctionAsync(base64String);
                }

                // First login request
                string loginData = $"loginFrom=uframe&postLoginHandler=default&layoutSelfAdapting=true&" +
                    $"externalLogin=qq&displayMode=popup&layout=vertical&bizId=2100001792&appId=kid_wdsj&gameId=wd&" +
                    $"css=http%3A%2F%2Fmicrogame.5054399.net%2Fv2%2Fresource%2FcssSdk%2Fdefault%2Flogin.css&" +
                    $"redirectUrl=&sessionId={captchaID ?? ""}&mainDivId=popup_login_div&includeFcmInfo=false&" +
                    $"level=8&regLevel=8&userNameLabel=4399%E7%94%A8%E6%88%B7%E5%90%8D&" +
                    $"userNameTip=%E8%AF%B7%E8%BE%93%E5%85%A54399%E7%94%A8%E6%88%B7%E5%90%8D&" +
                    $"welcomeTip=%E6%AC%A2%E8%BF%8E%E5%9B%9E%E5%88%B04399&sec=1&password={password}&" +
                    $"username={username}&inputCaptcha={captcha ?? ""}";
                StringContent content = new StringContent(loginData, Encoding.UTF8, "application/x-www-form-urlencoded");
                HttpResponseMessage loginResponse = await httpClient.PostAsync("https://ptlogin.4399.com/ptlogin/login.do?v=1", content);
                if (!loginResponse.IsSuccessStatusCode)
                {
                    result.Message = "登录请求失败";
                    return result;
                }

                var loginText = await loginResponse.Content.ReadAsStringAsync();
                if (loginText.Contains("验证码错误"))
                {
                    result.Message = "验证码错误";
                    return result;
                }

                if (loginText.Contains("密码错误"))
                {
                    result.Message = "密码错误";
                    return result;
                }

                if (loginText.Contains("用户不存在"))
                {
                    result.Message = "用户不存在";
                    return result;
                }

                var randtime = GetBetweenStrings(loginText, "parent.timestamp = \"", "\"");
                if (string.IsNullOrEmpty(randtime))
                {
                    result.Message = "获取时间戳失败";
                    return result;
                }

                var cookies = loginResponse.Headers.GetValues("Set-Cookie").ToList();
                if (cookies == null || cookies.Count == 0)
                {
                    result.Message = "Cookie获取失败";
                    return result;
                }
                string cookieString = string.Join("; ", cookies.Select(cookie => cookie.Split(';')[0]));
                CookieContainer cookieContainer = new CookieContainer();
                Uri targetUri = new Uri("https://ptlogin.4399.com/ptlogin/checkKidLoginUserCookie.do");
                foreach (var cookie in cookieString.Split(';'))
                {
                    var cookieParts = cookie.Split('=');
                    if (cookieParts.Length == 2)
                    {
                        cookieContainer.Add(targetUri, new Cookie(cookieParts[0].Trim(), cookieParts[1].Trim()));
                    }
                }

                // Second login check
                var checkUrl = $"https://ptlogin.4399.com/ptlogin/checkKidLoginUserCookie.do?" +
                    $"appId=kid_wdsj&gameUrl=http://cdn.h5wan.4399sj.com/microterminal-h5-frame?" +
                    $"game_id=500352&rand_time={randtime}&nick=null&onLineStart=false&" +
                    $"show=1&isCrossDomain=1&retUrl=http%253A%252F%252Fptlogin.4399.com" +
                    $"%252Fresource%252Fucenter.html%253Faction%253Dlogin%2526appId%253Dkid_wdsj%2526" +
                    $"loginLevel%253D8%2526regLevel%253D8%2526bizId%253D2100001792%2526externalLogin%253D" +
                    $"qq%2526qrLogin%253Dtrue%2526layout%253Dvertical%2526level%253D101%2526" +
                    $"css%253Dhttp%253A%252F%252Fmicrogame.5054399.net%252Fv2%252Fresource%252F" +
                    $"cssSdk%252Fdefault%252Flogin.css%2526v%253D2018_11_26_16%2526" +
                    $"postLoginHandler%253Dredirect%2526checkLoginUserCookie%253Dtrue%2526" +
                    $"redirectUrl%253Dhttp%25253A%25252F%25252Fcdn.h5wan.4399sj.com%25252F" +
                    $"microterminal-h5-frame%25253Fgame_id%25253D500352%252526rand_time%25253D{randtime}";
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, checkUrl);


                request.Headers.Add("Cookie", cookieString);

                HttpResponseMessage checkResponse = null;
                using (HttpClient tmpClient = new HttpClient(new HttpClientHandler
                {
                    Proxy = _httpConfig.Proxy,
                    UseProxy = _httpConfig.Proxy != null,
                    AllowAutoRedirect = false,
                    CookieContainer = cookieContainer
                }))
                {
                    tmpClient.DefaultRequestHeaders.Add("User-Agent", _userAgent);
                    checkResponse = await tmpClient.SendAsync(request);
                }

                if (checkResponse.StatusCode != HttpStatusCode.Redirect)
                {
                    result.Message = "检查登录状态失败";
                    return result;
                }

                var redirectUrl = checkResponse.Headers.Location?.ToString();
                if (string.IsNullOrEmpty(redirectUrl))
                {
                    result.Message = "获取重定向地址失败";
                    return result;
                }

                var uri = new Uri(redirectUrl);
                if (uri.Host != "cdn.h5wan.4399sj.com")
                {
                    result.Message = "重定向域名错误";
                    return result;
                }

                var query = HttpUtility.ParseQueryString(uri.Query);
                var sig = query["sig"];
                var uid = query["uid"];
                var time = query["time"];
                var validateState = query["validateState"];

                if (string.IsNullOrEmpty(sig) || string.IsNullOrEmpty(uid) ||
                    string.IsNullOrEmpty(time) || string.IsNullOrEmpty(validateState))
                {
                    result.Message = "解析重定向参数失败";
                    return result;
                }

                // Get SDK info
                var sdkUrl = $"https://microgame.5054399.net/v2/service/sdk/info?" +
                    $"callback=&queryStr=game_id%3D500352%26nick%3Dnull%26sig%3D{sig}%26" +
                    $"uid%3D{uid}%26fcm%3D0%26show%3D1%26isCrossDomain%3D1%26rand_time%3D{randtime}%26" +
                    $"ptusertype%3D4399%26time%3D{time}%26validateState%3D{validateState}%26" +
                    $"username%3D{username.ToLower()}&_={time}";

                var sdkResponse = await httpClient.GetAsync(sdkUrl);
                if (!sdkResponse.IsSuccessStatusCode)
                {
                    result.Message = "获取SDK信息失败";
                    return result;
                }

                var sdkJson = JObject.Parse(await sdkResponse.Content.ReadAsStringAsync());
                var sdkLoginData = sdkJson["data"]?["sdk_login_data"]?.ToString();
                if (string.IsNullOrEmpty(sdkLoginData))
                {
                    result.Message = "解析SDK数据失败";
                    return result;
                }

                var sdkParams = HttpUtility.ParseQueryString(sdkLoginData);
                var sessionId = sdkParams["token"];
                if (string.IsNullOrEmpty(sessionId))
                {
                    result.Message = "获取token失败";
                    return result;
                }

                // Generate sauth data
                var randomStrings = Enumerable.Range(0, 2)
                    .Select(_ => GenerateRandomString(32, new Dictionary<string, object> { { "custom", "0123456789ABCDEF" } }))
                    .ToArray();

                var sauth = new
                {
                    aim_info = "{\"aim\":\"127.0.0.1\",\"country\":\"CN\",\"tz\":\"+0800\",\"tzid\":\"\"}",
                    app_channel = "4399pc",
                    client_login_sn = randomStrings[0],
                    deviceid = randomStrings[1],
                    gameid = "x19",
                    gas_token = "",
                    ip = "127.0.0.1",
                    login_channel = "4399pc",
                    platform = "pc",
                    realname = "{\"realname_type\":\"0\"}",
                    sdk_version = "1.0.0",
                    sdkuid = uid,
                    sessionid = sessionId,
                    source_platform = "pc",
                    timestamp = time,
                    udid = randomStrings[1],
                    userid = username.ToLower()
                };

                var sauthJsonValue = JsonConvert.SerializeObject(sauth);
                var sauthJson = JsonConvert.SerializeObject(new { sauth_json = sauthJsonValue });

                // Final login requests
                request = new HttpRequestMessage(HttpMethod.Post, "https://mgbsdk.matrix.netease.com/x19/sdk/uni_sauth");
                request.Headers.Add("User-Agent", "WPFLauncher/0.0.0.0");
                content = new StringContent(sauthJsonValue, Encoding.UTF8, "application/json");
                request.Content = content;
                var uniResponse = await httpClient.SendAsync(request);

                if (!uniResponse.IsSuccessStatusCode)
                {
                    result.Message = "统一认证请求失败";
                    return result;
                }

                request = new HttpRequestMessage(HttpMethod.Post, "https://x19obtcore.nie.netease.com:8443/login-otp");
                request.Headers.Add("User-Agent", "WPFLauncher/0.0.0.0");
                content = new StringContent(sauthJson, Encoding.UTF8, "text/plain");
                request.Content = content;
                var finalResponse = await httpClient.SendAsync(request);

                if (!finalResponse.IsSuccessStatusCode)
                {
                    result.Message = "最终登录请求失败";
                    return result;
                }

                var finalJson = JObject.Parse(await finalResponse.Content.ReadAsStringAsync());
                if (finalJson["entity"]?["aid"] == null)
                {
                    result.Message = "获取aid失败";
                    return result;
                }

                result.Success = true;
                result.Message = "登录成功";
                result.SauthJson = sauthJson;
                result.SauthJsonValue = sauthJsonValue;
                
            }catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }

        private string GetBetweenStrings(string str, string start, string end)
        {
            var startIndex = str.IndexOf(start, StringComparison.Ordinal);
            if (startIndex == -1) return null;

            startIndex += start.Length;
            var endIndex = str.IndexOf(end, startIndex, StringComparison.Ordinal);
            return endIndex == -1 ? null : str.Substring(startIndex, endIndex - startIndex);
        }

        public string GenerateRandomString(int length = 10, Dictionary<string, object> options = null)
        {

            options = options ?? new Dictionary<string, object> { { "numbers", true }, { "lowercase", true } };
            var charPool = new StringBuilder();
            bool onlyChinese = true;
            bool useChinese = false;
            if (options.ContainsKey("custom") && options["custom"].ToString() != "")
            {
                charPool.Append(options["custom"].ToString());
                onlyChinese = false;
            }
            else
            {
                if (options.ContainsKey("numbers") && (bool)options["numbers"])
                {
                    charPool.Append("0123456789");
                    onlyChinese = false;
                }
                if (options.ContainsKey("lowercase") && (bool)options["lowercase"])
                {
                    charPool.Append("abcdefghijklmnopqrstuvwxyz");
                    onlyChinese = false;
                }
                if (options.ContainsKey("uppercase") && (bool)options["uppercase"])
                {
                    charPool.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
                    onlyChinese = false;
                }
                if (options.ContainsKey("symbols") && (bool)options["symbols"])
                {
                    charPool.Append("!@#$%^&*()-_=+[]{}|;:,.<>?/");
                    onlyChinese = false;
                }
                useChinese = options.ContainsKey("chinese") && (bool)options["chinese"];

            }

            var result = new StringBuilder();


            if (charPool.Length == 0 && !useChinese)
            {
                throw new ArgumentException("Character pool is empty. Please ensure options contain at least one character set.");
            }

            for (int i = 0; i < length; i++)
            {
                if (!(options.ContainsKey("custom") && options["custom"].ToString() != "") && (onlyChinese || (useChinese && _random.NextDouble() > 0.5)))
                {
                    result.Append(GenerateChineseCharacter());
                }
                else
                {
                    var index = _random.Next(charPool.Length);
                    result.Append(charPool.ToString()[index]);
                }
            }
            return result.ToString();
        }


        private static char GenerateChineseCharacter()
        {
            return (char)(_random.Next(0x4E00, 0x9FA5 + 1));
        }

        private string GetRandomDate(string startDate, string endDate)
        {
            var start = DateTime.ParseExact(startDate, "yyyyMMdd", CultureInfo.InvariantCulture);
            var end = DateTime.ParseExact(endDate, "yyyyMMdd", CultureInfo.InvariantCulture);
            var range = (end - start).Days;
            return start.AddDays(_random.Next(range)).ToString("yyyyMMdd");
        }

        private string GetIDCardLastCode(string idCard)
        {
            int[] factors = { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };
            string[] codes = { "1", "0", "X", "9", "8", "7", "6", "5", "4", "3", "2" };

            int sum = idCard.Take(17)
                .Select((c, i) => (c - '0') * factors[i])
                .Sum();

            return codes[sum % 11];
        }
    }

    
}

