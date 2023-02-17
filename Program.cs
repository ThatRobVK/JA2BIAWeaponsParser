using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace parser
{
    class Program
    {
        private const float StanceModifier0 = 1f; // Prone
        private const float StanceModifier1 = 0.9f; // Crouched
        private const float StanceModifier2 = 0.3f; // Standing hip
        private const float StanceModifier3 = 0.8f; // Standing aim

        private static FileInfo inputFileInfo = new FileInfo(@"C:\code\parser\weapons.txt");
        private static FileInfo outputFileInfo = new FileInfo(@"C:\code\parser\parsed.csv");

        private static List<WeaponInfo> weapons = new List<WeaponInfo>();

        // Main Method
        static void Main(string[] args)
        {
            if (!inputFileInfo.Exists)
            {
                Console.WriteLine(string.Format("Cannot find file [{0}]", inputFileInfo.FullName));
                return;
            }

            StringBuilder currentWeapon = new StringBuilder();
            string weaponName = "";

            using (StreamReader inputStream = new StreamReader(inputFileInfo.FullName))
            {
                while (inputStream.Peek() >= 0)
                {
                    string line = inputStream.ReadLine().Trim();

                    if (string.IsNullOrEmpty(line) || (line.Length > 3 && line.Substring(0, 3).Equals("//=")))
                    {
                        continue;
                    }
                    else if (line.Length > 3 && line.Substring(0, 3).Equals("// "))
                    {
                        weaponName = line.Substring(3);
                    }
                    else if (line.Length > 6 && line.Substring(0, 6).Equals("Weapon"))
                    {
                        currentWeapon.Clear();
                        currentWeapon.Append(weaponName).Append('\n');
                    }
                    else if (line.Equals("}"))
                    {
                        var weapon = ParseWeapon(currentWeapon.ToString());
                        if (weapon != null)
                        {
                            weapons.Add(weapon);
                        }
                    }
                    else if (!line.Equals("{"))
                    {
                        currentWeapon.Append(line).Append('\n');
                    }
                }

                inputStream.Close();
            }

            using (StreamWriter outputStream = new StreamWriter(outputFileInfo.FullName, false))
            {
                outputStream.WriteLine("Name,Weight,Price,Damage,BestRange,ClipSize,Ammunition,Quality,RPM,GunType,AccuracyProne,AccuracyCrouched,AccuracyStandingHip,AccuracyStandingAim");

                foreach (WeaponInfo weapon in weapons)
                {
                    outputStream.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                        weapon.Name,
                        weapon.Weight,
                        weapon.Price,
                        weapon.Damage,
                        weapon.BestRange,
                        weapon.ClipSize,
                        weapon.Ammunition,
                        weapon.Quality,
                        weapon.RPM,
                        weapon.GunType,
                        Math.Round(weapon.BestRange * weapon.StanceFactor0 * StanceModifier0, 2),
                        Math.Round(weapon.BestRange * weapon.StanceFactor1 * StanceModifier1, 2),
                        Math.Round(weapon.BestRange * weapon.StanceFactor2 * StanceModifier2, 2),
                        Math.Round(weapon.BestRange * weapon.StanceFactor3 * StanceModifier3, 2)
                    ));
                }

                outputStream.Close();
            }

        }

        private static WeaponInfo ParseWeapon(string weaponString)
        {
            WeaponInfo weaponInfo = new WeaponInfo();
            string[] lines = weaponString.Split('\n');

            if (lines.Length > 1)
            {
                string[] nameWords = lines[0].Split(' ');

                // Get hip and prone accuracy if supplied
                bool trimAccHip = false;
                if (nameWords.Length >= 5)
                {
                    string hip = nameWords[nameWords.Length - 3];
                    string acc = nameWords[nameWords.Length - 4];

                    if (acc.Length > 3 && acc.Substring(0, 3).Equals("acc") && hip.Length > 3 && hip.Substring(0, 3).Equals("hip"))
                    {
                        trimAccHip = true;
                    }
                }

                // Build up name again based on whether acc and hip were in the name string
                weaponInfo.Name = string.Join(' ', nameWords, 0, (trimAccHip) ? nameWords.Length - 4 : Math.Max(nameWords.Length - 2, 1));

                for (int i = 1; i < lines.Length; i++)
                {
                    var cleanLine = lines[i].Replace("  ", " ");
                    string[] words = cleanLine.Split(' ');
                    if (words.Length >= 2)
                    {
                        try
                        {
                            switch (words[0])
                            {
                                case "Weight":
                                    weaponInfo.Weight = int.Parse(words[1]);
                                    break;
                                case "Price":
                                    weaponInfo.Price = int.Parse(words[1]);
                                    break;
                                case "Damage":
                                    weaponInfo.Damage = int.Parse(words[1]);
                                    break;
                                case "BestRange":
                                    weaponInfo.BestRange = float.Parse(words[1]);
                                    break;
                                case "ClipSize":
                                    weaponInfo.ClipSize = int.Parse(words[1]);
                                    break;
                                case "Ammunition":
                                    weaponInfo.Ammunition = words[1];
                                    break;
                                case "Quality":
                                    weaponInfo.Quality = int.Parse(words[1]);
                                    break;
                                case "RPM":
                                    weaponInfo.RPM = int.Parse(words[1]);
                                    break;
                                case "GunType":
                                    weaponInfo.GunType = words[1];
                                    break;
                                case "StanceFactor":
                                    switch (words[1])
                                    {
                                        case "0":
                                            weaponInfo.StanceFactor0 = float.Parse(words[2]);
                                            break;
                                        case "1":
                                            weaponInfo.StanceFactor1 = float.Parse(words[2]);
                                            break;
                                        case "2":
                                            weaponInfo.StanceFactor2 = float.Parse(words[2]);
                                            break;
                                        case "3":
                                            weaponInfo.StanceFactor3 = float.Parse(words[2]);
                                            break;
                                    }
                                    break;
                            }
                        }
                        catch (System.Exception)
                        {
                            Console.WriteLine(string.Format("Error on {0}; line: {1}", weaponInfo.Name, lines[i]));
                        }

                    }
                }

                return weaponInfo;
            }

            return null;
        }
    }

    internal class WeaponInfo
    {
        public string Name;
        public int Weight;
        public int Price;
        public int Damage;
        public float BestRange;
        public int ClipSize;
        public string Ammunition;
        public int Quality;
        public int RPM;
        public string GunType;
        public float StanceFactor0 = 1;
        public float StanceFactor1 = 1;
        public float StanceFactor2 = 1;
        public float StanceFactor3 = 1;
    }

}
/*
// 38 S&W acc17.0 hip5.4 0.58 0.186
Weapon (36, HandGun)
{
	Weight 650
  	Price 349 //600
	ResourceId 4205	
	ShotEffectId 5608

	// logic:
	Damage 26
	BestRange 16.0 //14.0
	StanceFactor 0 1.05 //1
	StanceFactor 1 1.14 //0.9
	StanceFactor 2 1.13 //0.9
	StanceFactor 3 1.20 //0.9
	ClipSize 6
	Ammunition 38cal
	Quality 800 //400
	RPM 780 //450
  	GunType Handgun
	Icon 0 112 200 112 40
	Picture 0 8 0 2
	
	
	DisableAttachments
	AnchorPoint 0.0 0.5 -1.9
}
*/