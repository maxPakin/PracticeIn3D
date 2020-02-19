using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using HelixToolkit.UWP;
using PracticeIn3D.Models;
using SharpDX;
using SharpDX.WIC;
using Image = System.Drawing.Image;
using Size = Windows.Foundation.Size;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector2_SharpDX = SharpDX.Vector2;
using Vector3_SharpDX = SharpDX.Vector3;
using Vector2_netDxf = netDxf.Vector2;
using Vector3_netDxf = netDxf.Vector3;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PracticeIn3D.Dialogs
{
	public enum ChooseLayerDialogResult
	{
		Chosen,
		Canceled
	}
	public class LayerViewModel
	{
		public MainPageViewport Viewport { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
	}

	public class ChooseLayerDialogViewModel
	{
		private readonly ObservableCollection<LayerViewModel> _layersViewModels;
		public ObservableCollection<LayerViewModel> LayersViewModels => _layersViewModels;

		public ChooseLayerDialogViewModel(IEnumerable<LayerModel> layers)
		{
			_layersViewModels = new ObservableCollection<LayerViewModel>();
			DisplayRealViewModels(_layersViewModels, layers.ToList());
		}

		private static void DisplayRealViewModels(
			ObservableCollection<LayerViewModel> viewModelsList,
			List<LayerModel> layers)
		{
			foreach (LayerModel layer in layers)
			{
				LayerViewModel viewModel = new LayerViewModel
				{
					Name = layer.Name,
					Description = $"Кол-во объектов: {layer.Polylines.Count}",
					Viewport = CreatePreview(layer)
				};
				viewModelsList.Add(viewModel);
			}
		}

		private static MainPageViewport CreatePreview(LayerModel layer)
		{
			MeshBuilder builder = new MeshBuilder();
			foreach (PolylineModel polyline in layer.Polylines)
			{
				if (polyline.Points.Count < 2) continue;

				List<Vector3_SharpDX> downPolygon = new List<Vector3_SharpDX>();
				List<Vector3_SharpDX> upperPolygon = new List<Vector3_SharpDX>();

				downPolygon.Add(polyline.Points.First().TransformTo3DSharpDx(0));
				upperPolygon.Add(polyline.Points.First().TransformTo3DSharpDx(App.WallHeight));
				for (int i = 1; i < polyline.Points.Count; i++)
				{
					Vector3_SharpDX currentPoint = polyline.Points[i].TransformTo3DSharpDx(0);
					Vector3_SharpDX currentUpperPoint = polyline.Points[i].TransformTo3DSharpDx(App.WallHeight);

					builder.AddQuad(
						downPolygon.Last(),
						new Vector3_SharpDX(currentPoint.X, currentPoint.Y, currentPoint.Z),
						new Vector3_SharpDX(currentUpperPoint.X, currentUpperPoint.Y, currentUpperPoint.Z),
						upperPolygon.Last());

					downPolygon.Add(currentPoint);
					upperPolygon.Add(currentUpperPoint);
				}

				// Create cycle
				builder.AddQuad(
					downPolygon.Last(),
					downPolygon.First(),
					upperPolygon.First(),
					upperPolygon.Last());

				// Adding Upper Part
				builder.AddTriangleFan(
					fanPositions: upperPolygon, 
					fanNormals: new List<Vector3_SharpDX> { Vector3_SharpDX.UnitY }, 
					fanTextureCoordinates: new List<Vector2_SharpDX> { Vector2_SharpDX.UnitY } );

				// Adding Down Upper
				builder.AddTriangleFan(
					fanPositions: downPolygon,
					fanNormals: new List<Vector3_SharpDX> { -Vector3_SharpDX.UnitY },
					fanTextureCoordinates: new List<Vector2_SharpDX> { -Vector2_SharpDX.UnitY });
			}

			MeshGeometry3D polyWall = builder.ToMesh();
			BoundingSphere boundSphere = polyWall.BoundingSphere;

			MainPageViewport viewport = new MainPageViewport();
			viewport.Items.Add(new MeshGeometryModel3D()
			{
				Geometry = polyWall,
				Material = PhongMaterials.Gray
			});

			Vector3_SharpDX newDirection = new Vector3_SharpDX(boundSphere.Radius, boundSphere.Radius, -boundSphere.Radius);
			Vector3_SharpDX newPosition = boundSphere.Center + newDirection;
			viewport.Camera.AnimateTo(
				newPosition: newPosition,
				newDirection: newDirection,
				newUpDirection: viewport.Camera.UpDirection,
				animationTime: TimeSpan.FromSeconds(10).TotalMilliseconds);

			return viewport;
		}
	}

	public sealed partial class ChooseLayerDialog : ContentDialog
	{
		public ChooseLayerDialogResult Result { get; private set; }
		public string SelectedLayerName { get; private set; }

		public ChooseLayerDialog(IEnumerable<LayerModel> layers)
		{
			this.InitializeComponent();
			
			this.DataContext = new ChooseLayerDialogViewModel(layers);
			this.ViewModelsListView.Loaded += (sender, args) =>
				this.ViewModelsListView.SelectedItem = this.ViewModelsListView.Items?.First();
		}

		private void Choose_ButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			Result = ChooseLayerDialogResult.Chosen;
			SelectedLayerName = (this.ViewModelsListView.SelectedItem as LayerViewModel).Name;
		}

		private void Close_ButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			Result = ChooseLayerDialogResult.Canceled;
		}

		private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			IsPrimaryButtonEnabled = ViewModelsListView.SelectedItems.Count > 0;
		}
	}
}
