#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Engine.Tests\Extensions\DynamicTestExtensions.cs) is part of CiviKey. 
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
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using CK.Core;
using NUnit.Framework;
using Yodii.Engine;
using Yodii.Model;

namespace Yodii
{
    static class DynamicTestExtensions
    {
        public static void FullStartAndStop( this YodiiEngine @this, Action<YodiiEngine,IYodiiEngineResult> tests, [CallerMemberName]string callerName = null )
        {
            IActivityMonitor m = TestHelper.ConsoleMonitor;
            IYodiiEngineResult result;

            using( m.OpenInfo().Send( "FullStart for {0}.", callerName ) )
            {
                using( m.OpenInfo().Send( "FullStart()." ) )
                {
                    result = @this.StartEngine( false, false );
                    result.Trace( m );
                    tests( @this, result );
                    @this.StopEngine();
                }
                using( m.OpenInfo().Send( "FullStart( revertServices )." ) )
                {
                    result = @this.StartEngine( true, false );
                    result.Trace( m );
                    tests( @this, result );
                    @this.StopEngine();
                }
                using( m.OpenInfo().Send( "FullStart( revertPlugins )." ) )
                {
                    result = @this.StartEngine( false, true );
                    result.Trace( m );
                    tests( @this, result );
                    @this.StopEngine();
                }
                using( m.OpenInfo().Send( "FullStart( revertServices, revertPlugins )." ) )
                {
                    result = @this.StartEngine( true, true );
                    result.Trace( m );
                    tests( @this, result );
                    @this.StopEngine();
                }
            }
        }

        #region Plugins and Services
        public static IYodiiEngineExternal CheckAllDisabled( this IYodiiEngineExternal @this, string serviceOrPluginNames )
        {
            return CheckStatus( @this, RunningStatus.Disabled, serviceOrPluginNames, false );
        }

        public static IYodiiEngineExternal CheckAllStopped( this IYodiiEngineExternal @this, string serviceOrPluginNames )
        {
            return CheckStatus( @this, RunningStatus.Stopped, serviceOrPluginNames, false );
        }

        public static IYodiiEngineExternal CheckAllRunning( this IYodiiEngineExternal @this, string serviceOrPluginNames )
        {
            return CheckStatus( @this, RunningStatus.Running, serviceOrPluginNames, false );
        }

        public static IYodiiEngineExternal CheckAllRunningLocked( this IYodiiEngineExternal @this, string serviceOrPluginNames )
        {
            return CheckStatus( @this, RunningStatus.RunningLocked, serviceOrPluginNames, false );
        }

        public static IYodiiEngineExternal CheckDisabled( this IYodiiEngineExternal @this, string serviceOrPluginNames )
        {
            return CheckStatus( @this, RunningStatus.Disabled, serviceOrPluginNames, true );
        }

        public static IYodiiEngineExternal CheckStopped( this IYodiiEngineExternal @this, string serviceOrPluginNames )
        {
            return CheckStatus( @this, RunningStatus.Stopped, serviceOrPluginNames, true );
        }

        public static IYodiiEngineExternal CheckRunning( this IYodiiEngineExternal @this, string serviceOrPluginNames )
        {
            return CheckStatus( @this, RunningStatus.Running, serviceOrPluginNames, true );
        }

        public static IYodiiEngineExternal CheckRunningLocked( this IYodiiEngineExternal @this, string serviceOrPluginNames )
        {
            return CheckStatus( @this, RunningStatus.RunningLocked, serviceOrPluginNames, true );
        }

        static IYodiiEngineExternal CheckStatus( this IYodiiEngineExternal @this, RunningStatus status, string serviceOrPluginNames, bool expectedIsPartial )
        {
            string[] expected = serviceOrPluginNames.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
            var withTheStatus = @this.LiveInfo.Plugins.Where( p => p.RunningStatus == status ).Select( p => p.PluginInfo.PluginFullName );
            withTheStatus = withTheStatus.Concat( @this.LiveInfo.Services.Where( s => s.RunningStatus == status ).Select( s => s.ServiceInfo.ServiceFullName ) );
            TestExtensions.CheckContainsWithAlternative( expected, withTheStatus, expectedIsPartial );
            return @this;
        }
        #endregion

        #region Plugins only
        public static IYodiiEngineExternal CheckAllPluginsDisabled( this IYodiiEngineExternal @this, string pluginNames )
        {
            return CheckPluginsStatus( @this, RunningStatus.Disabled, pluginNames, false );
        }

        public static IYodiiEngineExternal CheckAllPluginsStopped( this IYodiiEngineExternal @this, string pluginNames )
        {
            return CheckPluginsStatus( @this, RunningStatus.Stopped, pluginNames, false );
        }

        public static IYodiiEngineExternal CheckAllPluginsRunning( this IYodiiEngineExternal @this, string pluginNames )
        {
            return CheckPluginsStatus( @this, RunningStatus.Running, pluginNames, false );
        }

        public static IYodiiEngineExternal CheckAllPluginsRunningLocked( this IYodiiEngineExternal @this, string pluginNames )
        {
            return CheckPluginsStatus( @this, RunningStatus.RunningLocked, pluginNames, false );
        }

        public static IYodiiEngineExternal CheckPluginsDisabled( this IYodiiEngineExternal @this, string pluginNames )
        {
            return CheckPluginsStatus( @this, RunningStatus.Disabled, pluginNames, true );
        }

        public static IYodiiEngineExternal CheckPluginsStopped( this IYodiiEngineExternal @this, string pluginNames )
        {
            return CheckPluginsStatus( @this, RunningStatus.Stopped, pluginNames, true );
        }

        public static IYodiiEngineExternal CheckPluginsRunning( this IYodiiEngineExternal @this, string pluginNames )
        {
            return CheckPluginsStatus( @this, RunningStatus.Running, pluginNames, true );
        }

        public static IYodiiEngineExternal CheckPluginsRunningLocked( this IYodiiEngineExternal @this, string pluginNames )
        {
            return CheckPluginsStatus( @this, RunningStatus.RunningLocked, pluginNames, true );
        }

        static IYodiiEngineExternal CheckPluginsStatus( this IYodiiEngineExternal @this, RunningStatus status, string pluginNames, bool expectedIsPartial )
        {
            string[] expected = pluginNames.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
            var withTheStatus = @this.LiveInfo.Plugins.Where( p => p.RunningStatus == status ).Select( p => p.PluginInfo.PluginFullName );
            TestExtensions.CheckContainsWithAlternative( expected, withTheStatus, expectedIsPartial );
            return @this;
        }
        #endregion

        #region Services only
        public static IYodiiEngineExternal CheckAllServicesDisabled( this IYodiiEngineExternal @this, string serviceNames )
        {
            return CheckServicesStatus( @this, RunningStatus.Disabled, serviceNames, false );
        }

        public static IYodiiEngineExternal CheckAllServicesStopped( this IYodiiEngineExternal @this, string serviceNames )
        {
            return CheckServicesStatus( @this, RunningStatus.Stopped, serviceNames, false );
        }

        public static IYodiiEngineExternal CheckAllServicesRunning( this IYodiiEngineExternal @this, string serviceNames )
        {
            return CheckServicesStatus( @this, RunningStatus.Running, serviceNames, false );
        }

        public static IYodiiEngineExternal CheckAllServicesRunningLocked( this IYodiiEngineExternal @this, string serviceNames )
        {
            return CheckServicesStatus( @this, RunningStatus.RunningLocked, serviceNames, false );
        }

        public static IYodiiEngineExternal CheckServicesDisabled( this IYodiiEngineExternal @this, string serviceNames )
        {
            return CheckServicesStatus( @this, RunningStatus.Disabled, serviceNames, true );
        }

        public static IYodiiEngineExternal CheckServicesStopped( this IYodiiEngineExternal @this, string serviceNames )
        {
            return CheckServicesStatus( @this, RunningStatus.Stopped, serviceNames, true );
        }

        public static IYodiiEngineExternal CheckServicesRunning( this IYodiiEngineExternal @this, string serviceNames )
        {
            return CheckServicesStatus( @this, RunningStatus.Running, serviceNames, true );
        }

        public static IYodiiEngineExternal CheckServicesRunningLocked( this IYodiiEngineExternal @this, string serviceNames )
        {
            return CheckServicesStatus( @this, RunningStatus.RunningLocked, serviceNames, true );
        }

        static IYodiiEngineExternal CheckServicesStatus( this IYodiiEngineExternal @this, RunningStatus status, string serviceNames, bool expectedIsPartial )
        {
            string[] expected = serviceNames.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
            var withTheStatus = @this.LiveInfo.Services.Where( s => s.RunningStatus == status ).Select( s => s.ServiceInfo.ServiceFullName );
            TestExtensions.CheckContainsWithAlternative( expected, withTheStatus, expectedIsPartial );
            return @this;
        }
        #endregion

    }
}
