# CoilWinderHelp

[toc]

## Issues

- [ ] Raspberry Pi Video Playback is choppy.
    - Need to figure out how to get it to play smoothly.

## Development Tasks

### SignalR

- [x] Get ID's from SignalR Connected Clients (.Net Console App on Raspberry Pi)
    - Pinging Client IP-Address
    - Route Query Parameters
    - Generate ID's

- [x] Ensure UI updates are sent to the correct client
  - Raspberry pi will send "stop" from console app.
  - Server looks up document directory path via database
  - SignalR notifies UI of Raspberry Pi Web Page

### Pages / Components

- [x] Automatic Instructions Viewer
  - used for raspberry pi web page

- [ ] Admin Dashboard
  - Adjust <key, value> pairs for stops
  - upload new documents to folders
  - manage connected Devices
    - restart Pi's?
    - check ping?
    - etc.

- [ ] Coil Winder Stats Dashboard
  - Charts to display stats for all machines
  - Users per machine
  - TBD..

### Backend

- [x] Database:f for "STOP" Lookup tables
- [ ] Stop <---> Video Lookup Tables
- [ ] Database: Raspberry Pi Console Log History