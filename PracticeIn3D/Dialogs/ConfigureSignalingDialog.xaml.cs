using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PracticeIn3D.Enums;
using PracticeIn3D.Models;
using PracticeIn3D.VIewModels;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PracticeIn3D.Dialogs
{
	public enum ConfigureSignalingDialogResult
	{
		Accepted,
		Canceled
	}

	public sealed partial class ConfigureSignalingDialog : ContentDialog
	{
		public ConfigureSignalingDialogResult Result { get; private set; }

		public SignalingViewModel ViewModel { get; set; }

		public ConfigureSignalingDialog(
			bool isAdding,
			SignalingViewModel signaling)
		{
			this.InitializeComponent();
			if (isAdding)
			{
				Title = "Adding a New Signaling";
				PrimaryButtonText = "Add";
			}
			else
			{
				Title = "Change Signaling Settings";
				PrimaryButtonText = "Accept";
			}
			this.TypeComboBox.ItemsSource = Enum.GetValues(typeof(SignalingType))
				.Cast<SignalingType>()
				.ToList();

			this.ViewModel = signaling;
			this.DataContext = signaling;
		}

		private void Add_ButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			Result = ConfigureSignalingDialogResult.Accepted;
		}

		private void Close_ButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			Result = ConfigureSignalingDialogResult.Canceled;
		}
	}
}
