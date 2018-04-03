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
	}
}