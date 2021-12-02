using System;
using System.Threading.Tasks;
using System.Timers;
using System.IO;
using KinderlyProcessor.Core.Interfaces;

namespace KinderlyProcessor
{
    public class Application
    {

        private readonly IKinderlyApiService _kinderlyApiService;
        private readonly Timer _timer;

        public Application(IKinderlyApiService kinderlyApiService)
        {
         
            _kinderlyApiService = kinderlyApiService;
            _timer = new Timer(60000) {AutoReset = true };
            _timer.Elapsed += OnTick;
          
        }

        public void Start()
        {
            _timer.Start();

        
        }

        public void Stop()
        {

            _timer.Stop();
        }

        private void OnTick(object sender, EventArgs args) {

           

            Task.Run(() => _kinderlyApiService.SendApprovedPaceyMembersAsync());
            Task.Run(() => _kinderlyApiService.ProcessDigitalContractsAsync("DigitalContractApiLive"));
           



        } 

        
    }

}
    
