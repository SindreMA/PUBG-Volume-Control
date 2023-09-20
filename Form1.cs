using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PUBG_Volume_Control
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            /*
            var sdsds = new ConfigFile();
            List<string> ls = new List<string>();
            ls.Add("tslgame");
            ls.Add("wow-64");
            sdsds.Games = ls;
            sdsds.VolumeState1 = "1";
            sdsds.VolumeState2 = "2";
            string ds = JsonConvert.SerializeObject(sdsds,Formatting.Indented);
            File.WriteAllText("config.json", ds);
            
               
    */
            if (Process.GetProcesses().ToList().Where(x=> x.ProcessName == Process.GetCurrentProcess().ProcessName).Count() > 1)
            {
               var pro = Process.GetProcesses().ToList().Where(x => x.ProcessName == Process.GetCurrentProcess().ProcessName);
                pro.ToArray()[1].Kill();



            }
            BackColor = Color.Lime;
            TransparencyKey = Color.Lime;
            InitializeComponent();
            hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);
            hook.RegisterHotKey(ModifierKeys.Shift, Keys.PageUp);
            this.Hide();
            this.WindowState = FormWindowState.Minimized;
               
            this.ShowInTaskbar = false;
            notifyIcon1.ShowBalloonTip(2000, "Running!", "The program have been started!", ToolTipIcon.Info);
            
        }

        protected override CreateParams CreateParams
        {//Remove window in alt tab
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80;
                return cp;
            }
        }
        public sealed class KeyboardHook : IDisposable
        {
            // Registers a hot key with Windows.
            [DllImport("user32.dll")]
            private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
            // Unregisters the hot key with Windows.
            [DllImport("user32.dll")]
            private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

            /// <summary>
            /// Represents the window that is used internally to get the messages.
            /// </summary>
            private class Window : NativeWindow, IDisposable
            {
                private static int WM_HOTKEY = 0x0312;

                public Window()
                {
                    // create the handle for the window.
                    this.CreateHandle(new CreateParams());
                }

                /// <summary>
                /// Overridden to get the notifications.
                /// </summary>
                /// <param name="m"></param>
                protected override void WndProc(ref Message m)
                {
                    base.WndProc(ref m);

                    // check if we got a hot key pressed.
                    if (m.Msg == WM_HOTKEY)
                    {
                        // get the keys.
                        Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);
                        ModifierKeys modifier = (ModifierKeys)((int)m.LParam & 0xFFFF);

                        // invoke the event to notify the parent.
                        if (KeyPressed != null)
                            KeyPressed(this, new KeyPressedEventArgs(modifier, key));
                    }
                }

                public event EventHandler<KeyPressedEventArgs> KeyPressed;

                #region IDisposable Members

                public void Dispose()
                {
                    this.DestroyHandle();
                }

                #endregion
            }

            private Window _window = new Window();
            private int _currentId;

            public KeyboardHook()
            {
                // register the event of the inner native window.
                _window.KeyPressed += delegate (object sender, KeyPressedEventArgs args)
                {
                    if (KeyPressed != null)
                        KeyPressed(this, args);
                };
            }

            /// <summary>
            /// Registers a hot key in the system.
            /// </summary>
            /// <param name="modifier">The modifiers that are associated with the hot key.</param>
            /// <param name="key">The key itself that is associated with the hot key.</param>
            public void RegisterHotKey(ModifierKeys modifier, Keys key)
            {
                // increment the counter.
                _currentId = _currentId + 1;

                // register the hot key.
                if (!RegisterHotKey(_window.Handle, _currentId, (uint)modifier, (uint)key))
                    throw new InvalidOperationException("Couldn’t register the hot key.");
            }

            /// <summary>
            /// A hot key has been pressed.
            /// </summary>
            public event EventHandler<KeyPressedEventArgs> KeyPressed;

            #region IDisposable Members

            public void Dispose()
            {
                // unregister all the registered hot keys.
                for (int i = _currentId; i > 0; i--)
                {
                    UnregisterHotKey(_window.Handle, i);
                }

                // dispose the inner native window.
                _window.Dispose();
            }

            #endregion
        }

        /// <summary>
        /// Event Args for the event that is fired after the hot key has been pressed.
        /// </summary>
        public class KeyPressedEventArgs : EventArgs
        {
            private ModifierKeys _modifier;
            private Keys _key;

            internal KeyPressedEventArgs(ModifierKeys modifier, Keys key)
            {
                _modifier = modifier;
                _key = key;
            }

            public ModifierKeys Modifier
            {
                get { return _modifier; }
            }

            public Keys Key
            {
                get { return _key; }
            }
        }

        /// <summary>
        /// The enumeration of possible modifiers.
        /// </summary>
        [Flags]
        public enum ModifierKeys : uint
        {
            Alt = 1,
            Control = 2,
            Shift = 4,
            Win = 8
        }
        KeyboardHook hook = new KeyboardHook();
        public bool muted = false;
        void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {

            notifyIcon1.Icon = SystemIcons.Exclamation;
            notifyIcon1.Visible = true;
           

            var file = Environment.CurrentDirectory + @"\config.json";
            if (!File.Exists(file))
            {
                notifyIcon1.ShowBalloonTip(5000, "Error!", "Cant find config file", ToolTipIcon.Error);
            }
            else
            {


                try
                {


                    ConfigFile config = new ConfigFile();

                    try
                    {

                        string json = File.ReadAllText(file);
                        config = JsonConvert.DeserializeObject<ConfigFile>(json);
                    }
                    catch (Exception)
                    {
                    }
                    var list = Process.GetProcesses();
                    foreach (var item in config.Games)
                    {


                        if (list.Any(X => X.ProcessName.ToLower() == item.ToLower()))
                        {
                            var game = Process.GetProcesses().Where(X => X.ProcessName.ToLower() == item.ToLower()).ToArray()[0];

                            ProcessStartInfo program = new ProcessStartInfo();
                            program.CreateNoWindow = true;
                            program.WindowStyle = ProcessWindowStyle.Hidden;

                            if (muted)
                            {
                                program.FileName = Environment.CurrentDirectory + @"\nircmd.exe";
                                program.Arguments = "setappvolume /" + game.Id + " " + config.VolumeState1;
                                Process processTemp = new Process();
                                processTemp.StartInfo = program;
                                processTemp.Start();
                            }
                            else
                            {
                                program.FileName = Environment.CurrentDirectory + @"\nircmd.exe";
                                program.Arguments = "setappvolume /" + game.Id + " " + config.VolumeState2;
                                Process processTemp = new Process();
                                processTemp.StartInfo = program;
                                processTemp.Start();
                            }


                        }
                    }
                    if (muted)
                    {
                        muted = false;
                    }
                    else
                    {
                        muted = true;
                    }

                }
                catch (Exception ex)
                {
                    this.Focus();
                    MessageBox.Show(ex.Message,"PUBG Volume Control",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    this.Focus();

                }
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

