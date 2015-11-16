using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace System
{


    public class Crypt
    {

        #region ***  Decoders  ***

        private class Decoder
        {
            private char[] chars;

            public Decoder(params char[] ch)
            {
                this.chars = ch;
            }

            public char this[int b]
            {
                get { return chars[b % chars.Length]; }
            }

            public string Decode(byte[] bytes)
            {
                char[] res = new char[bytes.Length];
                for (int i = 0; i < bytes.Length; i++)
                {
                    res[i] = this[bytes[i] % chars.Length];
                }
                return new string(res);
            }
        }

        private static Decoder numDecoder = new Decoder(
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        );

        private static Decoder alfaNumDecoder = new Decoder(
            // 0 i O wiecznie się mylą przy wklepywaniu w klawiaturę

            /*'0',*/ '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j',
            'k', 'l', 'm', 'm', /*'O',*/ 'p', 'q', 'r', 's', 't',
            'u', 'v', 'w', 'x', 'y', 'z'
        );

        #endregion

        public static string ToHexString(byte[] bytes)
        {
            char[] hexDigits = {
                '0', '1', '2', '3', '4', '5', '6', '7',
                '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

            char[] chars = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i];
                chars[i * 2] = hexDigits[b >> 4];
                chars[i * 2 + 1] = hexDigits[b & 0xF];
            }
            return new string(chars);
        }

        //160bit -> 20bytes -> 40HexChars
        public static string SHA1Hash(byte[] data)
        {
            //var hash = new SHA1CryptoServiceProvider().ComputeHash(data);
            var hash = new SHA1Managed().ComputeHash(data);
            return ToHexString(hash);
        }

        //512bit -> 64bytes -> 128HexChars
        public static string SHA512Hash(byte[] data)
        {
            //var hash = new SHA512CryptoServiceProvider().ComputeHash(data);
            var hash = new SHA512Managed().ComputeHash(data);
            return ToHexString(hash);
        }

        private static byte[] Md5Hash(string value)
        {
            return Md5Hash(Encoding.Unicode.GetBytes(value));
            //return Md5Hash(Encoding.Default.GetBytes(value));
        }

        public static byte[] Md5Hash(byte[] data)
        {
            return new MD5CryptoServiceProvider().ComputeHash(data);
        }

        public static string Md5(string value)
        {
            return ToHexString(Md5Hash(value));
        }

        public static string Md5(byte[] bytes)
        {
            return ToHexString(Md5Hash(bytes));
        }

        private static string Trim(string s, int resultLen)
        {
            if (s.Length > resultLen)
                s = s.Substring(0, resultLen);
            return s;
        }

        public static string ChSumAlphaNum(string value)
        {
            return alfaNumDecoder.Decode(Md5Hash(value));
        }

        public static string ChSumAlphaNum(string value, int sumLength)
        {
            return Trim(ChSumAlphaNum(value), sumLength);
        }

        public static string ChSumNum(string value)
        {
            return numDecoder.Decode(Md5Hash(value));
        }

        public static string ChSumNum(string value, int sumLength)
        {
            return Trim(ChSumNum(value), sumLength);
        }

        public static void CreateDsaKeys(string fileName)
        {
            DSA dsa = new DSACryptoServiceProvider();

            StreamWriter writer = new StreamWriter(Path.ChangeExtension(fileName, ".private.xml"));
            writer.WriteLine(dsa.ToXmlString(true));
            writer.Close();

            writer = new StreamWriter(Path.ChangeExtension(fileName, ".public.xml"));
            writer.WriteLine(dsa.ToXmlString(false));
            writer.Close();
        }


        public static bool DsaVerify(byte[] data, string signature, DSACryptoServiceProvider publicKey)
        {
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] signBuff = Convert.FromBase64String(signature);
            if (signBuff.Length != 40)
                return false;
            byte[] hash = sha1.ComputeHash(data);
            return publicKey.VerifySignature(hash, signBuff);
        }

        public static bool DsaVerify(string data, string signature)
        {
            byte[] dataBuff = Encoding.ASCII.GetBytes(data);
            return DsaVerify(dataBuff, signature, DSAProvider.PublicProvider);
        }

        public static string DsaSign(byte[] data, DSACryptoServiceProvider privateKey)
        {
            // DSA.CreateSignature oczekuje 20 bajtów danych, czyli tyle co daje SHA1
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] hash = sha1.ComputeHash(data);
            byte[] signBuff = privateKey.CreateSignature(hash);
            return Convert.ToBase64String(signBuff);
        }

        public static string DsaSign(string data, DSACryptoServiceProvider privateKey)
        {
            byte[] dataBuff = Encoding.ASCII.GetBytes(data);
            return DsaSign(dataBuff, privateKey);
        }

    }
}
