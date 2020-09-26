using System.IO;
using Winista.Mime;

namespace FileShare.Utilities
{
    public static class FindMimeHelpers
    {
        public static string GetMimeFromFile(string filePath)
        {
            return new MimeTypes().GetMimeTypeFromFile(filePath).Name;
        }
        public static string GetMimeFromByte(byte[] bytes)
        {
            return new MimeTypes().GetMimeType(bytes).Name;
        }
        public static string GetMimeFromStream(Stream stream)
        {
            return new MimeTypes().GetMimeType(ConverteStreamToByteArray(stream)).Name;
        }

        public static string[] GetExtensionsFromFile(string filePath)
        {
            return new MimeTypes().GetMimeTypeFromFile(filePath).Extensions;
        }
        public static string[] GetExtensionsFromByte(byte[] bytes)
        {
            return new MimeTypes().GetMimeType(bytes).Extensions;
        }
        public static string[] GetExtensionsFromStream(Stream stream)
        {
            return new MimeTypes().GetMimeType(ConverteStreamToByteArray(stream)).Extensions;
        }


        private static byte[] ConverteStreamToByteArray(Stream stream)
        {
            byte[] byteArray = new byte[16 * 1024];
            using (MemoryStream mStream = new MemoryStream())
            {
                int bit;
                while ((bit = stream.Read(byteArray, 0, byteArray.Length)) > 0)
                {
                    mStream.Write(byteArray, 0, bit);
                }
                return mStream.ToArray();
            }
        }
    }
}
