using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Input;
using HelixToolkit.UWP;
using SharpDX;
using Color = Windows.UI.Color;
using Point = Windows.Foundation.Point;

namespace PracticeIn3D
{
	public class MainPageViewport : Viewport3DX
	{
		private readonly PointLight3D _light;

		public MainPageViewport()
		{
			ShowViewCube = false;
			ShowCoordinateSystem = false;

			Camera = new PerspectiveCamera();
			EffectsManager = new DefaultEffectsManager();

			// Create Light
			_light = new PointLight3D
			{
				Color = Colors.White,
				Position = new Vector3(4, 3, 3)
			};

			Items.Add(_light);
		}

		protected override void OnManipulationDelta(ManipulationDeltaRoutedEventArgs e)
		{
			_light.Position = Camera.Position;
			base.OnManipulationDelta(e);
		}
	}
}
