using System.Runtime.InteropServices;


internal class Ocr
{
    // 导入ocr.dll中的ocr4399函数
    [DllImport("ocr.dll")]
    public static extern string ocr4399(string imageBase64);

    public static string GetOcrResult(string imageBase64)
    {

        // 将返回的 IntPtr 转换为字符串
        return ocr4399(imageBase64);
    }
}

