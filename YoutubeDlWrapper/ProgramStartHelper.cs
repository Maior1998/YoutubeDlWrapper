using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace YoutubeDlWrapper
{
    public class ProgramStartHelper
    {
        static ProgramStartHelper()
        {
        }
        public string FilePath { get; }

        public List<StartArgument> Args { get; } = new();

        public ProgramStartHelper(string filePath)
        {
            FilePath = filePath;

        }

        public int? ExitCode { get; private set; }


        private Process process;
        public async Task Start()
        {
            process = new();
            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                FileName = FilePath,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
                StandardErrorEncoding = System.Text.Encoding.UTF8,
                StandardInputEncoding = System.Text.Encoding.UTF8,
                UseShellExecute = false,
                Arguments = string.Join(" ", Args.Select(x => x.ToString()))
            };
            process.StartInfo = processStartInfo;
            process.OutputDataReceived += DataReceivedEventHandler;
            process.Start();
            await process.WaitForExitAsync();
        }

        public async Task<IEnumerable<string>> StartAndGetOutputAsync()
        {
            await Start();
            string res = process.StandardOutput.ReadToEnd();
            return res.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        }

        public DataReceivedEventHandler DataReceivedEventHandler { get; set; } = (arg1, arg2) => { };
        public EventHandler ProcessExitedEventHandler { get; set; } = (arg1, arg2) => { };

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public class StartArgument
        {
            public StartArgument(string name, string value = null)
            {
                Name = name;
                Value = value;
            }
            public string Name { get; }
            public string Value { get; }
            public bool IsNeedSurroundWithQuotes { get; set; }
            public override string ToString()
            {
                if (Value == null) return Name;
                return $"{Name} {(IsNeedSurroundWithQuotes ? $"\"{Value}\"" : Value)}";
            }
        }

    }

}
