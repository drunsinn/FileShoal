using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml.Serialization;
using WinSCP;
using Binding = System.Windows.Data.Binding;
using DragEventArgs = System.Windows.DragEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace FileShoal
{
    public partial class MainWindow : Window
    {

        BindingList<string> fileList = new BindingList<string>();
        List<MachineConfiguration> machineConfig = new List<MachineConfiguration>();

        public MainWindow()
        {
            InitializeComponent();

            XmlSerializer serializer = new XmlSerializer(typeof(List<MachineConfiguration>));
            using (FileStream stream = File.OpenRead(Properties.Settings.Default.MachineConfigPath))
            {
                machineConfig = (List<MachineConfiguration>)serializer.Deserialize(stream);
            }

            lstBoxFiles.ItemsSource = fileList;
            txtTargetPath.Text = Properties.Settings.Default.RemotePath;
            lblRemoteBasePath.Content = Properties.Settings.Default.RemoteBasePath;

            int numGridRows = 16;
            double numGridColums = (double)machineConfig.Count / numGridRows;
            for (int i = 0; i < numGridColums; i++)
            {
                Console.WriteLine("Add Column");
                ColumnDefinition gridCol = new ColumnDefinition();
                grdCheckBox.ColumnDefinitions.Add(gridCol);
            }
            for (int i = 0; i < numGridRows; i++)
            {
                RowDefinition gridRow = new RowDefinition();
                gridRow.Height = new GridLength(20);
                grdCheckBox.RowDefinitions.Add(gridRow);
            }

            int row = 0;
            int column = 0;
            foreach (MachineConfiguration machine in machineConfig)
            {
                System.Windows.Controls.CheckBox newCb = new System.Windows.Controls.CheckBox();

                Binding xBinding = new Binding
                {
                    Source = machine,
                    Path = new PropertyPath("ActiveTarget"),
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Mode = BindingMode.TwoWay
                };

                BindingOperations.SetBinding(newCb, System.Windows.Controls.CheckBox.IsCheckedProperty, xBinding);

                newCb.Content = machine.Name;

                Grid.SetRow(newCb, row);
                Grid.SetColumn(newCb, column);
                grdCheckBox.Children.Add(newCb);
                row++;
                if (row >= numGridRows)
                {
                    row = 0;
                    column++;
                }
            }

            LableStatus.Content = "Ready";

        }

        private void BtnSendClick(object sender, RoutedEventArgs e)
        {
            string targetFolder = GetRemotePath();

            LableStatus.Content = "Transfering Files";

            foreach (MachineConfiguration machine in machineConfig)
            {
                if (machine.ActiveTarget)
                {
                    try
                    {
                        SessionOptions sessionOptions = new SessionOptions
                        {
                            Protocol = Protocol.Sftp,
                            HostName = machine.Address,
                            UserName = Properties.Settings.Default.RemoteUserName,
                            SshHostKeyFingerprint = machine.Fingerprint,
                            SshPrivateKeyPath = Properties.Settings.Default.PrivateKeyPath,
                        };

                        using (Session session = new Session())
                        {
                            session.Open(sessionOptions);

                            TransferOptions transferOptions = new TransferOptions
                            {
                                TransferMode = TransferMode.Automatic
                            };

                            if (!session.FileExists(targetFolder))
                            {
                                session.CreateDirectory(targetFolder);
                            }

                            foreach (string filePath in fileList)
                            {
                                FileAttributes attr = File.GetAttributes(filePath);

                                if (attr.HasFlag(FileAttributes.Directory))
                                {
                                    Console.WriteLine("Directory");
                                }
                                else
                                {
                                    Console.WriteLine("File");
                                }

                                TransferOperationResult transferResult;
                                transferResult = session.PutFiles(filePath, targetFolder, false, transferOptions);

                                transferResult.Check();

                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        System.Windows.MessageBox.Show("Transfer faild with message {0}", exception.Message);
                    }
                }
            }
            LableStatus.Content = "Transfer Finished";
        }

        private string GetRemotePath()
        {
            string targetFolder = txtTargetPath.Text;

            if (!targetFolder.EndsWith("/"))
            {
                targetFolder += "/";
            }

            txtTargetPath.Text = targetFolder;
            Properties.Settings.Default.RemotePath = txtTargetPath.Text;

            targetFolder = Properties.Settings.Default.RemoteBasePath + targetFolder;
            return targetFolder;
        }

        private void FileListDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);

                for (int i = 0; i < files.Length; i++)
                {
                    fileList.Add(files[i]);
                }
            }
        }

        private void ListBoxKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                case Key.Back:
                    while (lstBoxFiles.SelectedItem != null)
                    {
                        fileList.Remove(lstBoxFiles.SelectedItem as string);
                    }
                    break;
            }
        }

        private void TxtTargetPathChanged(object sender, TextChangedEventArgs e)
        {
            string targetFolder = txtTargetPath.Text;

            targetFolder = targetFolder.Replace("\\", "/");
            targetFolder = targetFolder.Replace("../", "");

            txtTargetPath.Text = targetFolder;
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            File.Delete(Properties.Settings.Default.MachineConfigPath);
            using (FileStream stream = File.OpenWrite(Properties.Settings.Default.MachineConfigPath))
            {

                XmlSerializer serializer = new XmlSerializer(typeof(List<MachineConfiguration>));
                serializer.Serialize(stream, machineConfig);
            }
        }

        private void BtnRecive_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog
            {
                Description = "Select Target Folder"
            };

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LableStatus.Content = "Reciving Files";

                string localBasePath = folderBrowserDialog.SelectedPath;
                string remotePath = GetRemotePath();

                foreach (MachineConfiguration machine in machineConfig)
                {
                    if (machine.ActiveTarget)
                    {
                        string localPath = localBasePath + "\\" + machine.Name;
                        Directory.CreateDirectory(localPath);

                        try
                        {
                            SessionOptions sessionOptions = new SessionOptions
                            {
                                Protocol = Protocol.Sftp,
                                HostName = machine.Address,
                                UserName = Properties.Settings.Default.RemoteUserName,
                                SshHostKeyFingerprint = machine.Fingerprint,
                                SshPrivateKeyPath = Properties.Settings.Default.PrivateKeyPath,
                            };

                            using (Session session = new Session())
                            {
                                session.Open(sessionOptions);

                                TransferOptions transferOptions = new TransferOptions
                                {
                                    TransferMode = TransferMode.Automatic

                                };

                                TransferOperationResult transferResult;
                                transferResult = session.GetFiles(remotePath, localPath, false, transferOptions);

                                transferResult.Check();
                            }
                        }
                        catch (Exception exception)
                        {
                            System.Windows.MessageBox.Show("Transfer faild with message {0}", exception.Message);
                        }
                    }
                }
                LableStatus.Content = "Transfer Finished";
            }

        }

        public class MachineConfiguration
        {
            public MachineConfiguration() { }

            public MachineConfiguration(String name, String address, String fingerprint, Boolean activeTarget)
            {
                this.Name = name;
                this.Address = address;
                this.Fingerprint = fingerprint;
                this.ActiveTarget = activeTarget;
            }

            public String Name { get; set; }
            public String Address { get; set; }
            public String Fingerprint { get; set; }
            public Boolean ActiveTarget { get; set; }
        }
    }
}
