# UPnP_PortMapp0r
Add, remove and list port mappings (i.e. forwarding rules) to/from/of your UPnP-capable router from Windows.


### System Requirements
This tool only runs on a Windows operating system (as it is just a wrapper around NATUPNPLib).
It also needs a .NET Framework 4.0 compatible .NET platform installed (client profile suffices).


### Usage
<code>
UPnP_PortMapp0r - by Sgt. Nukem, version: 0.1.3.7
=================================================

Usage: UPnP_PortMapp0r <command> [parameters]

where <command> can be:
  list    - lists all port mappings.
  add     - adds a port mapping, specify parameters       <port> <protocol> <mapped-address> [mapped-port]
  remove  - removes a port mapping, specifiy parameters   <port> <protocol>

Exmaple:  > UPnP_PortMapp0r  add  443  TCP  192.168.1.200
</code>


### Prerequisites
You will need a UPnP-capable router in your local subnet and enable dynamic adding of port mappings to programs.

#### Example Router configuration:

Manufacturer:   AVM GmbH, Berlin, Germany
Model:          Fritz!Box 7320

Navigate to
  (german)   Internet -> Freigaben -> Portfreigaben
  (english)  Internet -> Sharing   -> Port Sharing

You will need to check the option
  (german)   [X] Änderungen der Sicherheitseinstellungen über UPnP gestatten"
  (english)  [X] Allow changes to security settings via UPnP

Otherwise you will get strange COM exceptions like "HRESULT: 0x80040209".
