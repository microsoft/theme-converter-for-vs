﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ThemeConverter {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ThemeConverter.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Show help text..
        /// </summary>
        internal static string Help {
            get {
                return ResourceManager.GetString("Help", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Theme Converter for Visual Studio v0.0.2
        ///A utility that converts VSCode theme json file(s) to VS pkgdef file(s).
        ///For Microsoft internal usage only.
        ///
        ///Arguments:.
        /// </summary>
        internal static string HelpHeader {
            get {
                return ResourceManager.GetString("HelpHeader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The source path of the theme json file or the folder that contains the theme files..
        /// </summary>
        internal static string Input {
            get {
                return ResourceManager.GetString("Input", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not find input file or folder: &quot;{0}&quot;..
        /// </summary>
        internal static string InputNotExistException {
            get {
                return ResourceManager.GetString("InputNotExistException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Launching Visual Studio..
        /// </summary>
        internal static string LaunchingVS {
            get {
                return ResourceManager.GetString("LaunchingVS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No JSON file was found under the given directory..
        /// </summary>
        internal static string NoJSONFoundException {
            get {
                return ResourceManager.GetString("NoJSONFoundException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The output folder path of the converted pkgdef(s). When specified, the converter will save the converted pkgdef(s) to this path. If not specified, the output will be saved under the same directory of the input file/folder. OPTIONAL.
        /// </summary>
        internal static string Output {
            get {
                return ResourceManager.GetString("Output", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Running UpdateConfiguration (this might take a while)..
        /// </summary>
        internal static string RunningUpdateConfiguration {
            get {
                return ResourceManager.GetString("RunningUpdateConfiguration", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The devenv.exe file was not found at &quot;{0}&quot;..
        /// </summary>
        internal static string TargetDevEnvNotExistException {
            get {
                return ResourceManager.GetString("TargetDevEnvNotExistException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The installation path of the target VS. When specified, the converter will patch the converted pkgdef(s) to the target VS, run &quot;updateConfiguration&quot; and launch this VS. (e.g.: &quot;C:\Program Files\Microsoft Visual Studio\2022\Preview&quot;) OPTIONAL.
        /// </summary>
        internal static string TargetVS {
            get {
                return ResourceManager.GetString("TargetVS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The target VS installation directory &quot;{0}&quot; does not exist..
        /// </summary>
        internal static string TargetVSNotExistException {
            get {
                return ResourceManager.GetString("TargetVSNotExistException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Fatal error running &quot;devenv.exe /updateconfiguration&quot;..
        /// </summary>
        internal static string UpdateConfigurationException {
            get {
                return ResourceManager.GetString("UpdateConfigurationException", resourceCulture);
            }
        }
    }
}
