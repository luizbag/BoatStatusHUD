using System.Linq;
using System.Reflection;

namespace BoatStatusHUD
{
    public static class PluginInfo
    {
        public static readonly string GUID = ((AssemblyMetadataAttribute)System.Attribute
            .GetCustomAttributes(typeof(BoatStatusHUDPlugin).Assembly, typeof(AssemblyMetadataAttribute))
            .FirstOrDefault(x => ((AssemblyMetadataAttribute)x).Key == "PluginGUID"))?.Value ?? "com.luizbag.sailwind.boatstatushud";

        public static readonly string Name = ((AssemblyMetadataAttribute)System.Attribute
            .GetCustomAttributes(typeof(BoatStatusHUDPlugin).Assembly, typeof(AssemblyMetadataAttribute))
            .FirstOrDefault(x => ((AssemblyMetadataAttribute)x).Key == "PluginName"))?.Value ?? "BoatStatusHUD";

        public static readonly string Version = typeof(BoatStatusHUDPlugin).Assembly
            .GetName().Version.ToString(3);
    }
}
