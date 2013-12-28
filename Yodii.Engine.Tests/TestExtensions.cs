﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NUnit.Framework;
using CK.Core;
using Yodii.Model;
using System.Runtime.CompilerServices;

namespace Yodii.Engine.Tests
{
    static class TestExtensions
    {

        public static void FullStaticResolutionOnly( this YodiiEngine @this, Action<IYodiiEngineStaticOnlyResult> tests, [CallerMemberName]string callerName = null )
        {
            IActivityMonitor m = TestHelper.ConsoleMonitor;
            IYodiiEngineStaticOnlyResult result;
            using( m.OpenInfo().Send( "FullStaticResolutionOnly for {0}.", callerName ) )
            {
                using( m.OpenInfo().Send( "StaticResolutionOnly( revertServices )." ) )
                {
                    result = @this.StaticResolutionOnly( true, false );
                    result.Trace( m );
                    tests( result );
                }
                using( TestHelper.ConsoleMonitor.OpenInfo().Send( "StaticResolutionOnly()." ) )
                {
                    result = @this.StaticResolutionOnly( false, false );
                    result.Trace( m );
                    tests( result );
                    @this.Stop();
                }
                using( TestHelper.ConsoleMonitor.OpenInfo().Send( "StaticResolutionOnly( revertPlugins )." ) )
                {
                    result = @this.StaticResolutionOnly( false, true );
                    result.Trace( m );
                    tests( result );
                }
                using( TestHelper.ConsoleMonitor.OpenInfo().Send( "StaticResolutionOnly( revertServices, revertPlugins )." ) )
                {
                    result = @this.StaticResolutionOnly( true, true );
                    result.Trace( m );
                    tests( result );
                }
            }
        }

        public static void CheckSuccess( this IYodiiEngineResult @this )
        {
            Assert.That( @this.Success, Is.True );
            Assert.That( @this.StaticFailureResult, Is.Null );
            Assert.That( @this.HostFailureResult, Is.Null );
            Assert.That( @this.ConfigurationFailureResult, Is.Null );
            Assert.That( @this.PluginCulprits, Is.Empty );
            Assert.That( @this.ServiceCulprits, Is.Empty );
        }

        public static void CheckWantedConfigSolvedStatusIs( this IYodiiEngineResult @this, string pluginOrServiceFullName, ConfigurationStatus wantedStatus )
        {
            if( @this.Success )
            {
                Assert.Fail( "Not implemented ==> TODO: IYodiiEngineResult SHOULD have a 'IYodiiEngine Engine' property!" );
            }
            else
            {
                var service = @this.StaticFailureResult.StaticSolvedConfiguration.Services.FirstOrDefault( s => s.ServiceInfo.ServiceFullName == pluginOrServiceFullName );
                if( service != null ) Assert.That( service.WantedConfigSolvedStatus, Is.EqualTo( wantedStatus ), String.Format( "Service '{0}' has a WantedConfigSolvedStatus = '{1}'. It must be '{2}'.", pluginOrServiceFullName, service.WantedConfigSolvedStatus, wantedStatus ) );
                else
                {
                    var plugin = @this.StaticFailureResult.StaticSolvedConfiguration.Plugins.FirstOrDefault( p => p.PluginInfo.PluginFullName == pluginOrServiceFullName );
                    if( plugin != null ) Assert.That( plugin.WantedConfigSolvedStatus, Is.EqualTo( wantedStatus ), String.Format( "Plugin '{0}' has a WantedConfigSolvedStatus = '{1}'. It must be '{2}'.", pluginOrServiceFullName, plugin.WantedConfigSolvedStatus, wantedStatus ) );
                    else Assert.Fail( String.Format( "Plugin or Service '{0}' not found.", pluginOrServiceFullName ) );
                }
            }
        }

        public static void CheckNoBlockingPlugins( this IYodiiEngineResult @this )
        {
            Assert.That( (@this.StaticFailureResult != null ? @this.StaticFailureResult.BlockingPlugins : Enumerable.Empty<IStaticSolvedPlugin>()), Is.Empty );
        }

        public static void CheckNoBlockingServices( this IYodiiEngineResult @this )
        {
            Assert.That( (@this.StaticFailureResult != null ? @this.StaticFailureResult.BlockingServices : Enumerable.Empty<IStaticSolvedService>()), Is.Empty );
        }

        public static void CheckAllBlockingPluginsAre( this IYodiiEngineResult @this, string names )
        {
            string[] n = names.Split( new[]{','}, StringSplitOptions.RemoveEmptyEntries );
            Assert.That( !@this.Success && @this.StaticFailureResult != null, String.Format( "{0} blocking plugins expected. No error found.", n.Length ) );
            CheckContainsAllWithAlternative( n, @this.StaticFailureResult.BlockingPlugins.Select( p => p.PluginInfo.PluginFullName ) );
        }

        public static void CheckAllBlockingServicesAre( this IYodiiEngineResult @this, string names )
        {
            string[] n = names.Split( new[]{','}, StringSplitOptions.RemoveEmptyEntries );
            Assert.That( !@this.Success && @this.StaticFailureResult != null, String.Format( "{0} blocking services expected. No error found.", n.Length ) );
            CheckContainsAllWithAlternative( n, @this.StaticFailureResult.BlockingServices.Select( s => s.ServiceInfo.ServiceFullName ) );
        }

        internal static void CheckContainsAllWithAlternative( string[] expected, IEnumerable<string> actual )
        {
            CheckContainsWithAlternative( expected, actual, false );
        }

        internal static void CheckContainsWithAlternative( string[] expected, IEnumerable<string> actual )
        {
            CheckContainsWithAlternative( expected, actual, true );
        }

        internal static void CheckContainsWithAlternative( string[] expected, IEnumerable<string> actual, bool expectedIsPartial )
        {
            foreach( var segment in expected )
            {
                var opt = segment.Split( new[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).Select( s => s.Trim() );
                if( !actual.Any( s => opt.Contains( s ) ) ) Assert.Fail( String.Format( "Expected '{0}' but it is missing in '{1}'.", segment, String.Join( ", ", actual ) ) );
            }
            if( !expectedIsPartial )
            {
                if( expected.Length < actual.Count() ) Assert.Fail( String.Format( "Expected '{0}' but was '{1}'.", String.Join( ", ", expected ), String.Join( ", ", actual ) ) );
            }
        }

    }
}
