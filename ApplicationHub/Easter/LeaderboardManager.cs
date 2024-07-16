using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ApplicationHub.Easter
{
    public static class LeaderboardManager
    {
        private static readonly string key = @"w+)&1$E@8RVv6U#z";

        public static void EncryptFile(string filePath)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);
            byte[] encryptedBytes = EncryptData(fileBytes);
            File.WriteAllBytes(filePath, encryptedBytes);
        }

        public static void DecryptFile(string filePath)
        {
            byte[] encryptedBytes = File.ReadAllBytes(filePath);
            byte[] decryptedBytes = DecryptData(encryptedBytes);
            File.WriteAllBytes(filePath, decryptedBytes);
        }


        public static Dictionary<string, double> GetLeaderboard(string filePath)
        {
            Dictionary<string, double> leaderboard = new Dictionary<string, double>();

            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                File.Create(filePath).Close();
            }
            else if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    byte[] encryptedData = Convert.FromBase64String(line);

                    byte[] decryptedData = DecryptData(encryptedData);
                    string[] parts = Encoding.UTF8.GetString(decryptedData).Split(',');

                    if (parts.Length == 2 && double.TryParse(parts[1], out double score))
                    {
                        leaderboard.Add(parts[0], score);
                    }
                }
            }

            return leaderboard.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        }



        public static void WriteLeaderboard(Dictionary<string, double> leaderboard, string filePath)
        {
            // Read existing leaderboard data from file
            Dictionary<string, double> existingLeaderboard = GetLeaderboard(filePath);

            // Update existing leaderboard data with new scores
            foreach (var entry in leaderboard)
            {
                if (existingLeaderboard.ContainsKey(entry.Key))
                {
                    double existingScore = existingLeaderboard[entry.Key];
                    if (entry.Value > existingScore)
                    {
                        existingLeaderboard[entry.Key] = entry.Value;
                    }
                }
                else
                {
                    existingLeaderboard.Add(entry.Key, entry.Value);
                }
            }

            // Write updated leaderboard data back to file
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var entry in existingLeaderboard)
                {
                    string data = $"{entry.Key},{entry.Value}";
                    byte[] encryptedData = EncryptData(Encoding.UTF8.GetBytes(data));
                    string encodedData = Convert.ToBase64String(encryptedData);
                    writer.WriteLine(encodedData);
                }
            }
        }



        private static byte[] EncryptData(byte[] data)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.GenerateIV();

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);

                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(data, 0, data.Length);
                    }

                    return msEncrypt.ToArray();
                }
            }
        }

        private static byte[] DecryptData(byte[] encryptedData)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = encryptedData.Take(aesAlg.BlockSize / 8).ToArray();

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(encryptedData.Skip(aesAlg.BlockSize / 8).ToArray()))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (MemoryStream msOutput = new MemoryStream())
                {
                    csDecrypt.CopyTo(msOutput);
                    return msOutput.ToArray();
                }
            }
        }
    }
}
