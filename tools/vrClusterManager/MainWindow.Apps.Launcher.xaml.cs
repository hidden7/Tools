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
	}
}