# Quokka.RISCV.Server
Internal server for handling requests to RISC-V toolchain

# Pick your OS

## For Windows
* Enable WSL on Windows machine
* Install Ubuntu 20.04 LTS from Microsoft Store
* Server will run on 18.04 LTS as well
* It also should run on other flavours of Linux, where .NET is available, I just have not tested them all
* There are reports of Quokka running on Debian and CentOS
* Run Ubuntu (first run will take some time, be patient, configure your user details)

NOTE: git should already be installed in Ubuntu by default

## For Linux
All good, proceed to installation section

# Installation
It is assumed that local server will be placed into ~/qrs location.

Clone integration server repository into your home directory
```
cd ~
git clone https://github.com/EvgenyMuryshkin/Quokka.RISCV.Server.git qrs
```

Run installation script from qrs

It will download and build all required tools, place launch and update scripts into home directory for easy of use
```
cd ~/qrs
chmod 766 ./install
./install
```

# Update
To update local server, run update script from home folder
```
~/update
```

# Launch
To launch server, run launch script
```
~/launch
```
