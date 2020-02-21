using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
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
using netDxf.Collections;
using netDxf.Entities;
using netDxf.Objects;
using netDxf.Tables;
using Newtonsoft.Json;
using PracticeIn3D.Dialogs;
using PracticeIn3D.Models;
using PracticeIn3D.Utilities;
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
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        // import .DXF format 
        // email 
        // Raspberry

        public static TextBlock LastDebugTextBlock;

        public MainPage()
        {
            this.InitializeComponent();

            Sandbox.AllowLeftRightRotation = true;
            Sandbox.AllowUpDownRotation = true;
            Sandbox.ShowCoordinateSystem = true;
            Sandbox.ShowCoordinateSystem = true;

            LastDebugTextBlock = DebugTextBlock;
        }

        private async void LoadFile(DxfDocument document)
        {
	        // Map to LayerModel
	        List<LayerModel> layerModels = document
		        .CreateLayerModels()
		        .Where(x => x.Polylines.Any())
		        .ToList();

			// Bad scenario
	        if (layerModels.Count < 1)
	        {
		        MessageDialog dialog = new MessageDialog(title: "Wrong file format...",
			        content: "There are no suitable layers in the loading file.\n" +
							 "See Help > Supported layers.");
		        await dialog.ShowAsync();
                return;
	        }

			// Good scenario
			List<LayerModel> selectedLayers;
			if (layerModels.Count < 2)
			{
				selectedLayers = new List<LayerModel> { layerModels.First() };
			}
	        else
	        {
				// Ask user about layer
				var chooseDialog = new ChooseLayerDialog(layerModels
					.Select(LayerViewModel.Create));
				await chooseDialog.ShowAsync();

				if (chooseDialog.Result == ChooseLayerDialogResult.Canceled) return;

				// Loading to scene
				selectedLayers = layerModels
					.Where(x => chooseDialog.SelectedLayers.Contains(x.Name))
					.ToList();
			}

			List<PolylineModel> polylines = selectedLayers
				.SelectMany(x => x.Polylines)
				.ToList();

			MeshGeometryModel3D newElement = GeometryHelper
		        .CreateGeometryFromLayerModel(polylines, App.WallHeight, PhongMaterials.Gray);

			
			Sandbox.ClearLoadedItems();
			Sandbox.Items.Add(newElement);
	        Sandbox.FocusCameraOn(newElement);
        }

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

        private void MenuBar_Camera_LookTop(object sender, RoutedEventArgs e)
        {
	        Element3D first = Sandbox.Items
		        .FirstOrDefault(x => x is MeshGeometryModel3D);
	        if (first is null) return;

	        Sandbox.FocusCameraOn(first, fromTop: true);
        }

        private void MenuBar_Camera_LookAround(object sender, RoutedEventArgs e)
        {
	        Element3D first = Sandbox.Items
		        .FirstOrDefault(x => x is MeshGeometryModel3D);
            if (first is null) return;

            Sandbox.FocusCameraOn(first);
        }

        private async void MenuBar_Help_SupportedLayers(object sender, RoutedEventArgs e)
        {
			MessageDialog dialog = new MessageDialog(title: "Supported layers",
				content: "> Only DXF files are used for download.\n" +
						 "> The layers where the polylines are present will be selected from the file.\n" +
						 "> Polylines can be closed or open (open will be closed automatically).\n" + 
				         "> Please note that polylines should not have holes, as this has not yet been implemented.");
			await dialog.ShowAsync();
		}
    }
}
