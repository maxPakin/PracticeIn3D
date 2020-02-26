using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.UI;
using HelixToolkit.UWP;
using PracticeIn3D.Enums;
using PracticeIn3D.Models;
using PracticeIn3D.Utilities;
using PracticeIn3D.VIewModels;
using SharpDX;
using Point = Windows.Foundation.Point;
using Point_SharpDx = SharpDX.Point;
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

		private ObservableCollection<SignalingViewModel> _signalings = new ObservableCollection<SignalingViewModel>();
		public ObservableCollection<SignalingViewModel> Signalings
		{
			get => _signalings;
			set
			{
				if (_signalings == value) return;
				_signalings.CollectionChanged -= OnSignalingsCollectionChanged;

				_signalings = value;
				_signalings.CollectionChanged += OnSignalingsCollectionChanged;
			}
		}

		private ObservableCollection<WallViewModel> _walls = new ObservableCollection<WallViewModel>();
		public ObservableCollection<WallViewModel> Walls
		{
			get => _walls;
			set
			{
				if (_walls == value) return;
				_walls.CollectionChanged -= OnWallsCollectionChanged;

				_walls = value;
				_walls.CollectionChanged += OnWallsCollectionChanged;
			}
		}

		public MainPageViewport()
		{
			this.ShowViewCube = false;
			this.ShowCoordinateSystem = false;
			this.AllowLeftRightRotation = false;
			this.AllowUpDownRotation = false;
			this.IsPanEnabled = false;
			this.CameraRotationMode = CameraRotationMode.Turnball;
			this.ZoomDistanceLimitNear = 100f;
			this.ZoomDistanceLimitFar = 500f;

			this.Camera = new PerspectiveCamera()
			{
				Position = Vector3_SharpDX.Zero,
				LookDirection = - Vector3_SharpDX.UnitX
				                - Vector3_SharpDX.UnitY
				                - Vector3_SharpDX.UnitZ,
				UpDirection = _defaultCameraUpDirection
			};

			this.EffectsManager = new DefaultEffectsManager();

			// Create Light
			this._light = new PointLight3D { Color = Colors.White, Range = 1000};
			this.Items.Add(_light);

			this.OnRendered += Event_OnRendered;
			this.OnMouse3DDown += Event_OnMouseDown;
			this.OnMouse3DMove += Event_OnMouseMove;
			this.OnMouse3DUp += Event_OnMouseUp;
		}

		private Point? _lastMousePoint = null;
		private void Event_OnMouseDown(object sender, MouseDown3DEventArgs e)
		{
			_lastMousePoint = e.Position;
			e.OriginalInputEventArgs.Handled = true;
		}

		private void Event_OnMouseMove(object sender, MouseMove3DEventArgs e)
		{
			if (_lastMousePoint is null) return;
			Point prevPoint = _lastMousePoint.Value;

			double dx = e.Position.X - prevPoint.X;
			double dy = e.Position.Y - prevPoint.Y;

			Vector3_SharpDX lookDirection = Camera.LookDirection;
			lookDirection.Normalize();

			double horizontalMovementX = lookDirection.X * Math.Cos(Math.PI / 2) - lookDirection.Y * Math.Sin(Math.PI / 2);
			double horizontalMovementY = lookDirection.X * Math.Sin(Math.PI / 2) + lookDirection.Y * Math.Cos(Math.PI / 2);

			Camera.Position += new Vector3_SharpDX(
				x: (float)(horizontalMovementX * dx * this.LeftRightPanSensitivity + lookDirection.X * dy),
				y: (float)(horizontalMovementY * dx * this.LeftRightPanSensitivity + lookDirection.Y * dy),
				z: 0);

			_lastMousePoint = e.Position;

			e.OriginalInputEventArgs.Handled = true;
		}

		private void Event_OnMouseUp(object sender, MouseUp3DEventArgs e)
		{
			_lastMousePoint = null;
			e.OriginalInputEventArgs.Handled = true;
		}


		private void Event_OnRendered(object sender, EventArgs e)
		{
			_light.Position = Camera.Position;
		}

		public void FocusCameraOn(BoundingBox bounds, bool fromTop = false)
		{
			float distance = new[] { bounds.Size.X, bounds.Size.Y, bounds.Size.Z }.Max();

			Vector3_SharpDX viewVector;
			if (fromTop)
				// We need to set 0.0001f because if I set just zero in X and Y, the Camera stops working
				viewVector = new Vector3_SharpDX(x: 0f, y: -0.1f * distance, z: distance * 1.5f);
			else
				viewVector = new Vector3_SharpDX(x: -distance, y: -distance, z: distance);

			Camera.AnimateTo(
				newPosition: bounds.Center + viewVector,
				newDirection: -viewVector,
				newUpDirection: _defaultCameraUpDirection,
				animationTime: TimeSpan.FromSeconds(1.5d).TotalMilliseconds);
		}


		private void OnSignalingsCollectionChanged(object _, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					{
						IEnumerable<SignalingViewModel> newViewModels = e.NewItems.Cast<SignalingViewModel>();
						foreach (SignalingViewModel newViewModel in newViewModels)
						{
							MeshBuilder builder = new MeshBuilder();
							builder.AddEllipsoid(center: new Vector3_SharpDX(
								x: newViewModel.CenterX, 
								y: newViewModel.CenterY, 
								z: newViewModel.CenterZ), 
								radiusx: 1, 
								radiusy: 1, 
								radiusz: 1);
							MeshGeometry3D mesh = builder.ToMesh();
							MeshGeometryModel3D element = new MeshGeometryModel3D
							{
								Name = newViewModel.Name, 
								Geometry = mesh,
								Material = newViewModel.Type == SignalingType.Standard
									? PhongMaterials.Red 
									: PhongMaterials.Green
							};

							element.Name = $"Signaling.{newViewModel.Name}";

							Items.Add(element);
						}

						break;
					}
				case NotifyCollectionChangedAction.Remove:
					{
						IEnumerable<SignalingViewModel> removedViewModels = e.OldItems.Cast<SignalingViewModel>();
						foreach (SignalingViewModel removedViewModel in removedViewModels)
						{
							Element3D removedElement = Items.FirstOrDefault(x => x.Name.EndsWith(removedViewModel.Name));
							if (removedElement is null) return;
							Items.Remove(removedElement);
						}

						break;
					}
				case NotifyCollectionChangedAction.Reset:
					{
						List<Element3D> removedElements = Items.Where(x => x.Name.StartsWith("Signaling.")).ToList();
						foreach (Element3D removedElement in removedElements)
						{
							Items.Remove(removedElement);
						}

						break;
					}
				default:
					throw new NotImplementedException($"CODE_33: {e.Action}");
			}

			InvalidateRender();
		}
		private void OnWallsCollectionChanged(object _, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					{
						IEnumerable<WallViewModel> newViewModels = e.NewItems.Cast<WallViewModel>();
						foreach (WallViewModel newViewModel in newViewModels)
						{
							MeshGeometryModel3D polygon = GeometryHelper.Create3DPolygon(
								points: newViewModel.Points,
								height: newViewModel.Height,
								material: PhongMaterials.Gray);

							polygon.Name = $"Wall.{newViewModel.Guid}";

							Items.Add(polygon);
						}

						break;
					}
				case NotifyCollectionChangedAction.Remove:
					{
						IEnumerable<WallViewModel> removedViewModels = e.OldItems.Cast<WallViewModel>();
						foreach (WallViewModel removedViewModel in removedViewModels)
						{
							Element3D removedElement = Items.FirstOrDefault(x => x.Name.EndsWith(removedViewModel.Guid));
							if (removedElement is null) return;
							Items.Remove(removedElement);
						}

						break;
					}
				case NotifyCollectionChangedAction.Reset: 
					{
						List<Element3D> removedElements = Items.Where(x => x.Name.StartsWith("Wall.")).ToList();
						foreach (Element3D removedElement in removedElements)
						{
							Items.Remove(removedElement);
						}

						break;
					}
				default:
					throw new NotImplementedException($"CODE_255: {e.Action}");
			}

			InvalidateRender();
		}
	}
}
