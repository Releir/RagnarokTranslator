using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.Translate.V3;

namespace Translator
{
    class Program
    {
        static void Main(string[] args)
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            static extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesRead);

            Console.Title = "New Era Translator";
            Process[] proc = Process.GetProcessesByName("New Era Ragnarok");

            int addr = 0x0107AB40;

            byte[] ReadBytes(IntPtr handle, long address, uint bytesToRead)
            {
                IntPtr ptrBytesRead;
                byte[] buffer = new byte[bytesToRead];

                ReadProcessMemory(handle, new IntPtr(address), buffer, bytesToRead, out ptrBytesRead);

                return buffer;
            }

            String lastMessage = "";
            String lastSender = "";

            while (true)
            {
                string[] msgContent = parseMemoryMessage(proc, addr);
                if (msgContent.Length == 2)
                {
                    String tempSender = msgContent[0].Trim();
                    String tempMsg = msgContent[1];
                    if (isMessageFromPlayer(tempSender, tempMsg))
                    {
                        lastMessage = tempMsg;
                        lastSender = tempSender;
                        String translatedMsg = translateMessage(lastMessage);
                        Console.WriteLine(lastSender + " : " + translatedMsg);
                    }
                }
            }

            string[] parseMemoryMessage(Process[] proc, int addr)
            {
                byte[] buffer = ReadBytes(proc[0].Handle, addr, 100);
                int firstNullIndex = Array.FindIndex(buffer, b => b == 0);
                string msg = Encoding.Default.GetString(buffer, 0, firstNullIndex);
                string[] msgContent = msg.Split(":");
                return msgContent;
            }

            Boolean isMessageFromPlayer(string sender, string msg)
            {
                return !msg.Equals(lastMessage, StringComparison.OrdinalIgnoreCase)
                        && !sender.Equals("Players Online", StringComparison.OrdinalIgnoreCase);
            }
            
            string translateMessage(String message)
            {
                TranslationServiceClient client = new TranslationServiceClientBuilder
                {
                    JsonCredentials = "{}"
                }.Build();
                TranslateTextRequest request = new TranslateTextRequest
                {
                    Contents = { message },
                    SourceLanguageCode = "id",     //Put language to translate here
                    TargetLanguageCode = "en-US",
                    Parent = new ProjectName("Project Name Here").ToString()
                };
                TranslateTextResponse response = client.TranslateText(request);
                Translation translation = response.Translations[0];
                return translation.TranslatedText;
            }
        }
    }
}
