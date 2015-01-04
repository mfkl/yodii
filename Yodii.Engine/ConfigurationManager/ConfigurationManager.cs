﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using Yodii.Model;

namespace Yodii.Engine
{
    internal class ConfigurationManager : IConfigurationManager
    {
        internal readonly ConfigurationLayerCollection Layers;
        FinalConfiguration _finalConfiguration;

        ConfigurationChangingEventArgs _currentEventArgs;

        public event EventHandler<ConfigurationChangingEventArgs> ConfigurationChanging;
        public event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;

        IConfigurationLayerCollection IConfigurationManager.Layers
        {
            get { return Layers; }
        }

        public FinalConfiguration FinalConfiguration
        {
            get { return _finalConfiguration; }
            private set
            {
                _finalConfiguration = value;
                RaisePropertyChanged();
            }
        }

        internal ConfigurationManager( YodiiEngine engine )
        {
            Engine = engine;
            Layers = new ConfigurationLayerCollection( this );
            _finalConfiguration = new FinalConfiguration();
        }

        public readonly YodiiEngine Engine;

        ConfigurationFailureResult FillFromConfiguration( string currentOperation, Dictionary<string, FinalConfigurationItem> final, Func<ConfigurationItem, bool> filter = null )
        {
            foreach( ConfigurationLayer layer in Layers )
            {
                ConfigurationStatus combinedStatus;
                string invalidCombination;

                foreach( ConfigurationItem item in layer.Items )
                {
                    if( filter == null || filter( item ) )
                    {
                        FinalConfigurationItem data;
                        if( final.TryGetValue( item.ServiceOrPluginFullName, out data ) )
                        {
                            combinedStatus = FinalConfigurationItem.Combine( item.Status, data.Status, out invalidCombination );
                            if( string.IsNullOrEmpty( invalidCombination ) )
                            {
                                StartDependencyImpact combinedImpact = (data.Impact|item.Impact).ClearUselessTryBits();
                                final[item.ServiceOrPluginFullName] = new FinalConfigurationItem( item.ServiceOrPluginFullName, combinedStatus, combinedImpact );
                            }
                            else return new ConfigurationFailureResult( String.Format( "{0}: {1} for {2}", currentOperation, invalidCombination, item.ServiceOrPluginFullName ) );
                        }            
                        else
                        {
                            final.Add( item.ServiceOrPluginFullName, new FinalConfigurationItem( item.ServiceOrPluginFullName, item.Status, item.Impact ) );
                        }
                    }
                }
            }
            return new ConfigurationFailureResult();
        }

        internal IYodiiEngineResult OnConfigurationItemChanging( ConfigurationItem item, FinalConfigurationItem data )
        {
            Debug.Assert( item != null && _finalConfiguration != null && Layers.Count != 0 );
            if( _currentEventArgs != null ) throw new InvalidOperationException( "Another change is in progress" );

            Dictionary<string, FinalConfigurationItem> final = new Dictionary<string, FinalConfigurationItem>();
            final.Add( item.ServiceOrPluginFullName, data );

            ConfigurationFailureResult internalResult = FillFromConfiguration( "Item changing", final, c => c != item );
            if( !internalResult.Success ) return new YodiiEngineResult( internalResult, Engine );

            FinalConfigurationChange status = FinalConfigurationChange.None;
            if( item.Status != data.Status ) status |= FinalConfigurationChange.StatusChanged;
            if( item.Impact != data.Impact ) status |= FinalConfigurationChange.ImpactChanged;

            return OnConfigurationChanging( final, finalConf => new ConfigurationChangingEventArgs( finalConf, status, item ) );
        }

        internal IYodiiEngineResult OnConfigurationItemAdding( ConfigurationItem newItem )
        {
            Dictionary<string, FinalConfigurationItem> final = new Dictionary<string, FinalConfigurationItem>();
            final.Add( newItem.ServiceOrPluginFullName, new FinalConfigurationItem( newItem.ServiceOrPluginFullName, newItem.Status, newItem.Impact ));
          
            ConfigurationFailureResult internalResult = FillFromConfiguration( "Adding configuration item", final );
            if( !internalResult.Success ) return new YodiiEngineResult( internalResult, Engine );

            return OnConfigurationChanging( final, finalConf => new ConfigurationChangingEventArgs( finalConf, FinalConfigurationChange.ItemAdded, newItem ) );
        }

        internal IYodiiEngineResult OnConfigurationItemRemoving( ConfigurationItem item )
        {
            Dictionary<string, FinalConfigurationItem> final = new Dictionary<string, FinalConfigurationItem>();

            ConfigurationFailureResult internalResult = FillFromConfiguration( null, final, c => c != item );
            Debug.Assert( internalResult.Success, "Removing a configuration item can not lead to an impossibility." );

            return OnConfigurationChanging( final, finalConf => new ConfigurationChangingEventArgs( finalConf, FinalConfigurationChange.ItemRemoved, item ) );
        }

        internal IYodiiEngineResult OnConfigurationLayerRemoving( ConfigurationLayer layer )
        {
            Dictionary<string, FinalConfigurationItem> final = new Dictionary<string, FinalConfigurationItem>();

            ConfigurationFailureResult internalResult = FillFromConfiguration( null, final, c => c.Layer != layer );
            Debug.Assert( internalResult.Success, "Removing a configuration layer can not lead to an impossibility." );

            return OnConfigurationChanging( final, finalConf => new ConfigurationChangingEventArgs( finalConf, FinalConfigurationChange.LayerRemoved, layer ) );
        }

        IYodiiEngineResult OnConfigurationChanging( Dictionary<string, FinalConfigurationItem> final, Func<FinalConfiguration, ConfigurationChangingEventArgs> createChangingEvent )
        {
            FinalConfiguration finalConfiguration = new FinalConfiguration( final );
            if( Engine.IsRunning )
            {
                Tuple<IYodiiEngineStaticOnlyResult,ConfigurationSolver> t = Engine.StaticResolutionByConfigurationManager( finalConfiguration );
                if( t.Item1 != null )
                {
                    Debug.Assert( !t.Item1.Success );
                    Debug.Assert( t.Item1.Engine == Engine );
                    return t.Item1;
                }
                return OnConfigurationChangingForExternalWorld( createChangingEvent( finalConfiguration ) ) ?? Engine.OnConfigurationChanging( t.Item2 );
            }
            return OnConfigurationChangingForExternalWorld( createChangingEvent( finalConfiguration ) ) ?? Engine.SuccessResult;
        }

        internal IYodiiEngineResult OnConfigurationClearing()
        {
            Dictionary<string,FinalConfigurationItem> final = new Dictionary<string, FinalConfigurationItem>();
            return OnConfigurationChanging( final, finalConf => new ConfigurationChangingEventArgs( finalConf ) );
        }

        IYodiiEngineResult OnConfigurationChangingForExternalWorld( ConfigurationChangingEventArgs eventChanging )
        {
            _currentEventArgs = eventChanging;
            RaiseConfigurationChanging( _currentEventArgs );
            if( _currentEventArgs.IsCanceled )
            {
                return new YodiiEngineResult( new ConfigurationFailureResult( _currentEventArgs.FailureExternalReasons ), Engine );
            }
            return null;
        }


        internal void OnConfigurationChanged()
        {
            Debug.Assert( _currentEventArgs != null );

            FinalConfiguration = _currentEventArgs.FinalConfiguration;
            if( _currentEventArgs.FinalConfigurationChange == FinalConfigurationChange.StatusChanged
                || _currentEventArgs.FinalConfigurationChange == FinalConfigurationChange.ItemAdded
                || _currentEventArgs.FinalConfigurationChange == FinalConfigurationChange.ItemRemoved
                || _currentEventArgs.FinalConfigurationChange == FinalConfigurationChange.ImpactChanged)
            {
                RaiseConfigurationChanged( new ConfigurationChangedEventArgs( FinalConfiguration, _currentEventArgs.FinalConfigurationChange, _currentEventArgs.ConfigurationItemChanged ) );
            }
            else
            {
                RaiseConfigurationChanged( new ConfigurationChangedEventArgs( FinalConfiguration, _currentEventArgs.FinalConfigurationChange, _currentEventArgs.ConfigurationLayerChanged ) );
            }
            _currentEventArgs = null;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged( [CallerMemberName] String propertyName = "" )
        {
            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        #endregion INotifyPropertyChanged

        private void RaiseConfigurationChanging( ConfigurationChangingEventArgs e )
        {
            var h = ConfigurationChanging;
            if( h != null ) h( this, e );
        }

        private void RaiseConfigurationChanged( ConfigurationChangedEventArgs e )
        {
            var h = ConfigurationChanged;
            if( h != null ) h( this, e );
        }

    }

}
