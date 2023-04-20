using System;
using System.Collections.Generic;
using System.Linq;
//using System.Data;
using System.Text;
using System.Xml;
using Landis.Library.Metadata;
using Landis.Core;
using Landis.Utilities;
using System.IO;
using Flel = Landis.Utilities;

namespace Landis
{
    public static class CoreMetadataHandler
    {

        public static CoreMetadata coreMetadata { get; set; }

        private static string log;

        public static string Log
        {
            get { return log; }
        }

        public static void InitializeMetadata(string coreVersion, Scenario scenario, IUserInterface ui)
        {
            List<string> coreInputs = GetCoreInputs(scenario);
            List<CoreExtensionMetadata> data = GetMetadataFromScenario(scenario);

            coreMetadata = new CoreMetadata(coreVersion, (uint)scenario.RandomNumberSeed, scenario.StartTime, scenario.EndTime, data, coreInputs);

            //---------------------------------------
            MetadataProvider mp = new MetadataProvider(coreMetadata);
            mp.WriteMetadataToXMLFile("Metadata", "LANDIS-II v" + coreMetadata.Version, "LANDIS-II v" + coreMetadata.Version);
            CreateLog(coreVersion, scenario);

            ui.WriteLine("Core metadata:");
            ui.WriteLine(Log);
        }

        private static void CreateLog(string coreVersion, Scenario scenario)
        {
            log = string.Empty;

            log += "LANDIS-II v" + coreVersion + ", Seed: " + scenario.RandomNumberSeed + ", ";
            log += scenario.EcoregionsMap + ", " + scenario.Ecoregions + ", " + scenario.Species + System.Environment.NewLine;

            log += "Succession:" + System.Environment.NewLine +  scenario.Succession.Info.Name + " Version: " + scenario.Succession.Info.Version + ", Input: " + scenario.Succession.InitFile + System.Environment.NewLine;
            log += "Disturbance Extensions:" + System.Environment.NewLine;

            foreach (var extension in scenario.Disturbances)
            {
                log += extension.Info.Name + " Version: " + extension.Info.Version + ", Input: " + extension.InitFile + System.Environment.NewLine;
            }

            log += "Other Extensions:" + System.Environment.NewLine;

            foreach (var extension in scenario.OtherExtensions)
            {
                log += extension.Info.Name + " Version: " + extension.Info.Version + ", Input: " + extension.InitFile + System.Environment.NewLine;
            }
        }

        private static void WriteMetadataToLog(IUserInterface ui)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode metadataNode = doc.CreateElement("landisMetadata");
            metadataNode.AppendChild(coreMetadata.Get_XmlNode(doc));
            ui.WriteLine("Core MetaData:");
            ui.WriteLine(metadataNode.OuterXml);
        }

        private static List<string> GetCoreInputs(Scenario scenario)
        {
            List<string> inputs = new List<string>();
            inputs.Add(scenario.EcoregionsMap);
            inputs.Add(scenario.Ecoregions);
            inputs.Add(scenario.Species);

            return inputs;
        }

        private static List<CoreExtensionMetadata> GetMetadataFromScenario(Scenario scenario)
        {
            List<CoreExtensionMetadata> metadata = new List<CoreExtensionMetadata>();

            metadata.Add(new CoreExtensionMetadata(scenario.Succession.Info.Name, scenario.Succession.Info.Version, new List<string>() { scenario.Succession.InitFile }));

            

            foreach(var extension in scenario.Disturbances)
            {
                metadata.Add(new CoreExtensionMetadata(extension.Info.Name, extension.Info.Version, new List<string>() { extension.InitFile }));
            }

            foreach(var extension in scenario.OtherExtensions)
            {
                metadata.Add(new CoreExtensionMetadata(extension.Info.Name, extension.Info.Version, new List<string>() { extension.InitFile }));
            }

            return metadata;
        }

        public static void CreateDirectory(string path)
        {
            //Require.ArgumentNotNull(path);
            path = path.Trim(null);
            if (path.Length == 0)
                throw new ArgumentException("path is empty or just whitespace");

            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Flel.Directory.EnsureExists(dir);
            }

            //return new StreamWriter(path);
            return;
        }
    }
}
