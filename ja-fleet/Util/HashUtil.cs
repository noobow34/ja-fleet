using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace jafleet.Util
{
    public static class HashUtil
    {
        /// <summary>
        /// 文字列のCRC32を計算して文字列で返す
        /// </summary>
        /// <param name="srcStr"></param>
        /// <returns></returns>
        public static string CalcCRC32(string srcStr)
        {
            byte[] bytes = new UTF8Encoding().GetBytes(srcStr);
            uint crc32 = new CRC32().Calc(bytes);

            return Convert.ToString(crc32, 16);
        }
    }
}
