#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Lab\MainWindow.xaml.cs) is part of CiviKey. 
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
using System.Windows;
using System.Windows.Threading;
using GraphX;
using GraphX.GraphSharp.Algorithms.Layout;

namespace Yodii.Lab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Fluent.RibbonWindow
    {
        readonly MainWindowViewModel _vm;
        readonly YodiiLayout _graphLayout;

        /// <summary>
        /// Creates the main window.
        /// </summary>
        public MainWindow()
        {
            _vm = new MainWindowViewModel( false );
            this.DataContext = _vm;

            _vm.NewNotification += _vm_NewNotification;
            _vm.CloseBackstageRequest += _vm_CloseBackstageRequest;
            _vm.VertexPositionRequest += _vm_VertexPositionRequest;
            _vm.AutoPositionRequest += _vm_AutoPositionRequest;

            _vm.Graph.GraphUpdateRequested += Graph_GraphUpdateRequested;

            InitializeComponent();

            GraphArea.GenerateGraphFinished += GraphArea_GenerateGraphFinished;
            GraphArea.RelayoutFinished += GraphArea_RelayoutFinished;

            ZoomBox.IsAnimationDisabled = false;
            ZoomBox.MaxZoomDelta = 2;
            GraphArea.UseNativeObjectArrange = false;
            //GraphArea.SideExpansionSize = new Size( 100, 100 );


            _graphLayout = new YodiiLayout();
            GraphArea.LayoutAlgorithm = _graphLayout;
            GraphArea.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;
            GraphArea.DefaultOverlapRemovalAlgorithm = GraphX.OverlapRemovalAlgorithmTypeEnum.FSA;
            GraphArea.EdgeCurvingEnabled = true;

            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;

            _vm.Graph.VertexAdded += Graph_VertexAdded;
            _vm.Graph.VertexRemoved += Graph_VertexRemoved;
            _vm.Graph.EdgeAdded += Graph_EdgeAdded;
            _vm.Graph.EdgeRemoved += Graph_EdgeRemoved;
        }

        void _vm_AutoPositionRequest( object sender, EventArgs e )
        {
            var result = MessageBox.Show(
                "Automatically position all elements in the graph?\nThis will reset all their positions.",
                "Auto-position elements",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No
                );
            if( result == MessageBoxResult.Yes )
            {
                _graphLayout.NextRecomputeForcesPositions = true;
                GraphArea.RelayoutGraph();
            }
        }

        void _vm_VertexPositionRequest( object sender, VertexPositionEventArgs e )
        {
            e.VertexPositions = GraphArea.GetVertexPositions();
        }

        void Graph_EdgeRemoved( YodiiGraphEdge e )
        {
            GraphArea.RemoveEdge( e );
        }

        void Graph_EdgeAdded( YodiiGraphEdge e )
        {
            GraphArea.AddEdge( e, new EdgeControl( GraphArea.VertexList[e.Source], GraphArea.VertexList[e.Target], e ) );
        }

        void Graph_VertexRemoved( YodiiGraphVertex vertex )
        {
            GraphArea.RemoveVertex( vertex );
        }

        void Graph_VertexAdded( YodiiGraphVertex vertex )
        {
            var control = new VertexControl( vertex );

            if( vertex.IsService )
            {
                if( vertex.LabServiceInfo.ServiceInfo.PositionInGraph.IsValid() ) control.SetPosition( vertex.LabServiceInfo.ServiceInfo.PositionInGraph );
            }
            else if( vertex.IsPlugin )
            {
                if( vertex.LabPluginInfo.PluginInfo.PositionInGraph.IsValid() ) control.SetPosition( vertex.LabPluginInfo.PluginInfo.PositionInGraph );
            }

            GraphArea.AddVertex( vertex, control );
            DragBehaviour.SetUpdateEdgesOnMove( control, true );
        }

        void _vm_CloseBackstageRequest( object sender, EventArgs e )
        {
            if( RibbonBackstage != null ) RibbonBackstage.IsOpen = false;
        }

        void MainWindow_Closing( object sender, System.ComponentModel.CancelEventArgs e )
        {
            _vm.StopAutosaveTimer();
            if( !_vm.SaveBeforeClosingFile() )
            {
                e.Cancel = true;
                _vm.StartAutosaveTimer();
            }
            else
            {
                _vm.ClearAutosave();
            }
        }

        void _vm_NewNotification( object sender, NotificationEventArgs e )
        {
            if( this.NotificationControl != null )
            {
                this.NotificationControl.AddNotification( e.Notification );
            }
        }

        void MainWindow_Loaded( object sender, RoutedEventArgs e )
        {
            GraphArea.GenerateGraph( _vm.Graph );

            if( _vm.HasAutosave() )
            {
                var result = MessageBox.Show(
                    "Program did not properly close last time.\nDo you wish to load the last automatic save?\nPressing No will discard the save.",
                    "Auto-save available",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Exclamation,
                    MessageBoxResult.Yes
                    );

                if( result == MessageBoxResult.Yes )
                {
                    _vm.LoadAutosave();
                }
                else
                {
                    _vm.ClearAutosave();
                }
            }

            _vm.StartAutosaveTimer();
        }

        void GraphArea_RelayoutFinished( object sender, EventArgs e )
        {
            ZoomBox.ZoomToFill();
            ZoomBox.Mode = GraphX.Controls.ZoomControlModes.Custom;
            foreach( var item in GraphArea.VertexList )
            {
                DragBehaviour.SetIsDragEnabled( item.Value, true );
                item.Value.EventOptions.PositionChangeNotification = true;
            }
            GraphArea.ShowAllEdgesLabels();
            GraphArea.InvalidateVisual();

        }

        void GraphArea_GenerateGraphFinished( object sender, EventArgs e )
        {
            GraphArea.GenerateAllEdges();
            ZoomBox.ZoomToFill();
            ZoomBox.Mode = GraphX.Controls.ZoomControlModes.Custom;
            foreach( var item in GraphArea.VertexList )
            {
                DragBehaviour.SetIsDragEnabled( item.Value, true );
                item.Value.EventOptions.PositionChangeNotification = true;
            }
            GraphArea.ShowAllEdgesLabels();
            GraphArea.InvalidateVisual();
        }

        void Graph_GraphUpdateRequested( object sender, EventArgs e )
        {
            GraphArea.RelayoutGraph();
        }

        private void StackPanel_MouseDown( object sender, System.Windows.Input.MouseButtonEventArgs e )
        {
            FrameworkElement vertexPanel = sender as FrameworkElement;

            YodiiGraphVertex vertex = vertexPanel.DataContext as YodiiGraphVertex;

            _vm.SelectedVertex = null;
            _vm.SelectedVertex = vertex;
        }

        private void graphLayout_MouseDown( object sender, System.Windows.Input.MouseButtonEventArgs e )
        {
            _vm.SelectedVertex = null;
        }

        private void ExportToPngButton_Click( object sender, RoutedEventArgs e )
        {
            GraphArea.ExportAsPNG( true );
        }

    }
}
