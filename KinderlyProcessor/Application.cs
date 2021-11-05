﻿using System;
using KinderlyProcessor.Interfaces;
using System.Threading.Tasks;
using System.Timers;
using System.IO;

namespace KinderlyProcessor
{
    public class Application
    {

        private readonly IKinderlyApiService _kinderlyApiService;
        private Timer _timer;

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

            //Task.Run(() => _kinderlyApiService.SendApprovedPaceyMembersAsync());

            Task.Run(() => _kinderlyApiService.ProcessDigitalContractsAsync());

            //string[] lines = new string[] { DateTime.Now.ToString() };
            ////string path = @"C:\Users\a.salad\Desktop\Pacey\Kinderly\ImAlive.txt";
            //string path = @"C:\Programs\KinderlyProcessor\ImAlive.txt";

            //File.AppendAllLines(path, lines);


        } 

        
    }

}
    
