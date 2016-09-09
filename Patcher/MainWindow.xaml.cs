namespace Patcher
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;

    using Microsoft.Win32;

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string PRE_PATCHER_VERSION = "ldstr      \"1.";
        private const string PATCHER_VERSION = @"[P]";

        private const string PRE_LOAD_MODS = @"call       void Planetbase.GameManager::initQualitySettings()";
        private const string LOAD_MODS = @"call void Planetbase.GameManager::loadMods()";

        private const string GAME_STATE_GAME = @".class public auto ansi beforefieldinit Planetbase.GameStateGame";
        private const string GAME_STATE_GAME_UPDATE = @"update(float32 timeStep) cil managed";
        private const string PRE_UPDATE_MODS = @"ret";
        private const string UPDATE_MODS = @"call void Planetbase.GameManager::updateMods()";

        private const string PRE_MODS_FIELD = @".field public notserialized static class Planetbase.GameManager mInstance";
        private const string MODS_FIELD = @".field public static class [mscorlib]System.Collections.Generic.List`1<class Planetbase.IMod> modList";

        private const string PRE_IMOD = @".class public auto ansi beforefieldinit Planetbase.GameManager";
        private const string IMOD = @".class interface public auto ansi abstract Planetbase.IMod
{
	// Methods
	.method public hidebysig newslot abstract virtual 
		instance void Init () cil managed 
	{
	} // end of method IMod::Init

	.method public hidebysig newslot abstract virtual 
		instance void Update () cil managed 
	{
	} // end of method IMod::Update

} // end of class Planetbase.IMod";

        private const string PRE_MOD_FUNCS = @"} // end of class Planetbase.GameManager";
        private const string MOD_FUNCS = @".method public hidebysig static 
		void loadMods () cil managed 
	{
		// Method begins at RVA 0x20bc
		// Code size 224 (0xe0)
		.maxstack 5
		.locals init (
			[0] string,
			[1] char,
			[2] string[],
			[3] int32,
			[4] class [mscorlib]System.Type[],
			[5] int32,
			[6] class Planetbase.IMod,
			[7] class [mscorlib]System.Exception
		)

		IL_0000: newobj instance void class [mscorlib]System.Collections.Generic.List`1<class Planetbase.IMod>::.ctor()
		IL_0005: stsfld class [mscorlib]System.Collections.Generic.List`1<class Planetbase.IMod> Planetbase.GameManager::modList
		IL_000a: call string Planetbase.Util::getFilesFolder()
		IL_000f: ldsfld char [mscorlib]System.IO.Path::DirectorySeparatorChar
		IL_0014: stloc.1
		IL_0015: ldloca.s 1
		IL_0017: constrained. [mscorlib]System.Char
		IL_001d: callvirt instance string [mscorlib]System.Object::ToString()
		IL_0022: ldstr " + "\"Mods\"" + @"
		IL_0027: call string[mscorlib] System.String::Concat(string, string, string)
        IL_002c: stloc.0
		IL_002d: ldloc.0
		IL_002e: call bool[mscorlib] System.IO.Directory::Exists(string)
        IL_0033: brfalse IL_00df

        IL_0038: ldloc.0
		IL_0039: ldstr " + "\"*.dll\"" + @"
		IL_003e: call string[][mscorlib] System.IO.Directory::GetFiles(string, string)
        IL_0043: stloc.2
		IL_0044: ldc.i4.0
		IL_0045: stloc.3
		IL_0046: br IL_00d6
        // loop start (head: IL_00d6)
        IL_004b: ldloc.2
			IL_004c: ldloc.3
			IL_004d: ldelem.ref
			IL_004e: call class [mscorlib]
        System.Reflection.Assembly[mscorlib] System.Reflection.Assembly::LoadFile(string)
            IL_0053: callvirt instance class [mscorlib]
        System.Type[][mscorlib] System.Reflection.Assembly::GetTypes()
            IL_0058: stloc.s 4
			IL_005a: ldc.i4.0
			IL_005b: stloc.s 5
			IL_005d: br.s IL_00ca
            // loop start (head: IL_00ca)
        IL_005f: ldtoken Planetbase.IMod
                IL_0064: call class [mscorlib]
        System.Type[mscorlib] System.Type::GetTypeFromHandle(valuetype[mscorlib] System.RuntimeTypeHandle)
                IL_0069: ldloc.s 4
				IL_006b: ldloc.s 5
				IL_006d: ldelem.ref
				IL_006e: callvirt instance bool[mscorlib] System.Type::IsAssignableFrom(class [mscorlib]
        System.Type)
				IL_0073: brfalse.s IL_00c4
                .try
                {
            IL_0075: ldloc.s 4
                    IL_0077: ldloc.s 5
                    IL_0079: ldelem.ref
					IL_007a: call object[mscorlib] System.Activator::CreateInstance(class [mscorlib]
        System.Type)
					IL_007f: isinst Planetbase.IMod
                    IL_0084: stloc.s 6
                    IL_0086: ldloc.s 6
                    IL_0088: callvirt instance void Planetbase.IMod::Init()
                    IL_008d: ldsfld class [mscorlib]
        System.Collections.Generic.List`1<class 
        Planetbase.IMod> Planetbase.GameManager::modList
IL_0092: ldloc.s 6
					IL_0094: callvirt instance void class [mscorlib]
        System.Collections.Generic.List`1<class 
        Planetbase.IMod>::Add(!0)
                    IL_0099: leave.s IL_00c4
                } // end .try
				catch [mscorlib]
    System.Exception
				{
					IL_009b: stloc.s 7
					IL_009d: ldstr " + "\"<MOD> \"" + @"
					IL_00a2: ldloc.s 7
					IL_00a4: callvirt instance string[mscorlib] System.Exception::get_Message()
                    IL_00a9: ldstr " + "\"\\nFailed to load type: \"" + @"
					IL_00ae: ldloc.s 4
					IL_00b0: ldloc.s 5
					IL_00b2: ldelem.ref
					IL_00b3: callvirt instance string[mscorlib] System.Type::get_FullName()
                    IL_00b8: call string[mscorlib] System.String::Concat(string, string, string, string)
                    IL_00bd: call void[UnityEngine]
    UnityEngine.Debug::Log(object)
					IL_00c2: leave.s IL_00c4
                } // end handler

IL_00c4: ldloc.s 5
				IL_00c6: ldc.i4.1
				IL_00c7: add
                IL_00c8: stloc.s 5

				IL_00ca: ldloc.s 5
				IL_00cc: ldloc.s 4
				IL_00ce: ldlen
                IL_00cf: conv.i4
                IL_00d0: blt.s IL_005f
            // end loop

IL_00d2: ldloc.3
			IL_00d3: ldc.i4.1
			IL_00d4: add
            IL_00d5: stloc.3

			IL_00d6: ldloc.3
			IL_00d7: ldloc.2
			IL_00d8: ldlen
            IL_00d9: conv.i4
            IL_00da: blt IL_004b
        // end loop

IL_00df: ret
	} // end of method Mod::loadMods

	.method public hidebysig static
        void updateMods() cil managed
{
		// Method begins at RVA 0x21b8
		// Code size 38 (0x26)
		.maxstack 2
		.locals init (
			[0] int32
		)

		IL_0000: ldc.i4.0
		IL_0001: stloc.0
		IL_0002: br.s IL_0018
           // loop start (head: IL_0018)
           IL_0004: ldsfld class [mscorlib]
System.Collections.Generic.List`1<class 
Planetbase.IMod> Planetbase.GameManager::modList
IL_0009: ldloc.0
			IL_000a: callvirt instance !0 class [mscorlib]
System.Collections.Generic.List`1<class 
Planetbase.IMod>::get_Item(int32)
            IL_000f: callvirt instance void
Planetbase.IMod::Update()
			IL_0014: ldloc.0
			IL_0015: ldc.i4.1
			IL_0016: add
            IL_0017: stloc.0

			IL_0018: ldloc.0
			IL_0019: ldsfld class [mscorlib]
System.Collections.Generic.List`1<class 
Planetbase.IMod> Planetbase.GameManager::modList
IL_001e: callvirt instance int32 class [mscorlib]
System.Collections.Generic.List`1<class 
Planetbase.IMod>::get_Count()
            IL_0023: blt.s IL_0004
        // end loop

IL_0025: ret
	} // end of method Mod::updateMods";

        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void FrameworkElement_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Width = this.buttonSelect.ActualWidth + 30 + this.labelDll.ActualWidth
                         + (string.IsNullOrEmpty(this.labelDll.Text) ? 5 : 10);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
                          {
                              Filter = "Assembly-CSharp.dll|Assembly-CSharp.dll",
                              Multiselect = false,
                              Title = "Choose Assembly-CSharp.dll location"
                          };
            if (ofd.ShowDialog().GetValueOrDefault(false))
            {
                this.labelDll.Text = ofd.FileName;
                this.FrameworkElement_OnSizeChanged(this, null);
                //if (File.Exists(this.labelDll.Text + ".bck"))
                //{
                //    this.buttonRestore.Content = "State: Patched! Click to restore!";
                //    this.buttonRestore.IsEnabled = true;
                //}
                //else
                {
                    //this.buttonRestore.Content = "State: Probably Unpatched";
                    this.buttonPatch.IsEnabled = true;
                }
            }
        }

        private async void ButtonBase2_OnClick(object sender, RoutedEventArgs e)
        {
            // Do the stuff
            if (string.IsNullOrEmpty(this.labelDll.Text))
            {
                return;
            }
            if (!File.Exists(this.labelDll.Text))
            {
                return;
            }

            this.buttonSelect.IsEnabled = false;
            this.buttonPatch.IsEnabled = false;
            //this.buttonRestore.IsEnabled = false;

            this.buttonPatch.Content = "Copying working DLL...";

            await this.labelDll.Dispatcher.InvokeAsync(
                () =>
                    {
                        if (File.Exists("Assembly-CSharp.dll"))
                        {
                            File.Delete("Assembly-CSharp.dll");
                        }

                        File.Copy(this.labelDll.Text, "Assembly-CSharp.dll");
                    });

            this.buttonPatch.Content = "Preparing ILDASM...";

            if (!File.Exists("ildasm.exe"))
            {
                using (
                    var ildasm = Application.GetResourceStream(new Uri("ildasm.exe", UriKind.RelativeOrAbsolute)).Stream
                    )
                using (var ildasmfile = new FileStream("ildasm.exe", FileMode.Create))
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

            this.buttonPatch.Content = "Decompiling...";

            var ildasmProc = Process.Start("ildasm.exe", "/out=Assembly-CSharp-orig.il /utf8 Assembly-CSharp.dll");
            await Task.Run(() => ildasmProc.WaitForExit());

            this.buttonPatch.Content = "Injecting IL...";

            Console.WriteLine("BUILDING GRAPH");

            MSILNode rootNode = new MSILNode();
            rootNode.parseString(File.ReadAllText("Assembly-CSharp-orig.il"));

            Console.WriteLine("READ ALL DATA");

            using (var ILStream = new FileStream("Assembly-CSharp-orig.il", FileMode.Open))
            using (var reader = new StreamReader(ILStream, Encoding.UTF8))
            using (var ILStreamNew = new FileStream("Assembly-CSharp.il", FileMode.OpenOrCreate))
            using (var writer = new StreamWriter(ILStreamNew, Encoding.UTF8))
            {
                bool inGameStateGame = false;
                bool inUpdate = false;
                ILStream.Seek(0, SeekOrigin.Begin);
                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();

                    //if (line.Contains("literal "))
                    //{
                    //    line = line.Replace("literal ", "");
                    //}

                    if (line.Contains("private "))
                    {
                        if (line.Contains(".field"))
                            line = line.Replace("private ", "public notserialized ");
                        else
                            line = line.Replace("private ", "public ");
                    }

                    if (line.Contains("family "))
                    {
                        if (line.Contains(".field"))
                            line = line.Replace("family ", "public notserialized ");
                        else
                            line = line.Replace("family ", "public ");
                    }

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

                    // Init mods
                    if (line.Contains(PRE_LOAD_MODS))
                    {
                        await writer.WriteLineAsync(line);
                        await writer.WriteLineAsync(LOAD_MODS);
                        continue;
                    }

                    // Update mods
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

                    // Mod list field
                    if (line.Contains(PRE_MODS_FIELD))
                    {
                        await writer.WriteLineAsync(line);
                        await writer.WriteLineAsync(MODS_FIELD);
                        continue;
                    }

                    // IMod
                    if (line.Contains(PRE_IMOD))
                    {
                        await writer.WriteLineAsync(IMOD);
                        await writer.WriteLineAsync(line);
                        continue;
                    }

                    // Mod Funtions
                    if (line.Contains(PRE_MOD_FUNCS))
                    {
                        await writer.WriteLineAsync(MOD_FUNCS);
                        await writer.WriteLineAsync(line);
                        continue;
                    }

                    if (line.Contains(PRE_PATCHER_VERSION))
                    {
                        line = line.Substring(0, line.Length - 1) + PATCHER_VERSION + "\"";
                    }

                    await writer.WriteLineAsync(line);
                }
            }

            this.buttonPatch.Content = "Preparing ILASM...";

            if (!File.Exists("ilasm.exe"))
            {
                using (
                    var ildasm = Application.GetResourceStream(new Uri("ilasm.exe", UriKind.RelativeOrAbsolute)).Stream)
                using (var ildasmfile = new FileStream("ilasm.exe", FileMode.Create))
                {
                    await ildasm.CopyToAsync(ildasmfile);
                }
            }

            if (!File.Exists("fusion.dll"))
            {
                using (
                    var fusion = Application.GetResourceStream(new Uri("fusion.dll", UriKind.RelativeOrAbsolute)).Stream
                    )
                using (var fusionfile = new FileStream("fusion.dll", FileMode.Create))
                {
                    await fusion.CopyToAsync(fusionfile);
                }
            }

            File.Delete("Assembly-CSharp.dll");

            this.buttonPatch.Content = "Recompiling...";

            var ilasmProc = Process.Start("ilasm.exe", "/dll Assembly-CSharp.il");
            await Task.Run(() => ilasmProc.WaitForExit());

            this.buttonPatch.Content = "Backing up...";

            var bckPath = Path.Combine(Path.GetDirectoryName(this.labelDll.Text), "Assembly-CSharp.dll.bck");
            if (File.Exists(bckPath))
            {
                await this.labelDll.Dispatcher.InvokeAsync(() => { File.Delete(bckPath); });
            }
            if (!File.Exists(bckPath))
            {
                await this.labelDll.Dispatcher.InvokeAsync(() => { File.Copy(this.labelDll.Text, bckPath); });
            }

            this.buttonPatch.Content = "Installing ML...";

            await this.labelDll.Dispatcher.InvokeAsync(
                () =>
                    {
                        if (File.Exists(this.labelDll.Text))
                        {
                            File.Delete(this.labelDll.Text);
                        }

                        File.Move("Assembly-CSharp.dll", this.labelDll.Text);
                    });

            this.buttonPatch.Content = "Creating Mods-Folder...";

            var modsFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Planetbase",
                "Mods");
            if (!Directory.Exists(modsFolder))
            {
                Directory.CreateDirectory(modsFolder);
            }

            this.buttonPatch.Content = "Cleaning up...";

            await Task.Run(
                () =>
                    {
                        //File.Delete("Assembly-CSharp.il");
                        File.Delete("Assembly-CSharp-orig.il");
                        File.Delete("Assembly-CSharp-orig.res");
                        File.Delete("fusion.dll");
                        File.Delete("ilasm.exe");
                        File.Delete("ildasm.exe");
                    });

            this.buttonPatch.Content = "Done!";
        }

        private void ButtonBase3_OnClick(object sender, RoutedEventArgs e)
        {
            File.Delete(this.labelDll.Text);
            File.Move(this.labelDll.Text + ".bck", this.labelDll.Text);
            //this.buttonRestore.Content = "State: Restored to original";
            this.buttonPatch.IsEnabled = true;
            //this.buttonRestore.IsEnabled = false;
        }
    }
}