using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

		public VRConfig currentConfig;
		public AppRunner m_AppRunner;


		public MainWindow()
		{
			InitializeComponent();

			m_AppRunner = new AppRunner();
			TabAppsLauncher.DataContext = m_AppRunner;
			TabAppsLogging.DataContext = m_AppRunner;
			appLogTextBox.DataContext = AppLogger.instance;
			SetDefaultConfig();
			SetViewportPreview();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			UpdateWindowTitle();
		}





		private void SetViewportPreview()
		{

			ViewportPreview viewportPreview = new ViewportPreview();
			screenResolutionGrid.DataContext = viewportPreview;
			viewportCanvas.DataContext = viewportPreview;
			previewViewport.DataContext = currentConfig;
		}


		private void Save(bool isSaveAs)
		{
			if (currentConfig.Validate())
			{
				try
				{
					string currentFileName = RegistrySaver.ReadStringFromRegistry(RegistrySaver.configName);
					if (!isSaveAs && File.Exists(currentFileName))
					{
						File.WriteAllText(currentFileName, currentConfig.CreateConfig());
					}
					else
					{
						SaveFileDialog saveFileDialog = new SaveFileDialog();
						saveFileDialog.Filter = cfgFileExtention;
						if (saveFileDialog.ShowDialog() == true)
						{
							currentFileName = saveFileDialog.FileName;
							currentConfig.name = Path.GetFileNameWithoutExtension(currentFileName);
							RegistrySaver.RemoveAllRegistryValues(RegistrySaver.configName);
							RegistrySaver.AddRegistryValue(RegistrySaver.configName, currentFileName);
							File.WriteAllText(currentFileName, currentConfig.CreateConfig());
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
		public void SaveAsConfig(object sender, RoutedEventArgs e)
		{
			Save(true);
		}
		public void OpenConfig(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = cfgFileExtention;
			if (openFileDialog.ShowDialog() == true)
			{
				ConfigFileParser(openFileDialog.FileName);
			}
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

		public void SaveConfig(object sender, RoutedEventArgs e)
		{
			Save(false);
		}

		private void SetDefaultConfig()
		{
			string configPath = RegistrySaver.ReadStringFromRegistry(RegistrySaver.configName);
			if (string.IsNullOrEmpty(configPath))
			{
				CreateConfig();

			}
			else
			{
				ConfigFileParser(configPath);
			}
		}
		public void NewConfig(object sender, RoutedEventArgs e)
		{
			CreateConfig();
		}
		void CreateConfig()
		{
			RegistrySaver.RemoveAllRegistryValues(RegistrySaver.configName);
			currentConfig = new VRConfig();
			this.DataContext = currentConfig;
			//crutch. for refactoring
			currentConfig.selectedSceneNodeView = null;
			AppLogger.Add("New config initialized");
			UpdateWindowTitle();
			SetViewportPreview();
		}


		//Config file parser
		private void ConfigFileParser(string filePath)
		{
			CreateConfig();
			Parser.Parse(filePath, currentConfig);
			//Set first items in listboxes and treeview as default if existed
			currentConfig.SelectFirstItems();
			RefreshUiControls();
			try
			{
				((CollectionViewSource)this.Resources["cvsInputTrackers"]).View.Refresh();
			}
			catch (NullReferenceException)
			{

			}
			//sceneNodeTrackerCb.SelectedIndex = -1;
			UpdateWindowTitle();
			//SetViewportPreview();
		}

		//crutch for refreshing all listboxes and comboboxes after binding
		private void RefreshUiControls()
		{
			inputsListBox.Items.Refresh();
			screensListBox.Items.Refresh();
			viewportsListBox.Items.Refresh();
			nodesListBox.Items.Refresh();
			parentWallsCb.Items.Refresh();
			screensCb.Items.Refresh();
			viewportsCb.Items.Refresh();
			cameraTrackerIDCb.Items.Refresh();
			sceneNodeTrackerCb.Items.Refresh();
			sceneNodesTreeView.Items.Refresh();
		}

		//Setting title of widow.
		private void UpdateWindowTitle()
		{
			this.Title = currentConfig.name + " - vrCluster runner & configurator ver. " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
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
						SceneNodeView ParentItem = currentConfig.FindParentNode(_sourceItem);
						if (ParentItem == null)
						{
							((List<SceneNodeView>)sceneNodesTreeView.ItemsSource).Remove(_sourceItem);
						}
						else
						{
							ParentItem.children.Remove(_sourceItem);
						}
						//adding dragged TreeViewItem in target TreeViewItem
						if (_targetItem == null)
						{
							((List<SceneNodeView>)sceneNodesTreeView.ItemsSource).Add(_sourceItem);
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
					sceneNodesTreeView.Items.Refresh();
				}
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

		private void CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			m_AppRunner.GenerateLogLevelsString();
		}

		private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			m_AppRunner.GenerateLogLevelsString();
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			m_AppRunner.GenerateLogLevelsString();
		}

		private void CopyToClipboard(string text)
		{
			if (text != String.Empty)
			{
				Clipboard.SetText(text);
			}
		}



		#region Config buttons
		private void configsCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			m_AppRunner.ChangeConfigSelection(m_AppRunner.selectedConfig);
		}

		private void configsCb_DropDownOpened(object sender, EventArgs e)
		{
			ctrlComboConfigs.Items.Refresh();
		}

		private void onBtnConfigNew_Click(object sender, RoutedEventArgs e) => throw new NotImplementedException();

		private void onBtnConfigAdd_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = cfgFileExtention;
			if (openFileDialog.ShowDialog() == true)
			{
				string configPath = openFileDialog.FileName;
				if (!m_AppRunner.configs.Exists(x => x == configPath))
				{
					m_AppRunner.AddConfig(configPath);
					ctrlComboConfigs.Items.Refresh();
				}
			}
		}

		private void onBtnConfigDel_Click(object sender, RoutedEventArgs e)
		{
			if (ctrlComboConfigs.SelectedItem != null)
			{
				YesNoDialog dialogResult = new YesNoDialog("Do you really want to delete selected Config file?");
				dialogResult.Owner = this;
				if ((bool)dialogResult.ShowDialog())
				{
					m_AppRunner.DeleteConfig();

					ctrlComboConfigs.Items.Refresh();
				}
			}
		}

		private void onBtnConfigDelAll_Click(object sender, RoutedEventArgs e) => throw new NotImplementedException();
		#endregion

		#region Log buttons
		private void onBtnLogCopy_Click(object sender, RoutedEventArgs e)
		{
			CopyToClipboard(appLogTextBox.Text);
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
				m_AppRunner.AddApplication(appPath);
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
					m_AppRunner.DeleteApplication();
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
			m_AppRunner.RunCommand();
		}

		private void statusBtn_Click(object sender, RoutedEventArgs e)
		{
			m_AppRunner.StatusCommand();
		}

		private void killBtn_Click(object sender, RoutedEventArgs e)
		{
			m_AppRunner.KillCommand();
		}
		#endregion

		#region Page: APPS - logs
		private void logsFolderBtn_Click(object sender, RoutedEventArgs e)
		{
			m_AppRunner.CollectLogs();
		}

		private void logsClearBtn_Click(object sender, RoutedEventArgs e)
		{
			YesNoDialog dialogResult = new YesNoDialog("Do you really want to clean all logs?");
			dialogResult.Owner = this;
			if ((bool)dialogResult.ShowDialog())
			{
				m_AppRunner.CleanLogs();
			}
		}
		#endregion

		#region Page: Edit config - cluster node
		private void onCbMasterNode_Click(object sender, RoutedEventArgs e)
		{
			foreach (ClusterNode node in currentConfig.clusterNodes)
			{
				if (node == currentConfig.selectedNode)
				{
					node.isMaster = true;
					AppLogger.Add("Cluster node " + currentConfig.selectedNode.id + " set as master node");
				}
				else
				{
					node.isMaster = false;
				}

			}
		}

		private void onBtnClusterNodeAdd_Click(object sender, RoutedEventArgs e)
		{
			currentConfig.clusterNodes.Add(new ClusterNode());
			currentConfig.selectedNode = currentConfig.clusterNodes.LastOrDefault();
			nodesListBox.Items.Refresh();
			nodeIdTb.Focus();
			AppLogger.Add("New cluster node added");
			AppLogger.Add("WARNING! Change default values");
		}

		private void onBtnClusterNodeDel_Click(object sender, RoutedEventArgs e)
		{
			if (currentConfig.selectedNode != null)
			{
				var id = currentConfig.selectedNode.id;
				int selectedIndex = currentConfig.clusterNodes.IndexOf(currentConfig.selectedNode);
				currentConfig.clusterNodes.RemoveAt(selectedIndex);
				currentConfig.selectedNode = currentConfig.clusterNodes.FirstOrDefault();

				AppLogger.Add("Cluster node " + id + " deleted");
			}
			nodesListBox.Items.Refresh();
		}
		#endregion

		#region Page: Edit config - screens
		private void onBtnScreenAdd_Click(object sender, RoutedEventArgs e)
		{
			currentConfig.screens.Add(new Screen());
			currentConfig.selectedScreen = currentConfig.screens.LastOrDefault();
			screensListBox.Items.Refresh();
			screensCb.Items.Refresh();
			screenIdTb.Focus();
			AppLogger.Add("New screen added");
			AppLogger.Add("WARNING! Change default values");
		}

		private void onBtnScreenDel_Click(object sender, RoutedEventArgs e)
		{
			if (currentConfig.selectedScreen != null)
			{
				var id = currentConfig.selectedScreen.id;
				int selectedIndex = currentConfig.screens.IndexOf(currentConfig.selectedScreen);
				currentConfig.screens.RemoveAt(selectedIndex);
				currentConfig.selectedScreen = currentConfig.screens.FirstOrDefault();

				AppLogger.Add("Screen " + id + " deleted");
			}
			screensListBox.Items.Refresh();
			screensCb.Items.Refresh();
		}

		#endregion

		#region Page: Edit config - viewports
		private void onBtnViewportAdd_Click(object sender, RoutedEventArgs e)
		{
			currentConfig.viewports.Add(new Viewport());
			currentConfig.selectedViewport = currentConfig.viewports.LastOrDefault();
			viewportsListBox.Items.Refresh();
			viewportsCb.Items.Refresh();
			viewportIdTb.Focus();
			AppLogger.Add("New viewport added");
			AppLogger.Add("WARNING! Change default values");
		}

		private void onBtnViewportDel_Click(object sender, RoutedEventArgs e)
		{
			if (currentConfig.selectedViewport != null)
			{
				var id = currentConfig.selectedViewport.id;
				int selectedIndex = currentConfig.viewports.IndexOf(currentConfig.selectedViewport);

				currentConfig.viewports.RemoveAt(selectedIndex);
				currentConfig.selectedViewport = currentConfig.viewports.FirstOrDefault();

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
			if (currentConfig.selectedSceneNodeView != null)
			{
				newItem.parent = currentConfig.selectedSceneNodeView.node;
			}
			SceneNodeView newViewItem = new SceneNodeView(newItem);
			newViewItem.isSelected = true;
			currentConfig.sceneNodes.Add(newItem);

			SceneNodeView parentNode = currentConfig.FindParentNode(newViewItem);
			if (parentNode == null)
			{
				currentConfig.sceneNodesView.Add(newViewItem);
			}
			else
			{
				parentNode.children.Add(newViewItem);
				parentNode.isExpanded = true;
			}
			AppLogger.Add("New scene node added");
			AppLogger.Add("WARNING! Change default values");
			sceneNodesTreeView.Items.Refresh();
			sceneNodeIdTb.Focus();
		}

		private void onBtnSceneNodeDel_Click(object sender, RoutedEventArgs e)
		{
			if (sceneNodesTreeView.SelectedItem != null)
			{
				SceneNodeView item = (SceneNodeView)sceneNodesTreeView.SelectedItem;
				var id = item.node.id;
				currentConfig.DeleteSceneNode(item);
				currentConfig.selectedSceneNodeView = currentConfig.sceneNodesView.FirstOrDefault();
				AppLogger.Add("Scene Node " + id + " deleted");
			}
			sceneNodesTreeView.Items.Refresh();
			parentWallsCb.Items.Refresh();
		}

		//Drag and drop in TreeView implementation
		Point _lastMouseDown;
		SceneNodeView draggedItem, _target;
		bool isNode = true;
		private void treeView_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Left)
			{
				if (e.OriginalSource is Grid)
				{
					isNode = false;
				}
				else
				{
					_lastMouseDown = e.GetPosition(sceneNodesTreeView);
					isNode = true;
				}
			}
		}

		private void treeView_MouseMove(object sender, MouseEventArgs e)
		{
			try
			{
				if (e.LeftButton == MouseButtonState.Pressed)
				{
					if (!(e.OriginalSource is System.Windows.Controls.Primitives.Thumb))
					{
						Point currentPosition = e.GetPosition(sceneNodesTreeView);

						if ((Math.Abs(currentPosition.X - _lastMouseDown.X) > 10.0) ||
							(Math.Abs(currentPosition.Y - _lastMouseDown.Y) > 10.0))
						{
							if (isNode)
							{
								draggedItem = (SceneNodeView)sceneNodesTreeView.SelectedItem;
							}
							if (draggedItem != null)
							{
								DragDropEffects finalDropEffect =
					DragDrop.DoDragDrop(sceneNodesTreeView,
						sceneNodesTreeView.SelectedValue,
									DragDropEffects.Move);
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
		private void sceneNodesTreeView_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (e.OriginalSource is Grid && currentConfig.selectedSceneNodeView != null)
			{
				currentConfig.selectedSceneNodeView.isSelected = false;
				currentConfig.selectedSceneNodeView = null;
				sceneNodesTreeView.ItemsSource = null;
				sceneNodesTreeView.ItemsSource = currentConfig.sceneNodesView;

			}
			isNode = true;
		}

		//Sets selected treeview item
		private void sceneNodesTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			currentConfig.selectedSceneNodeView = (SceneNodeView)sceneNodesTreeView.SelectedItem;


			if (currentConfig.selectedSceneNodeView != null)
			{
				if (currentConfig.inputs.Contains(currentConfig.selectedSceneNodeView.node.tracker))
				{
					sceneNodeTrackerCb.SelectedItem = currentConfig.selectedSceneNodeView.node.tracker;
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
				Point currentPosition = e.GetPosition(sceneNodesTreeView);

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
			if (currentConfig.selectedInput != null)
			{
				var id = currentConfig.selectedInput.id;
				int selectedIndex = currentConfig.inputs.IndexOf(currentConfig.selectedInput);
				currentConfig.inputs.RemoveAt(selectedIndex);
				AppLogger.Add(currentConfig.selectedInput.type + " input " + id + " deleted");
				currentConfig.selectedInput = currentConfig.inputs.FirstOrDefault();
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

		private void addInput(string type)
		{
			if (type != null)
			{
				if (type == "tracker")
				{
					currentConfig.inputs.Add(new TrackerInput { id = "TrackerInputId", address = "TrackerInputName@127.0.0.1", locationX = "0", locationY = "0", locationZ = "0", rotationP = "0", rotationR = "0", rotationY = "0", front = "X", right = "Y", up = "-Z" });
				}
				else
				{
					InputDeviceType currentType = (InputDeviceType)System.Enum.Parse(typeof(InputDeviceType), type);
					currentConfig.inputs.Add(new BaseInput { id = "InputId", type = currentType, address = "InputName@127.0.0.1" });
				}
				currentConfig.selectedInput = currentConfig.inputs.LastOrDefault();
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