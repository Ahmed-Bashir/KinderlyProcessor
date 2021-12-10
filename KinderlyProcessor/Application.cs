using KinderlyProcessor.Interfaces;
using System;
using System.Timers;

namespace KinderlyProcessor
{
    public class Application
    {
        private readonly IKinderlyApiService _kinderlyApiService;
        private readonly Timer _timer;

        public Application(IKinderlyApiService kinderlyApiService)
        {
            _kinderlyApiService = kinderlyApiService;
            OnTick(null, EventArgs.Empty);
            _timer = new Timer(TimeSpan.FromSeconds(60).TotalMilliseconds);
            _timer.Elapsed += OnTick;
        }

        public void Start() => _timer.Start();

        public void Stop() => _timer.Stop();

        private void OnTick(object sender, EventArgs args)
        {
            //_kinderlyApiService.SendApprovedPaceyMembersAsync().GetAwaiter().GetResult();
            _kinderlyApiService.ProcessDigitalContractsAsync().GetAwaiter().GetResult();
        }
    }
}

