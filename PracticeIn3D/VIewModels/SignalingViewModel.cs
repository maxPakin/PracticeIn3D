using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelixToolkit.UWP;
using PracticeIn3D.Enums;
using PracticeIn3D.Models;

namespace PracticeIn3D.VIewModels
{
	public class SignalingViewModel
	{
		public float CenterX { get; set; }
		public float CenterY { get; set; }
		public float CenterZ { get; set; }
		public SignalingType Type { get; set; }
		public string Name { get; set; }

		public SignalingViewModel(
			float centerX,
			float centerY,
			float centerZ)
		{
			CenterX = centerX;
			CenterY = centerY;
			CenterZ = centerZ;
			Type = SignalingType.Standard;
			Name = "New Signaling";
		}

		public SignalingViewModel(
			float centerX, 
			float centerY, 
			float centerZ, 
			SignalingType type,
			string name)
		{
			CenterX = centerX;
			CenterY = centerY;
			CenterZ = centerZ;
			Type = type;
			Name = name;
		}
	}
}
