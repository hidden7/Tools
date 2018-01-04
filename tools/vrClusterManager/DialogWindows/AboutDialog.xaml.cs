using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace vrClusterManager
{
	/// <summary>
	/// Interaction logic for InfoDialog.xaml
	/// </summary>
	public partial class AboutDialog : Window
	{
		public AboutDialog()
		{
			InitializeComponent();

			UpdateContent();
		}

		private void UpdateContent()
		{
			MessageTextBlock.Text = 
				"VR Cluster management application\n" +
				"version " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + "\n" +
				"\u00a9 Pixela Labs LLC. All rights reserved.\n" +
				"http://vrcluster.io/";
		}

		private void OkBtn_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
