using System.Collections.Generic;
using UnityEngine;

namespace FPS.Progression
{
    /// <summary>
    /// Local persistent shop data: owned weapons, equipped weapon, selected
    /// player. Uses PlayerPrefs (works fully offline).
    /// </summary>
    public static class ShopProgress
    {
        private const string OwnedKey = "FPS_Shop_Owned";
        private const string EquippedKey = "FPS_Shop_Equipped";
        private const string CoinsKey = "FPS_Shop_Coins";
        private const string PlayerKey = "FPS_Shop_Player";

        public static List<string> OwnedWeapons
        {
            get
            {
                string raw = PlayerPrefs.GetString(OwnedKey, "");
                var list = new List<string>();
                if (string.IsNullOrEmpty(raw))
                    return list;
                string[] parts = raw.Split(',');
                for (int i = 0; i < parts.Length; i++)
                {
                    if (!string.IsNullOrEmpty(parts[i]))
                        list.Add(parts[i]);
                }
                return list;
            }
        }

        public static string EquippedWeapon
        {
            get { return PlayerPrefs.GetString(EquippedKey, ""); }
            private set { PlayerPrefs.SetString(EquippedKey, value); PlayerPrefs.Save(); }
        }

        public static int Coins
        {
            get { return PlayerPrefs.GetInt(CoinsKey, 0); }
            private set { PlayerPrefs.SetInt(CoinsKey, value); PlayerPrefs.Save(); }
        }

        public static string SelectedPlayer
        {
            get { return PlayerPrefs.GetString(PlayerKey, "PlayerDefault"); }
            private set { PlayerPrefs.SetString(PlayerKey, value); PlayerPrefs.Save(); }
        }

        public static bool IsOwned(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId))
                return false;
            return OwnedWeapons.Contains(weaponId);
        }

        public static bool IsEquipped(string weaponId)
        {
            return EquippedWeapon == weaponId;
        }

        public static void BuyWeapon(string weaponId, int price)
        {
            if (IsOwned(weaponId))
                return;
            if (Coins < price)
                return;
            Coins = Coins - price;
            string raw = PlayerPrefs.GetString(OwnedKey, "");
            PlayerPrefs.SetString(OwnedKey, (raw.Length > 0 ? raw + "," : "") + weaponId);
            PlayerPrefs.Save();
        }

        public static void EquipWeapon(string weaponId)
        {
            if (!IsOwned(weaponId))
                return;
            EquippedWeapon = weaponId;
        }

        public static void SelectPlayer(string playerId)
        {
            SelectedPlayer = playerId;
        }

        public static void AddCoins(int amount)
        {
            Coins = Coins + amount;
        }

        public static void GiveDefaultItem(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId))
                return;
            if (IsOwned(weaponId))
                return;
            string raw = PlayerPrefs.GetString(OwnedKey, "");
            PlayerPrefs.SetString(OwnedKey, (raw.Length > 0 ? raw + "," : "") + weaponId);
            PlayerPrefs.Save();
        }
    }
}