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
using PracticeIn3D.Utilities;
using PracticeIn3D.VIewModels;
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
	public class ChooseLayerViewModel
	{
		public static ChooseLayerViewModel Create(LayerViewModel layer)
		{
			if (layer.Walls.Count < 1) return null;

			MainPageViewport viewport = new MainPageViewport
			{
				IsHitTestVisible = false,
				Walls = layer.Walls
			};

			viewport.FocusCameraOn(layer.Walls.Bounds());

			return new ChooseLayerViewModel
			{
				Layer = layer,
				Description = $"Кол-во объектов: {layer.Walls.Count}",
				Viewport = viewport
			};
		}

		public MainPageViewport Viewport { get; set; }
		public LayerViewModel Layer { get; set; }
		public string Description { get; set; }
	}

	public class ChooseLayerDialogViewModel
	{
		public ObservableCollection<ChooseLayerViewModel> LayersViewModels { get; }

		public ChooseLayerDialogViewModel(IEnumerable<ChooseLayerViewModel> layersViewModels)
		{
			LayersViewModels = new ObservableCollection<ChooseLayerViewModel>(layersViewModels);
		}
	}

	public sealed partial class ChooseLayerDialog : ContentDialog
	{
		public ChooseLayerDialogResult Result { get; private set; }
		public List<LayerViewModel> SelectedLayers { get; private set; }

		public ChooseLayerDialog(IEnumerable<ChooseLayerViewModel> layersViewModels)
		{
			this.InitializeComponent();
			
			this.SelectedLayers = new List<LayerViewModel>();
			this.DataContext = new ChooseLayerDialogViewModel(layersViewModels);
			this.ViewModelsListView.Loaded += (sender, args) =>
				this.ViewModelsListView.SelectedItem = this.ViewModelsListView.Items?.First();
		}

		private void Choose_ButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			Result = ChooseLayerDialogResult.Chosen;
			SelectedLayers = this.ViewModelsListView.SelectedItems
				.Where(x => x is ChooseLayerViewModel)
				.Cast<ChooseLayerViewModel>()
				.Select(x => x.Layer)
				.ToList();
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
