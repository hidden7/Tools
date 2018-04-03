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