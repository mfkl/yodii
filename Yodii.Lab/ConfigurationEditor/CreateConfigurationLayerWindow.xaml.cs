#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Lab\ConfigurationEditor\CreateConfigurationLayerWindow.xaml.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Yodii.Lab.ConfigurationEditor
{
    /// <summary>
    /// Interaction logic for CreateConfigurationLayerWindow.xaml
    /// </summary>
    internal partial class CreateConfigurationLayerWindow : Window
    {
        public CreateConfigurationLayerWindow()
        {
            InitializeComponent();
            this.NewLayerName.Focus();
        }

        private void OkButton_Click( object sender, RoutedEventArgs e )
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
