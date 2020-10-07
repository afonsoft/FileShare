using System.Collections.Generic;
using System.IO;
using Winista.Mime;

namespace FileShare.Utilities
{
    public static class FindMimeHelpers
    {
        private static MimeTypes mimeTypes => new MimeTypes();

        public static Dictionary<string, string> ListOfMimeType { get; set; }

        public static string GetMimeFromFile(string filePath)
        {
            return mimeTypes.GetMimeTypeFromFile(filePath).Name;
        }
        public static string GetMimeFromByte(byte[] bytes)
        {
            return mimeTypes.GetMimeType(bytes).Name;
        }
        public static string GetMimeFromStream(Stream stream)
        {
            return mimeTypes.GetMimeType(ConverteStreamToByteArray(stream)).Name;
        }

        public static string[] GetExtensionsFromFile(string filePath)
        {
            return mimeTypes.GetMimeTypeFromFile(filePath).Extensions;
        }
        public static string[] GetExtensionsFromByte(byte[] bytes)
        {
            return mimeTypes.GetMimeType(bytes).Extensions;
        }
        public static string[] GetExtensionsFromStream(Stream stream)
        {
            return mimeTypes.GetMimeType(ConverteStreamToByteArray(stream)).Extensions;
        }

        public static string GetMimeFromExtensions(string ext)
        {
            string typeExt = "";

            foreach (KeyValuePair<string, string> kvp in ListOfMimeType)
            {
                if (kvp.Key == ext)
                {
                    typeExt = kvp.Value;
                    break;
                }
            }

            if (string.IsNullOrEmpty(typeExt))
                typeExt = "application/octet-stream";

            return typeExt;
        }

        private static byte[] ConverteStreamToByteArray(Stream stream)
        {
            byte[] byteArray = new byte[16 * 1024];
            stream.Position = 0;
            using (MemoryStream mStream = new MemoryStream())
            {
                int bit;
                while ((bit = stream.Read(byteArray, 0, byteArray.Length)) > 0)
                {
                    mStream.Write(byteArray, 0, bit);
                }
                stream.Position = 0;
                return mStream.ToArray();
            }
        }
    }
}
