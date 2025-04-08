using System;
using System.Diagnostics;
using System.IO; // This is for System.IO.Path
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace BypassSwitcher
{
    public partial class MainWindow : Window
    {
        private const string ConfigFilePath = "BypassSwitcherConfig.txt"; // Path to the config file
        private INIIO iniReader = new INIIO(AppDomain.CurrentDomain.BaseDirectory + "/BypassSwitcherConfig.ini");

        public MainWindow() {
            InitializeComponent();
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            IniFileCreation();
            DataContext = new WindowBlureffect(this, AccentState.ACCENT_ENABLE_BLURBEHIND) { BlurOpacity = 100 };
            FillComboBoxWithBatFiles();
            IniFileLoadPreferences(); // Load the last selected index

            // Check if the checkbox is checked and if winws.exe is not running
            if (StartBypassCheckBox.IsChecked == true && !IsProcessRunning("winws")) {
                StartLastSelectedBat();
            }
        }

        private void IniFileCreation() {
            if (!iniReader.HasKey("Settings", "SelectedBypass"))
                iniReader.WriteValue("Settings", "SelectedBypass", "0");
            if (!iniReader.HasKey("Settings", "RunOnLoad"))
                iniReader.WriteValue("Settings", "RunOnLoad", "false");
        }
        private void IniFileLoadPreferences() {
            StartBypassCheckBox.IsChecked = bool.Parse(iniReader.ReadValue("Settings", "RunOnLoad"));
            int SelectedIndex = int.Parse(iniReader.ReadValue("Settings", "SelectedBypass"));
            if (SelectedIndex >= 0 && SelectedIndex < BypassTypeComboBox.Items.Count) {
                BypassTypeComboBox.SelectedIndex = SelectedIndex;
            }
        }

        private void StartBypassCheckBox_Click(object sender, RoutedEventArgs e) {
            string value = StartBypassCheckBox.IsChecked.ToString();
            iniReader.WriteValue("Settings", "RunOnLoad", value);
        }

        private void FillComboBoxWithBatFiles() {
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var batFiles = Directory.GetFiles(exeDirectory, "general*.bat");

            BypassTypeComboBox.Items.Clear();

            foreach (var file in batFiles) {
                string fileName = System.IO.Path.GetFileName(file);
                BypassTypeComboBox.Items.Add(new ComboBoxItem { Content = fileName });
            }

            if (BypassTypeComboBox.Items.Count > 0) {
                BypassTypeComboBox.SelectedIndex = 0;
            } else {
                BypassTypeComboBox.IsEnabled = false;
                RefreshButton.IsEnabled = false;
                ApplyBypassButton.IsEnabled = false;
                StartBypassCheckBox.IsEnabled = false;

                MessageBox.Show("Не найдено файлов обхода.\r\nПроверьте папку где находится эта программа.\r\nПрограмма будет закрыта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        private int ChangeCounter = 0;
        private void BypassTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (ChangeCounter <= 2) { ChangeCounter++; return; }
            string value = BypassTypeComboBox.SelectedIndex.ToString();
            iniReader.WriteValue("Settings", "SelectedBypass", value);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e) {
            FillComboBoxWithBatFiles();
            GC.Collect();
        }

        private void ApplyBypassButton_Click(object sender, RoutedEventArgs e) {
            if (BypassTypeComboBox.SelectedItem is ComboBoxItem selectedItem) {
                string fileName = selectedItem.Content.ToString();
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = System.IO.Path.Combine(exeDirectory, fileName);

                CloseProcess("winws");

                try {
                    Process.Start(new ProcessStartInfo {
                        FileName = filePath,
                        UseShellExecute = true,
                        CreateNoWindow = false
                    });
                } catch (Exception ex) {
                    MessageBox.Show($"Ошибка при запуске файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            } else {
                MessageBox.Show("Пожалуйста, выберите файл для запуска.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void StartLastSelectedBat() {
            if (BypassTypeComboBox.SelectedItem is ComboBoxItem selectedItem) {
                string fileName = selectedItem.Content.ToString();
                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = System.IO.Path.Combine(exeDirectory, fileName);

                try {
                    Process.Start(new ProcessStartInfo {
                        FileName = filePath,
                        UseShellExecute = true,
                        CreateNoWindow = false
                    });
                } catch (Exception ex) {
                    MessageBox.Show($"Ошибка при запуске файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void CloseProcess(string processName) {
            var processes = Process.GetProcessesByName(processName);
            foreach (var process in processes) {
                try {
                    process.Kill();
                    process.WaitForExit();
                } catch (Exception ex) {
                    MessageBox.Show($"Ошибка при закрытии процесса {processName}: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool IsProcessRunning(string processName) {
            return Process.GetProcessesByName(processName).Any();
        }


        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_DragOver(object sender, DragEventArgs e) {
            e.Handled = true;
        }
    }
}