using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace testMQ
{
    /*
# Accessibility shortcut: 0x46 (Keyboard)
# Control shortcut: 0x47 (Keyboard)
# AppSwitch: 0x4d (Keyboard End)
# Home: 0x4a (Keyboard Home)
# Previous item: 0x50 (Keyboard LeftArrow)
# Next item: 0x4f (Keyboard RightArrow)
# Select item: 0x51 (Keyboard DownArrow)
# Tap: 0x52 (Keyboard UpArrow)
- with switch setting group off
- with scan mode to manual
- with auto hide disabled.
    */
    class KeyInput
    {
        static KeyInput pThis = null;
        string str_serial_port = string.Empty;
        SerialPort _port = null;
        public static KeyInput getInstance()
        {
            if (pThis == null)
                pThis = new KeyInput();
            return pThis;
        }
        public KeyInput()
        {

        }
        public void setSerialPort(String port)
        {
            str_serial_port = port;
        }
        public bool start()
        {
            bool ret = false;
            try
            {
                if (!string.IsNullOrEmpty(str_serial_port))
                {
                    _port = new System.IO.Ports.SerialPort(str_serial_port);
                    _port = new SerialPort(str_serial_port, 9600);
                    _port.BaudRate = 9600;
                    _port.Parity = Parity.None;
                    _port.StopBits = StopBits.One;
                    _port.DataBits = 8;
                    _port.Handshake = Handshake.None;
                    _port.RtsEnable = true;
                    _port.DtrEnable = true;
                    _port.ReadTimeout = 1000;
                    _port.WriteTimeout = 1000;
                    _port.Open();
                    ret = _port.IsOpen;
                }
            }
            catch (Exception ex)
            {
                Program.logIt(ex.Message);
                Program.logIt(ex.StackTrace);
            }
            return ret;
        }
        public bool close()
        {
            bool ret = true;
            if (_port != null && _port.IsOpen)
                _port.Close();
            return ret;
        }
        public bool sendKey(byte ch)
        {
            bool ret = false;
            try
            {
                if (_port != null && _port.IsOpen)
                {
                    byte[] b = new byte[5];
                    b[0] = 0x83;
                    b[1] = 0x00;
                    b[2] = 0x00;
                    b[3] = ch;
                    b[4] = 0x00;
                    _port.Write(b, 0, 5);
                }
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }
        public bool sendKey(byte[] data)
        {
            bool ret = false;
            try
            {
                if (_port != null && _port.IsOpen && data != null && data.Length > 0)
                {
                    //byte[] b = new byte[5];
                    //b[0] = 0x83;
                    //b[1] = 0x00;
                    //b[2] = 0x00;
                    //b[3] = ch;
                    //b[4] = 0x00;
                    _port.Write(data, 0, data.Length);
                }
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }

    }
}
