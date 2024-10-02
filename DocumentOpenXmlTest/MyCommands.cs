
using DocumentFormat.OpenXml;

[assembly: CommandClass(typeof(DocumentOpenXmlTest.MyCommands))]
[assembly: ExtensionApplication(typeof(DocumentOpenXmlTest.MyCommands))]

namespace DocumentOpenXmlTest
{
    public class MyCommands : IExtensionApplication
    {
        [CommandMethod("OpenXmlTest")]
        public static void RunMyCommand()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;

            // enum type "PresentationDocumentType" from DocumentFormat.OpenXml.dll
            var obj = DocumentFormat.OpenXml.PresentationDocumentType.Presentation;
            // enum type "FileFormatVersions" from DocumentFormat.OpenXml.Framework.dll
            var fileVersion = FileFormatVersions.Office2016;

            ed.WriteMessage($"\nOpenXml.PresentationDocumentType: {obj}");
            ed.WriteMessage($"\nOpenXml.FileFormatVersion: {fileVersion}");
        }

        public void Initialize()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            if (dwg!=null)
            {
                dwg.Editor.WriteMessage("\nOpenXml Test project loaded.");
            }
        }

        public void Terminate()
        {
        
        }
    }
}
