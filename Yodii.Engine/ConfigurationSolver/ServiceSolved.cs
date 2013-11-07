﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Engine
{
    internal class ServiceSolved : IServiceSolved
    {
        readonly ServiceDisabledReason _serviceDisabledReason;
        readonly RunningRequirement _configSolvedStatus;
        readonly IServiceInfo _serviceInfo;
        readonly ConfigurationStatus _configurationStatus;
        readonly RunningStatus? _runningStatus;

        public ServiceSolved( IServiceInfo serviceInfo, ServiceDisabledReason serviceDisabledReason, RunningRequirement configSolvedStatus, ConfigurationStatus configurationStatus, RunningStatus? nullable )
        {
            Debug.Assert( serviceInfo != null );
            _serviceInfo = serviceInfo;
            _serviceDisabledReason = serviceDisabledReason;
            _configSolvedStatus = configSolvedStatus;
            _configurationStatus = configurationStatus;
            _runningStatus = nullable;
        }

        public bool IsBlocking { get { return _configSolvedStatus >= RunningRequirement.Runnable && _serviceDisabledReason != ServiceDisabledReason.None; } }
        public bool IsDisabled { get { return _serviceDisabledReason != ServiceDisabledReason.None; } }

        public IServiceInfo ServiceInfo { get { return _serviceInfo; } }

        public ServiceDisabledReason ConfigDisabledReason { get { return _serviceDisabledReason; } }

        public ConfigurationStatus ConfigurationStatus { get { return _configurationStatus; } }

        public RunningRequirement ConfigSolvedStatus { get { return _configSolvedStatus; } }

        public RunningStatus? RunningStatus { get { return _runningStatus; } }

        public override string ToString()
        {
            return String.Format( "{0} - {1} - {2}", _serviceInfo.ServiceFullName, IsDisabled ? _serviceDisabledReason.ToString() : "!Disabled", ConfigSolvedStatus.ToString() );
        }
    }
}
