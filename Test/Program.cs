using System.Reflection;

namespace Test {
    internal class Program {
        static void Main() {
            var asm = Assembly.Load("Microsoft.ConfigCI.Commands, Version=10.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            ConfigCIPatcher.Patcher.Patch();
            var t = asm.GetType("Microsoft.SecureBoot.UserConfig.VersionInfo");
            var builder = t.GetMethod("BuildPFNDictionary", BindingFlags.NonPublic | BindingFlags.Static);
            builder.Invoke(null, null);
        }
    }
}
