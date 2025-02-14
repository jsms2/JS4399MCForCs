using JS4399MC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var config = new JS4399HttpConfig
        {
        };

        var js4399 = new JS4399(config);

        Func<string, Task<string>> captchaFunc = async (base64) =>
        {
            string code = Ocr.GetOcrResult(base64);
            return code;
        };


        var registerResult = await js4399.JS4399RegisterAsync(null, captchaFunc);
        Console.WriteLine(registerResult.Success ?
            $"注册成功: {registerResult.Username}/{registerResult.Password}" :
            $"注册失败: {registerResult.Message}");
        if (!registerResult.Success) return;
        var loginParams = new Dictionary<string, object>{
                { "username", registerResult.Username },
                { "password", registerResult.Password }
            };

        var loginResult = await js4399.JS4399LoginAsync(loginParams, captchaFunc);
        Console.WriteLine(loginResult.Success ?
            $"登录成功:\n{loginResult.SauthJson}" :
            $"登录失败: {loginResult.Message}");
        Console.ReadLine();
    }
}
