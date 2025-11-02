using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace MithrixWank.Content
{
    public abstract class ContentBase<T> : ContentBase where T : ContentBase<T>
    {
        //This, which you will see on all the -base classes, will allow both you and other modders to enter through any class with this to access internal fields/properties/etc as if they were a member inheriting this -Base too from this class.
        public static T Instance { get; private set; }

        public ContentBase()
        {
            if (Instance != null) throw new InvalidOperationException("Singleton class \"" + typeof(T).Name + "\" inheriting ContentBase was instantiated twice");
            Instance = this as T;
        }
    }

    public abstract class ContentBase
    {
        public abstract string ConfigCategoryString { get; }
        public abstract string ConfigOptionName { get; }

        public abstract string ConfigDescriptionString { get; }

        public ConfigEntry<bool> Enabled { get; private set; }

        protected virtual void ReadConfig(ConfigFile config)
        {
            Enabled = config.Bind<bool>(ConfigCategoryString, ConfigOptionName, true, ConfigDescriptionString);
        }

        internal void Init(ConfigFile config)
        {
            ReadConfig(config);
            if (!Enabled.Value) return;
            Setup();
        }

        protected virtual void Setup()
        {

        }
    }
}
