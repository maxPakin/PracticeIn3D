using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector2_SharpDX = SharpDX.Vector2;
using Vector3_SharpDX = SharpDX.Vector3;
using Vector2_netDxf = netDxf.Vector2;
using Vector3_netDxf = netDxf.Vector3;

namespace PracticeIn3D.Models
{
	public class LayerModel
	{
		public string Name { get; private set; }
		public List<PolylineModel> Polylines { get; private set; }

		public LayerModel(string name, IEnumerable<PolylineModel> polylines)
		{
			this.Name = name;
			this.Polylines = polylines.ToList();
		}
	}
}
