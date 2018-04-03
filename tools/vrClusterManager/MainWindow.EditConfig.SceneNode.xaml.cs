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
	}
}