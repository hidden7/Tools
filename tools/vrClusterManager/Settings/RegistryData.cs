using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace vrClusterManager.Settings
{
	public class RegistryData
	{
		private const string BaseRegistryPath = "SOFTWARE\\Pixela Labs\\vrCluster";

		private const string KeyParameters = "Parameters";

		private const string ParamDefaultConfig = "DefaultConfig";
		private const string ParamCommonCmdLineArgs = "CommonArgs";

		public const string appList = "appList";
		public const string configList = "configList";
		public const string isNoSoundName = "isNoSound";
		public const string isAllCoresName = "isAllCores";
		public const string isNoTextureStreamingName = "isNoTextureStreaming";
		public const string isFullscreen = "isFullscreen";
		public const string renderApiName = "renderApi";
		public const string renderModeName = "renderMode";


		public string GetDefaultConfigFile()
		{
			return ReadStringFromRegistry(ParamDefaultConfig);
		}

		public string GetCommonCmdLineArgs()
		{
			return ReadStringValue(KeyParameters, ParamCommonCmdLineArgs);
		}

		private string[] ReadRegistry(string key)
		{
			RegistryKey pixelaKey = Registry.CurrentUser.OpenSubKey(BaseRegistryPath, true);
			if (pixelaKey == null)
			{
				pixelaKey = Registry.CurrentUser.CreateSubKey(baseRegistryPath);
			}

			string[] valueNamesArray = null;
			if (pixelaKey != null)
			{
				RegistryKey regKey = pixelaKey.OpenSubKey(key, true);
				if (regKey == null)
				{
					regKey = pixelaKey.CreateSubKey(key);
				}
				if (regKey != null)
				{
					valueNamesArray = regKey.GetValueNames();
				}
			}

			return valueNamesArray;
		}

		public string ReadStringFromRegistry(string key)
		{

			string[] valueNamesArray = ReadRegistry(key);

			string regKey = null;
			if (valueNamesArray != null && valueNamesArray.Length > 0)
			{
				regKey = valueNamesArray.GetValue(0) as string;
			}

			return regKey;
		}

		public List<string> ReadStringsFromRegistry(string key)
		{

			string[] valueNamesArray = ReadRegistry(key);

			List<string> regKeys = null;
			if (valueNamesArray != null)
			{
				regKeys = new List<string>(valueNamesArray);
			}

			return regKeys;
		}

		public void AddRegistryValue(string key, string value)
		{
			UpdateRegistry(key, value, true);
		}

		public string ReadStringValue(string key, string name)
		{
			return ReadValue(key, name) as string;
		}

		public bool ReadBoolValue(string key, string name)
		{

			return Convert.ToBoolean(ReadValue(key, name));
		}

		public string FindSelectedRegValue(string key)
		{
			string valueName = null;
			try
			{
				RegistryKey regKey = Registry.CurrentUser.OpenSubKey(BaseRegistryPath + "\\" + key, true);
				if (regKey != null)
				{
					string[] valueNamesArray = ReadRegistry(key);
					foreach (string item in valueNamesArray)
					{
						if (Convert.ToBoolean(regKey.GetValue(item)))
						{
							return item;
						}
					}

				}
			}
			catch (Exception /*exception*/)
			{
				//AppLogger.Add("Can't find registry value. EXCEPTION: " + exception.Message);
			}
			return valueName;
		}

		public void RemoveRegistryValue(string key, string value)
		{
			RegistryKey regKey = Registry.CurrentUser.OpenSubKey(BaseRegistryPath + "\\" + key, true);
			regKey.DeleteValue(value);
		}

		public void RemoveAllRegistryValues(string key)
		{
			RegistryKey regKey = Registry.CurrentUser.OpenSubKey(BaseRegistryPath + "\\" + key, true);
			string[] values = regKey.GetValueNames();
			foreach (string value in values)
			{
				regKey.DeleteValue(value);
			}
		}

		public void UpdateRegistry(string key, string name, object value)
		{
			try
			{
				RegistryKey regKey = Registry.CurrentUser.OpenSubKey(BaseRegistryPath + "\\" + key, true);
				if (regKey == null)
				{
					Registry.CurrentUser.CreateSubKey(baseRegistryPath + "\\" + key);
					UpdateRegistry(key, name, value);
				}
				regKey.SetValue(name, value);
			}
			catch (Exception /*exception*/)
			{
				//AppLogger.Add("Can't update registry value. EXCEPTION: " + exception.Message);
			}
		}

		private object ReadValue(string key, string name)
		{
			try
			{
				RegistryKey regKey = Registry.CurrentUser.OpenSubKey(baseRegistryPath + "\\" + key, true);
				return regKey.GetValue(name);
			}
			catch (Exception /*exception*/)
			{
				//AppLogger.Add("Can't read value from registry. EXCEPTION: " + exception.Message);
				return null;
			}
		}
	}
}
