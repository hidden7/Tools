using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace vrClusterManager.Settings
{
	public static class RegistryData
	{
		private const string BaseRegistryPath = "SOFTWARE\\Pixela Labs\\vrCluster";

		public const string KeyRunParams  = "RunParameters";
		public const string KeyConfigs    = "Configs";
		public const string KeyApps       = "Apps";

		public const string ValDefaultApp        = "DefaultApp";
		public const string ValDefaultConfig     = "DefaultConfig";
		public const string ValCommonCmdLineArgs = "CommonArgs";

		public static bool GetBoolValue(string key, string name)
		{
			return Convert.ToBoolean(ReadValue(key, name));
		}

		public static void SetBoolValue(string key, string name, bool value)
		{
			SetValue(key, name, value);
		}

		public static string GetStringValue(string key, string name)
		{
			return ReadValue(key, name) as string;
		}

		public static void SetStringValue(string key, string name, string value)
		{
			SetValue(key, name, value);
		}

		public static void RemoveRegistryValue(string key, string value)
		{
			try
			{
				RegistryKey regKey = Registry.CurrentUser.OpenSubKey(BaseRegistryPath + "\\" + key, true);
				regKey.DeleteValue(value);
			}
			catch (Exception)
			{
				//todo: logs
			}
		}

		public static void RemoveAllRegistryValues(string key)
		{
			try
			{
				RegistryKey regKey = Registry.CurrentUser.OpenSubKey(BaseRegistryPath + "\\" + key, true);
				string[] values = regKey.GetValueNames();
				foreach (string value in values)
					regKey.DeleteValue(value);
			}
			catch (Exception)
			{
				//todo: logs
			}
		}

		public static List<string> GetValueNames(string key)
		{
			List<string> names = new List<string>();
			try
			{
				RegistryKey regKey = Registry.CurrentUser.OpenSubKey(BaseRegistryPath + "\\" + key, false);
				names = new List<string>(regKey.GetValueNames());
			}
			catch (Exception)
			{
				//todo: logs
			}

			return names;
		}

		private static object ReadValue(string key, string name)
		{
			try
			{
				RegistryKey regKey = Registry.CurrentUser.OpenSubKey(BaseRegistryPath + "\\" + key, false);
				return regKey.GetValue(name);
			}
			catch (Exception /*exception*/)
			{
				//AppLogger.Add("Can't read value from registry. EXCEPTION: " + exception.Message);
				return null;
			}
		}

		private static void SetValue(string key, string name, object value)
		{
			try
			{
				RegistryKey regKey = Registry.CurrentUser.OpenSubKey(BaseRegistryPath + "\\" + key, true);
				regKey.SetValue(name, value);
			}
			catch (Exception /*exception*/)
			{
				//AppLogger.Add("Can't write value to registry. EXCEPTION: " + exception.Message);
			}
		}
	}
}
