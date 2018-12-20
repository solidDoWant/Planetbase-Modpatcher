using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Patcher;

namespace PatcherTest
{
    public partial class PatcherWindow : Form
    {
        private string AssemblyFolder {
            get
            {
                if (string.IsNullOrWhiteSpace(AssemblyPathBox.Text))
                {
                    return null;
                }

                return Path.GetDirectoryName(AssemblyPathBox.Text) + Path.DirectorySeparatorChar;
            }
        } 
        private string DefaultBackupFile => $"{AssemblyFolder}{Path.GetFileNameWithoutExtension(AssemblyPathBox.Text)}.bak";

        public PatcherWindow()
        {
            InitializeComponent();
        }

        private void SelectAssemblyButton_Click(object sender, EventArgs e)
        {
            if (!BrowseButtonClick("Assembly-CSharp.dll", AssemblyPathBox)) return;

            ConditionallyUpdateDependencyBoxes();
        }

        private void AssemblyPathBox_Validating(object sender, CancelEventArgs e)
        {
            ConditionallyUpdateDependencyBoxes();
        }

        private void SelectFirstPassButton_Click(object sender, EventArgs e)
        {
            BrowseButtonClick("Assembly-CSharp-firstpass.dll", FirstPassPathBox);
        }

        private void SelectUnityEngineButton_Click(object sender, EventArgs e)
        {
            BrowseButtonClick("UnityEngine.dll", UnityEnginePathBox);
        }

        private void SelectUIButton_Click(object sender, EventArgs e)
        {
            BrowseButtonClick("UnityEngine.UI.dll", UIPathBox);
        }

        private void ConditionallyUpdateDependencyBoxes()
        {
            if (string.IsNullOrWhiteSpace(AssemblyPathBox.Text)) return;

            UpdateBox("Assembly-CSharp-firstpass.dll", FirstPassPathBox);
            UpdateBox("UnityEngine.dll", UnityEnginePathBox);
            UpdateBox("UnityEngine.UI.dll", UIPathBox);

            void UpdateBox(string fileName, TextBox updateBox)
            {
                var filePath = AssemblyFolder + fileName;

                if (!File.Exists(filePath)) return;

                updateBox.Text = filePath;
                updateBox.Enabled = false;
            }
        }

        private static OpenFileDialog OpenPBFileDialog(string fileName)
        {
            return new OpenFileDialog
            {
                Filter = $"{fileName}|{fileName}",
                Multiselect = false,
                Title = $"Select the {fileName} file",
                InitialDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\Planetbase\Planetbase_Data\Managed"
            };
        }

        private static bool BrowseButtonClick(string fileName, TextBox pathBox)
        {
            var assemblyDialog = OpenPBFileDialog(fileName);

            if (assemblyDialog.ShowDialog() != DialogResult.OK) return false;

            pathBox.Text = assemblyDialog.FileName;

            return true;
        }

        private void RestoreButton_Click(object sender, EventArgs e)
        {
            //Setup the form
            ProgressBar.Value = 0;
            SetButtonsEnabled(false);

            if (!RestoreAssemblyCSharp())
            {
                SetButtonsEnabled(true);
                return;
            }

            SetButtonsEnabled(true);
            ProgressBar.Value = ProgressBar.Maximum;
        }

        private bool RestoreAssemblyCSharp()
        {
            //Look for the default backup file
            var backupFile = DefaultBackupFile;

            if (!File.Exists(DefaultBackupFile))
            {
                var backupFileDialog = OpenPBFileDialog("Assembly-CSharp.bak");

                if (backupFileDialog.ShowDialog() != DialogResult.OK) return false;

                backupFile = backupFileDialog.FileName;
            }

            try
            {
                if (File.Exists(AssemblyPathBox.Text))
                {
                    File.Delete(AssemblyPathBox.Text);
                }
                else
                {
                    AssemblyPathBox.Text =
                        $"{Path.Combine(Path.GetDirectoryName(backupFile), Path.GetFileNameWithoutExtension(backupFile))}.dll";
                    ConditionallyUpdateDependencyBoxes();
                }

                File.Copy(backupFile, AssemblyPathBox.Text);
            }
            catch (IOException)
            {
                Fail($"{AssemblyPathBox.Text} is currently in use, cannot restore.");
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                Fail(
                    $"Process not authorized to access {AssemblyPathBox.Text}. Please run as administrator and try again.");
                return false;
            }

            return true;
        }

        private bool ValidateAssemblyCSharpFileOrDirectory()
        {
            if (string.IsNullOrWhiteSpace(AssemblyPathBox.Text))
            {
                MessageBox.Show("Please enter a path for the restored Assembly-CSharp.dll file.", "Invalid path",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetButtonsEnabled(true);
                return false;
            }

            if (File.Exists(AssemblyPathBox.Text) || Directory.Exists(AssemblyPathBox.Text)) return true;

            MessageBox.Show("Please enter a valid path for the restored Assembly-CSharp.dll file.", "Invalid path",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        //TODO make this non-blocking
        private void UpdateButton_Click(object updateSender, EventArgs updateArgs)
        {
            //Setup the form
            ProgressBar.Value = 0;
            SetButtonsEnabled(false);

            if (!ValidateAssemblyCSharpFileOrDirectory()) return;

            var downloadFilePath = $"{AssemblyFolder}PlanetbaseFramework.zip";

            //Setup the web client
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; //Stupid .NET
            using (var client = new WebClient())
            {
                client.Headers.Add("user-agent", "a potato");
                client.DownloadProgressChanged += (o, args) => ProgressBar.Value = args.ProgressPercentage;
                client.DownloadFileCompleted += CompletedCallback;

                //Download the manifest to get the latest framework
                dynamic deserializedJson = new JavaScriptSerializer().DeserializeObject(
                    client.DownloadString(
                        "https://api.github.com/repos/soliddowant/planetbase-framework/releases/latest"));
                string frameworkUrl = //TODO make fewer assumptions here
                    ((dynamic[]) deserializedJson["assets"]).FirstOrDefault(asset =>
                        asset["content_type"].Equals("application/x-zip-compressed"))["browser_download_url"];

                //Download and extract the file
                if (File.Exists(downloadFilePath)) File.Delete(downloadFilePath);
                client.DownloadFileTaskAsync(frameworkUrl, downloadFilePath);
            }

            //Callback to jump to once the download as completed
            void CompletedCallback(object downloadSender, AsyncCompletedEventArgs downloadE)
            {
                try
                {
                    if (downloadE.Error != null) throw downloadE.Error;

                    using (var downloadedFileStream = new FileStream(downloadFilePath, FileMode.Open))
                    using(var archive = new ZipArchive(downloadedFileStream, ZipArchiveMode.Read))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            var newFilePath = AssemblyFolder + entry.FullName;
                            if (File.Exists(newFilePath)) File.Delete(newFilePath);

                            using (var entryStream = entry.Open())
                            using(var extractedFileStream = File.Create(newFilePath))
                            {
                                entryStream.CopyTo(extractedFileStream);
                                extractedFileStream.Close();
                            }
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    Fail($"Failed to extract file: Not authorized to write to {downloadFilePath}");
                    return;
                }

                SetButtonsEnabled(true);
                ProgressBar.Value = ProgressBar.Maximum;
            }
        }

        private void SetButtonsEnabled(bool value)
        {
            //Select buttons
            SelectAssemblyButton.Enabled = value;
            SelectFirstPassButton.Enabled = value;
            SelectUnityEngineButton.Enabled = value;
            SelectUIButton.Enabled = value;

            //Action buttons
            RestoreButton.Enabled = value;
            UpdateButton.Enabled = value;
            PatchButton.Enabled = value;
        }

        private void PatchButton_Click(object sender, EventArgs e)
        {
            //Setup the form
            ProgressBar.Value = 0;
            SetButtonsEnabled(false);

            if (!File.Exists(AssemblyPathBox.Text))
            {
                Fail("Could not find Assembly-CSharp.dll. Please update the file's path and try again.");
                return;
            }

            var frameworkPath = $"{AssemblyFolder}PlanetbaseFramework.dll";

            if (!File.Exists(frameworkPath))
            {
                Fail("Could not find PlanetbaseFramework.dll. Please update framework and try again.");
                return;
            }

            //Patch the DLL
            try
            {
                //Backup the file
                if(File.Exists(DefaultBackupFile)) File.Delete(DefaultBackupFile);
                File.Copy(AssemblyPathBox.Text, DefaultBackupFile);

                //Add folders to resolver path
                var resolver = new PBResolver(FirstPassPathBox.Text, UnityEnginePathBox.Text, UIPathBox.Text);
                resolver.AddSearchDirectory(Path.GetDirectoryName(AssemblyFolder));

                //Add reference to framework
                using (var planetbaseModule = ModuleDefinition.ReadModule(AssemblyPathBox.Text, new ReaderParameters { AssemblyResolver = resolver }))
                using (var frameworkAssembly = AssemblyDefinition.ReadAssembly(frameworkPath, new ReaderParameters { AssemblyResolver = resolver }))
                {
                    frameworkAssembly.Name.Version = new Version(0, 0, 0, 0);
                    var frameworkModule = frameworkAssembly.MainModule;

                    var gameManagerType =
                        planetbaseModule.Types.First(type => type.FullName.Equals("Planetbase.GameManager"));

                    planetbaseModule.AssemblyReferences.Add(frameworkAssembly.Name);

                    //Add call to mod loader initialization method
                    var gameMangerConstructor = gameManagerType.Methods.First(method =>
                        method.FullName.Equals("System.Void Planetbase.GameManager::.ctor()"));

                    var modLoadType =
                        frameworkModule.Types.First(type => type.FullName.Equals("PlanetbaseFramework.Modloader"));
                    var loadModMethodDefinition = modLoadType.Methods.First(method => method.Name.Equals("LoadMods"));

                    var planetbaseScopeLoadModMethodDefinition =
                        planetbaseModule.ImportReference(loadModMethodDefinition);

                    var loadModInstruction = Instruction.Create(OpCodes.Call, planetbaseScopeLoadModMethodDefinition);

                    gameMangerConstructor.Body.Instructions.Insert(gameMangerConstructor.Body.Instructions.Count - 1,
                        loadModInstruction);

                    //Add call to mod loader update method
                    var updateMethod = gameManagerType.Methods.First(method => method.Name.Equals("update"));

                    var updateModMethodDefinition =
                        modLoadType.Methods.First(method => method.Name.Equals("UpdateMods"));

                    var planetbaseScopeUpdateModMethodDefinition =
                        planetbaseModule.ImportReference(updateModMethodDefinition);

                    var updateModInstruction =
                        Instruction.Create(OpCodes.Call, planetbaseScopeUpdateModMethodDefinition);

                    updateMethod.Body.Instructions.Insert(updateMethod.Body.Instructions.Count - 1,
                        updateModInstruction);

                    //Update the version number
                    var definitionsType = planetbaseModule.Types.First(type => type.Name.Equals("Definitions"));
                    definitionsType.Fields.First(field => field.Name.Equals("VersionNumber")).Constant += "[P 2.3]";

                    //Make all fields public
                    var fields = planetbaseModule.Types
                        .Where(type => type.Namespace.Equals("Planetbase") && type.HasFields) //Get all types
                        .SelectMany(type => type.Fields).ToList(); //Get all fields

                    var privateFields = fields.Where(field => !field.IsPublic);

                    foreach (var field in privateFields)
                    {
                        field.IsPublic = true;
                    }

                    foreach (var field in fields)
                    {
                        field.HasConstant = false;
                    }

                    //Make all methods public
                    var privateMethods = planetbaseModule.Types
                        .Where(type => type.Namespace.Equals("Planetbase") && type.HasMethods) //Get all types
                        .SelectMany(type => type.Methods) //Get all methods
                        .Where(methods => !methods.IsPublic); //Get all private methods

                    foreach (var method in privateMethods)
                    {
                        method.IsPublic = true;
                    }

                    //Add hooks for prefab replacement
                    var moduleTypeLoadPrefabMethod = planetbaseModule.GetType("Planetbase", "ModuleType").Methods
                        .First(method => method.Name.Equals("loadPrefab"));
                    moduleTypeLoadPrefabMethod.IsVirtual = true;

                    //Add hooks for new menu item
                    var setGameStateTitleMethod = gameManagerType.Methods
                        .First(method => method.Name.Equals("setGameStateTitle"));

                    var titleGameStateReplacementConstructor = frameworkAssembly.MainModule.Types.First(type =>
                            type.FullName.Equals("PlanetbaseFramework.GameStateTitleReplacement")).Methods
                        .First(method => method.Name.Equals(".ctor"));

                    var planetbaseScopeTitleGameStateReplacementConstructor =
                        planetbaseModule.ImportReference(titleGameStateReplacementConstructor);

                    setGameStateTitleMethod.Body.Instructions
                            .First(instruction => instruction.OpCode == OpCodes.Newobj).Operand =
                        planetbaseScopeTitleGameStateReplacementConstructor;

                    //Remove bad references
                    var mscorlib4Reference = planetbaseModule.AssemblyReferences.First(assembly =>
                        assembly.Name.Equals("mscorlib") && assembly.Version.Equals(new Version(4, 0, 0, 0)));
                    planetbaseModule.AssemblyReferences.Remove(mscorlib4Reference);

                    var planetbaseFrameworkReference =
                        planetbaseModule.AssemblyReferences.First(reference =>
                            reference.Name.Equals("Assembly-CSharp"));
                    planetbaseModule.AssemblyReferences.Remove(planetbaseFrameworkReference);

                    //Set the .NET version to 4.0.0.0
                    var mscorlibReference =
                        planetbaseModule.AssemblyReferences.First(assembly =>
                            assembly.Name.Equals("mscorlib") && assembly.Version.Equals(new Version(2, 0, 5, 0)));
                    mscorlibReference.Version = new Version(4, 0, 0, 0);
                    mscorlibReference.PublicKeyToken = null;

                    //Save the file
                    planetbaseModule.Write($"{AssemblyFolder}Assembly-CSharp-Patched.dll");
                }

                //Replace the file
                File.Delete(AssemblyPathBox.Text);
                File.Move($"{AssemblyFolder}Assembly-CSharp-Patched.dll", AssemblyPathBox.Text);
            }
            catch (InvalidOperationException)
            {
                Fail(
                    "Failed to find type in Planetbase Assembly-CSharp.dll, PlanetbaseFramework.dll, or mscorlib v4.0.0.0");

                RestoreAssemblyCSharp();

                return;
            }

            SetButtonsEnabled(true);
            ProgressBar.Value = ProgressBar.Maximum;
            ProgressBar.Text = "Done!";
        }

        private void Fail(string message)
        {
            if(message != null) MessageBox.Show(message);
            SetButtonsEnabled(true);
            ProgressBar.Value = ProgressBar.Minimum;
        }
    }
}
