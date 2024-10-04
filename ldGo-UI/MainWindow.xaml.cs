using System;
using System.Net.NetworkInformation;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Net.Sockets;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using System.Reflection;
using System.Windows.Threading;

namespace ldGoUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NicTools NicTool;
        string _nicID = "";
        string _outFile = Directory.GetCurrentDirectory() + @"\switch-data.json";
        string _ldgoLocation = Directory.GetCurrentDirectory() + @"\ldGo.exe";
        ProcessStartInfo _psi;
        int ReadoutTimerValue = 0;

        string _protocolArg = string.Empty;
        Process _ldgoProcess = null;
        SwitchData _data { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(Resolve);

            if (!File.Exists(_ldgoLocation))
            {
                using (FileStream stream = new FileStream(_ldgoLocation, FileMode.CreateNew, FileAccess.Write))
                {
                    stream.Write(Properties.Resources.ldgo, 0, Properties.Resources.ldgo.Length);
                }
            }

            NicTool = new NicTools();

            RefreshNicsDropDown();

            _psi = new ProcessStartInfo();
            _psi.FileName = _ldgoLocation;
            _psi.RedirectStandardError = true;
            _psi.RedirectStandardOutput = true;
            _psi.UseShellExecute = false;
            _psi.CreateNoWindow = true;
        }
        private void cb_network_cards_DropDownClosed(object sender, EventArgs e)
        {
            string nicLookup = cb_network_cards.Text;
            if (nicLookup == "")
            {
                return;
            }
            //  remove the tick or cross from the text of the dropdown menu
            nicLookup = nicLookup.Remove(0, 4);
            
            NetworkInterface result = NicTool.Nics.First(obj => obj.Name == nicLookup);

            lb_interface_value.Content = result.Description;
            _nicID = result.Id;
            lb_ip_value.Content = "";
            foreach (UnicastIPAddressInformation ip in result.GetIPProperties().UnicastAddresses)
            {
                if (ip.Address.AddressFamily == AddressFamily.InterNetwork && result.OperationalStatus == OperationalStatus.Up)
                {
                    lb_ip_value.Content = ip.Address.ToString();
                    break;
                }
            }

            lb_mac_value.Content = NicTool.ConvertMac(result.GetPhysicalAddress().ToString());
        }
        private async void btn_get_data_Click(object sender, RoutedEventArgs e)
        {   
            try
            {
                File.Delete(_outFile);
            } catch {}

            SetControls(false);

            tb_output.Text = "";

            DispatcherTimer updateStatusTimer = new DispatcherTimer();
            updateStatusTimer.Interval = TimeSpan.FromSeconds(1);
            updateStatusTimer.Tick += UpdateStatusTimer_Tick;
            ReadoutTimerValue = 0;

            _psi.Arguments = "-n " + _nicID + " -o " + _outFile + _protocolArg;

            using (_ldgoProcess = Process.Start(_psi))
            using (StreamReader err = _ldgoProcess.StandardError)
            {
                updateStatusTimer.Start();
                lb_status.Content = "Status: starting listener " + ReadoutTimerValue + " seconds elapsed";

                bool done = await Task.Run(() => WaitForProcess()); 
                
                if (done)
                {
                    updateStatusTimer.Tick -= UpdateStatusTimer_Tick;
                    updateStatusTimer.Stop();

                    string error = err.ReadToEnd();
                    if(error != null)
                    {
                        lb_status.Content = error;
                    }
                    try
                    {
                        _data = NicTool.GetSwitchDataFromFile(_outFile);
                        tb_output.Text = NicTool.GetStringOfStructValues(_data);
                        lb_status.Content = "Status: complete";
                    }
                    catch(FileNotFoundException){
                        lb_status.Content = "Error: No link data found ";
                    }
                    catch (Exception ex)
                    {
                        lb_status.Content = "Error: " + ex.Message + "\ngold.exe exit code: " + _ldgoProcess.ExitCode + " " + error;
                    }
                }
                try { 
                    _ldgoProcess.Kill();
                }
                catch (InvalidOperationException ){}
                SetControls(true);
            }
        }

        private void UpdateStatusTimer_Tick(object sender, EventArgs e)
        {
            ReadoutTimerValue++;
            lb_status.Content = "Status: Starting listener " + ReadoutTimerValue + " seconds elapsed";
        }
        private async Task<bool> WaitForProcess()
        {
            _ldgoProcess.WaitForExit();
            return true;
        }
        private void CloseProcess(object sender, EventArgs e)
        {
            try
            {
                _ldgoProcess.Kill();
            }catch (Exception) { }
        }
        private void SetControls(bool state)
        {
            btn_get_data.IsEnabled = state;
            btn_copy.IsEnabled = state;            
            btn_refresh.IsEnabled = state;
            btn_save.IsEnabled = state;
        }
        public void RefreshNicsDropDown()
        {
            SetControls(false);
            NicTool.LoadNics();
            cb_network_cards.Items.Clear();
            foreach (string nicName in NicTool.GetNicNames())
            {
                cb_network_cards.Items.Add(nicName);
            }
            SetControls(true);
        }
        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ldgoProcess.Kill();
            }
            catch (NullReferenceException) { lb_status.Content = "Error: Cannot cancel when not running"; }
            catch (Exception) { }
        }
        private void rb_all_Checked(object sender, RoutedEventArgs e)
        {
            _protocolArg = "";
        }

        private void rb_cdp_Checked(object sender, RoutedEventArgs e)
        {
            _protocolArg = " -p cdp";
        }

        private void rb_lldp_Checked(object sender, RoutedEventArgs e)
        {
            _protocolArg = " -p lldp";
        }
        private void lb_ip_value_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Clipboard.SetText(lb_ip_value.Content.ToString());
            }
            catch (Exception) { }
        }
        private void lb_interface_value_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Clipboard.SetText(lb_interface_value.Content.ToString());
            }
            catch (Exception) { }

        }
        private void lb_mac_value_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Clipboard.SetText(lb_mac_value.Content.ToString());
            }
            catch (Exception) { }
        }
        private static Assembly Resolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Substring(0, args.Name.IndexOf(",")) == "Newtonsoft.Json")
            {
                return Assembly.Load(Properties.Resources.Newtonsoft_Json);
            }

            return null;
        }
        private void btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshNicsDropDown();
        }
        private void mi_npcap_click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://npcap.com/#download");
        }
        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog()
            {
                Filter = "Text Files(*.txt)|*.txt|All(*.*)|*",
                FileName = "switch-data.txt",
                DefaultExt = ".txt"
            };

            if (save.ShowDialog() == true)
            {
                File.WriteAllText(save.FileName, NicTool.GetStringOfStructNamesValues(_data));
            }

        }
        private void btn_copy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(NicTool.GetStringOfStructNamesValues(_data));
        }
        private void mi_settings_Click(object sender, RoutedEventArgs e)
        {
            Settings settingsMenu = new Settings();
            settingsMenu.ShowDialog();
        }

        private void me_help_Click(object sender, RoutedEventArgs e)
        {
            HelpPage help = new HelpPage();
            help.ShowDialog();
        }
    }
}