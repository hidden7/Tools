using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Forms;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using System.Reflection;
using vrClusterManager.Config;
using vrClusterManager.Settings;

namespace vrClusterManager
{
	public partial class MainWindow : Window
	{
		static readonly string cfgFileExtention = "CAVE config file (*.cfg)|*.cfg";
		static readonly string appFileExtention = "CAVE VR application (*.exe)|*.exe";

		public VRConfig     m_Config  = new VRConfig();

		public MainWindow()
		{
			InitializeComponent();
			InitializeInternals();
		}

		private void InitializeInternals()
		{
			Configs = RegistryData.GetValueNames(RegistryData.KeyConfigs);
			Configs.RemoveAll(x => string.IsNullOrEmpty(x));

			Applications = RegistryData.GetValueNames(RegistryData.KeyApps);
			Applications.RemoveAll(x => string.IsNullOrEmpty(x));

			CommonCmdLineArgs = RegistryData.GetStringValue(RegistryData.KeySettings, RegistryData.ValCommonCmdLineArgs);

			InitLogCategories();
			RefreshUiControls();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			LoadDefaultConfig();
		}

		private void NewConfig(object sender, RoutedEventArgs e)
		{
			LoadNewConfig();
		}
		private void SaveConfig(object sender, RoutedEventArgs e)
		{
			SaveImpl(false);
		}
		private void SaveAsConfig(object sender, RoutedEventArgs e)
		{
			SaveImpl(true);
		}
		private void OpenConfig(object sender, RoutedEventArgs e)
		{
			AddConfigFromFile();
		}
		private void Exit(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
		private void About_Click(object sender, RoutedEventArgs e)
		{
			AboutDialog aboutDialog = new AboutDialog();
			aboutDialog.Owner = this;
			aboutDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			aboutDialog.ShowDialog();
		}
		private void SaveImpl(bool isSaveAs)
		{
			if (m_Config.Validate())
			{
				try
				{
					string currentFileName = null;// RegistryData.GetStringValue(RegistryData.KeySettings, RegistryData.ValDefaultConfig);
					if (!isSaveAs && File.Exists(currentFileName))
					{
						File.WriteAllText(currentFileName, m_Config.CreateConfig());
					}
					else
					{
						SaveFileDialog saveFileDialog = new SaveFileDialog();
						saveFileDialog.Filter = cfgFileExtention;
						if (saveFileDialog.ShowDialog() == true)
						{
							currentFileName = saveFileDialog.FileName;
							m_Config.name = Path.GetFileNameWithoutExtension(currentFileName);

							//RegistryData.RemoveAllRegistryValues(RegistryData.ValDefaultConfig);
							//RegistryData.SetStringValue(RegistryData.KeySettings, RegistryData.ValDefaultConfig, currentFileName);
							File.WriteAllText(currentFileName, m_Config.CreateConfig());
						}
					}
					UpdateWindowTitle();
					AppLogger.Add("Config saved to " + currentFileName);
				}
				catch (Exception exception)
				{
					InfoDialog errorDialog = new InfoDialog("Error! \nCan not save configuration file. \nSee exception message in Log");
					errorDialog.Owner = this;
					errorDialog.Width = 350;
					errorDialog.Height = 200;
					errorDialog.Show();
					AppLogger.Add("ERROR! Can not save config to file. EXCEPTION: " + exception.Message);
				}
			}
			else
			{
				InfoDialog errorDialog = new InfoDialog("Error! \nCan not save configuration file. \nWrong config. See errors in Log");
				errorDialog.Owner = this;
				errorDialog.Width = 350;
				errorDialog.Height = 200;
				errorDialog.Show();
				AppLogger.Add("ERROR! Can not save config to file. Errors in configuration");
			}
		}
		private void UpdateWindowTitle()
		{
			this.Title = "VRCluster Manager ver. " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + " - " + m_Config.name;
		}

		private void SetViewportPreview()
		{
			ViewportPreview viewportPreview = new ViewportPreview();
			screenResolutionGrid.DataContext = viewportPreview;
			viewportCanvas.DataContext = viewportPreview;
			previewViewport.DataContext = m_Config;
		}

		private void RefreshUiControls()
		{
			TabAppsLauncher.DataContext = this;
			TabAppsLogging.DataContext = this;
			ctrlComboConfigs.DataContext = this;
			ctrlTextAppLog.DataContext = AppLogger.instance;

			ctrlComboConfigs.Items.Refresh();

			inputsListBox.Items.Refresh();
			screensListBox.Items.Refresh();
			viewportsListBox.Items.Refresh();
			nodesListBox.Items.Refresh();
			parentWallsCb.Items.Refresh();
			screensCb.Items.Refresh();
			viewportsCb.Items.Refresh();
			cameraTrackerIDCb.Items.Refresh();
			sceneNodeTrackerCb.Items.Refresh();
			ctrlTreeSceneNodes.Items.Refresh();
		}

		private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
		{
			if (e.Item is TrackerInput)
			{
				e.Accepted = true;
			}
			else
			{
				e.Accepted = false;
			}
		}

		private void CopyItem(SceneNodeView _sourceItem, SceneNodeView _targetItem)
		{
			//Alert if node sets as child of it's own child node
			if (_targetItem != null && _sourceItem.FindNodeInChildren(_targetItem) != null)
			{
				InfoDialog alertDialog = new InfoDialog("Scene node can not be a child node of own children!");
				alertDialog.Owner = this;
				alertDialog.Show();
			}
			else
			{

				//Asking user wether he want to drop the dragged TreeViewItem here or not
				YesNoDialog dialogResult = new YesNoDialog("Would you like to drop " + _sourceItem.node.id + " into " + _targetItem.node.id + "");
				dialogResult.Owner = this;
				if ((bool)dialogResult.ShowDialog())
				{
					try
					{

						//finding Parent TreeViewItem of dragged TreeViewItem 
						SceneNodeView ParentItem = m_Config.FindParentNode(_sourceItem);
						if (ParentItem == null)
						{
							((List<SceneNodeView>)ctrlTreeSceneNodes.ItemsSource).Remove(_sourceItem);
						}
						else
						{
							ParentItem.children.Remove(_sourceItem);
						}
						//adding dragged TreeViewItem in target TreeViewItem
						if (_targetItem == null)
						{
							((List<SceneNodeView>)ctrlTreeSceneNodes.ItemsSource).Add(_sourceItem);
							_sourceItem.node.parent = null;
						}
						else
						{
							_targetItem.children.Add(_sourceItem);
							_sourceItem.node.parent = _targetItem.node;
						}
						//Set SceneNode of _targetItem as parent node for _sourceItem Scene Node
					}
					catch
					{
					}
					ctrlTreeSceneNodes.Items.Refresh();
				}
			}
		}

		private void CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			GenerateLogLevelsString();
		}

		private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			GenerateLogLevelsString();
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			GenerateLogLevelsString();
		}

		private void CopyToClipboard(string text)
		{
			if (text != String.Empty)
			{
				Clipboard.SetText(text);
			}
		}

		#region Config operations
		private void AddConfigFromFile()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = cfgFileExtention;
			if (openFileDialog.ShowDialog() == true)
			{
				string configPath = openFileDialog.FileName;
				if (!Configs.Exists(x => x == configPath))
				{
					try
					{
						Configs.Add(configPath);
						RegistryData.SetStringValue(RegistryData.KeyConfigs, configPath, string.Empty);
						AppLogger.Add("Configuration file [" + configPath + "] has been added to the list");

						ctrlComboConfigs.Items.Refresh();

						//if(Configs.Count > 1)
							ctrlComboConfigs.SelectedValue = configPath;
					}
					catch (Exception)
					{
						AppLogger.Add("ERROR! Couldn't add configuration file [" + configPath + "] to the list");
					}
				}
			}
		}

		private void LoadNewConfig()
		{
			m_Config = new VRConfig();

			AppLogger.Add("New config initialized");

			ctrlComboConfigs.SelectedIndex = -1;

			this.DataContext = m_Config;
			UpdateWindowTitle();
			SetViewportPreview();
			RefreshUiControls();
		}

		private void LoadDefaultConfig()
		{
			string configPath = RegistryData.GetStringValue(RegistryData.KeyConfigs, null);

			if (string.IsNullOrEmpty(configPath))
			{
				if (ctrlComboConfigs.Items.Count > 0)
				{
					ctrlComboConfigs.SelectedIndex = 0;
				}
				else
				{
					LoadNewConfig();
				}
			}
			else
			{
				int idx = Configs.FindIndex(x => x == configPath);
				if (idx >= 0)
				{
					ctrlComboConfigs.SelectedIndex = idx;
				}
				else
				{
					if (ctrlComboConfigs.Items.Count > 0)
					{
						ctrlComboConfigs.SelectedIndex = 0;
					}
					else
					{
						LoadNewConfig();
					}
				}
			}
		}

		private void LoadConfig(string filePath)
		{
			RegistryData.SetStringValue(RegistryData.KeyConfigs, filePath, string.Empty);
			m_Config = ConfigParser.Parse(filePath);

			//Set first items in listboxes and treeview as default if existed
			m_Config.SelectFirstItems();
			RefreshUiControls();
			try
			{
				((CollectionViewSource)this.Resources["cvsInputTrackers"]).View.Refresh();
			}
			catch (NullReferenceException)
			{

			}

			UpdateWindowTitle();
			SetViewportPreview();
		}

		private void DeleteCurrentConfig()
		{
			if (ctrlComboConfigs.SelectedItem != null && ctrlComboConfigs.Items.Count > 0)
			{
				//YesNoDialog dialogResult = new YesNoDialog("Do you really want to delete selected Config file?");
				//dialogResult.Owner = this;
				//if ((bool)dialogResult.ShowDialog())
				{
					string configPath = ctrlComboConfigs.SelectedValue as string;
					int idx = ctrlComboConfigs.SelectedIndex;

					Configs.Remove(configPath);
					RegistryData.RemoveRegistryValue(RegistryData.KeyConfigs, configPath);
					AppLogger.Add("Configuration file [" + configPath + "] has been removed from the list");

					ctrlComboConfigs.Items.Refresh();

					if (ctrlComboConfigs.Items.Count > 0)
					{
						ctrlComboConfigs.SelectedIndex = Math.Min(idx, ctrlComboConfigs.Items.Count - 1);
					}
					else
					{
						LoadNewConfig();
					}
				}
			}
		}

		private void DeleteAllConfigs()
		{
			if (ctrlComboConfigs.Items.Count > 0)
			{
				YesNoDialog dialogResult = new YesNoDialog("Do you really want to delete all config files?");
				dialogResult.Owner = this;
				if ((bool)dialogResult.ShowDialog())
				{
					RegistryData.RemoveAllRegistryValues(RegistryData.KeyConfigs);
					Configs.Clear();
					ctrlComboConfigs.Items.Refresh();
				}

				LoadNewConfig();
			}
		}
		#endregion

		#region Config buttons
		private void onComboConfigs_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
			{
				LoadConfig(e.AddedItems[0] as string);
			}
		}

		private void onComboConfigs_DropDownOpened(object sender, EventArgs e)
		{
			ctrlComboConfigs.Items.Refresh();
		}

		private void onBtnConfigNew_Click(object sender, RoutedEventArgs e)
		{
			LoadNewConfig();
		}

		private void onBtnConfigAdd_Click(object sender, RoutedEventArgs e)
		{
			AddConfigFromFile();
		}

		private void onBtnConfigDel_Click(object sender, RoutedEventArgs e)
		{
			DeleteCurrentConfig();
		}

		private void onBtnConfigDelAll_Click(object sender, RoutedEventArgs e)
		{
			DeleteAllConfigs();
		}
		#endregion

		#region Log buttons
		private void onBtnLogCopy_Click(object sender, RoutedEventArgs e)
		{
			CopyToClipboard(ctrlTextAppLog.Text);
		}

		private void onBtnLogSave_Click(object sender, RoutedEventArgs e) => throw new NotImplementedException();

		private void onBtnLogClean_Click(object sender, RoutedEventArgs e)
		{
			AppLogger.CleanLog();
		}
		#endregion
	}
}
