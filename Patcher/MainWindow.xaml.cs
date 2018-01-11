namespace Patcher
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using Microsoft.Win32;

    //Completely rewritinig thisi is pretty high up on my todo list, but it's good enough for the initial release.

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string PRE_PATCHER_VERSION = "ldstr      \"1.";
        private const string PATCHER_VERSION = @"[P 2.1]";

        private const string PRE_LOAD_MODS = @"call       void Planetbase.GameManager::initQualitySettings()";
        private const string LOAD_MODS = @"call void [PlanetbaseFramework]PlanetbaseFramework.Modloader::LoadMods()";

        private const string GAME_STATE_GAME = @".class public auto ansi beforefieldinit Planetbase.GameStateGame";
        private const string GAME_STATE_GAME_UPDATE = @"update(float32 timeStep) cil managed";
        private const string PRE_UPDATE_MODS = @"ret";
        private const string UPDATE_MODS = @"call void [PlanetbaseFramework]PlanetbaseFramework.Modloader::UpdateMods()";


        public MainWindow()
        {
            InitializeComponent();
        }

        private void FrameworkElement_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Width = buttonSelect.ActualWidth + 30 + labelDll.ActualWidth
                         + (string.IsNullOrEmpty(labelDll.Text) ? 5 : 10);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
                          {
                              Filter = "Assembly-CSharp.dll|Assembly-CSharp.dll",
                              Multiselect = false,
                              Title = "Choose Assembly-CSharp.dll location",
                              InitialDirectory = @"C:\Program Files (x86)\Steam\steamapps\common\Planetbase\Planetbase_Data\Managed"
            };
            if (ofd.ShowDialog().GetValueOrDefault(false))
            {
                labelDll.Text = ofd.FileName;
                FrameworkElement_OnSizeChanged(this, null);
                buttonPatch.IsEnabled = true;
            }
        }

        private async void ButtonBase2_OnClick(object sender, RoutedEventArgs e)
        {
            // Do the stuff
            if (string.IsNullOrEmpty(labelDll.Text))
            {
                return;
            }
            if (!File.Exists(labelDll.Text))
            {
                return;
            }

            buttonSelect.IsEnabled = false;
            buttonPatch.IsEnabled = false;

            buttonPatch.Content = "Copying working DLL...";

            await labelDll.Dispatcher.InvokeAsync(
                () =>
                    {
                        if (File.Exists("Assembly-CSharp.dll"))
                        {
                            File.Delete("Assembly-CSharp.dll");
                        }

                        File.Copy(labelDll.Text, "Assembly-CSharp.dll");
                    });

            buttonPatch.Content = "Preparing ILDASM...";

            if (!File.Exists("ildasm.exe"))
            {
                using (Stream ildasm = Application.GetResourceStream(new Uri("ildasm.exe", UriKind.RelativeOrAbsolute)).Stream)
                using (FileStream ildasmfile = new FileStream("ildasm.exe", FileMode.Create))
                {
                    await ildasm.CopyToAsync(ildasmfile);
                }
            }

            if (File.Exists("Assembly-CSharp.il"))
            {
                File.Delete("Assembly-CSharp.il");
            }

            if (File.Exists("Assembly-CSharp.res"))
            {
                File.Delete("Assembly-CSharp.res");
            }

            if (File.Exists("Assembly-CSharp-orig.il"))
            {
                File.Delete("Assembly-CSharp-orig.il");
            }

            buttonPatch.Content = "Decompiling...";

            Process ildasmProc = Process.Start("ildasm.exe", "/out=Assembly-CSharp-orig.il /utf8 Assembly-CSharp.dll");
            await Task.Run(() => ildasmProc.WaitForExit());

            buttonPatch.Content = "Injecting IL...";

            Console.WriteLine("READ ALL DATA");

            using (FileStream ilStream = new FileStream("Assembly-CSharp-orig.il", FileMode.Open))
            using (StreamReader reader = new StreamReader(ilStream, Encoding.UTF8))
            using (FileStream ilStreamNew = new FileStream("Assembly-CSharp.il", FileMode.OpenOrCreate))
            using (StreamWriter writer = new StreamWriter(ilStreamNew, Encoding.UTF8))
            {
                bool inGameStateGame = false;
                bool inUpdate = false;
                ilStream.Seek(0, SeekOrigin.Begin);
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();

                    if (line.Contains("private "))  //Make private fields public
                    {
                        line = line.Replace("private ", line.Contains(".field") ? "public notserialized " : "public ");
                    }

                    if (line.Contains("family "))   //Cant remember what this does off the top of my head
                    {
                        line = line.Replace("family ", line.Contains(".field") ? "public notserialized " : "public ");
                    }

                    //This section allows for new new ModuleTypes
                    if (line.Contains(".method public hidebysig instance class [UnityEngine]UnityEngine.GameObject "))
                    {
                        string nextLine = await reader.ReadLineAsync();
                        if (nextLine.Contains("loadPrefab(int32 sizeIndex) cil managed"))
                        {
                            line = line.Replace("hidebysig ", "hidebysig newslot virtual ");
                        }

                        await writer.WriteLineAsync(line);
                        await writer.WriteLineAsync(nextLine);
                        continue;
                    }

                    if (line.Contains("instance class [UnityEngine]UnityEngine.GameObject Planetbase.ModuleType::loadPrefab(int32)"))
                    {
                        line = line.Replace("call    ", "callvirt");
                    }

                    //Adds a reference to the framework's DLL
                    if(line.Contains(".assembly extern UnityEngine.UI"))
                    {
                        await writer.WriteLineAsync(line);
                        await writer.WriteLineAsync(await reader.ReadLineAsync());  //Skip 3 lines
                        await writer.WriteLineAsync(await reader.ReadLineAsync());
                        await writer.WriteLineAsync(await reader.ReadLineAsync());

                        await writer.WriteLineAsync(".assembly extern PlanetbaseFramework");
                        await writer.WriteLineAsync("{");
                        await writer.WriteLineAsync("}");
                        
                        continue;
                    }

                    //Allows for mods to be compiled against later version of .Net. Currently working getting >2.0.5.0 features workiing
                    if (line.Contains(".assembly extern mscorlib"))
                    {
                        await writer.WriteLineAsync(line);
                        await writer.WriteLineAsync(await reader.ReadLineAsync());
                        await writer.WriteLineAsync(await reader.ReadLineAsync());

                        line = await reader.ReadLineAsync();

                        line = line.Replace("2:0:5:0", "4:0:0:0");

                        await writer.WriteLineAsync(line);
                        continue;
                    }

                    //Allows the framework to inject new title menu buttons
                    if (line.Contains("setGameStateTitle() cil managed"))
                    {
                        await writer.WriteLineAsync(line);
                        await writer.WriteLineAsync(await reader.ReadLineAsync());
                        await writer.WriteLineAsync(await reader.ReadLineAsync());
                        await writer.WriteLineAsync(await reader.ReadLineAsync());
                        await writer.WriteLineAsync(await reader.ReadLineAsync());
                        await writer.WriteLineAsync(await reader.ReadLineAsync());
                        await writer.WriteLineAsync(await reader.ReadLineAsync());
                        string nextLine = await reader.ReadLineAsync();
                        nextLine = nextLine.Replace("Planetbase.GameStateTitle", "[PlanetbaseFramework]PlanetbaseFramework.GameStateTitleReplacement");
                        await writer.WriteLineAsync(nextLine);
                        continue;
                    }

                    // Calls the LoadMods() method in the framework's modloader
                    if (line.Contains(PRE_LOAD_MODS))
                    {
                        await writer.WriteLineAsync(line);
                        await writer.WriteLineAsync(LOAD_MODS);
                        continue;
                    }

                    // Calls the UpdateMods() method in the framework's modloader
                    if (line.Contains(GAME_STATE_GAME))
                        inGameStateGame = true;
                    if (inGameStateGame && line.Contains(GAME_STATE_GAME_UPDATE))
                        inUpdate = true;
                    if (inUpdate && line.Contains(PRE_UPDATE_MODS))
                    {
                        line = line.Replace("ret", UPDATE_MODS);
                        await writer.WriteLineAsync(line);
                        await writer.WriteLineAsync("ret");

                        inGameStateGame = false;
                        inUpdate = false;
                        continue;
                    }

                    //Patches the version number on the title screen
                    if (line.Contains(PRE_PATCHER_VERSION))
                    {
                        line = line.Substring(0, line.Length - 1) + PATCHER_VERSION + "\"";
                    }

                    await writer.WriteLineAsync(line);
                }
            }

            buttonPatch.Content = "Preparing ILASM...";

            if (!File.Exists("ilasm.exe"))
            {
                using (Stream ildasm = Application.GetResourceStream(new Uri("ilasm.exe", UriKind.RelativeOrAbsolute)).Stream)
                using (FileStream ildasmfile = new FileStream("ilasm.exe", FileMode.Create))
                {
                    await ildasm.CopyToAsync(ildasmfile);
                }
            }

            if (!File.Exists("fusion.dll"))
            {
                using (Stream fusion = Application.GetResourceStream(new Uri("fusion.dll", UriKind.RelativeOrAbsolute)).Stream )
                using (var fusionfile = new FileStream("fusion.dll", FileMode.Create))
                {
                    await fusion.CopyToAsync(fusionfile);
                }
            }

            File.Delete("Assembly-CSharp.dll");

            buttonPatch.Content = "Recompiling...";

            Process ilasmProc = Process.Start("ilasm.exe", "/dll Assembly-CSharp.il");
            await Task.Run(() => ilasmProc.WaitForExit());

            buttonPatch.Content = "Backing up...";

            string bckPath = Path.Combine(Path.GetDirectoryName(labelDll.Text), "Assembly-CSharp.dll.bck");
            if (File.Exists(bckPath))
            {
                await labelDll.Dispatcher.InvokeAsync(() => { File.Delete(bckPath); });
            }
            if (!File.Exists(bckPath))
            {
                await labelDll.Dispatcher.InvokeAsync(() => { File.Copy(labelDll.Text, bckPath); });
            }

            buttonPatch.Content = "Installing ML...";

            await labelDll.Dispatcher.InvokeAsync(
                () =>
                    {
                        if (File.Exists(labelDll.Text))
                        {
                            File.Delete(labelDll.Text);
                        }

                        File.Move("Assembly-CSharp.dll", labelDll.Text);
                    });

            buttonPatch.Content = "Creating Mods-Folder...";

            string modsFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Planetbase",
                "Mods");
            if (!Directory.Exists(modsFolder))
            {
                Directory.CreateDirectory(modsFolder);
            }

            buttonPatch.Content = "Cleaning up...";

            await Task.Run(
                () =>
                    {
                        File.Delete("Assembly-CSharp.il");
                        File.Delete("Assembly-CSharp-orig.il");
                        File.Delete("Assembly-CSharp-orig.res");
                        File.Delete("fusion.dll");
                        File.Delete("ilasm.exe");
                        File.Delete("ildasm.exe");
                    });

            buttonPatch.Content = "Done!";
        }
    }
}