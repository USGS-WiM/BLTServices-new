//------------------------------------------------------------------------------
//----- Security -----------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2016 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   generate salted Hash password 
//
//discussion:   
//  http://www.codeproject.com/Articles/704865/Salted-Password-Hashing-Doing-it-Right
//     

#region Comments
// 7.03.12 - jkn - Created from openrasta.security.RequriesRoleAttribute.cs

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace BLTServices.Security
{
    public static class Cryptography
    {
        #region Constants
        public const int SALT_BYTES = 24;
        #endregion
        #region Methods
        public static string CreateSalt()
        {
            byte[] salt = null;
            try
            {
                salt = new byte[SALT_BYTES];
                using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                {
                    rng.GetBytes(salt);
                }//end using
                return Convert.ToBase64String(salt);
            }
            catch (CryptographicException ex)
            {
                throw new Exception("Random number generator not available", ex);
            }
            catch (ArgumentNullException ex)
            {
                throw new Exception("Invalid argument given to random number generator", ex);
            }
        }
        public static string GenerateSHA256Hash(string password, string salt)
        {
            byte[] bytes;
            byte[] hash;
            try
            {
                bytes = Encoding.UTF8.GetBytes(password + salt);
                using (SHA256Managed sha256hashstring = new SHA256Managed()) { 
                    hash = sha256hashstring.ComputeHash(bytes);                
                }//end using;                
                
                return ByteArrayToHexString(hash);
            }
            catch (CryptographicException ex)
            {
                throw new Exception("Failed to generate hash",ex);
            }
        }
        public static Boolean VerifyPassword(string password, string salt, string goodHash)
        {
            string passwordToVerify = string.Empty;
            try
            {
                passwordToVerify = GenerateSHA256Hash(password, salt);
                return SlowEquals(passwordToVerify, goodHash);

            }
            catch (Exception ex)
            {
                return false;
            }
        
        }
        #endregion
        #region Helper Methods
        private static Boolean SlowEquals(byte[] a, byte[] b)
      {
          int diff = a.Length ^ b.Length;
          for (int i = 0; i < a.Length && i < b.Length; i++)
             diff |= a[i] ^ b[i];
         return diff == 0;
       }
        private static Boolean SlowEquals(string a, string b)
        {
            int diff = a.Length ^ b.Length;
            for (int i = 0; i < a.Length && i < b.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }
        private static string ByteArrayToHexString(byte[] ba){
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach(byte b in ba){
                hex.AppendFormat("{0:x2}",b);
            }//next b

            return hex.ToString();
        }
        private static byte[] HexStringToByteArray(string hex) {
            int numbersChars = hex.Length;
            byte[] bytes = new byte[numbersChars / 2];
            for (int i = 0; i < numbersChars; i += 2) {
                bytes[1 / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }// next i
            return bytes;
        }
        #endregion
       
    }//end class
}//end namespace
