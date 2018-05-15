using NScrapy.Infra;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace NScrapy.Downloader.Middleware
{
    public class HttpDecompressionMiddleware : EmptyDownloaderMiddleware

    {
        public override async void PostDownload(IResponse response)
        {
            //Decompress the content if the response is compressed
            var encoding = response.RawResponseMessage.Content.Headers.ContentEncoding.FirstOrDefault();
            var resultStream = await response.RawResponseMessage.Content.ReadAsStreamAsync();
            var decompressedBody = string.Empty;
            if (encoding != null)
            {
                if (encoding.ToLower() == "gzip")
                {
                    decompressedBody = this.Decompressor(resultStream, ContentCompressType.GZip);
                }
                else if (encoding.ToLower() == "deflate")
                {
                    decompressedBody = this.Decompressor(resultStream, ContentCompressType.Deflate);
                }
                else
                {
                    decompressedBody = response.RawResponseMessage.Content.ReadAsStringAsync().Result;
                }
            }
            else
            {
                decompressedBody = response.RawResponseMessage.Content.ReadAsStringAsync().Result;
            }
            response.ResponsePlanText = decompressedBody;
        }
        private string Decompressor(Stream inputStream, ContentCompressType compressType)
        {
            using (MemoryStream decompressedSteam = new MemoryStream())
            {
                var buffer = new Byte[1024];
                Stream decompressorStream = null;
                if (compressType == ContentCompressType.GZip)
                {
                    decompressorStream = new GZipStream(inputStream, CompressionMode.Decompress);
                }
                else if (compressType == ContentCompressType.Deflate)
                {
                    decompressorStream = new DeflateStream(inputStream, CompressionMode.Decompress);
                }
                try
                {
                    int len = 0;
                    while ((len = decompressorStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        decompressedSteam.Write(buffer, 0, len);
                    }
                }
                finally
                {
                    decompressorStream.Dispose();
                }
                return UTF8Encoding.UTF8.GetString(decompressedSteam.ToArray());
            }
        }
    }

    public enum ContentCompressType
    {
        GZip,
        Deflate
    }
}
