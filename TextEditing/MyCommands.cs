
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;

[assembly: CommandClass(typeof(TextEditing.MyCommands))]

namespace TextEditing
{
    public class MyCommands
    {
        [CommandMethod("EditTextExternal")]
        public static void EditTextWithExternalEditor()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;

            var opt = new PromptEntityOptions("\nSelect TEXT/MTEXT:");
            opt.SetRejectMessage("\nInvalid selection: must be TEXT?MTEXT.");
            opt.AddAllowedClass(typeof(DBText), true);
            opt.AddAllowedClass(typeof(MText), true);
            var res = ed.GetEntity(opt);
            if (res.Status != PromptStatus.OK) return;

            using (var tran = dwg.TransactionManager.StartTransaction())
            {
                var textEnt = tran.GetObject(res.ObjectId, OpenMode.ForRead);
                string text = "";
                bool isMText = false;
                if (textEnt is DBText txt)
                {
                    text = txt.TextString;
                }
                else if (textEnt is MText mtxt)
                {
                    text = mtxt.Contents;
                    isMText = true;
                }
                if (!string.IsNullOrEmpty(text))
                {
                    var editedText = EditTextWithNotePadPlus(text, isMText);
                    if (editedText != text)
                    {
                        textEnt.UpgradeOpen();
                        if (textEnt is DBText)
                        {
                            ((DBText)textEnt).TextString=editedText;
                        }
                        else if (textEnt is MText)
                        {
                            ((MText)textEnt).Contents=editedText;
                        }
                    }
                }

                tran.Commit();
            }
        }

        private static string EditTextWithNotePadPlus(string entityText, bool isMText)
        {
            var tempFile=$"C:\\Temp\\{Guid.NewGuid()}.txt";
            string inputText;
            if (isMText)
            {
                inputText = entityText.Replace("\\P", "\r\n");
            }
            else
            {
                inputText = entityText;
            }
            File.WriteAllText(tempFile, inputText);

            var notePadPlus = @"C:\Program Files\Notepad++\notepad++.exe";
            string editedText = inputText;
            Process process = new Process();
            process.StartInfo.FileName = notePadPlus;
            process.StartInfo.Arguments = tempFile;
            process.EnableRaisingEvents = true;
            process.Exited += (o, e) =>
            {
                editedText=File.ReadAllText(tempFile);
                File.Delete(tempFile);
            };

            process.Start();
            process.WaitForExit();
            if (!isMText)
            {
                return editedText.Replace("\n", "").Replace("\r", "");
            }
            else
            {
                return editedText.Replace("\r\n","\\P");
            }
        }

        [CommandMethod("EditTextInternal")]
        public static void EditTextWithInternalEditor()
        {
            var dwg = CadApp.DocumentManager.MdiActiveDocument;
            var ed = dwg.Editor;

            var opt = new PromptEntityOptions("\nSelect TEXT/MTEXT:");
            opt.SetRejectMessage("\nInvalid selection: must be TEXT?MTEXT.");
            opt.AddAllowedClass(typeof(DBText), true);
            opt.AddAllowedClass(typeof(MText), true);
            var res = ed.GetEntity(opt);
            if (res.Status != PromptStatus.OK) return;

            using (var tran = dwg.TransactionManager.StartTransaction())
            {
                var textEnt = tran.GetObject(res.ObjectId, OpenMode.ForRead);
                if (textEnt is DBText txt)
                {
                    ObjectId[] ids = new ObjectId[] { };
                    InplaceTextEditor.Invoke(txt,ref ids);
                }
                else if (textEnt is MText mtxt)
                {
                    InplaceTextEditor.Invoke(mtxt, new InplaceTextEditorSettings());
                }

                tran.Commit();
            }
        }
    }
}
