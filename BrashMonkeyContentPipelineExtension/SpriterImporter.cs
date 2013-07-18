/*==========================================================================
 * Project: BrashMonkeyContentPipelineExtension
 * File: SpriterImporter.cs
 *
 *==========================================================================
 * Author:
 * Geoff "NowSayPillow" Lodder
 *==========================================================================*/

using System.IO;
using System.Xml.Linq;

using Microsoft.Xna.Framework.Content.Pipeline;

namespace BrashMonkeyContentPipelineExtension {
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to import a file from disk into the specified type, TImport.
    /// 
    /// This should be part of a Content Pipeline Extension Library project.
    /// 
    /// TODO: change the ContentImporter attribute to specify the correct file
    /// extension, display name, and default processor for this importer.
    /// </summary>
    [ContentImporter(".SCML", DisplayName = "SCML - BrashMonkey Spriter Importer", DefaultProcessor = "SpriterProcessor")]
    public class SpriterImporter : ContentImporter<XDocument> {
        public override XDocument Import(string p_filename, ContentImporterContext p_context) {
            XDocument l_xmlDoc = XDocument.Load(p_filename);

            l_xmlDoc.Document.Root.Add(new XElement("File", new XAttribute("name", Path.GetFileName(p_filename)), new XAttribute("path", Path.GetDirectoryName(p_filename))));

            return l_xmlDoc;
        }
    }
}
