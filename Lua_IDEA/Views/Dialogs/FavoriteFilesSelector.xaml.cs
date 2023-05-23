// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Lua_IDEA.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Lua_IDEA.Views.Dialogs;
public sealed partial class FavoriteFilesSelector : ContentDialog
{
    private readonly FavoriteFilesSelectorViewModel viewModel;
      
    public FavoriteFilesSelector()
    {
        this.viewModel = viewModel;
        this.InitializeComponent();
    }
    private void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        viewModel.GetFavoritePathsCommand.Execute(null);
    }
}
