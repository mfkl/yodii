﻿#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Host\Plugin\StStartContext.cs) is part of CiviKey. 
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
using System.Diagnostics;
using Yodii.Model;

namespace Weavers.Plugin
{
    [DebuggerDisplay( "Implementation={Implementation}" )]
    class StStartContext : StContext, IPreStartContext, IStartContext
    {
        ServiceManager.Impact _swappedImpact;
        Action<Action<IYodiiEngineExternal>> _actionCollector;

        public StStartContext(PluginProxy plugin, RunningStatus status, Dictionary<object, object> shared, bool wasDisabled, Action<Action<IYodiiEngineExternal>> actionCollector)
            : base(plugin, status, shared)
        {
            Debug.Assert(actionCollector != null);
            WasDisabled = wasDisabled;
            _actionCollector = actionCollector;
        }

        public readonly bool WasDisabled;

        public Action<IStopContext> RollbackAction { get; set; }

        public ServiceManager.Impact SwappedServiceImpact
        {
            get { return _swappedImpact; }
            set
            {
                Debug.Assert(_swappedImpact == null && value != null);
                _swappedImpact = value;
                Status = StContext.StStatus.StartingSwap;
            }
        }

        public IYodiiService PreviousPluginCommonService
        {
            get { return _swappedImpact != null ? (IYodiiService)_swappedImpact.Service : null; }
        }

        public StContext PreviousImpl
        {
            get { return _swappedImpact != null ? _swappedImpact.Implementation : null; }
        }

        IYodiiPlugin IPreStartContext.PreviousPlugin { get { return PreviousImpl != null ? PreviousImpl.Plugin.RealPluginObject : null; } }

        bool IPreStartContext.HotSwapping
        {
            get { return Status == StStatus.StartingHotSwap; }
            set
            {
                if(PreviousImpl == null) throw new InvalidOperationException("PreviousPluginMustNotBeNull");
                PreviousImpl.HotSwapped = value;
                HotSwapped = value;
            }
        }

        bool IStartContext.CancellingPreStop => false;

        bool IStartContext.HotSwapping => Status == StStatus.StartingHotSwap;

        public void PostAction(Action<IYodiiEngineExternal> delayedAction)
        {
            if(delayedAction == null) throw new ArgumentNullException(nameof(delayedAction));
            _actionCollector(delayedAction);
        }


        public override string ToString()
        {
            return
                $"Start: {Plugin.PluginInfo.PluginFullName}, Previous={(PreviousImpl != null ? PreviousImpl.Plugin.PluginInfo.PluginFullName : "null")}";
        }
    }

}