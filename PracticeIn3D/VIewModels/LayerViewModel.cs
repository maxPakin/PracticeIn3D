using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace PracticeIn3D.VIewModels
{
	public class LayerViewModel
	{
		public string Name { get; set; }
		public ObservableCollection<WallViewModel> Walls { get; set; }
	}
}
