﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;
//using Microsoft.Win32;
using vrClusterManager.Config;
using vrClusterManager.Settings;

namespace vrClusterManager
{
	public class AppRunner : INotifyPropertyChanged
	{

		// net
		public static int nodeListenerPort = 41000;
		public const string ArgListenerPort = "listener_port=";

		public const string LogCategoriesList = "logCategoriesList.conf";

		public const string registryPath = "SOFTWARE\\Pixela Labs\\vrCluster";

		//Log file name
		public const string logFileName = "logFilename.log";

		public static string logLevels = "";

		private const string logFilenamePrefix = "LogUVR_";

		private const string temporaryZipDir = @"\__dirForZIP\";

		// cluster commands
		private const string cCmdStart = "start ";  // [space] is required here
		private const string cCmdKill = "kill ";
		private const string cCmdStatus = "status";

		// run application params\keys
		private const string uvrParamStatic = " -uvr_cluster -nosplash";

		private const string uvrParamConfig = " uvr_cfg=";     // note:  no need [-] before it
		private const string uvrParamLogFilename = " log=";         // note:  no need [-] before it
		private const string uvrParamCameraDefault = " uvr_camera=";  // note:  no need [-] before it

		// switches
		private const string uvrParamOpenGL3 = " -opengl3";
		private const string uvrParamOpenGL4 = " -opengl4";
		private const string uvrParamStereo = " -quad_buffer_stereo";
		private const string uvrParamNoSound = " -nosound";
		private const string uvrParamFixedSeed = " -fixedseed";
		private const string uvrParamNoWrite = " -nowrite";
		private const string uvrParamFullscreen = " -fullscreen";
		private const string uvrParamWindowed = " -windowed";
		private const string uvrParamForceLogFlush = " -forcelogflush";
		private const string uvrParamNoTextureStreaming = " -notexturestreaming";
		private const string uvrParamUseAllAvailableCores = " -useallavailablecores";


		private Dictionary<string, string> _renderApiParams = new Dictionary<string, string>
		{
			{"OpenGL3", " -opengl3" },
			{"OpenGL4", " -opengl4" },
			{"DirectX 11", " -dx11" }
		};
		public Dictionary<string, string> renderApiParams
		{
			get { return _renderApiParams; }
			set { Set(ref _renderApiParams, value, "renderApiParams"); }
		}

		//Selected OpenGL parameter
		private KeyValuePair<string, string> _selectedRenderApiParam;
		public KeyValuePair<string, string> selectedRenderApiParam
		{
			get { return _selectedRenderApiParam; }
			set
			{
				Set(ref _selectedRenderApiParam, value, "selectedRenderApiParam");
				RegistrySaver.UpdateRegistry(RegistrySaver.paramsList, RegistrySaver.renderApiName, value.Key);
				GenerateCmdStartApp();
			}
		}

		private Dictionary<string, string> _renderModeParams = new Dictionary<string, string>
		{
			{"Mono", " -uvr_dev_mono" },
			{"Frame sequential", " -quad_buffer_stereo" },
			{"Side-by-side", " -uvr_dev_side_by_side" },
			{"Top-bottom", " -uvr_dev_top_bottom" }
		};
		public Dictionary<string, string> renderModeParams
		{
			get { return _renderModeParams; }
			set { Set(ref _renderModeParams, value, "renderModeParams"); }
		}

		//Selected OpenGL parameter
		private KeyValuePair<string, string> _selectedRenderModeParam;
		public KeyValuePair<string, string> selectedRenderModeParam
		{
			get { return _selectedRenderModeParam; }
			set
			{
				Set(ref _selectedRenderModeParam, value, "selectedRenderModeParam");
				RegistrySaver.UpdateRegistry(RegistrySaver.paramsList, RegistrySaver.renderModeName, value.Key);
				GenerateCmdStartApp();
			}
		}

		private bool _isUseAllCores;
		public bool isUseAllCores
		{
			get { return _isUseAllCores; }
			set
			{
				Set(ref _isUseAllCores, value, "isUseAllCores");
				RegistrySaver.UpdateRegistry(RegistrySaver.paramsList, RegistrySaver.isAllCoresName, value);
				GenerateCmdStartApp();
			}
		}

		private bool _isFixedSeed;
		public bool isFixedSeed
		{
			get { return _isFixedSeed; }
			set
			{
				Set(ref _isFixedSeed, value, "isFixedSeed");
				RegistrySaver.UpdateRegistry(RegistrySaver.paramsList, RegistrySaver.isFixedSeedName, value);
				GenerateCmdStartApp();
			}
		}

		private bool _isNotextureStreaming;
		public bool isNotextureStreaming
		{
			get { return _isNotextureStreaming; }
			set
			{
				Set(ref _isNotextureStreaming, value, "isNotextureStreaming");
				RegistrySaver.UpdateRegistry(RegistrySaver.paramsList, RegistrySaver.isNoTextureStreamingName, value);
				GenerateCmdStartApp();
			}
		}

		private bool _isLogEnabled;
		public bool isLogEnabled
		{
			get { return _isLogEnabled; }
			set
			{
				Set(ref _isLogEnabled, value, "isLogEnabled");
				GenerateCmdStartApp();
			}
		}

		private bool _isForceLogFlush;
		public bool isForceLogFlush
		{
			get { return _isForceLogFlush; }
			set
			{
				Set(ref _isForceLogFlush, value, "isForceLogFlush");
				GenerateCmdStartApp();
			}
		}

		private bool _isLogZip;
		public bool isLogZip
		{
			get { return _isLogZip; }
			set
			{ Set(ref _isLogZip, value, "isLogZip"); }
		}

		private bool _isLogRemove;
		public bool isLogRemove
		{
			get { return _isLogRemove; }
			set { Set(ref _isLogRemove, value, "isLogRemove"); }
		}

		//Applications list
		private List<string> _applications;
		public List<string> applications
		{
			get { return _applications; }
			set { Set(ref _applications, value, "applications"); }
		}

		//Configs list
		private List<string> _configs;
		public List<string> configs
		{
			get { return _configs; }
			set { Set(ref _configs, value, "configs"); }
		}

		//Cameras list
		private List<string> _cameras = new List<string>()
		{
			"camera_static",
			"camera_dynamic",
			"camera_custom_1",
			"camera_custom_2",
			"camera_custom_3",
			"camera_custom_4",
			"camera_custom_5",
			"camera_custom_6",
			"camera_custom_7",
			"camera_custom_8",
			"camera_custom_9"
		};
		public List<string> cameras
		{
			get { return _cameras; }
			set { Set(ref _cameras, value, "cameras"); }
		}

		//Log categories list
		private List<LogCategory> _logCategories;
		public List<LogCategory> logCategories
		{
			get { return _logCategories; }
			set { Set(ref _logCategories, value, "logCategories"); }
		}

		//Log file name
		private string _logFile;
		public string logFile
		{
			get { return _logFile; }
			set
			{
				Set(ref _logFile, value, "logFile");
				GenerateCmdStartApp();
			}
		}

		//Additional command line params
		private string _additionalParams;
		public string additionalParams
		{
			get { return _additionalParams; }
			set
			{
				Set(ref _additionalParams, value, "additionalParams");
				RegistrySaver.UpdateRegistry(RegistrySaver.paramsList, RegistrySaver.additionalParamsName, value);
				GenerateCmdStartApp();
			}
		}

		//Command line string
		string cmd;

		//Command line string with app path
		private string _commandLine;
		public string commandLine
		{
			get { return _commandLine; }
			set { Set(ref _commandLine, value, "commandLine"); }
		}

		//Selected Application
		private string _selectedApplication;
		public string selectedApplication
		{
			get { return _selectedApplication; }
			set
			{
				Set(ref _selectedApplication, value, "selectedApplication");
				GenerateCmdStartApp();
			}
		}

		//Selected Config file
		private string _selectedConfig;
		public string selectedConfig
		{
			get { return _selectedConfig; }
			set
			{
				Set(ref _selectedConfig, value, "selectedConfig");
				GenerateCmdStartApp();
				//runningConfig = new Config();
				//Parser.Parse(_selectedConfig, runningConfig);
				//SetSelectedConfig();
			}
		}

		//private Config runningConfig;

		//Selected Camera
		private string _selectedCamera;
		public string selectedCamera
		{
			get { return _selectedCamera; }
			set
			{
				Set(ref _selectedCamera, value, "selectedCamera");
				RegistrySaver.UpdateRegistry(RegistrySaver.paramsList, RegistrySaver.curCamera, value);
				GenerateCmdStartApp();
			}
		}

		public enum ClusterCommandType
		{
			Run,
			Kill,
			Status,

			COUNT
		}

		public AppRunner()
		{

			InitOptions();
			InitConfigLists();
			InitLogCategories();
			logFile = logFileName;
		}

		//Implementation of INotifyPropertyChanged method for TwoWay binding
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnNotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		//Set property with OnNotifyPropertyChanged call
		protected void Set<T>(ref T field, T newValue, string propertyName)
		{
			field = newValue;
			OnNotifyPropertyChanged(propertyName);
		}

		private List<string> ReadConfigFile(string confFilePath)
		{
			List<string> infoList = new List<string>();

			try
			{
				string[] lines = System.IO.File.ReadAllLines(confFilePath);
				infoList = lines.ToList();

				AppLogger.Add("Read file [" + confFilePath + "] finished.");
			}
			catch (Exception exception)
			{
				AppLogger.Add("Can't read file [" + confFilePath + "]. EXCEPTION: " + exception.Message);
			}

			return infoList;
		}

		//Reloading all config lists
		private void InitConfigLists()
		{

			applications = RegistrySaver.ReadStringsFromRegistry(RegistrySaver.appList);
			AppLogger.Add("Applications loaded successfully");
			configs = RegistrySaver.ReadStringsFromRegistry(RegistrySaver.configList);
			SetSelectedConfig();
			AppLogger.Add("Configs loaded successfully");
			AppLogger.Add("List of Active nodes loaded successfully");

			try
			{
				selectedCamera = cameras.First(x => x == RegistrySaver.ReadStringValue(RegistrySaver.paramsList, RegistrySaver.curCamera));
			}
			catch (Exception)
			{
				selectedCamera = cameras.SingleOrDefault(x => x == "camera_dynamic");
			}
		}

		private void InitLogCategories()
		{
			List<string> logCats = ReadConfigFile(LogCategoriesList);
			if (logCategories == null)
			{
				logCategories = new List<LogCategory>();
			}
			else
			{
				logCategories.Clear();
			}
			foreach (string logCat in logCats)
			{
				logCategories.Add(new LogCategory(logCat));
			}
		}

		public void SetSelectedConfig()
		{
			selectedConfig = string.Empty;
			string selected = RegistrySaver.FindSelectedRegValue(RegistrySaver.configList);
			if (!string.IsNullOrEmpty(selected))
			{
				selectedConfig = configs.Find(x => x == selected);

			}
		}

		private void InitOptions()
		{
			try
			{
				selectedCamera = cameras.First(x => x == RegistrySaver.ReadStringValue(RegistrySaver.paramsList, RegistrySaver.curCamera));
			}
			catch (Exception)
			{
				selectedCamera = cameras.SingleOrDefault(x => x == "camera_dynamic");
			}

			try
			{
				selectedRenderApiParam = renderApiParams.First(x => x.Key == RegistrySaver.ReadStringValue(RegistrySaver.paramsList, RegistrySaver.renderApiName));
			}
			catch (Exception)
			{
				selectedRenderApiParam = renderApiParams.SingleOrDefault(x => x.Key == "OpenGL3");
			}

			try
			{
				selectedRenderModeParam = renderModeParams.First(x => x.Key == RegistrySaver.ReadStringValue(RegistrySaver.paramsList, RegistrySaver.renderModeName));
			}
			catch (Exception)
			{
				selectedRenderApiParam = renderModeParams.SingleOrDefault(x => x.Key == "Mono");
			}


			additionalParams = RegistrySaver.ReadStringValue(RegistrySaver.paramsList, RegistrySaver.additionalParamsName);
			isUseAllCores = RegistrySaver.ReadBoolValue(RegistrySaver.paramsList, RegistrySaver.isAllCoresName);
			isFixedSeed = RegistrySaver.ReadBoolValue(RegistrySaver.paramsList, RegistrySaver.isFixedSeedName);
			isNotextureStreaming = RegistrySaver.ReadBoolValue(RegistrySaver.paramsList, RegistrySaver.isNoTextureStreamingName);
			AppLogger.Add("Application Options initialized");
		}

		//Generating command line for the App
		private void GenerateCmdStartApp()
		{
			string appPath = selectedApplication;
			string confString = uvrParamConfig + selectedConfig;

			// switches
			string swRenderApi = "";
			string swRenderMode = "";
			string swFixedSeed = "";
			//string swFullscreen = uvrParamFullscreen;
			string swNoTextureStreaming = "";
			string swUseAllAvailableCores = "";

			swRenderApi = selectedRenderApiParam.Value;
			swRenderMode = selectedRenderModeParam.Value;
			swFixedSeed = uvrParamFixedSeed;
			swNoTextureStreaming = (isNotextureStreaming) ? uvrParamNoTextureStreaming : "";
			swUseAllAvailableCores = (isUseAllCores) ? uvrParamUseAllAvailableCores : "";


			// logging params
			string swNoWrite = (isLogEnabled) ? "" : uvrParamNoWrite;

			string swForceLogFlush = "";
			string paramLogFilename = "";
			string logLevelsSetup = "";

			if (isLogEnabled)
			{
				swForceLogFlush = (isForceLogFlush) ? uvrParamForceLogFlush : "";
				paramLogFilename = uvrParamLogFilename + logFile;
				logLevelsSetup = logLevels;
			}


			// camera by default
			string paramDefaultCamera = "";
			if (selectedCamera != null)
			{
				paramDefaultCamera = uvrParamCameraDefault + selectedCamera;
			}

			// additional params

			// cmd
			cmd = confString + paramDefaultCamera + swRenderApi + swRenderMode + uvrParamStatic + swFixedSeed
								 + swNoTextureStreaming + swUseAllAvailableCores + swForceLogFlush + swNoWrite
								 + paramLogFilename + " " + additionalParams + logLevelsSetup;
			if (isLogEnabled)
			{
				cmd = cmd + logLevels;
			}

			// set value
			commandLine = appPath + cmd;

			//return cmd;
		}

		public void GenerateLogLevelsString()
		{
			string resString = String.Empty;

			resString = " -LogCmds=\"";
			foreach (LogCategory item in logCategories)
			{
				if (item.isChecked)
				{
					resString += item.id + " " + item.value.ToString() + ", ";
				}
			}

			resString += "\"";
			logLevels = resString;
			GenerateCmdStartApp();
		}

		private List<ClusterNode> GetClusterNodes()
		{
			VRConfig runningConfig = new VRConfig();
			return Parser.Parse(selectedConfig, runningConfig).clusterNodes;
		}

		public void RunCommand()
		{
			ClusterCommand(ClusterCommandType.Run, GetClusterNodes());
		}

		public void KillCommand()
		{
			ClusterCommand(ClusterCommandType.Kill, GetClusterNodes());
		}

		public void StatusCommand()
		{
			ClusterCommand(ClusterCommandType.Status, GetClusterNodes());
		}

		private void ClusterCommand(ClusterCommandType ccType, List<ClusterNode> nodes)
		{
			// get all nodes address

			if (nodes.Count == 0)
			{
				return;
			}

			// gen.command for cluster nodes
			string commandCmd = "";

			switch (ccType)
			{
				case ClusterCommandType.Run:
					commandCmd = cCmdStart + selectedApplication;
					break;

				case ClusterCommandType.Kill:
					commandCmd = cCmdKill;
					break;

				case ClusterCommandType.Status:
					commandCmd = cCmdStatus;
					break;
			}

			// send cmd for each node
			string cl = string.Empty;
			foreach (ClusterNode node in nodes)
			{
				string windowCommand = string.Empty;
				string fullscreenParam = uvrParamFullscreen;
				if (node.isWindowed)
				{
					windowCommand = windowCommand + " WinX=" + node.winX + " WinY=" + node.winY + " ResX=" + node.resX + " ResY=" + node.resY;
					fullscreenParam = uvrParamWindowed;
				}

				if (ccType == ClusterCommandType.Run)
				{
					cl = " uvr_node=" + node.id + windowCommand + cmd + fullscreenParam;

				}
				string clusterCmd = commandCmd + cl;
				SendDaemonCommand(node.address, clusterCmd);
			}



		}

		private async void SendDaemonCommand(string nodeAddress, string cmd)
		{
			TcpClient nodeClient = new TcpClient();

			try
			{
				await nodeClient.ConnectAsync(nodeAddress, nodeListenerPort);
				NetworkStream networkStream = nodeClient.GetStream();
				StreamWriter clientStreamWriter = new StreamWriter(networkStream);

				if (networkStream.CanWrite)
				{
					clientStreamWriter.Write(cmd);
					clientStreamWriter.Flush();
				}
				else
				{
					AppLogger.Add("Can't write to client on node [" + nodeAddress + "]");
				}

				nodeClient.Close();
			}
			catch (Exception exception)
			{
				AppLogger.Add("Can't connect to node " + nodeAddress + ". EXCEPTION: " + exception.Message);
			}

		}

		public void CleanLogs()
		{
			CleanLogFolders(GetClusterNodes());
		}

		private void CleanLogFolders(List<ClusterNode> nodes)
		{
			foreach (ClusterNode node in nodes)
			{
				string dirPath = GetLogFolder(node.address);
				if (dirPath != null)
				{
					RemoveAllRecursively(dirPath);
					AppLogger.Add("Removed all files in : " + dirPath);
				}
			}
		}

		private string GetLogFolder(string node)
		{
			string fullpath = null;
			//get path
			string appPath = selectedApplication;
			// remove drive-name, like      [C:]
			if (appPath != null)
			{
				//fast crutch with localhost. refactoring needed!!
				if (node != "127.0.0.1")
				{
					appPath = appPath.Substring(2, appPath.Length - 2);
				}
				// remove filename and extension
				string logPath = Path.GetDirectoryName(appPath);

				// replace slash to back-slash
				logPath.Replace("/", "\\");

				string projectName = Path.GetFileNameWithoutExtension(appPath);
				if (node != "127.0.0.1")
				{
					fullpath = @"\\" + node + logPath + @"\" + projectName + @"\Saved\Logs\";
				}
				else
				{
					fullpath = logPath + @"\" + projectName + @"\Saved\Logs\";
				}

			}
			else
			{
				AppLogger.Add("WARNING! Cannot create logs, select application for start");
			}
			return fullpath;
		}


		private void RemoveAllRecursively(string rootDir)
		{
			try
			{
				// remove all files
				var allFilesToDelete = Directory.EnumerateFiles(rootDir, "*.*", SearchOption.AllDirectories);
				foreach (var file in allFilesToDelete)
				{
					File.Delete(file);
				}

				// remove all directories
				DirectoryInfo diRoot = new DirectoryInfo(rootDir);
				foreach (DirectoryInfo subDir in diRoot.GetDirectories())
				{
					subDir.Delete(true);
				}
			}
			catch (Exception exception)
			{
				AppLogger.Add("RemoveAllRecursively. EXCEPTION: " + exception.Message);
			}
		}

		public void CollectLogs()
		{
			CollectLogFiles(GetClusterNodes());
		}

		private void CollectLogFiles(List<ClusterNode> nodes)
		{
			//List<string> nodes = GetNodes();

			FolderBrowserDialog fbDialog = new FolderBrowserDialog();
			//fbDialog.Description = "Select a folder for save log files from nodes :";
			if (fbDialog.ShowDialog() != DialogResult.OK || nodes.Count == 0)
			{
				return;
			}



			// clean all files except *.zip, *.rar


			// list of new files
			List<string> fileList = new List<string>();

			// copy + rename
			foreach (ClusterNode node in nodes)
			{
				string logFilename = logFile;

				string logFilenameSep = (logFilename == string.Empty) ? "" : ("_");

				string srcLogPath = GetLogFolder(node.address) + Path.GetFileNameWithoutExtension(selectedApplication) + ".log";
				string dstLogPath = fbDialog.SelectedPath + @"\" + logFilenamePrefix + node.id + logFilenameSep + logFilename;
				string logMsg = "[" + srcLogPath + "] to [" + dstLogPath + "]";

				// add to list
				fileList.Add(dstLogPath);

				try
				{
					File.Copy(srcLogPath, dstLogPath);
					AppLogger.Add("Copied file from " + logMsg);
				}
				catch (Exception exception)
				{
					AppLogger.Add("Can't copy file from " + logMsg + ". EXCEPTION: " + exception.Message);
				}
			}

			// create archive
			if (!isLogZip)
			{
				return;
			}

			string currentTime = DateTime.Now.ToString("HHmmss");
			string currentDate = DateTime.Now.ToString("yyyyMMdd");

			string zipFilename = fbDialog.SelectedPath + @"\" + logFilenamePrefix + currentDate + "_" + currentTime + ".zip";

			CreateZipLogs(zipFilename, fileList);

			// clean *.log-files
			if (isLogRemove)
			{
				RemoveListOfFiles(fileList);
			}
		}

		private void CreateZipLogs(string zipFilename, List<string> files)
		{
			if (files.Count == 0)
			{
				return;
			}

			string currentDir = Path.GetDirectoryName(zipFilename);

			string dirForZip = currentDir + temporaryZipDir;

			// create tmp-dir
			Directory.CreateDirectory(dirForZip);

			// copy to temporary folder for zip
			foreach (string file in files)
			{
				File.Copy(file, dirForZip + Path.GetFileName(file));
			}

			try
			{
				// pack it
				ZipFile.CreateFromDirectory(dirForZip, zipFilename, CompressionLevel.Optimal, false);

				// remove tmp dir and all files after ZIP
				RemoveAllRecursively(dirForZip);
				Directory.Delete(dirForZip);
			}
			catch (Exception exception)
			{
				AppLogger.Add("CreateZipLogs. EXCEPTION: " + exception.Message);
			}
		}

		private void RemoveListOfFiles(List<string> fList)
		{
			foreach (string file in fList)
			{
				File.Delete(file);
			}
		}

		public void AddApplication(string appPath)
		{
			if (!applications.Contains(appPath))
			{
				applications.Add(appPath);
				RegistrySaver.AddRegistryValue(RegistrySaver.appList, appPath);
				AppLogger.Add("Application [" + appPath + "] added to list");
			}
			else
			{
				AppLogger.Add("WARNING! Application [" + appPath + "] is already in the list");
			}

		}

		public void DeleteApplication()
		{
			applications.Remove(selectedApplication);
			RegistrySaver.RemoveRegistryValue(RegistrySaver.appList, selectedApplication);
			AppLogger.Add("Application [" + selectedApplication + "] deleted");

			selectedApplication = null;
		}

		public void ChangeConfigSelection(string configPath)
		{
			try
			{
				foreach (string config in configs)
				{
					if (config != configPath)
					{
						RegistrySaver.UpdateRegistry(RegistrySaver.configList, config, false);
					}
					else
					{
						RegistrySaver.UpdateRegistry(RegistrySaver.configList, config, true);
					}
				}
			}
			catch (Exception exception)
			{
				AppLogger.Add("ERROR while changing config selection. EXCEPTION: " + exception.Message);
			}
		}

		public void AddConfig(string configPath)
		{
			try
			{
				configs.Add(configPath);
				selectedConfig = configs.Find(x => x == configPath);
				RegistrySaver.AddRegistryValue(RegistrySaver.configList, configPath);
				ChangeConfigSelection(configPath);
				AppLogger.Add("Configuration file [" + configPath + "] added to list");
			}
			catch (Exception)
			{
				AppLogger.Add("ERROR! Can not add configuration file [" + configPath + "] to list");
			}
		}

		public void DeleteConfig()
		{
			configs.Remove(selectedConfig);
			RegistrySaver.RemoveRegistryValue(RegistrySaver.configList, selectedConfig);
			AppLogger.Add("Configuration file [" + selectedConfig + "] deleted");
			selectedConfig = configs.FirstOrDefault();
		}

		//public bool AddNode(string node)
		//{
		//    try
		//    {
		//        if (!activeNodes.Exists(x => x.key == node))
		//        {
		//            activeNodes.Add(new ActiveNode(node, true));
		//            RegistrySaver.AddRegistryValue(RegistrySaver.nodeList, node);
		//            AppLogger.Add("Node [" + node + "] added to list");
		//            return true;
		//        }
		//        else
		//        {
		//            AppLogger.Add("WARNING! Node [" + node + "] is already in the list");
		//            return false;
		//        }
		//    }
		//    catch (Exception e)
		//    {
		//        AppLogger.Add("ERROR! can not add node [" + node + "] to list");
		//        return false;
		//    }
		//}

		//public void DeleteNodes(List<ActiveNode> nodes)
		//{

		//    foreach (ActiveNode node in nodes)
		//    {
		//        RegistrySaver.RemoveRegistryValue(RegistrySaver.nodeList, node.key);
		//        activeNodes.Remove(node);
		//        AppLogger.Add("Node [" + node.key + "] deleted");
		//    }

		//}
	}
}
