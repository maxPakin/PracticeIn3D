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
using netDxf.Objects;
using Newtonsoft.Json;
using SharpDX.Direct3D11;
using Vector3 = SharpDX.Vector3;

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

            // Create Light
            var light = new PointLight3D
            {
                Color = Colors.White,
                Position = new Vector3(4, 3, 3)
            };

            // Added Cube to Scene
            Sandbox.Items.Add(light);

            // Create Cube
            var meshBuilder = new MeshBuilder();
            meshBuilder.AddCube();
            var cubeMesh = meshBuilder.ToMesh();
            var cube = new MeshGeometryModel3D
            {
	            Geometry = cubeMesh,
	            Material = PhongMaterials.Red
            };

            // Added Cube to Scene
            Sandbox.Items.Add(cube);

            // Starts Camera Movement
            Vector3 cubeCenter = Vector3.Zero;
            foreach (Vector3 cubeMeshPosition in cubeMesh.Positions)
	            cubeCenter += cubeMeshPosition;
            cubeCenter /= cubeMesh.Positions.Count;

            var newPosition = new Vector3(5, 3, 4);
            Sandbox.Camera.AnimateTo(
	            newPosition: newPosition, 
	            newDirection: cubeCenter - newPosition,
                newUpDirection: Sandbox.Camera.UpDirection,
                animationTime: TimeSpan.FromSeconds(10).TotalMilliseconds);
        }

        private async void MenuBar_File_Open_Click(object sender, RoutedEventArgs e)
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

            // Create Message Dialog
            MessageDialog messageDialog = new MessageDialog(
	            content: "Done")
            {
                DefaultCommandIndex = 0,
                CancelCommandIndex = 0,
                Commands = { new UICommand("Close") },
                Options = MessageDialogOptions.AcceptUserInputAfterDelay
            };

            // Show Results
            await messageDialog.ShowAsync();
        }
    }
}
