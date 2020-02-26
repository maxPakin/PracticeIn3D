using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using HelixToolkit.UWP;
using netDxf;
using netDxf.Entities;
using netDxf.Objects;
using netDxf.Tables;
using Newtonsoft.Json;
using PracticeIn3D.Dialogs;
using PracticeIn3D.Enums;
using PracticeIn3D.Models;
using PracticeIn3D.Utilities;
using PracticeIn3D.VIewModels;
using SharpDX.Direct3D11;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector2_SharpDX = SharpDX.Vector2;
using Vector3_SharpDX = SharpDX.Vector3;
using Vector2_netDxf = netDxf.Vector2;
using Vector3_netDxf = netDxf.Vector3;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PracticeIn3D
{
	public class MainPageViewModel
	{
		public float SavedCameraX { get; set; }
		public float SavedCameraY { get; set; }
		public float SavedCameraZ { get; set; }
		public float SavedCameraRotatingX { get; set; }
		public float SavedCameraRotatingY { get; set; }
		public float SavedCameraRotatingZ { get; set; }
		public ObservableCollection<SignalingViewModel> Signalings { get; set; }
		public ObservableCollection<WallViewModel> Walls { get; set; }

		public MainPageViewModel(
			float savedCameraX, 
			float savedCameraY, 
			float savedCameraZ, 
			float savedCameraRotatingX, 
			float savedCameraRotatingY, 
			float savedCameraRotatingZ,
			IEnumerable<SignalingViewModel> signalings,
			IEnumerable<WallViewModel> walls)
		{
			this.SavedCameraX = savedCameraX;
			this.SavedCameraY = savedCameraY;
			this.SavedCameraZ = savedCameraZ;
			this.SavedCameraRotatingX = savedCameraRotatingX;
			this.SavedCameraRotatingY = savedCameraRotatingY;
			this.SavedCameraRotatingZ = savedCameraRotatingZ;
			this.Signalings = new ObservableCollection<SignalingViewModel>(signalings);
			this.Walls = new ObservableCollection<WallViewModel>(walls);
		}

		public MainPageViewModel()
			: this(
				savedCameraX: 10, 
				savedCameraY: 10, 
				savedCameraZ: 10, 
				savedCameraRotatingX: -1, 
				savedCameraRotatingY: -1, 
				savedCameraRotatingZ: -1, 
				signalings: Enumerable.Empty<SignalingViewModel>(), 
				walls: Enumerable.Empty<WallViewModel>())
		{

		}


	}

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
	    public MainPageViewModel ViewModel { get; set; }

	    public MainPage()
        {
            this.InitializeComponent();

            MainPageViewModel viewModel = null ?? new MainPageViewModel();
            Sandbox.Camera.Position = new Vector3_SharpDX(
	            x: viewModel.SavedCameraX,
	            y: viewModel.SavedCameraY,
	            z: viewModel.SavedCameraZ);
            Sandbox.Camera.LookDirection = new Vector3_SharpDX(
	            x: viewModel.SavedCameraRotatingX,
	            y: viewModel.SavedCameraRotatingY,
	            z: viewModel.SavedCameraRotatingZ);

            Sandbox.ShowCoordinateSystem = true;
            Sandbox.OnMouse3DDown += Sandbox_OnMouseDown;
            Sandbox.Signalings = viewModel.Signalings;
            Sandbox.Walls = viewModel.Walls;

			this.ViewModel = viewModel;
			this.DataContext = viewModel;
		}



	    private async void RequestAddSignaling(Vector3_SharpDX position)
	    {
		    ConfigureSignalingDialog dialog = new ConfigureSignalingDialog(
			    isAdding: true,
			    signaling: new SignalingViewModel(
				    centerX: position.X,
				    centerY: position.Y,
				    centerZ: position.Z));;
			await dialog.ShowAsync();

			if (dialog.Result == ConfigureSignalingDialogResult.Canceled) return;

			ViewModel.Signalings.Add(dialog.ViewModel);
	    }

		private void RequestAddCamera(Vector3_SharpDX position)
		{

		}

		private async void LoadFile(DxfDocument document)
        {
	        // Map to LayerModel
	        List<LayerViewModel> validLayers = document
		        .CreateLayerModels()
		        .Where(x => x.Polylines.Any())
		        .Select(x => new LayerViewModel()
		        {
					Name = x.Name,
					Walls = new ObservableCollection<WallViewModel>(x.Polylines
						.Select(y => new WallViewModel()
						{
							Height = App.WallHeight,
							Points = y.Points
						}))
		        })
		        .ToList();

			// Bad scenario
	        if (validLayers.Count < 1)
	        {
		        MessageDialog dialog = new MessageDialog(title: "Wrong file format...",
			        content: "There are no suitable layers in the loading file.\n" +
							 "See Help > Supported layers.");
		        await dialog.ShowAsync();
                return;
	        }

			// Good scenario
			List<LayerViewModel> selectedLayers;
			if (validLayers.Count < 2)
			{
				selectedLayers = new List<LayerViewModel> { validLayers.First() };
			}
	        else
	        {
				// Ask user about layer
				var dialog = new ChooseLayerDialog(validLayers
					.Select(ChooseLayerViewModel.Create));
				await dialog.ShowAsync();

				if (dialog.Result == ChooseLayerDialogResult.Canceled) return;

				// Loading to scene
				selectedLayers = dialog.SelectedLayers;
	        }

			List<WallViewModel> walls = selectedLayers
				.SelectMany(x => x.Walls)
				.ToList();

			ViewModel.Walls.Clear();
			ViewModel.Signalings.Clear();

			ViewModel.Walls.AddRange(walls);
			Sandbox.FocusCameraOn(ViewModel.Walls.Bounds());
        }

		#region MenuBar
		#region File
		private async void MenuBar_File_Open(object sender, RoutedEventArgs e)
		{
			// Create picker
			FileOpenPicker picker = new FileOpenPicker()
			{
				CommitButtonText = "Open",
				SuggestedStartLocation = PickerLocationId.Desktop,
				ViewMode = PickerViewMode.Thumbnail
			};

			// Create filter for picker
			picker.FileTypeFilter.Add(".dxf");

			// Pick a file and exit if file not picked
			StorageFile file = await picker.PickSingleFileAsync();
			if (file is null) return;

			// Create stream to the file
			DxfDocument document;
			using (Stream stream = await file.OpenStreamForReadAsync())
			{
				// Load to the DxfDocument
				document = DxfDocument.Load(stream);
				if (document is null) return;
			}

			LoadFile(document);
		}

		private async void MenuBar_File_OpenSample(object sender, RoutedEventArgs e)
		{
			StorageFile storageFile = await StorageFile
				.GetFileFromApplicationUriAsync("Resources/Sample.dxf".CreateUriToResource());

			// Create stream to the file
			DxfDocument document;
			using (Stream stream = await storageFile.OpenStreamForReadAsync())
			{
				// Load to the DxfDocument
				document = DxfDocument.Load(stream);
				if (document is null) return;
			}

			LoadFile(document);
		}
		#endregion
		#region Camera
		private void MenuBar_Camera_LookTop(object sender, RoutedEventArgs e)
		{
			if (!Sandbox.Walls.Any()) return;
			Sandbox.FocusCameraOn(ViewModel.Walls.Bounds(), fromTop: true);
		}

		private void MenuBar_Camera_LookAround(object sender, RoutedEventArgs e)
		{
			if (!Sandbox.Walls.Any()) return;
			Sandbox.FocusCameraOn(ViewModel.Walls.Bounds());
		}
		#endregion
		#region Help
		private async void MenuBar_Help_SupportedLayers(object sender, RoutedEventArgs e)
		{
			MessageDialog dialog = new MessageDialog(title: "Supported layers",
				content: "> Only DXF files are used for download.\n" +
				         "> The layers where the polylines are present will be selected from the file.\n" +
				         "> Polylines can be closed or open (open will be closed automatically).\n" +
				         "> Please note that polylines should not have holes, as this has not yet been implemented.");
			await dialog.ShowAsync();
		}
		#endregion
		#endregion

		#region Flyout
		private void Sandbox_OnMouseDown(object sender, MouseDown3DEventArgs e)
		{
			if (e.OriginalInputEventArgs.Pointer.PointerDeviceType != PointerDeviceType.Mouse) return;
			PointerPointProperties properties = e.OriginalInputEventArgs.GetCurrentPoint(this).Properties;

			if (properties.IsLeftButtonPressed)
			{
				// Left button pressed
				// Ignore
			}
			else if (properties.IsRightButtonPressed)
			{
				// Right button pressed
				if (e.HitTestResult is null) return;

				CallFlyout(
					targetElement: sender as UIElement, 
					position: e.Position,
					position3D: e.HitTestResult.PointHit);
			}
		}

		private void CallFlyout(UIElement targetElement, Windows.Foundation.Point position, Vector3_SharpDX position3D)
		{
			MenuFlyoutItem signalingItem = new MenuFlyoutItem { Text = "Add signaling" };
			signalingItem.Click += (a, b) => RequestAddSignaling(position3D);
			MenuFlyoutItem cameraItem = new MenuFlyoutItem { Text = "Add camera", IsEnabled = false };
			cameraItem.Click += (a, b) => RequestAddCamera(position3D);

			MenuFlyout myFlyout = new MenuFlyout()
			{
				Items = {
					signalingItem,
					cameraItem
				}
			};

			myFlyout.ShowAt(targetElement, position);
		}
		#endregion
    }
}
