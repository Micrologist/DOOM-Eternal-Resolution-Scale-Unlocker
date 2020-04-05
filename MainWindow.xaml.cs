using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace DE_ResScale_Unlocker
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		globalKeyboardHook kbHook = new globalKeyboardHook();
		Process process;

		DeepPointer scaleSettingsDP;
		IntPtr scaleSettingsPtr;

		DeepPointer minResSettingDP;
		IntPtr minResSettingPtr;

		bool scaleEnabled = true;

		float minimumRS = 0.01f;

		float[] onePercentSettings = new float[32] { 1.0f, 0.968f, 0.936f, 0.904f, 0.872f, 0.84f, 0.808f, 0.776f, 0.744f, 0.712f, 0.68f, 0.648f, 0.616f, 0.584f, 0.552f, 0.52f, 0.488f, 0.456f, 0.424f, 0.392f, 0.36f, 0.328f, 0.296f, 0.264f, 0.232f, 0.2f, 0.168f, 0.136f, 0.104f, 0.072f, 0.04f, 0.01f };

		public MainWindow()
		{
			InitializeComponent();
			kbHook.KeyDown += KbHook_KeyDown;
			kbHook.KeyUp += KbHook_KeyUp;
			kbHook.HookedKeys.Add(System.Windows.Forms.Keys.F10);
		}



		private void buttonTest_Click(object sender, RoutedEventArgs e)
		{
			if (!Hook())
				return;

			minimumRS = (float.Parse(minTB.Text) / 100f);

			List<float> floatList = new List<float>();
			for (int i = 0; i < 30; i++)
			{
				floatList.Add((float)(1 - (i+1) * 0.032));
			}

			byte[] onePercentBytes = new byte[32 * 4];
			Buffer.BlockCopy(onePercentSettings, 0, onePercentBytes, 0, onePercentBytes.Length);

			process.VirtualProtect(scaleSettingsPtr, 32 * 4, MemPageProtect.PAGE_READWRITE);
			process.WriteBytes(scaleSettingsPtr, onePercentBytes);
			process.WriteBytes(minResSettingPtr, FloatToBytes(minimumRS));

			scaleEnabled = true;
			System.Windows.Forms.MessageBox.Show("Settings applied, please resize your game window (or toggle fullscreen with Alt+Enter) for the changes to take effect.", "Success");

		}

		public void KbHook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == System.Windows.Forms.Keys.F10)
			{
				if (!Hook())
				{
					e.Handled = true;
					return;
				}

				if (scaleEnabled)
					process.WriteBytes(minResSettingPtr, FloatToBytes(1.0f));
				else
					process.WriteBytes(minResSettingPtr, FloatToBytes(minimumRS));

				scaleEnabled = !scaleEnabled;
					
			}
			e.Handled = true;
		}

		public void KbHook_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			e.Handled = true;
		}

		public byte[] FloatToBytes(float f)
		{
			byte[] output = new byte[4];
			float[] floatArray = new float[1] { f };
			Buffer.BlockCopy(floatArray, 0, output, 0, 4);
			return output;
		}

		public bool Hook()
		{
			List<Process> processList = Process.GetProcesses().ToList().FindAll(x => x.ProcessName.Contains("DOOMEternalx64vk"));
			if (processList.Count == 0)
			{
				process = null;
				return false;
			}
			process = processList[0];
			if (process.HasExited)
				return false;
			SetPointersByModuleSize(process.MainModule.ModuleMemorySize);
			minResSettingDP.DerefOffsets(process, out minResSettingPtr);
			scaleSettingsDP.DerefOffsets(process, out scaleSettingsPtr);
			return true;
		}

		public void SetPointersByModuleSize(int moduleSize)
		{
			if (moduleSize == 507191296 || moduleSize == 515133440 || moduleSize == 510681088) // STEAM VERSION
			{
				scaleSettingsDP = new DeepPointer("DOOMEternalx64vk.exe", 0x2626E70);
				minResSettingDP = new DeepPointer("DOOMEternalx64vk.exe", 0x5BF22F4);
			}
			else if (moduleSize == 450445312 || moduleSize == 444944384) // BETHESDA VERSION
			{
				scaleSettingsDP = new DeepPointer("DOOMEternalx64vk.exe", 0x25F2FB0);
				minResSettingDP = new DeepPointer("DOOMEternalx64vk.exe", 0x5BB4874);
			}
			else //UNKNOWN GAME VERSION
			{
				System.Windows.Forms.MessageBox.Show("This game version is not supported.", "Unsupported Game Version");
				process = null;
			}
		}

	}
}
