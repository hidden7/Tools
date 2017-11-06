using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vrClusterManager.Config
{
	class DynamicCamera : Camera
	{
		public BaseInput tracker { get; set; }
		public string trackerChannel { get; set; }
	}
}
