using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace RTLS.Engine
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if (!DEBUG)
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] 
			    { 
				    new EngineService(), 
			    };
                ServiceBase.Run(ServicesToRun);
#else
            EngineService engineService = new EngineService();
            engineService.Start();
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
          #endif           
        }
    }

    [RunInstaller(true)]
    public class WindowsServiceInstaller : Installer
    {
        private readonly ServiceProcessInstaller _serviceProcessInstaller = new ServiceProcessInstaller();
        private readonly ServiceInstaller _serviceInstaller = new System.ServiceProcess.ServiceInstaller();

        public WindowsServiceInstaller()
        {
            //Process Account
            _serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            _serviceProcessInstaller.Password = null;
            _serviceProcessInstaller.Username = null;

            //Install
            _serviceInstaller.ServiceName = "RTLSEnginePostgre";
            _serviceInstaller.DisplayName = "RTLS Postgre Engine";
            _serviceInstaller.Description = "RTLS Postgre Engine";
            _serviceInstaller.StartType = ServiceStartMode.Automatic;

            //Install ekle
            this.Installers.AddRange(new Installer[] {
            this._serviceProcessInstaller,
            this._serviceInstaller});
        }
    }
}
