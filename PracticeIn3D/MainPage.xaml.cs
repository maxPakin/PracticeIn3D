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


        public MainPage()
        {
            this.InitializeComponent();

            Sandbox.ShowCoordinateSystem = true;

            // Create Cube
            var meshBuilder = new MeshBuilder();
            meshBuilder.AddBox(
	            center: Vector3_SharpDX.Zero,
	            xlength: 10,
	            ylength: 0.01f,
	            zlength: 10);
            var cubeMesh = meshBuilder.ToMesh();
            var cube = new MeshGeometryModel3D
            {
	            Geometry = cubeMesh,
	            Material = PhongMaterials.Gray
            };

            // Added Cube to Scene
            Sandbox.Items.Add(cube);

            // Starts Camera Movement
            Vector3_SharpDX cubeCenter = Vector3_SharpDX.Zero;
            foreach (Vector3_SharpDX cubeMeshPosition in cubeMesh.Positions)
	            cubeCenter += cubeMeshPosition;
            cubeCenter /= cubeMesh.Positions.Count;

            Vector3_SharpDX newPosition = new Vector3_SharpDX(5, 3, 4);
            Sandbox.Camera.AnimateTo(
	            newPosition: newPosition, 
	            newDirection: cubeCenter - newPosition,
                newUpDirection: Sandbox.Camera.UpDirection,
                animationTime: TimeSpan.FromSeconds(10).TotalMilliseconds);

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
            using Stream stream = await file.OpenStreamForReadAsync();

            // Load to the DxfDocument
            DxfDocument document = DxfDocument.Load(stream);
            if (document is null) return;

            IEnumerable<LayerModel> layerModels = document.CreateLayerModels();

            // Ask user about layer
            var chooseDialog = new ChooseLayerDialog(layerModels);
            await chooseDialog.ShowAsync();
            if (chooseDialog.Result == ChooseLayerDialogResult.Canceled) return;


            // Create Message Dialog
            MessageDialog messageDialog = new MessageDialog(
	            content: "Done")
            {
                DefaultCommandIndex = 0,
                CancelCommandIndex = 0,
                Commands = { new UICommand("Close") },
            };

            // Show Results
            await messageDialog.ShowAsync();
        }

        private async void MenuBar_File_Debug(object sender, RoutedEventArgs e)
        {
	        using (MemoryStream stream = Sandbox.RenderToBitmapStream())
	        {
		        // Create Message Dialog
		        MessageDialog messageDialog = new MessageDialog(
			        content: (stream is null).ToString())
		        {
			        DefaultCommandIndex = 0,
			        CancelCommandIndex = 0,
			        Commands = { new UICommand("Close") },
		        };

		        // Show Results
		        await messageDialog.ShowAsync();
            }
        }
    }
}
