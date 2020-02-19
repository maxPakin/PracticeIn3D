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
	public class PolylineModel
	{
		public List<Vector2> Points { get; set; }

		public PolylineModel(IEnumerable<Vector2> points)
		{
			List<Vector2> pointsList = points.ToList();

			if (pointsList.First().Equals(pointsList.Last()))
				this.Points = pointsList.Skip(1).ToList();
			else
				this.Points = pointsList;
		}
	}
}
