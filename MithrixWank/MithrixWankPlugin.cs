using BepInEx;
using R2API.Utils;
using MithrixWank.Modules;
using System.Reflection;
using System;
using System.Linq;
using MithrixWank.Content;
using System.Security.Permissions;
using System.Security;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace MithrixWank
{
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInPlugin("com.RiskySleeps.MithrixWank", "MithrixWank", "1.0.3")]
    public class MithrixWankPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Files.PluginInfo = Info;
            new PluginContentPack().Initialize();
            Modules.Assets.LoadAssetBundle();
            LanguageTokens.RegisterLanguageTokens();
            AddToAssembly();
        }

        private void AddToAssembly()
        {
            var fixTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(ContentBase)));

            foreach (var contentType in fixTypes)
            {
                ContentBase content = (ContentBase)Activator.CreateInstance(contentType);
                content.Init(Config);
            }
        }
    }
}
