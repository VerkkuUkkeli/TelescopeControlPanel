using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DirectInput;
using System.Threading;
using System.Windows;

namespace JoystrickControlDemo
{
    class JoystickMonitor
    {
        private String _joystickName;

        // initialize object variables
        public JoystickMonitor(string joystickName)
        {
            _joystickName = joystickName;
        }

        // Poll joystick state
        public void PollJoystick(IProgress<JoystickUpdate> progress, CancellationToken cancellationToken)
        {
            var directInput = new DirectInput();
            int devicesFound = 0;
            DeviceInstance device = null;

            foreach (var d in directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AttachedOnly))
            {
                String trimmedProductName = d.ProductName.Substring(0, _joystickName.Length);

                if (devicesFound > 0)
                {
                    throw new Exception("More than one Joystick present, only one allowed!");
                }

                if (String.Compare(trimmedProductName, _joystickName) == 0)
                {
                    devicesFound++;
                    device = d;
                }
            }

            if (device == null)
            {
                throw new NullReferenceException("No such device found!");
            }

            var joystick = new Joystick(directInput, device.InstanceGuid);

            joystick.Properties.BufferSize = 128;
            joystick.Acquire();

            // Loop until a CancellationRequest is made
            while (!cancellationToken.IsCancellationRequested)
            {
                joystick.Poll();
                JoystickUpdate[] states = joystick.GetBufferedData();

                foreach (var state in states)
                {
                    progress.Report(state);
                }
            }
        }

    }

}
