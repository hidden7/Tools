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

			additionalParams = RegistryData.GetStringValue(RegistryData.KeySettings, RegistryData.ValCommonCmdLineArgs);

			InitLogCategories();

			RefreshUiControls();
		}


		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			LoadDefaultConfig();
		}

		private void AddConfigFromFile()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = cfgFileExtention;
			if (openFileDialog.ShowDialog() == true)
			{
				string configPath = openFileDialog.FileName;
				if (!Configs.Exists(x => x == configPath))
				{
					AddConfig(configPath);
				}
			}
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


		private void LoadNewConfig()
		{
			m_Config = new VRConfig();

			AppLogger.Add("New config initialized");

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
		private void LoadSelectedConfig()
		{
			//ctrlComboConfigs.SelectedValue as string
		}

		//Config file parser
		private void LoadConfigImpl(string filePath)
		{
			LoadNewConfig();
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
		}

		private void LoadConfigImpl()
		{
			UpdateWindowTitle();
			SetViewportPreview();
		}

		private void SetViewportPreview()
		{

			ViewportPreview viewportPreview = new ViewportPreview();
			screenResolutionGrid.DataContext = viewportPreview;
			viewportCanvas.DataContext = viewportPreview;
			previewViewport.DataContext = m_Config;
		}



		//crutch for refreshing all listboxes and comboboxes after binding
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

		private void UpdateWindowTitle()
		{
			this.Title = "VRCluster Manager ver. " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + " - " + m_Config.name;
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

		public static void ConfigModifyIndicator()
		{
			if (!Application.Current.MainWindow.Title.StartsWith("*"))
			{
				Application.Current.MainWindow.Title = "*" + Application.Current.MainWindow.Title;
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



		#region Config buttons
		private void onComboConfigs_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			RegistryData.SetStringValue(RegistryData.KeyConfigs, null, ctrlComboConfigs.SelectedValue as string);
			LoadSelectedConfig();
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
			if (ctrlComboConfigs.SelectedItem != null)
			{
				YesNoDialog dialogResult = new YesNoDialog("Do you really want to delete selected Config file?");
				dialogResult.Owner = this;
				if ((bool)dialogResult.ShowDialog())
				{
					Configs.Remove(selectedConfig);
					RegistryData.RemoveRegistryValue(RegistryData.KeyConfigs, selectedConfig);
					AppLogger.Add("Configuration file [" + selectedConfig + "] deleted");
					selectedConfig = Configs.FirstOrDefault();

					ctrlComboConfigs.Items.Refresh();
				}
			}
		}

		private void onBtnConfigDelAll_Click(object sender, RoutedEventArgs e)
		{
			if (ctrlComboConfigs.SelectedItem != null)
			{
				YesNoDialog dialogResult = new YesNoDialog("Do you really want to delete all config file?");
				dialogResult.Owner = this;
				if ((bool)dialogResult.ShowDialog())
				{
					ctrlComboConfigs.Items.Clear();

					Configs.Remove(selectedConfig);
					RegistryData.RemoveRegistryValue(RegistryData.KeyConfigs, selectedConfig);
					AppLogger.Add("Configuration file [" + selectedConfig + "] deleted");
					selectedConfig = Configs.FirstOrDefault();

					ctrlComboConfigs.Items.Refresh();
				}
			}
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

		#region Page: APPS - launcher
		private void onBtnApplicationAdd_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = appFileExtention;
			if (openFileDialog.ShowDialog() == true)
			{
				string appPath = openFileDialog.FileName;
				AddApplication(appPath);
				applicationsListBox.Items.Refresh();
			}
		}

		private void onBtnApplicationDel_Click(object sender, RoutedEventArgs e)
		{
			if (applicationsListBox.SelectedItem != null)
			{
				YesNoDialog dialogResult = new YesNoDialog("Do you really want to delene selected application?");
				dialogResult.Owner = this;
				if ((bool)dialogResult.ShowDialog())
				{
					DeleteApplication();
					applicationsListBox.Items.Refresh();
				}
			}
		}

		private void copyLogBtn_Click(object sender, RoutedEventArgs e)
		{
			CopyToClipboard(commandTextBox.Text);
		}

		private void runBtn_Click(object sender, RoutedEventArgs e)
		{
			RunCommand();
		}

		private void statusBtn_Click(object sender, RoutedEventArgs e)
		{
			StatusCommand();
		}

		private void killBtn_Click(object sender, RoutedEventArgs e)
		{
			KillCommand();
		}
		#endregion

		#region Page: APPS - logs
		private void logsFolderBtn_Click(object sender, RoutedEventArgs e)
		{
			CollectLogs();
		}

		private void logsClearBtn_Click(object sender, RoutedEventArgs e)
		{
			YesNoDialog dialogResult = new YesNoDialog("Do you really want to clean all logs?");
			dialogResult.Owner = this;
			if ((bool)dialogResult.ShowDialog())
			{
				CleanLogs();
			}
		}
		#endregion

		#region Page: Edit config - cluster node
		private void onCbMasterNode_Click(object sender, RoutedEventArgs e)
		{
			foreach (ClusterNode node in m_Config.clusterNodes)
			{
				if (node == m_Config.selectedNode)
				{
					node.isMaster = true;
					AppLogger.Add("Cluster node " + m_Config.selectedNode.id + " set as master node");
				}
				else
				{
					node.isMaster = false;
				}

			}
		}

		private void onBtnClusterNodeAdd_Click(object sender, RoutedEventArgs e)
		{
			m_Config.clusterNodes.Add(new ClusterNode());
			m_Config.selectedNode = m_Config.clusterNodes.LastOrDefault();
			nodesListBox.Items.Refresh();
			nodeIdTb.Focus();
			AppLogger.Add("New cluster node added");
			AppLogger.Add("WARNING! Change default values");
		}

		private void onBtnClusterNodeDel_Click(object sender, RoutedEventArgs e)
		{
			if (m_Config.selectedNode != null)
			{
				var id = m_Config.selectedNode.id;
				int selectedIndex = m_Config.clusterNodes.IndexOf(m_Config.selectedNode);
				m_Config.clusterNodes.RemoveAt(selectedIndex);
				m_Config.selectedNode = m_Config.clusterNodes.FirstOrDefault();

				AppLogger.Add("Cluster node " + id + " deleted");
			}
			nodesListBox.Items.Refresh();
		}
		#endregion

		#region Page: Edit config - screens
		private void onBtnScreenAdd_Click(object sender, RoutedEventArgs e)
		{
			m_Config.screens.Add(new Screen());
			m_Config.selectedScreen = m_Config.screens.LastOrDefault();
			screensListBox.Items.Refresh();
			screensCb.Items.Refresh();
			screenIdTb.Focus();
			AppLogger.Add("New screen added");
			AppLogger.Add("WARNING! Change default values");
		}

		private void onBtnScreenDel_Click(object sender, RoutedEventArgs e)
		{
			if (m_Config.selectedScreen != null)
			{
				var id = m_Config.selectedScreen.id;
				int selectedIndex = m_Config.screens.IndexOf(m_Config.selectedScreen);
				m_Config.screens.RemoveAt(selectedIndex);
				m_Config.selectedScreen = m_Config.screens.FirstOrDefault();

				AppLogger.Add("Screen " + id + " deleted");
			}
			screensListBox.Items.Refresh();
			screensCb.Items.Refresh();
		}
		#endregion

		#region Page: Edit config - viewports
		private void onBtnViewportAdd_Click(object sender, RoutedEventArgs e)
		{
			m_Config.viewports.Add(new Viewport());
			m_Config.selectedViewport = m_Config.viewports.LastOrDefault();
			viewportsListBox.Items.Refresh();
			viewportsCb.Items.Refresh();
			viewportIdTb.Focus();
			AppLogger.Add("New viewport added");
			AppLogger.Add("WARNING! Change default values");
		}

		private void onBtnViewportDel_Click(object sender, RoutedEventArgs e)
		{
			if (m_Config.selectedViewport != null)
			{
				var id = m_Config.selectedViewport.id;
				int selectedIndex = m_Config.viewports.IndexOf(m_Config.selectedViewport);

				m_Config.viewports.RemoveAt(selectedIndex);
				m_Config.selectedViewport = m_Config.viewports.FirstOrDefault();

				AppLogger.Add("Viewport " + id + " deleted");
			}
			viewportsListBox.Items.Refresh();
			viewportsCb.Items.Refresh();
		}
		#endregion

		#region Page: Edit config - camera
		private void onBtnCameraAdd_Click(object sender, RoutedEventArgs e) => throw new NotImplementedException();

		private void onBtnCameraDel_Click(object sender, RoutedEventArgs e) => throw new NotImplementedException();
		#endregion

		#region Page: Edit config - scene nodes
		private void onBtnSceneNodeAdd_Click(object sender, RoutedEventArgs e)
		{
			SceneNode newItem = new SceneNode();
			if (m_Config.selectedSceneNodeView != null)
			{
				newItem.parent = m_Config.selectedSceneNodeView.node;
			}
			SceneNodeView newViewItem = new SceneNodeView(newItem);
			newViewItem.isSelected = true;
			m_Config.sceneNodes.Add(newItem);

			SceneNodeView parentNode = m_Config.FindParentNode(newViewItem);
			if (parentNode == null)
			{
				m_Config.sceneNodesView.Add(newViewItem);
			}
			else
			{
				parentNode.children.Add(newViewItem);
				parentNode.isExpanded = true;
			}
			AppLogger.Add("New scene node added");
			AppLogger.Add("WARNING! Change default values");
			ctrlTreeSceneNodes.Items.Refresh();
			sceneNodeIdTb.Focus();
		}

		private void onBtnSceneNodeDel_Click(object sender, RoutedEventArgs e)
		{
			if (ctrlTreeSceneNodes.SelectedItem != null)
			{
				SceneNodeView item = (SceneNodeView)ctrlTreeSceneNodes.SelectedItem;
				var id = item.node.id;
				m_Config.DeleteSceneNode(item);
				m_Config.selectedSceneNodeView = m_Config.sceneNodesView.FirstOrDefault();
				AppLogger.Add("Scene Node " + id + " deleted");
			}
			ctrlTreeSceneNodes.Items.Refresh();
			parentWallsCb.Items.Refresh();
		}

		//Drag and drop in TreeView implementation
		Point _lastMouseDown;
		SceneNodeView draggedItem, _target;
		bool isNode = true;
		private void onTreeSceneNodes_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				if (e.OriginalSource is Grid)
				{
					isNode = false;
				}
				else
				{
					_lastMouseDown = e.GetPosition(ctrlTreeSceneNodes);
					isNode = true;
				}
			}
		}

		private void onTreeSceneNodes_MouseMove(object sender, MouseEventArgs e)
		{
			try
			{
				if (e.LeftButton == MouseButtonState.Pressed)
				{
					if (!(e.OriginalSource is System.Windows.Controls.Primitives.Thumb))
					{
						Point currentPosition = e.GetPosition(ctrlTreeSceneNodes);

						if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) ||
							(Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
						{
							if (isNode)
							{
								draggedItem = (SceneNodeView)ctrlTreeSceneNodes.SelectedItem;
							}
							if (draggedItem != null)
							{
								DragDropEffects finalDropEffect = DragDrop.DoDragDrop(ctrlTreeSceneNodes, ctrlTreeSceneNodes.SelectedValue, DragDropEffects.Move);
								//Checking target is not null and item is
								//dragging(moving)
								if (finalDropEffect == DragDropEffects.Move)
								{
									if (_target == null)
									{
										CopyItem(draggedItem, _target);
										draggedItem = null;
									}
									else
									{
										// A Move drop was accepted
										if (!draggedItem.node.Equals(_target.node))

										{
											CopyItem(draggedItem, _target);
											_target = null;
											draggedItem = null;
										}
									}
								}
							}

						}
					}
				}
			}
			catch
			{
			}
		}

		//Crutch for deselecting all nodes. refactoring needed!!!!!
		private void onTreeSceneNodes_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (e.OriginalSource is Grid && m_Config.selectedSceneNodeView != null)
			{
				m_Config.selectedSceneNodeView.isSelected = false;
				m_Config.selectedSceneNodeView = null;
				ctrlTreeSceneNodes.ItemsSource = null;
				ctrlTreeSceneNodes.ItemsSource = m_Config.sceneNodesView;

			}
			isNode = true;
		}

		//Sets selected treeview item
		private void onTreeSceneNodes_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			m_Config.selectedSceneNodeView = (SceneNodeView)ctrlTreeSceneNodes.SelectedItem;


			if (m_Config.selectedSceneNodeView != null)
			{
				if (m_Config.inputs.Contains(m_Config.selectedSceneNodeView.node.tracker))
				{
					sceneNodeTrackerCb.SelectedItem = m_Config.selectedSceneNodeView.node.tracker;
				}
				else
				{
					sceneNodeTrackerCb.SelectedItem = null;
				}
			}
		}

		private void treeView_DragOver(object sender, DragEventArgs e)
		{
			try
			{
				Point currentPosition = e.GetPosition(ctrlTreeSceneNodes);

				if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) ||
				   (Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
				{
					// Verify that this is a valid drop and then store the drop target
					SceneNodeView item = e.OriginalSource as SceneNodeView;
					if (CheckDropTarget(draggedItem, item))
					{
						e.Effects = DragDropEffects.Move;
					}
					else
					{
						e.Effects = DragDropEffects.None;
					}
				}
				e.Handled = true;
			}
			catch
			{

			}
		}

		private void treeView_Drop(object sender, DragEventArgs e)
		{
			try
			{
				SceneNodeView TargetItem = null;
				e.Effects = DragDropEffects.None;
				e.Handled = true;
				if (sender is TreeViewItem)
				{
					// Verify that this is a valid drop and then store the drop target
					ContentPresenter presenter = e.Source as ContentPresenter;
					TargetItem = presenter.Content as SceneNodeView;
				}

				if (draggedItem != null)
				{
					_target = TargetItem;
					e.Effects = DragDropEffects.Move;
				}
			}
			catch
			{
			}
		}

		private bool CheckDropTarget(SceneNodeView _sourceItem, object _targetItem)
		{
			SceneNodeView target = (SceneNodeView)_targetItem;
			//Check whether the target item is meeting your condition
			bool _isEqual = false;
			if (!_sourceItem.node.Equals(target.node))
			{
				_isEqual = true;
			}
			return _isEqual;

		}
		#endregion

		#region Page: Edit config - input
		private void onBtnInputAdd_Click(object sender, RoutedEventArgs e)
		{
			InputTypeDialog dialogChooser = new InputTypeDialog();
			dialogChooser.Owner = this;
			dialogChooser.Title = "Select new input type";
			dialogChooser.ShowDialog();
			if (dialogChooser.DialogResult.Value)
			{
				string type = dialogChooser.inputType;
				addInput(type);
				inputIdTb.Focus();
				AppLogger.Add("New " + type + " input added");
				AppLogger.Add("WARNING! Change default values");
			}
		}

		private void onBtnInputDel_Click(object sender, RoutedEventArgs e)
		{
			if (m_Config.selectedInput != null)
			{
				var id = m_Config.selectedInput.id;
				int selectedIndex = m_Config.inputs.IndexOf(m_Config.selectedInput);
				m_Config.inputs.RemoveAt(selectedIndex);
				AppLogger.Add(m_Config.selectedInput.type + " input " + id + " deleted");
				m_Config.selectedInput = m_Config.inputs.FirstOrDefault();
				try
				{
					((CollectionViewSource)this.Resources["cvsInputTrackers"]).View.Refresh();
				}
				catch (NullReferenceException)
				{

				}
				RefreshUiControls();
			}
		}

		private void onTextCommonCmdLineArgs_LostFocus(object sender, RoutedEventArgs e)
		{

		}

		private void addInput(string type)
		{
			if (type != null)
			{
				if (type == "tracker")
				{
					m_Config.inputs.Add(new TrackerInput { id = "TrackerInputId", address = "TrackerInputName@127.0.0.1", locationX = "0", locationY = "0", locationZ = "0", rotationP = "0", rotationR = "0", rotationY = "0", front = "X", right = "Y", up = "-Z" });
				}
				else
				{
					InputDeviceType currentType = (InputDeviceType)System.Enum.Parse(typeof(InputDeviceType), type);
					m_Config.inputs.Add(new BaseInput { id = "InputId", type = currentType, address = "InputName@127.0.0.1" });
				}
				m_Config.selectedInput = m_Config.inputs.LastOrDefault();
				RefreshUiControls();
				//inputsListBox.Items.Refresh();
				try
				{
					((CollectionViewSource)this.Resources["cvsInputTrackers"]).View.Refresh();
				}
				catch (NullReferenceException)
				{

				}
			}
		}
		#endregion
	}
}