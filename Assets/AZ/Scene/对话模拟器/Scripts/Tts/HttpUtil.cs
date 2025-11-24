using System; // 确保 using System; 存在

using System.IO;

using System.Net;

using System.Text;

using System.Collections.Generic;



namespace OpenapiDemo

{

    public static class HttpUtil

    {

        public static byte[] doPost(string url, Dictionary<string, string[]> header, Dictionary<string, string[]> postParams, string resultType)

        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "POST";



            // 设置Header

            if (header != null)

            {

                foreach (var h in header)

                {

                    if ("Content-Type".Equals(h.Key))

                    {

                        request.ContentType = h.Value[0];

                    }

                    else

                    {

                        request.Headers.Add(h.Key, h.Value[0]);

                    }

                }

            }



            // 添加post数据

            StringBuilder postData = new StringBuilder();

            if (postParams != null)

            {

                bool first = true;

                foreach (var p in postParams)

                {

                    if (first)

                    {

                        first = false;

                    }

                    else

                    {

                        postData.Append("&");

                    }

                    

                    // ############ 这里是唯一的修改点 ############

                    // 使用 Uri.EscapeDataString 替代 System.Web.HttpUtility.UrlEncode

                    postData.Append(p.Key).Append("=").Append(Uri.EscapeDataString(p.Value[0]));

                    // #########################################

                }

            }



            byte[] data = Encoding.UTF8.GetBytes(postData.ToString());

            request.ContentLength = data.Length;



            using (Stream stream = request.GetRequestStream())

            {

                stream.Write(data, 0, data.Length);

            }



            // 获取返回数据

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if ("audio".Equals(resultType))

            {

                using (Stream stream = response.GetResponseStream())

                {

                    using (MemoryStream ms = new MemoryStream())

                    {

                        byte[] buffer = new byte[1024];

                        int count;

                        while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)

                        {

                            ms.Write(buffer, 0, count);

                        }

                        return ms.ToArray();

                    }

                }

            }

            return null;

        }

    }

}