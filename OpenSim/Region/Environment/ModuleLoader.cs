using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using OpenSim.Framework.Console;
using OpenSim.Region.Environment.Scenes;
using OpenSim.Region.Environment.Interfaces;
using OpenSim.Region.Environment.Modules;

namespace OpenSim.Region.Environment
{
    public class ModuleLoader
    {

        public Dictionary<string, Assembly> LoadedAssemblys = new Dictionary<string, Assembly>();

        public List<IRegionModule> LoadedModules = new List<IRegionModule>();
        public Dictionary<string, IRegionModule> LoadedSharedModules = new Dictionary<string, IRegionModule>();
        
        public ModuleLoader()
        {

        }

        /// <summary>
        /// Should have a module factory?
        /// </summary>
        /// <param name="scene"></param>
        public void CreateDefaultModules(Scene scene, string exceptModules)
        {
            IRegionModule module  = new XferModule();
            InitialiseModule(module, scene);

            module = new ChatModule();
            InitialiseModule(module, scene);
            
            module = new AvatarProfilesModule();
            InitialiseModule(module, scene);

            this.LoadRegionModule("OpenSim.Region.ExtensionsScriptModule.dll", "ExtensionsScriptingModule", scene);

            string lslPath = System.IO.Path.Combine("ScriptEngines", "OpenSim.Region.ScriptEngine.DotNetEngine.dll");
            this.LoadRegionModule(lslPath, "LSLScriptingModule", scene);

        }


        public void LoadDefaultSharedModules(string exceptModules)
        {
            DynamicTextureModule dynamicModule = new DynamicTextureModule();
            this.LoadedSharedModules.Add(dynamicModule.GetName(), dynamicModule);
        }

        public void InitialiseSharedModules(Scene scene)
        {
            foreach (IRegionModule module in this.LoadedSharedModules.Values)
            {
                module.Initialise(scene);
                scene.AddModule(module.GetName(), module); //should be doing this?
            }
        }

        private void InitialiseModule(IRegionModule module, Scene scene)
        {
            module.Initialise(scene);
            scene.AddModule(module.GetName(), module);
            LoadedModules.Add(module);
        }

        /// <summary>
        ///  Loads/initialises a Module instance that can be used by mutliple Regions
        /// </summary>
        /// <param name="dllName"></param>
        /// <param name="moduleName"></param>
        /// <param name="scene"></param>
        public void LoadSharedModule(string dllName, string moduleName)
        {
            IRegionModule module = this.LoadModule(dllName, moduleName);
            if (module != null)
            {
                this.LoadedSharedModules.Add(module.GetName(), module);
            }
        }

        public void LoadRegionModule(string dllName, string moduleName, Scene scene)
        {
            IRegionModule module = this.LoadModule(dllName, moduleName);
            if (module != null)
            {
                this.InitialiseModule(module, scene);
            }
        }

        /// <summary>
        /// Loads a external Module (if not already loaded) and creates a new instance of it.
        /// </summary>
        /// <param name="dllName"></param>
        /// <param name="moduleName"></param>
        /// <param name="scene"></param>
        public IRegionModule LoadModule(string dllName, string moduleName)
        {
            Assembly pluginAssembly = null;
            if (LoadedAssemblys.ContainsKey(dllName))
            {
                pluginAssembly = LoadedAssemblys[dllName];
            }
            else
            {
                pluginAssembly = Assembly.LoadFrom(dllName);
                this.LoadedAssemblys.Add(dllName, pluginAssembly);
            }

            IRegionModule module = null;
            foreach (Type pluginType in pluginAssembly.GetTypes())
            {
                if (pluginType.IsPublic)
                {
                    if (!pluginType.IsAbstract)
                    {
                        Type typeInterface = pluginType.GetInterface("IRegionModule", true);

                        if (typeInterface != null)
                        {
                            module = (IRegionModule)Activator.CreateInstance(pluginAssembly.GetType(pluginType.ToString()));
                            break;
                        }
                        typeInterface = null;
                    }
                }
            }
            pluginAssembly = null;

            if ((module != null ) || (module.GetName() == moduleName))
            {
                return module;
            }

            return null;

        }

        public void PostInitialise()
        {
            foreach (IRegionModule module in this.LoadedSharedModules.Values)
            {
                module.PostInitialise();
            }

            foreach (IRegionModule module in this.LoadedModules)
            {
                module.PostInitialise();
            }
        }

        public void ClearCache()
        {
            this.LoadedAssemblys.Clear();
        }
    }
}
