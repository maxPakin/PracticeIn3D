using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using HelixToolkit.UWP;
using SharpDX;
using Color = Windows.UI.Color;

namespace PracticeIn3D
{
	public class MainPageViewport : Viewport3DX
	{
		public MainPageViewport()
		{
			ShowViewCube = false;
			ShowCoordinateSystem = true;

			Camera = new PerspectiveCamera();
			EffectsManager = new DefaultEffectsManager();

			var builder = new MeshBuilder();
			builder.AddCube();

			Items.Clear();

			// Add Light
			var light = new PointLight3D()
			{
				Color = Colors.White,
				Position = Camera.Position
			};

			Items.Add(light);

			var cube = new MeshGeometryModel3D()
			{
				Geometry = builder.ToMesh(),
				Material = PhongMaterials.Green
			};

			// Add Cube
			Items.Add(cube);
		}
	}
}
