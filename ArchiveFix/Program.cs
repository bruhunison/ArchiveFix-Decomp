using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using RageLib.GTA5.Archives;
using RageLib.GTA5.ArchiveWrappers;
using RageLib.GTA5.Cryptography;
using RageLib.GTA5.Cryptography.Helpers;
using RageLib.Helpers;

namespace ArchiveFix
{
	internal class Program
	{
		private static bool IsInvokedFromConsole => GetConsoleProcessList(new uint[2], 2u) > 1;

		private static void Main(string[] args)
		{
			try
			{
				Console.OutputEncoding = Encoding.Unicode;
				Func<int, string> getArg = (int idx) => (args.Length > idx) ? args[idx] : string.Empty;
				Dictionary<string, Action> dictionary = new Dictionary<string, Action>
				{
					{
						"fix",
						delegate
						{
							FixArchive(getArg(1));
						}
					},
					{ "fetch", FetchKeys },
					{ "donotuse_really", BuildHashTables }
				};
				string text = getArg(0).ToLowerInvariant();
				if (!string.IsNullOrWhiteSpace(text) && File.Exists(args[0]))
				{
					Console.WriteLine("Ah Nigga, don't hate me 'cause I'm beautiful, nigga. maybe if you got rid of that old Yee-Yee ass haircut you got you'd get some bitches on your dick.");
					Console.WriteLine("oh, better yet maybe Tanisha'll call your-dog ass if she ever stop fucking with that brain surgeon or lawyer she fucking with.");
					Console.WriteLine("Niggga.");
					Console.WriteLine();
					if (File.Exists(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "gtav_ng_encrypt_luts.dat")))
					{
						getArg = (int idx) => args[0];
						text = "fix";
					}
					else
					{
						Console.WriteLine("... you'll have to run the `fetch' command first, though. You have 0 cryptokeys!");
						Console.WriteLine();
					}
				}
				Action value;
				if (!dictionary.TryGetValue(text, out value))
				{
					ConsoleColor foregroundColor = Console.ForegroundColor;
					Console.Write(string.Format("Invalid command `{0}'. Try one of these instead: {1} (without brackets, pretty please with big ", text, string.Join(" .:. ", dictionary.Keys.Select((string a) => $"[{a}]"))));
					Console.ForegroundColor = ConsoleColor.DarkRed;
					Console.Write("\ud83c\udf52");
					Console.ForegroundColor = foregroundColor;
					Console.WriteLine(" on top)");
					Console.WriteLine();
					Console.WriteLine($"For help, contact one of the following:\n * the administrator of the machine called {Environment.MachineName}\n * whoever is called {Environment.UserName} (what a dumb name, right?)\n * the vendor of this software (Affluent Fix Productions, Ltd.)\n * your culture's deity (if applicable)\n * law enforcement (outside the U.S. and Canada)\n * Arxan Technologies at 855-99-ARXAN (855-992-7926, toll-free from within the North American Numbering Plan).");
					Console.WriteLine();
					Console.WriteLine("The team at Affluent Fix Productions Ltd. thanks you for purchasing this tool.");
					Console.ForegroundColor = ConsoleColor.Blue;
					Console.Write("Affluent Fix™");
					Console.ForegroundColor = foregroundColor;
				}
				else
				{
					value();
				}
			}
			finally
			{
				if (!IsInvokedFromConsole)
				{
					Console.WriteLine();
					Console.WriteLine("Press the `any' key to exit.");
					Console.ReadKey();
				}
			}
		}

		private static void FetchKeys()
		{
			Process process = Process.GetProcessesByName("GTA5").FirstOrDefault();
			bool flag = false;
			if (process == null)
			{
				process = Process.GetProcessesByName("FiveM").FirstOrDefault((Process a) => !string.IsNullOrWhiteSpace(a.MainWindowTitle) && a.MainWindowTitle.Contains("Auto V"));
				flag = true;
				if (process == null)
				{
					process = Process.GetProcessesByName("FiveReborn").FirstOrDefault((Process a) => !string.IsNullOrWhiteSpace(a.MainWindowTitle) && a.MainWindowTitle.Contains("Auto V"));
					if (process == null)
					{
						Console.WriteLine("Hey, mate, listen up. There's a time for reason, and a time for stupidity.");
						Console.WriteLine("Now is neither. Run Grand Theft Auto V. Then try doing whatever it is you were doing again.");
						return;
					}
					Console.WriteLine("mumble, mumble, you're using a FiveM ripoff, good for you I guess, you better own a license to the game, or you should be executed by firing squad...");
				}
				else
				{
					Console.WriteLine("FiveM, eh? didn't that get shut down?");
				}
			}
			string text = process.MainModule.FileName;
			if (flag)
			{
				text = Path.Combine((from a in File.ReadAllLines(Path.Combine(Path.GetDirectoryName(text), "CitizenFX.ini"))
					where a.StartsWith("IVPath=")
					select a.Split('=').Last()).FirstOrDefault(), "GTA5.exe");
				Console.WriteLine();
			}
			if (!File.Exists(text))
			{
				Console.WriteLine($"hey, {text} does not exist. why?");
				return;
			}
			Console.WriteLine($"Reading a few easy things in {process.ProcessName} ({process.Id}) - please wait a few moments...");
			Console.WriteLine("Any output from this process is not endorsed by Affluent Fix.");
			Console.WriteLine();
			if (!File.Exists(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\gtav_ng_key.dat"))
			{
				GTA5Constants.PC_NG_KEYS = ProcessHashFind(process, GTA5HashConstants.PC_NG_KEY_HASHES, 272);
				if (GTA5Constants.PC_NG_KEYS == null)
				{
					Console.WriteLine("Again, Affluent Fix takes no responsibility for any loss of life that may follow from global thermonuclear war.");
					return;
				}
				Console.WriteLine(" ng keys found!");
				CryptoIO.WriteNgKeys(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\gtav_ng_key.dat", GTA5Constants.PC_NG_KEYS);
			}
			else
			{
				Console.WriteLine("... you already had them? well, sorry then.");
			}
			Console.WriteLine();
			Console.WriteLine("The next line is endorsed by Affluent Fix.. keeping the reds away!");
			Console.WriteLine($"Finding the hard as can be shizzle in {text}... this'll take a while! Don't panic, we're cats!");
			Console.WriteLine();
			GTA5Constants.Generate(File.ReadAllBytes(text));
			Console.WriteLine();
			Console.WriteLine("That's that nonsense dealt with! Go crack some omelettes!");
		}

		private static byte[][] ProcessHashFind(Process process, IList<byte[]> hashes, int length = 32)
		{
			IntPtr baseAddress = process.MainModule.BaseAddress;
			int num = ReadProcess<int>(process.Handle, baseAddress + 60);
			int num2 = ReadProcess<int>(process.Handle, baseAddress + num + 80);
			byte[][] array = new byte[hashes.Count][];
			byte[] array2 = new byte[2097152];
			for (int i = 20971520; i < num2; i += array2.Length)
			{
				IntPtr lpNumberOfBytesRead;
				ReadProcessMemory(process.Handle, baseAddress + i, array2, array2.Length, out lpNumberOfBytesRead);
				Console.Write(".");
				using (MemoryStream memoryStream = new MemoryStream(array2))
				{
					foreach (var item in from a in HashSearch.SearchHashes((Stream)memoryStream, (IList<byte[]>)GTA5HashConstants.PC_NG_KEY_HASHES, 272).Select((byte[] value, int idx) => new
						{
							Value = value,
							Index = idx
						})
						where a.Value != null
						select a)
					{
						array[item.Index] = item.Value;
						Console.WriteLine($" found {item.Index}!");
					}
					Console.Write(".");
				}
				if (array.Count((byte[] a) => a == null) == 0)
				{
					break;
				}
			}
			if (array.Count((byte[] a) => a == null) > 0)
			{
				Console.WriteLine("We didn't find some product, man! We'll be ruined!");
				return null;
			}
			return array;
		}

		private unsafe static T ReadProcess<T>(IntPtr hProcess, IntPtr address)
		{
			int num = Marshal.SizeOf<T>();
			byte[] array = new byte[num];
			IntPtr lpNumberOfBytesRead;
			ReadProcessMemory(hProcess, address, array, num, out lpNumberOfBytesRead);
			fixed (byte* value = array)
			{
				return Marshal.PtrToStructure<T>(new IntPtr(value));
			}
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

		private static void FixArchive(string packName)
		{
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				GTA5Constants.LoadFromPath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
			}
			catch (FileNotFoundException)
			{
				Console.WriteLine("Not all cryptokeys are present. Try generating them together with some gobblegums! The `fetch' command is there for a reason.");
				return;
			}
			catch (Exception arg)
			{
				Console.WriteLine($"Error loading crypto keys. {arg}");
				return;
			}
			if (string.IsNullOrWhiteSpace(packName))
			{
				Console.WriteLine("Uh, buddy, you have to specify a file name!");
				return;
			}
			try
			{
				RageArchiveWrapper7 val = RageArchiveWrapper7.Open(packName);
				try
				{
					if ((int)val.archive_.Encryption != 0)
					{
						Console.WriteLine("This packfile is already encrypted - what are you trying to do? That literally maketh no sense!");
						return;
					}
					val.archive_.Encryption = (RageArchiveEncryption7)2;
					val.Flush();
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
				Console.WriteLine($"Done. Modified packfile {packName} to be encrypted using platform key data.");
				Console.WriteLine($"Do note the encryption is dependent on the file name - if it's not called {Path.GetFileName(packName)} it will not decrypt anywhere.");
			}
			catch (FileNotFoundException ex2)
			{
				Console.WriteLine($"Oops - there's no file by the name of {ex2.FileName}. Better try that again!");
			}
			catch (Exception arg2)
			{
				Console.WriteLine($"Failed performing tasks. {arg2}");
			}
		}

		private static void BuildHashTables()
		{
			Console.WriteLine(string.Format("{0}'{1} warranty is now void. Affluent Fix Productions Ltd. is not responsible for bricked devices, dead SD cards, thermonuclear war, or you getting fired because the alarm app failed.", Environment.UserName, Environment.UserName.EndsWith("s") ? "" : "s"));
			Console.WriteLine($"This product has disavowed {Environment.MachineName}. Press the right tabular key to continue.");
			Console.WriteLine();
			if (Console.ReadKey(true).Key != ConsoleKey.Tab)
			{
				Console.WriteLine("Override protocol failed. Goodbye.");
				return;
			}
			Console.WriteLine($"Systematic override activated for {Environment.UserName} on {Environment.MachineName}. Prepare for unforeseen consequences.");
			try
			{
				using (FileStream fileStream = File.OpenRead("L:\\tdt\\gtav_ng_key.dat"))
				{
					using (SHA1CryptoServiceProvider sHA1CryptoServiceProvider = new SHA1CryptoServiceProvider())
					{
						Console.WriteLine("Locating prime data for planetary annihilation.");
						Console.WriteLine();
						byte[] array = new byte[272];
						for (int i = 0; i < 101; i++)
						{
							fileStream.Read(array, 0, array.Length);
							byte[] source = sHA1CryptoServiceProvider.ComputeHash(array);
							Console.WriteLine(string.Format("new byte[] {{ {0} }},", string.Join(", ", source.Select((byte a) => "0x" + a.ToString("X2")))));
						}
					}
				}
				Console.WriteLine();
				Console.WriteLine("... and so be it.");
			}
			catch (FileNotFoundException)
			{
				Console.WriteLine("Dimensional rift opened. Your planet is now nullified.");
			}
		}

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern uint GetConsoleProcessList(uint[] ProcessList, uint ProcessCount);
	}
}
