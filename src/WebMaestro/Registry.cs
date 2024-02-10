using Microsoft.Win32;

namespace WebMaestro
{
    static class RegistryHelper
    {
        internal static RegistryKey GetUserRegistryKey()
        {
            string keyPath = string.Format("Software\\{0}\\{1}", "BrightRay", "WebMaestro");
            return Registry.CurrentUser.CreateSubKey(keyPath);
        }

    }
}
