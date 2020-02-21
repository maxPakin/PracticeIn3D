using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelixToolkit.UWP;
using Poly2Tri;
using Poly2Tri.Triangulation;
using Poly2Tri.Triangulation.Delaunay;
using PracticeIn3D.Models;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector2_SharpDX = SharpDX.Vector2;
using Vector3_SharpDX = SharpDX.Vector3;
using Vector2_netDxf = netDxf.Vector2;
using Vector3_netDxf = netDxf.Vector3;
using Poly2Tri.Triangulation.Polygon;
using Poly2Tri.Utility;
using Polygon_Poly2Tri = Poly2Tri.Triangulation.Polygon.Polygon;


namespace PracticeIn3D.Utilities
{
	public static class GeometryHelper
	{
		public static Vector3 AddZ(this Vector2 vector, float z)
		{
			return new Vector3(vector, z);
		}

		public static Vector3_SharpDX Map(this Vector3 vector)
		{
			return new Vector3_SharpDX(vector.X, vector.Y, vector.Z);
		}

		public static MeshGeometryModel3D CreateGeometryFromLayerModel(List<PolylineModel> polylines, float height, Material material)
		{
			MeshBuilder builder = new MeshBuilder();
			
			foreach (List<Vector2> points in polylines.Select(x => x.Points))
			{
				if (points.Count < 3) continue;

				// Create poly now to indicate winding order
				Polygon_Poly2Tri upPolygon = new Polygon_Poly2Tri(
					points: points.Select(point => new PolygonPoint(point.X, point.Y)));

				List<Vector2> pointByClockwise =
					upPolygon.WindingOrder == Point2DList.WindingOrderType.AntiClockwise
						? points.AsEnumerable().Reverse().ToList()
						: points;

				// Adding walls
				Vector3 firstUpPoint = pointByClockwise.First().AddZ(height);
				Vector3 firstDownPoint = pointByClockwise.First().AddZ(0);
				Vector3 lastUpPoint = firstUpPoint;
				Vector3 lastDownPoint = firstDownPoint;
				for (int i = 1; i < pointByClockwise.Count; i++)
				{
					Vector3 currentUpPoint = pointByClockwise[i].AddZ(height);
					Vector3 currentDownPoint = pointByClockwise[i].AddZ(0);

					builder.AddQuad(
						currentDownPoint.Map(),
						lastDownPoint.Map(),
						lastUpPoint.Map(),
						currentUpPoint.Map());

					lastUpPoint = currentUpPoint;
					lastDownPoint = currentDownPoint;
				}

				// Create cycle
				builder.AddQuad(
					firstDownPoint.Map(),
					lastDownPoint.Map(),
					lastUpPoint.Map(),
					firstUpPoint.Map());

				// Adding Upper Part
				P2T.Triangulate(upPolygon);
				foreach (var trianglePoints in upPolygon.Triangles.Select(x => x.Points))
				{
					builder.AddTriangle(
						p0: new Vector3_SharpDX((float)trianglePoints.Item0.X, (float)trianglePoints.Item0.Y, height),
						p1: new Vector3_SharpDX((float)trianglePoints.Item1.X, (float)trianglePoints.Item1.Y, height),
						p2: new Vector3_SharpDX((float)trianglePoints.Item2.X, (float)trianglePoints.Item2.Y, height));
				}
			}

			MeshGeometry3D polyWall = builder.ToMesh();
			MeshGeometryModel3D element = new MeshGeometryModel3D()
			{
				Geometry = polyWall,
				Material = material
			};

			return element;
		}
	}
}
