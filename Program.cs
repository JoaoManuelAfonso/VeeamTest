using System;
using System.IO;
using System.Threading;

class Program
{
    private static string source;
    private static string replica;
    private static string logFile;
    private static int interval;
    private static Timer timer;

    private static void Main(string[] args)
    {
        source = args[0];
        replica = args[1];
        logFile = args[2];
        interval = Int32.Parse(args[3]);

        timer = new Timer(new TimerCallback(runInterval), null, 0, interval * 1000);

        source = Directory.CreateDirectory(source).FullName;
        replica = Directory.CreateDirectory(replica).FullName;
        Console.WriteLine("Press any key to end");
        Console.WriteLine("Folders were created in " + source + " and " + replica);
        Console.WriteLine("Checking every " + interval + " seconds");
        Console.ReadLine();
    }
    private static void runInterval(object state)
    {
        string[] sources = Directory.GetFiles(source);
        string[] replicas = Directory.GetFiles(replica);
        createFile(sources, replicas);
        deleteFile(sources, replicas);
        updateFile(sources);
    }

    private static void createFile(string[] sources, string[] replicas)
    {
        foreach (string source in sources)
        {
            bool exists = false;
            string sourceFileName = Path.GetFileName(source);

            foreach (string replica in replicas)
            {
                if (Path.GetFileName(replica) == sourceFileName)
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                string destination = Path.Combine(replica, sourceFileName);
                File.Copy(source, destination);
                File.AppendAllText(logFile, "File added to replica from source at " + DateTime.Now + "\n");
                Console.WriteLine("New file added to replica: " + sourceFileName);
            }
        }
    }
    private static void deleteFile(string[] sources, string[] replicas)
    {
        foreach (string replica in replicas)
        {
            bool exists = false;
            string replicaFileName = Path.GetFileName(replica);

            foreach (string source in sources)
            {
                if (Path.GetFileName(source) == replicaFileName)
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                File.Delete(replica);
                File.AppendAllText(logFile, "File deleted from replica at " + DateTime.Now + "\n");
                Console.WriteLine("File deleted from replica: " + replicaFileName);
            }
        }
    }

    private static void updateFile(string[] sources)
    {
        foreach (string source in sources)
        {
            string fileName = Path.GetFileName(source);
            string replicaFile = Path.Combine(replica, fileName);

            if (File.Exists(replicaFile))
            {
                DateTime sourceTime = File.GetLastWriteTime(source);
                DateTime replicaTime = File.GetLastWriteTime(replicaFile);

                if (sourceTime > replicaTime)
                {
                    File.Copy(source, replicaFile, true);
                    File.AppendAllText(logFile, "File updated in replica: " + fileName + " at " + DateTime.Now + "\n");
                    Console.WriteLine("File updated in replica: " + fileName);
                }
            }
        }
    }
}
