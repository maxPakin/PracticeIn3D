using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using HelixToolkit.UWP;
using PracticeIn3D.Models;
using SharpDX;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector2_SharpDX = SharpDX.Vector2;
using Vector3_SharpDX = SharpDX.Vector3;
using Vector2_netDxf = netDxf.Vector2;
using Vector3_netDxf = netDxf.Vector3;

namespace PracticeIn3D
{
	public class MainPageViewport : Viewport3DX
	{
		private readonly PointLight3D _light;
		private readonly Vector3_SharpDX _defaultCameraUpDirection = Vector3_SharpDX.UnitZ;

		public MainPageViewport()
		{
			ShowViewCube = false;
			ShowCoordinateSystem = false;
			AllowLeftRightRotation = false;
			AllowUpDownRotation = false;
			CameraRotationMode = CameraRotationMode.Turnball;
			ZoomDistanceLimitNear = 0.001f;
			ZoomDistanceLimitFar = 400f;

			Camera = new PerspectiveCamera()
			{
				Position = Vector3_SharpDX.Zero,
				LookDirection = - Vector3_SharpDX.UnitX
				                - Vector3_SharpDX.UnitY
				                - Vector3_SharpDX.UnitZ,
				UpDirection = _defaultCameraUpDirection
			};
			
			EffectsManager = new DefaultEffectsManager();

			// Create Light
			_light = new PointLight3D { Color = Colors.White, Range = 1000};
			Items.Add(_light);
			MarkAsNoLoaded(_light);

			OnRendered += OnOnRendered;
		}

		private void OnOnRendered(object sender, EventArgs e)
		{
			_light.Position = Camera.Position;
		}

		public void FocusCameraOn(Element3D element, bool fromTop = false)
		{
			BoundingBox bounds = element.Bounds;
			float distance = new[] { bounds.Size.X, bounds.Size.Y, bounds.Size.Z }.Max();

			Vector3_SharpDX viewVector;
			if (fromTop)
			{
				// We need to keep the direction of the camera so that it does not flip
				int xSign = Math.Sign(Camera.LookDirection.X);
				int ySign = Math.Sign(Camera.LookDirection.Y);
				// We need to set 0.0001f because if I set just zero in X and Y, the Camera stops working
				viewVector = new Vector3_SharpDX(x: 0f, y: -0.1f * distance, z: distance * 1.5f);
			}
			else
			{
				viewVector = new Vector3_SharpDX(x: -distance, y: -distance, z: distance);
			}

			Camera.AnimateTo(
				newPosition: bounds.Center + viewVector,
				newDirection: -viewVector,
				newUpDirection: _defaultCameraUpDirection,
				animationTime: TimeSpan.FromSeconds(2).TotalMilliseconds);
		}


		private readonly List<Element3D> _noLoadedItems = new List<Element3D>();
		public void MarkAsNoLoaded(Element3D item)
		{
			_noLoadedItems.Add(item);
		}

		public void ClearLoadedItems()
		{
			List<Element3D> listToRemove = Items
				.Where(x => !_noLoadedItems.Contains(x))
				.ToList();

			foreach (Element3D removable in listToRemove)
				Items.Remove(removable);
		}
	}
}
