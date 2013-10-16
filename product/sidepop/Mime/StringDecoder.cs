﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace sidepop.Mime
{

    /// <summary>
    /// Utility class that decodes string
    /// </summary>
    public static class StringDecoder
    {
        
        /// <summary>
        /// Decode the recieved content
        /// </summary>
        /// <returns></returns>
        public static string DecodeContent(MimeEntity entity)
        {
            switch (entity.ContentTransferEncoding)
            {
                case TransferEncoding.Base64:
                    return DecodeBase64(entity.ContentLines, entity.ContentType.CharSet);

                case TransferEncoding.QuotedPrintable:
                    byte[] decodedBytes = QuotedPrintableEncoding.Decode(entity.ContentLines);
                    return Decode(decodedBytes, entity.ContentType.CharSet);

                case TransferEncoding.SevenBit:
                default:
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (byte[] line in entity.ContentLines)
                        {
                            if (sb.Length > 0)
                            {
                                sb.AppendLine();
                            }

                            sb.Append(DecodeBytesWithSpecificCharset(line, entity.ContentType.CharSet));
                        }

                        return sb.ToString();
                    }
                
            }
        }

        /// <summary>
        /// Decode the received single line using the correct decoder
        /// </summary>
        private static string DecodeSingleLine(string line, TransferEncoding encoding, string charSet)
        {
            switch (encoding)
            {
                case TransferEncoding.Base64:
                    {
                        byte[] decodedBytes = Encoding.ASCII.GetBytes(line);
                        return DecodeBase64(new byte[][]{decodedBytes}, charSet);
                    }
                case TransferEncoding.QuotedPrintable:
                    {
                        byte[] decodedBytes = QuotedPrintableEncoding.DecodeSingleLine(line);
                        return Decode(decodedBytes, charSet);
                    }
                case TransferEncoding.SevenBit:
                default:
                    return line;
            }
        }

        /// <summary>
        /// Decode the content into the given charset
        /// </summary>
        private static string Decode(byte[] content, string charSet)
        {
            if (charSet != null)
            {
                return DecodeBytesWithSpecificCharset(content, charSet);
            }
            else
            {
                string decodedBytesString = Encoding.UTF8.GetString(content);
                return decodedBytesString;
            }
        }

        /// <summary>
        /// Decodes a Base64 string and returns a string intepreted with the specified charset.
        /// </summary>
        private static string DecodeBase64(byte[][] content, string charSet)
        {
            byte[] allBytes = content.SelectMany(b => b).ToArray();
            byte[] decodedBytes = Convert.FromBase64String(Encoding.ASCII.GetString(allBytes));
                        
            if (charSet != null)
            {
                return DecodeBytesWithSpecificCharset(decodedBytes, charSet);
            }
            else
            {
                return ByteArrayToString(decodedBytes);
            }
        }

        /// <summary>
        /// Converts a string to a byte array without using any encoding.
        /// If we deal with a character that is unicode : ie not fitting into a single byte, we will lose information
        /// </summary>
        public static byte[] StringToByteArray(string decoded)
        {
            byte[] byteArray = decoded.ToCharArray().Select(c => (byte)c).ToArray();
            return byteArray;
        }

        /// <summary>
        /// Converts a string to a byte array without using any encoding.
        /// If we deal with a character that is unicode : ie not fitting into a single byte, we will lose information
        /// </summary>
        public static string ByteArrayToString(byte[] encoded)
        {
            string decoded = new string(encoded.Select(b => (char)b).ToArray());
            return decoded;
        }

        /// <summary>
        /// Use the specified charset to decode the given bytes.
        /// </summary>
        private static string DecodeBytesWithSpecificCharset(byte[] decodedBytes, string charSet)
        {
            Encoding encoding = MimeEntity.GetEncoding(charSet);
            encoding = DetectRealEncoding(decodedBytes, encoding);

            string decodedBytesString = encoding.GetString(decodedBytes);
            return decodedBytesString;
        }

        /// <summary>
        /// Some email client may specify an encoding but it is not the correct encoding. Try to prevent this situation
        /// </summary>
        private static Encoding DetectRealEncoding(byte[] decodedBytes, Encoding encoding)
        {
            //It happens that a MimePart says it is encoded with ISO-8859-1 but it is really Windows-1252
            //Convention is that when any char in the range 0x80 through 0x92 is present, chances are that the encoding is Windows-1252
            if (encoding.HeaderName.Equals("ISO-8859-1", StringComparison.InvariantCultureIgnoreCase) && decodedBytes.Any(b => b >= 0x80 && b <= 0x92))
            {
                //See thread: http://stackoverflow.com/questions/3714061/php-problems-converting-character-from-iso-8859-1-to-utf-8
                encoding = Encoding.GetEncoding("Windows-1252");
            }

            return encoding;
        }

        /// <summary>
        /// Regex for extracting text encoded in QuetedPrintable or Base64 string such as: =?iso-8859-1?Q?Fr=E9d=E9ric_Vandal?=
        /// </summary>
        private const string _encodedWordsPattern = "\\s*=\\?([^\\?]+)\\?([BbQq])\\?([^\\?]+)\\?=";

        /// <summary>
        /// replaces all the occurences of : =?charset?Q|B?string?= into the specified value.
        /// </summary>
        public static string DecodeEncodedWords(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return Regex.Replace(value, _encodedWordsPattern, DecodeEncodedWord);
        }

        /// <summary>
        /// For a match, 
        /// 
        /// 1-extracts the 3 parts
        /// 2-decodes the third part using the specified encoding in second part (either B for Base64 or Q for QuotedPrintable)
        ///   and interpret the result using the specified charset in first part.
        /// 
        /// </summary>
        private static string DecodeEncodedWord(Match t)
        {
            // t is in the following format:
            //     =?xxxx?y?zzzzzzzzzz?=
            // where xxxx is character set name, like UTF-8, ISO-8859-1,
            // etc.,
            // y is one of B or Q which specifies encoding method,
            // zzzzzzzzz is an encoded data.

            string encodingName = t.Groups[1].Value;   // e.g. ISO-8859-1, ISO-2022-JP, UTF-8, etc.
            string encodingType = t.Groups[2].Value;   // B or Q (case insensitive)
            string encodedData = t.Groups[3].Value;    // encoded data

            TransferEncoding encoding;
            if (encodingType.ToUpper() == "B")
            {
                encoding = TransferEncoding.Base64;
            }
            else
            {
                // replace "_" by "=20"
                encoding = TransferEncoding.QuotedPrintable;
                encodedData = Regex.Replace(encodedData, "_", "=20");
            }

            return DecodeSingleLine(encodedData, encoding, encodingName);
        }
    }
}
