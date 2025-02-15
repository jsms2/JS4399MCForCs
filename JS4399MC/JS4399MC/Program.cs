using JS4399MC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

class Program
{
    static async Task Main(string[] args)
    {

        JS4399 js4399 = new JS4399(new JS4399HttpConfig
        {
        });

        Func<string, Task<string>> captchaFunc = async (base64) =>
        {
            string code = Ocr.GetOcrResult(base64);
            return code;
        };


        JS4399Result registerResult = await js4399.JS4399RegisterAsync(null, captchaFunc);
        Console.WriteLine(registerResult.Success ?
            $"注册成功: {registerResult.Username}/{registerResult.Password}" :
            $"注册失败: {registerResult.Message}");
        if (!registerResult.Success) return;


        JS4399Result loginResult = await js4399.JS4399LoginAsync(new Dictionary<string, object>{
            { "username", registerResult.Username },
            { "password", registerResult.Password }
        }, captchaFunc);
        Console.WriteLine(loginResult.Success ?
            $"登录成功:\n{loginResult.SauthJson}" :
            $"登录失败: {loginResult.Message}");
        Console.ReadLine();
    }
}
