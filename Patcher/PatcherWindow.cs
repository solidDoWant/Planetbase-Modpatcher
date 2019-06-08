using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace Patcher
{
    public partial class PatcherWindow : Form
    {
        public PatcherWindow()
        {
            InitializeComponent();
        }

        private string AssemblyFolder
        {
            get
            {
                if (string.IsNullOrWhiteSpace(AssemblyPathBox.Text))
                    return null;

                return Path.GetDirectoryName(AssemblyPathBox.Text) + Path.DirectorySeparatorChar;
            }
        }

        private string DefaultBackupFile =>
            $"{AssemblyFolder}{Path.GetFileNameWithoutExtension(AssemblyPathBox.Text)}.bak";

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

        private static OpenFileDialog OpenPbFileDialog(string fileName)
        {
            return new OpenFileDialog
            {
                Filter = $@"{fileName}|{fileName}",
                Multiselect = false,
                Title = $@"Select the {fileName} file",
                InitialDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\Planetbase\Planetbase_Data\Managed"
            };
        }

        private static bool BrowseButtonClick(string fileName, TextBox pathBox)
        {
            var assemblyDialog = OpenPbFileDialog(fileName);

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
                var backupFileDialog = OpenPbFileDialog("Assembly-CSharp.bak");

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
                string frameworkUrl =
                    ((dynamic[]) deserializedJson["assets"]).FirstOrDefault(asset =>
                        asset["content_type"].Equals("application/x-zip-compressed"))?["browser_download_url"];

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
                    using (var archive = new ZipArchive(downloadedFileStream, ZipArchiveMode.Read))
                    {
                        foreach (var entry in archive.Entries)
                        {
                            var newFilePath = Path.Combine(AssemblyFolder, entry.FullName);

                            using (var entryStream = entry.Open())
                            using (var extractedFileStream = File.Create(newFilePath))
                            {
                                if (File.Exists(newFilePath)) File.Delete(newFilePath);

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

            var assemblyPath = AssemblyPathBox.Text;

            if (!File.Exists(assemblyPath))
            {
                Fail("Could not find Assembly-CSharp.dll. Please update the file's path and try again.");
                return;
            }

            var frameworkPath = Path.Combine(AssemblyFolder, "PlanetbaseFramework.dll");

            if (!File.Exists(frameworkPath))
            {
                Fail("Could not find PlanetbaseFramework.dll. Please update framework and try again.");
                return;
            }

            var patchedAssemblyPath = Path.Combine(AssemblyFolder, "Assembly-CSharp-Patched.dll");

            if (File.Exists(frameworkPath))
                File.Delete(patchedAssemblyPath);

            //Patch the DLL
            try
            {
                //Backup the file
                if (File.Exists(DefaultBackupFile))
                    File.Delete(DefaultBackupFile);

                File.Copy(assemblyPath, DefaultBackupFile);

                //Patch the game assembly
                PatchBuilder.Patch(
                    assemblyPath,
                    FirstPassPathBox.Text,
                    UnityEnginePathBox.Text,
                    UIPathBox.Text,
                    frameworkPath,
                    patchedAssemblyPath
                );

                //Replace the file
                File.Delete(assemblyPath);
                File.Move(patchedAssemblyPath, assemblyPath);
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
            if (message != null) MessageBox.Show(message);
            SetButtonsEnabled(true);
            ProgressBar.Value = ProgressBar.Minimum;
        }
    }
}