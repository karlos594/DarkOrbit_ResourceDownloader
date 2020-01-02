using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Net;
using System.Xml;

namespace DarkOrbit_ResourceDownloader
{
    class Program
    {
        private const string DIR_NAME = "DarkOrbit_Resources";

        private const string MAIN_RESOURCES_URL = "http://test2.darkorbit.bigpoint.com/spacemap/";
        private const string RESOURCES_URL = "http://test2.darkorbit.bigpoint.com/spacemap/xml/resources.xml";
        private const string RESOURCES_3D_URL = "http://test2.darkorbit.bigpoint.com/spacemap/xml/resources_3d.xml";
        private const string RESOURCES_3D_PARTICLES_URL = "http://test2.darkorbit.bigpoint.com/spacemap/xml/resources_3d_particles.xml";

        static void Main(string[] args)
        {
            if(!IsWriteable(Environment.CurrentDirectory))
            {
                Console.WriteLine("Current directory is not writeable!");
                Console.ReadKey();
                return;
            }

            while(true)
            {
                Console.Clear();
                Console.WriteLine("DarkOrbit Resources Downloader");
                Console.WriteLine();
                Console.WriteLine("Using resources url: " + RESOURCES_URL);
                Console.WriteLine("Using resources_3d url: " + RESOURCES_3D_URL);
                Console.WriteLine("Using resources_3d_particles url: " + RESOURCES_3D_PARTICLES_URL);
                Console.WriteLine();
                Console.WriteLine("Select your prefered action . . .");
                Console.WriteLine("Press [ 0 for exit ] [ 1 for resources ] [ 2 for resources_3d ] [ 3 for resources_3d_particles ] [ 4 for all resources ]");
                ConsoleKeyInfo pressedKey = Console.ReadKey(true);
                char action = pressedKey.KeyChar;

                switch(action)
                {
                    case '0':
                        return;
                    case '1':
                        DownloadResources(RESOURCES_URL, "resources");
                        break;
                    case '2':
                        DownloadResources(RESOURCES_3D_URL, "resources_3d");
                        break;
                    case '3':
                        DownloadResources(RESOURCES_3D_PARTICLES_URL, "resources_3d");
                        break;
                    case '4':
                        DownloadResources(RESOURCES_URL, "resources");
                        DownloadResources(RESOURCES_3D_URL, "resources_3d");
                        DownloadResources(RESOURCES_3D_PARTICLES_URL, "resources_3d");
                        break;
                    default:
                        continue;
                }

                Console.WriteLine();
                Console.WriteLine("Completed.");
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
        }

        private static void DownloadResources(string resourcesUrl, string dirName)
        {
            Console.Clear();

            string mainDirPath = Path.Combine(Environment.CurrentDirectory, Path.Combine(DIR_NAME, dirName));
            Directory.CreateDirectory(mainDirPath);

            var locations = new Dictionary<string, string>();

            try
            {
                using (XmlReader reader = XmlReader.Create(resourcesUrl))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            string nodeName = reader.Name;
                            if (nodeName == "location")
                            {
                                string id = reader["id"];
                                string path = reader["path"];

                                if (!locations.ContainsKey(id))
                                {
                                    locations.Add(id, path);

                                    Directory.CreateDirectory(Path.Combine(mainDirPath, path));
                                }
                            }
                            else if (nodeName == "file")
                            {
                                string name = reader["name"];
                                string type = reader["type"];
                                string location = reader["location"];
                                string path = locations[location];

                                string urlPath = MAIN_RESOURCES_URL + path + name + "." + type;
                                string filePath = Path.Combine(mainDirPath, Path.Combine(path, name + "." + type));

                                DownloadFile:

                                try
                                {
                                    DownloadFile(urlPath, filePath);
                                }
                                catch(Exception)
                                {
                                    Console.WriteLine();
                                    Console.WriteLine("Dropped connection. Waiting 10 seconds.");
                                    System.Threading.Thread.Sleep(10000);
                                    goto DownloadFile;
                                }

                                Console.WriteLine("Downloaded: " + urlPath);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception: " + ex.ToString());
                Console.WriteLine("ERROR: " + ex.Message);
            }
        }

        private static void DownloadFile(string url, string path)
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(url, path);
            }
        }

        private static bool IsWriteable(string directory)
        {
            FileIOPermission writePermission = new FileIOPermission(FileIOPermissionAccess.Write, directory);
            PermissionSet pSet = new PermissionSet(PermissionState.None);
            pSet.AddPermission(writePermission);

            return pSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet);
        }
    }
}
