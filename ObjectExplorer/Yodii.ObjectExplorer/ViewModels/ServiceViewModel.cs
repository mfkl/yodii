using System;
using System.Diagnostics;
using System.Reflection;
using NullGuard;
using PropertyChanged;
using Yodii.Model;

namespace Yodii.ObjectExplorer.ViewModels
{
    [ImplementPropertyChanged]
    public class ServiceViewModel : YodiiItemViewModelBase
    {
        [AllowNull]
        public ILiveServiceInfo Service { get { return (ILiveServiceInfo)LiveItem; } }

        public ServiceViewModel()
        {

        }
        public override IAssemblyInfo AssemblyInfo
        {
            get
            {
                return Service.ServiceInfo.AssemblyInfo;
            }
        }
    }
}