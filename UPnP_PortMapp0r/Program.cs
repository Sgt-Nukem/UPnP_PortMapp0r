using NATUPNPLib;
using System;
using System.Collections;


namespace UPnP_PortMapp0r
{
    enum HRESULT : uint
	{
		// returned by NATUPNPLib
		E_USER_EXCEPTION			= 0x80040208,
		E_SERVICE_SPECIFIC_ERROR	= 0x8007042A,


		E_ACCESSDENIED		= 0x80070005,
        E_FAIL				= 0x80004005,
        E_INVALIDARG		= 0x80070057,
        E_OUTOFMEMORY		= 0x8007000E,
        E_UNEXPECTED		= 0x8000FFFF,
        S_OK				= 0x0,
        S_FALSE				= 0x1
    }


    class Program
    {
        static void PrintHeader()
        {
			Console.WriteLine();
			Console.WriteLine("UPnP_PortMapp0r - by Sgt. Nukem, version: " + "0.1.3.8");
            Console.WriteLine("=================================================" + "\n");
        }

        static void PrintUsage(string errorMessage)
        {
            Console.WriteLine("Usage: UPnP_PortMapp0r <command> [parameters]" + "\n");
            Console.WriteLine("where <command> can be:");
			//Console.WriteLine("  list    - lists all port mappings. Specifiy parameter '/enabled-only' or '/disabled-only' to list only those enabled or disabled, respectively.");
			//Console.WriteLine("  add     - adds a port mapping. Specifiy parameters '/external-port=<EXT>', '/address=<ADDR>', '/port=<PORT>', '/description=<DESC>', too.");
			//Console.WriteLine("  remove  - removes a port mapping. Specifiy parameter '/port=<PORT>' to choose which one.");
			Console.WriteLine("  list    - lists all port mappings.");
			Console.WriteLine("  add     - adds a port mapping, specify parameters       <port> <protocol> <mapped-address> [mapped-port]");
			Console.WriteLine("  remove  - removes a port mapping, specifiy parameters   <port> <protocol>");
			Console.WriteLine();
			Console.WriteLine("Exmaple:  > UPnP_PortMapp0r  add  443  TCP  192.168.1.200");
			Console.WriteLine();
			FailureExit(HRESULT.E_INVALIDARG, errorMessage);
		}

        static void Main(string[] args)
        {
            UPnPNAT upnpNAT = null;
            try
            {
                upnpNAT = new UPnPNAT();
            }
            catch (Exception exception)
            {
                FailureExit(HRESULT.E_ACCESSDENIED, "Could not open NATUPNP library. Exception: " + exception.Message);
            }

            if( upnpNAT == null )
            {
                FailureExit(HRESULT.E_ACCESSDENIED, "Failed to instantiate NATUPNP library.");
            }

            PrintHeader();

			if (args.Length < 1)
			{
				PrintUsage("No command given.");
				return;
			}

            if( args[0] == "list" )
            {
                ListMappings(upnpNAT, args);
            }
			else if (args[0] == "add")
			{
				AddMapping(upnpNAT, args);
			}
			else if (args[0] == "remove")
			{
				RemoveMapping(upnpNAT, args);
			}
			else
			{
				PrintUsage("Invalid command given.");
			}
        }


        private static void RemoveMapping(UPnPNAT upnpNAT, string[] args)
        {
			if (args.Length < 1 + 2)
			{
				FailureExit(HRESULT.E_INVALIDARG, "You need to specifiy the port and protocol.");
			}

			int port;
			if (!int.TryParse(args[1], out port) || port < 0 || port > 65536)
			{
				FailureExit(HRESULT.E_INVALIDARG, "The specified port is invalid");
			}

			string protocol = args[2].ToUpperInvariant();
			if( protocol != "TCP" && protocol != "UDP" )
			{
				FailureExit(HRESULT.E_INVALIDARG, "The specified protocol is invalid");
			}

			try
			{
				var portMapp0r = retrievePortMapp0rOrExit(upnpNAT);
				portMapp0r.Remove(port, protocol);
			}
			catch (System.IO.FileNotFoundException exc)
			{
				FailureExit(HRESULT.S_FALSE, "The specified port mapping does not exist.");      // TODO: just return success then?
			}

			Console.WriteLine("OK");
			Console.WriteLine();
			Console.WriteLine();
		}


        private static void AddMapping(UPnPNAT upnpNAT, string[] args)
        {
			//	     <port> <protocol> <mapped-address> [mapped-port]"

			if (args.Length < 1 + 3)
            {
                // PrintAddMappingUsage()
                FailureExit(HRESULT.E_INVALIDARG, "You need to specifiy at least the port, protocol and mapped address.");
            }

			//ParseParameters(args);

			int port;
			if (!int.TryParse(args[1], out port) || port < 0 || port > 65536)
			{
				FailureExit(HRESULT.E_INVALIDARG, "The specified port is invalid");
			}

			string protocol = args[2].ToUpperInvariant();
			if (protocol != "TCP" && protocol != "UDP")
			{
				FailureExit(HRESULT.E_INVALIDARG, "The specified protocol is invalid");
			}

			string internalAddress = args[3];

			// TODO: CHECK IP 6+4

			int internalPort;
			if (args.Length > 1 + 3)
			{
				if (!int.TryParse(args[4], out internalPort) || internalPort < 0 || internalPort > 65536)
				{
					FailureExit(HRESULT.E_INVALIDARG, "The specified mapped-port is invalid");
				}
			} else
			{
				internalPort = port;
			}

			try
			{
				var portMapp0r = retrievePortMapp0rOrExit(upnpNAT);
				portMapp0r.Add(port, protocol, internalPort, internalAddress, true, "UPnP_PortMapp0r");
			}
			catch (System.Runtime.InteropServices.COMException exception)
			{
				var hresult = (UInt32)exception.ErrorCode;
				var hresultName = Enum.IsDefined(typeof(HRESULT), hresult) ? " (" + Enum.GetName(typeof(HRESULT), hresult) + ")" : "";
				Console.WriteLine("Received error from NATUPNPLib / the router: 0x{0:X}{1}", hresult, hresultName);
				
				if (hresult == (UInt32)HRESULT.E_USER_EXCEPTION)
				{
					Console.WriteLine();
					Console.WriteLine("Hint: You will get 0x{0:X} if you provide 127.0.0.1 as mapped address (which would be the router itself)." /* " probably means you fucked up!" */, hresult);
					Console.WriteLine();
					Console.WriteLine();
				}
				else if (hresult == (UInt32)HRESULT.E_SERVICE_SPECIFIC_ERROR)
				{
					Console.WriteLine();
					Console.WriteLine("Hint: You will get 0x{0:X} if you provide a mapped address the router is either not responsible for AT ALL or the device is not connected to the router at the moment." /* " probably means you fucked up!" */, hresult);
					Console.WriteLine();
					Console.WriteLine();
				}

				FailureExit(Enum.IsDefined(typeof(HRESULT), hresult) ? (HRESULT)hresult : HRESULT.S_FALSE, "Could not add port mapping. Exception: " + exception.Message);
			}
			catch (Exception exception)
			{
				FailureExit(HRESULT.S_FALSE, "Could not add port mapping. Exception: " + exception.Message);
			}

			Console.WriteLine("OK");
			Console.WriteLine();
			Console.WriteLine();
		}

		//const string paramExternalPort = "ext";
		//const string paramProtocol = "proto";
		//const string paramAddress = "ip";
		//const string paramPort = "port";
		//const string paramDisabled = "disabled";
		//const string paramEnabled = "enabled";
		//const string paramDescription = "desc";

		//private static void ParseParameters(string[] args)
		//{
		//    for(int i=1; i<args.Length; i++)
		//    {

		//    }
		//}

		private static IStaticPortMappingCollection retrievePortMapp0rOrExit(UPnPNAT upnpNAT)
		{
			IStaticPortMappingCollection staticMappings = upnpNAT.StaticPortMappingCollection;
			if (staticMappings == null)
			{
				FailureExit(HRESULT.S_FALSE, "Could not access static port mappings from NAT router. This can happen if it does not support UPnP, UPnP is not enabled, or its security settings disallows changes to port mappings via UPnP.");
			}
			return staticMappings;
		}



	private static void ListMappings(UPnPNAT upnpNAT, string[] args)
        {
            if( args.Length > 1 )
            {
                FailureExit(HRESULT.E_INVALIDARG, "Parameters not yet implemented.");
            }

			var portMapp0r = retrievePortMapp0rOrExit(upnpNAT);


			Console.Write("Found " + portMapp0r.Count + " port mappings");
            if (portMapp0r.Count > 0)
            {
                Console.WriteLine(":");
                Console.WriteLine();

                PrintPortMappingHeader();

                IEnumerator enumerat0r = portMapp0r.GetEnumerator();
                while( enumerat0r.MoveNext() )
                {
                    IStaticPortMapping portMapping = enumerat0r.Current as IStaticPortMapping;
                    if( portMapping == null )
                    {
                        FailureExit(HRESULT.E_UNEXPECTED, "The port mappings just got updated or changed otherwise. Please run me again to get the current results.");
                    }

                    PrintPortMapping(portMapping);
                }

                PrintPortMappingFooter();
            }
            else
            {
                Console.WriteLine(".");
            }

            Console.WriteLine();
			Console.WriteLine();
		}

        private static void PrintPortMappingHeader()
        {
            Console.WriteLine("       Description  Proto  External Address / Port   Internal Address / Port Enabled");
            Console.WriteLine("------------------------------------------------------------------------------------");
        }

        private static void PrintPortMappingFooter()
        {
            Console.WriteLine("------------------------------------------------------------------------------------");
        }

        private static void PrintPortMapping(IStaticPortMapping portMapping)
        {
            Console.WriteLine("| {0,16} | {1,3} | {2,15} | {3,5} | {4,15} | {5,5} | {6,3} |", portMapping.Description, portMapping.Protocol, portMapping.ExternalIPAddress, portMapping.ExternalPort,
                portMapping.InternalClient, portMapping.InternalPort, (portMapping.Enabled ? "(X)" : "( )"));
        }

        private static void FailureExit(HRESULT exitCode, string exitMessage)
        {
            Console.WriteLine("Error: " + exitMessage);
			Console.WriteLine();
			Console.WriteLine();
			Environment.Exit( (int)exitCode );
        }
    }
}
