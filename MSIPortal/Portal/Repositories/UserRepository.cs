using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Portal.Models
{
    public class UserRepository
    {
        private MSIPortalEntities db = new MSIPortalEntities();

        public IQueryable<Users> List()
        {
            return db.Users;
        }
        public string GenaratePassword()
        {
            string Sk = "";

            Sk = Convert.ToString(Guid.NewGuid()).Replace(@"-", "")
                      + Convert.ToString(Guid.NewGuid()).Replace(@"-", "");

            Sk = Sk.Substring(0, 10);

            bool poExist = db.Users.Count(p => p.password == Sk) > 0;

            while (poExist)
            {
                Sk = Convert.ToString(Guid.NewGuid()).Replace(@"-", "")
                       + Convert.ToString(Guid.NewGuid()).Replace(@"-", "");

                Sk = Sk.Substring(0, 10);

                poExist = db.Users.Count(p => p.password == Sk) > 0;

            }

            return Encrypt(Sk);
        }

        public string UpdatePassword(int ID, string oldpassword, string newpassword)
        {

            Users user = db.Users.Find(ID);

            string password = "";

            if (user != null)
            {
                if (user.password == Encrypt(oldpassword))
                {
                    user.password = Encrypt(newpassword);
                    user.Active = true;
                    password = newpassword;
                }
                else
                {
                    throw new Exception();
                }

            }

            db.SaveChanges();

            return password;

        }

      

        public void AllowResellerToEdit(int ID, int check)
        {
            Resellers reseller = db.Resellers.Find(ID);

            if (reseller != null)
            {
                try
                {
                    reseller.IsAllowedToEdit = Convert.ToBoolean(check);
                    db.SaveChanges();
                }
                catch
                {
                    throw new Exception();
                }
            }

           
        }

        public string Encrypt(string forencrpyt)
        {
            string encrypted = string.Empty;
            string EncryptionKey = "sds@p!c@550";
            byte[] clearBytes = Encoding.Unicode.GetBytes(forencrpyt);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                     encrypted = Convert.ToBase64String(ms.ToArray());
                }
            }
            return encrypted;
        }

        public string Decrypt(string fordecrypt)
        {
            try
            {
                string EncryptionKey = "sds@p!c@550";
                string decryptedpassword = fordecrypt.Replace(" ", "+");
                byte[] cipherBytes = Convert.FromBase64String(fordecrypt);
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();
                        }
                        decryptedpassword = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
                return decryptedpassword;
            }
            catch
            {
                return fordecrypt;
            }
        }
    }
}