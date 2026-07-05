using System;

namespace BoatStatusHUD
{
    public class ShipProfile
    {
        public string Key;
        public string DisplayName;
        public float MaxCapacity;
        public float MaxFreeboard;
    }

    public class ShipProfileList
    {
        public ShipProfile[] Profiles;
    }
}