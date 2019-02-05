﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NARFO_BE
{
    public class encryption
    {/*
        public static String sha256_hash(string value)
        {
            StringBuilder Password = new StringBuilder();

            using (var hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Password.Append(b.ToString("x2"));
            }

            return Password.ToString();
        }*/
        public static byte[] SaltBytes { get; private set; }

       

        public static string ComputeHash(string plainText, byte[] salt)
        {
            //var hash = 256;
            int minSaltLength = 4, maxSaltLength = 16;

            byte[] saltBytes = null;
            if (salt != null)
            {
                saltBytes = salt;
            }
            else
            {
                Random r = new Random();
                int SaltLength = r.Next(minSaltLength, maxSaltLength);
                SaltBytes = new byte[SaltLength];
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                rng.GetNonZeroBytes(SaltBytes);
                rng.Dispose();
            }

            byte[] plainData = ASCIIEncoding.UTF8.GetBytes(plainText);
            byte[] plainDataWithSalt = new byte[plainData.Length + SaltBytes.Length];

            for (int x = 0; x < plainData.Length; x++)
                plainDataWithSalt[x] = plainData[x];
            for (int n = 0; n < SaltBytes.Length; n++)
                plainDataWithSalt[plainData.Length + n] = SaltBytes[n];

            byte[] hashValue = null;

          
            SHA256Managed sha = new SHA256Managed();
            hashValue = sha.ComputeHash(plainDataWithSalt);
            sha.Dispose();
                   

            byte[] result = new byte[hashValue.Length + SaltBytes.Length];
            for (int x = 0; x < hashValue.Length; x++)
                result[x] = hashValue[x];
            for (int n = 0; n < SaltBytes.Length; n++)
                result[hashValue.Length + n] = SaltBytes[n];

            return Convert.ToBase64String(result);
        }

        public static bool Confirm(string plainText, string hashValue)
        {
            
            byte[] hashBytes = Convert.FromBase64String(hashValue);
            int hashSize = 32;

            byte[] saltBytes = new byte[hashBytes.Length - hashSize];

            for (int x = 0; x < saltBytes.Length; x++)
                saltBytes[x] = hashBytes[hashSize + x];

            string newHash = ComputeHash(plainText, saltBytes);

            return (hashValue == newHash);
        }
    }
}
