using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NScrapy.Infra
{
    public static class NScrapyHelper
    {
        //Compress the input string and then save as Base64 format
        public async static Task<string> CompressString(string str)
        {
            using (MemoryStream outputStream = new MemoryStream())
            {
                var data = UTF8Encoding.UTF8.GetBytes(str);
                var compressStream = new GZipStream(outputStream, CompressionMode.Compress);
                await compressStream.WriteAsync(data, 0, data.Length);
                compressStream.Flush();
                compressStream.Close();
                return Convert.ToBase64String(outputStream.ToArray());
            }
        }

        public  static IResponse DecompressResponse(string str)
        {            
            var serializedResponseStr =  DecompressResponseStr(str);
            var response = JsonConvert.DeserializeObject<HttpResponse>(serializedResponseStr);
            return response;
        }

        private static string DecompressResponseStr(string str)
        {
            var compressedBytes = Convert.FromBase64String(str);
            using (MemoryStream ms = new MemoryStream(compressedBytes))
            {
                var decompressStream = new GZipStream(ms, CompressionMode.Decompress);
                using (var outputStream = new MemoryStream())
                {
                    var buffer = new byte[1024];
                    int i = 0;
                    while((i= decompressStream.Read(buffer, 0, buffer.Length))>0)
                    {
                        outputStream.Write(buffer, 0, i);
                    }
                    return Encoding.UTF8.GetString(outputStream.ToArray());
                }
            }
        }
    }
}
