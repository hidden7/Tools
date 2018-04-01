using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vrClusterManager.Settings;
using vrClusterManager.Config;

namespace vrClusterManager
{
	public static class ConfigParser
	{
		//Config file parser
		public static VRConfig Parse(string filePath)
		{
			VRConfig config = new VRConfig();

			try
			{
				foreach (string origLine in File.ReadLines(filePath))
				{
					string line = origLine.Trim().ToLower();
					if (line == string.Empty || line.First() == '#')
					{
						// Skip this line
					}
					else if (line.StartsWith("[cluster_node]"))
					{
						config.ClusterNodeParse(line);
					}
					else if (line.StartsWith("[scene_node]"))
					{
						config.SceneNodeParse(line);
					}
					else if (line.StartsWith("[screen]"))
					{
						config.ScreenParse(line);
					}
					else if (line.StartsWith("[viewport]"))
					{
						config.ViewportParse(line);
					}
					else if (line.StartsWith("[camera]"))
					{
						config.CameraParse(line);
					}
					else if (line.StartsWith("[input]"))
					{
						config.InputsParse(line);
					}
					else if (line.StartsWith("[general]"))
					{
						config.GeneralParse(line);
					}
					else if (line.StartsWith("[stereo]"))
					{
						config.StereoParse(line);
					}
					else if (line.StartsWith("[debug]"))
					{
						config.DebugParse(line);
					}
					else if (line.StartsWith("[render]"))
					{
					}
					else if (line.StartsWith("[custom]"))
					{
					}
				}

				config.sceneNodesView = config.ConvertSceneNodeList(config.sceneNodes);
				config.name = Path.GetFileNameWithoutExtension(filePath);

				AppLogger.Add("Config " + config.name + " parsed successfully");
			}
			catch (FileNotFoundException)
			{
				AppLogger.Add("Path not found: " + filePath);
			}
			catch (ArgumentException)
			{
				AppLogger.Add("Couldn't parse file: " + filePath);
			}

			return config;
		}
	}
}
