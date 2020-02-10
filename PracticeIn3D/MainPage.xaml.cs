using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using HelixToolkit.UWP;
using SharpDX;

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
    }
}
