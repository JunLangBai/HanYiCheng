using System;

using System.Collections.Generic;

using System.Security.Cryptography;

using System.Text;



namespace OpenapiDemo

{

    public static class AuthV3Util

    {

        /// <summary>

        /// 添加鉴权相关参数 - aone

        /// </summary>

        /// <param name="appKey">应用ID</param>

        /// <param name="appSecret">应用密钥</param>

        /// <param name="paramsMap">请求参数表</param>

        public static void addAuthParams(string appKey, string appSecret, Dictionary<string, string[]> paramsMap)

        {

            // 添加请求时间

            string curtime = ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds).ToString();

            // 添加salt

            string salt = Guid.NewGuid().ToString();



            // 添加signType

            string signType = "v3";

            paramsMap.Add("curtime", new string[] { curtime });

            paramsMap.Add("salt", new string[] { salt });

            paramsMap.Add("signType", new string[] { signType });

            // 添加应用ID

            paramsMap.Add("appKey", new string[] { appKey });

            // 添加签名

            string sign = calculateSign(appKey, appSecret, paramsMap["q"][0], salt, curtime);

            paramsMap.Add("sign", new string[] { sign });

        }



        /// <summary>

        /// 计算签名

        /// </summary>

        /// <param name="appKey">应用ID</param>

        /// <param name="appSecret">应用密钥</param>

        /// <param name="q">请求内容</param>

        /// <param name="salt">随机字符串</param>

        /// <param name="curtime">请求时间</param>

        /// <returns>签名</returns>

        private static string calculateSign(string appKey, string appSecret, string q, string salt, string curtime)

        {

            if (q == null)

            {

                q = "";

            }

            string strSrc = appKey + getInput(q) + salt + curtime + appSecret;

            return sha256(strSrc);

        }



        private static string getInput(string q)

        {

            if (q == null)

            {

                return "";

            }

            int len = q.Length;

            return len <= 20 ? q : q.Substring(0, 10) + len + q.Substring(len - 10, 10);

        }



        private static string sha256(string str)

        {

            using (SHA256Managed sha256 = new SHA256Managed())

            {

                byte[] bytes = Encoding.UTF8.GetBytes(str);

                byte[] hash = sha256.ComputeHash(bytes);

                StringBuilder sb = new StringBuilder();

                foreach (byte b in hash)

                {

                    sb.Append(b.ToString("x2"));

                }

                return sb.ToString();

            }

        }

    }

}