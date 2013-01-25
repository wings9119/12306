using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace _12306Helper
{
    public class Common
    {
        private static string StripHTML(string stringToStrip)
        {
            // paring using RegEx           //
            stringToStrip = Regex.Replace(stringToStrip, "</p(?:\\s*)>(?:\\s*)<p(?:\\s*)>", "\n\n", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            stringToStrip = Regex.Replace(stringToStrip, "<br(?:\\s*)/>", "\n", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            stringToStrip = Regex.Replace(stringToStrip, "\"", "''", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            stringToStrip = StripHtmlXmlTags(stringToStrip);
            return stringToStrip;
        }
        private static string StripHtmlXmlTags(string content)
        {
            return Regex.Replace(content, "<[^>]+>", "", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }
        /// <summary>  
        /// 清除html标记  
        /// </summary>  
        /// <param name="content">带有html字符串</param>  
        /// <returns> 清除html标记后的字符串</returns>  
        public static String ClearHtml(String str)
        {
            return (String.IsNullOrEmpty(str)) ? String.Empty : System.Text.RegularExpressions.Regex.Replace(str, @"<[^>]*>", String.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        public static bool IsNumeric(string str) //接收一个string类型的参数,保存到str里
        {
            if (str == null || str.Length == 0)    //验证这个参数是否为空
                return false;                           //是，就返回False
            ASCIIEncoding ascii = new ASCIIEncoding();//new ASCIIEncoding 的实例
            byte[] bytestr = ascii.GetBytes(str);         //把string类型的参数保存到数组里

            foreach (byte c in bytestr)                   //遍历这个数组里的内容
            {
                if (c < 48 || c > 57)                          //判断是否为数字
                {
                    return false;                              //不是，就返回False
                }
            }
            return true;                                        //是，就返回True
        }

        //备注 数字，字母的ASCII码对照表
        /*
        0~9数字对应十进制48－57 
        a~z字母对应的十进制97－122十六进制61－7A 
        A~Z字母对应的十进制65－90十六进制41－5A
        */

    }
}
