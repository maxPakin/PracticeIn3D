using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using Vector2 = System.Numerics.Vector2;

namespace PracticeIn3D.VIewModels
{
	public class WallViewModel
	{
		public string Guid { get; } = System.Guid.NewGuid().ToString();
		public float Height { get; set; }
		public List<Vector2> Points { get; set; }

		public BoundingBox Bounds => new BoundingBox(
			minimum: new Vector3(
				x: Points.Select(y => y.X).Min(),
				y: Points.Select(y => y.Y).Min(),
				z: 0),
			maximum: new Vector3(
				x: Points.Select(y => y.X).Min(),
				y: Points.Select(y => y.Y).Max(),
				z: Height));
	}
}
