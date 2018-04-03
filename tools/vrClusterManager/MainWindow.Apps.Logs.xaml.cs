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
	}
}