using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using netDxf;
using netDxf.Tables;
using PracticeIn3D.Models;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector2_SharpDX = SharpDX.Vector2;
using Vector3_SharpDX = SharpDX.Vector3;
using Vector2_netDxf = netDxf.Vector2;
using Vector3_netDxf = netDxf.Vector3;

namespace PracticeIn3D
{
	public static class Extensions
	{
		public static BitmapImage LoadImage(this string path)
		{
			return new BitmapImage(new Uri(new Uri("ms-appx://"), "/" + path));
		}

		public static IEnumerable<LayerModel> CreateLayerModels(this DxfDocument document)
		{
			foreach (Layer layer in document.Layers)
			{
				IEnumerable<PolylineModel> polylines = document.Polylines
					.Where(x => x.Layer.Equals(layer))
					.Select(x => new PolylineModel(x.Vertexes.Select(y => new Vector2((float) y.Position.X, (float) y.Position.Y))));
				IEnumerable<PolylineModel> lwPolylines = document.LwPolylines
					.Where(x => x.Layer.Equals(layer))
					.Select(x => new PolylineModel(x.Vertexes.Select(y => new Vector2((float) y.Position.X, (float) y.Position.Y))));

				yield return new LayerModel(
					name: layer.Name,
					polylines: polylines.Concat(lwPolylines));
			}
		}

		public static Vector3_SharpDX TransformTo3DSharpDx(this Vector2 vector2, float thirdValue)
		{
			return new Vector3_SharpDX(vector2.X, thirdValue, vector2.Y);
		}
	}
}
