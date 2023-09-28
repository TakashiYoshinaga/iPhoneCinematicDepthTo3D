//Copyright © 2023 Takashi Yoshinaga
//This code is provided as a sample and is not responsible for any errors.
//This app is using following tools.
//GPAC2.2 https://gpac.wp.imt.fr/
//(Installer: https://gpac.wp.imt.fr/downloads/gpac-nightly-builds/)
//FFmpeg https://ffmpeg.org/
//(Executable: https://github.com/BtbN/FFmpeg-Builds/releases/download/autobuild-2023-09-27-12-48/ffmpeg-N-112191-g58b6c0c327-win64-lgpl.zip)
//To use this project, put ffmpeg.exe in the same folder of mp4box.exe

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DepthExtractor
{
    public partial class Form1 : Form
    {
        string _originalFileName = null;
        string _folderPath = null;
        const string mp4boxPath = @"C:\Program Files\GPAC\mp4box.exe";
        const string ffmpegPath = @"C:\Program Files\GPAC\ffmpeg.exe";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool result = GetFileName();
            button2.Visible = result;
            label2.Text = "";
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            label2.Text = "Processing...";
            await Task.Run(() =>
            {
                //Extract 2nd steram
                string arguments = $"-add self#2:hdlr=vide {_originalFileName} -out {_folderPath}\\remux.mp4";
                ExecuteCommand(mp4boxPath, arguments);
                ExecuteCommand(ffmpegPath, "");

                //Extract depth video
                //See more detail: https://trac.ffmpeg.org/wiki/Encode/VP8
                arguments = $"-vcodec hevc -i {_folderPath}\\remux.mp4 -map 0:1 -c:v vp8 -qmin 0 -qmax 50 -crf 5 -b:v 1M -pix_fmt yuv420p {_folderPath}\\depth_output.webm -y";
                ExecuteCommand(ffmpegPath, arguments);

                //Extract color video
                //UV mapping is performed on the depth image, so any size is acceptable.
                //However, using sizes of 2^n offers faster processing, so the size is fixed at 512x512 for this case.
                arguments = $"-vcodec hevc -i {_folderPath}\\remux.mp4 -map 0:0 -c:v vp8 -qmin 0 -qmax 50 -crf 5 -b:v 1M -pix_fmt yuv420p -vf scale=512:512 {_folderPath}\\color_output.webm -y";
                ExecuteCommand(ffmpegPath, arguments);    
            });
            Console.WriteLine("Done!");
            label2.Text = "Done! (Check depth_output.mp4)";
            button2.Enabled = true;
            button1.Enabled = true;
        }

        private void ExecuteCommand(string executablePath, string arguments)
        {
            // Configure process settings
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            // Execute the process and wait for it to complete
            try
            {
                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();

                    // Asynchronously read output and error
                    process.OutputDataReceived += (sender, e) => { Console.WriteLine( e.Data ); };
                    process.ErrorDataReceived += (sender, e) => { Console.WriteLine(e.Data); };

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Wait for the process to exit
                    process.WaitForExit();
                 }
            }
            catch (Exception ex)
            {
                // Error handling
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }
        private bool GetFileName()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Set the filter to only show .mov files
            openFileDialog.Filter = "MOV files (*.mov)|*.mov|All files (*.*)|*.*";

            // Show the open file dialog
            DialogResult result = openFileDialog.ShowDialog();
            // Show the open file dialog
            if (result == DialogResult.OK)
            {
                // Get the full path of the selected file
                _originalFileName = openFileDialog.FileName;
                // Get the directory path where the selected file exists
                _folderPath = Path.GetDirectoryName(_originalFileName);
                label1.Text = openFileDialog.SafeFileName;
               
                return true;
            }
            label1.Text = "";
            return false;
        }

    }
}
