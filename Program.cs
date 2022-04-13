using System;
using System.IO;
using System.Security.Cryptography;
// I will use English because my written Russian is worse than English.
namespace VeaamMultithreadingSignatureTask
{
    class Program
    {
        public static string ByteArrayReadableString(byte[] array)
        {
            var readableByteArray = "";
            for (int i = 0; i < array.Length; i++)
            {
                readableByteArray += $"{array[i]:X2}";
                if ((i % 4) == 3 && i != array.Length - 1) readableByteArray += " ";
            }
            return readableByteArray;
        }
        static void Main(string[] args)
        {
            var wrongUsageMessageHead = $"Usage: {System.AppDomain.CurrentDomain.FriendlyName} <file path> <chunk size>";
            if (args.Length != 2)
            {
                Console.WriteLine(wrongUsageMessageHead);
                return;
            }

            string filePathStr = args[0];
            string chunkSizeStr = args[1];

            if (!File.Exists(filePathStr))
            {
                Console.WriteLine($"{wrongUsageMessageHead}\n {filePathStr} doesn't exist");
                return;
            }

            if (!int.TryParse(chunkSizeStr, out int chunckSize))
            {
                Console.WriteLine($"{wrongUsageMessageHead}\n ${chunkSizeStr} is not an integer");
                return;
            }

            try
            {
                using var fileStream = new FileStream(filePathStr, FileMode.Open, FileAccess.Read);
                var chunckBuffer = new byte[chunckSize];
                var producerConsumerQueue = new ProducerConsumerQueue(Environment.ProcessorCount);
                int counter = 0;
                while (fileStream.Read(chunckBuffer, 0, chunckSize) == chunckSize)
                {
                    
                    var chunck = new byte[chunckBuffer.Length];
                    Array.Copy(chunckBuffer, chunck, chunckBuffer.Length);
                    int counterCopy = counter;
                    producerConsumerQueue.EnqueueItem(() =>
                    {
                        using var sha256 =  SHA256.Create();
                        byte[] hashedChunck = sha256.ComputeHash(chunck);
                        Console.WriteLine($"{counterCopy} {ByteArrayReadableString(hashedChunck)}");

                    });
                    ++counter;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message} {ex.StackTrace}");
            }

        }
    }
}
