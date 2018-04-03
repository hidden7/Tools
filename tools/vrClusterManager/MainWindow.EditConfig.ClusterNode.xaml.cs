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
	}
}