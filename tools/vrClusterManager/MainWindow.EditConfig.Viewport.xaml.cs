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
	}
}