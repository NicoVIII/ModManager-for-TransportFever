# TPF-ModManager
Attempt to create a tool to support users managing their mods for TransportFever.

## How to use it
1. Download the .zip file and extract it at any place you wish.
2. Open the **settings.json** file with the editor of your choice.
3. Insert the path to the "mods" folder of your Transport Fever Installation. If you have problems to do this correctly, look [here](#settings).

   **For windows users:**
   Please use "\\\" or "/" as delimiter in the path. If you use the default "\" the program will NOT work.
   So use a path like "C:\\Spiele\\Transport Fever" or "C:/Spiele/Transport Fever".

You need to open a terminal for all following steps. Under Windows you can simply hold SHIFT and rightclick into the folder where your "TPFMM.exe" is located and choose "Open command window here".

* To **install** new mods, go to http://transportfever.net and look for mods you like. Type into your terminal `TPFMM.exe install` followed by the urls of your chosen mods seperated by spaces. For now it is **not possible** to install mods, for which the creator uploaded **multiple zip files**.

  **Note:** You probably have to write "mono TPFMM.exe" instead of "TPFMM.exe" if you are **not on a windows system.**

  **Example:**
  ~~~~
  TPFMM.exe install https://www.transportfever.net/filebase/index.php/Entry/2322-v200-in-verschiedenen-Versionen/ https://www.transportfever.net/filebase/index.php/Entry/2366-BR-242-Holzroller/
  ~~~~

* To see **which mods are installed**, type `TPFMM.exe list`.

* To see the available **updates** for installed mods, simply type `TPFMM.exe update`. This command will **NOT** upgrade your mods, it will just show you, which ones are available.

* To **upgrade** your installed mods, type `TPFMM.exe upgrade`. This command will look which mods have another version available on the website, delete the old ones and installs the new version.

* To see, which commands are available use `TPFMM.exe help` or simply `TPFMM.exe`

## <a name="settings"></a> Settings
Look at this example configuration of **settings.json**:
~~~~
{
    "tpfModPath": "C:\\Spiele\\Transport Fever\\mods",
    "deleteZips": true
}
~~~~
**tpfModPath**: The string with this label should be the path to **your** mods folder of Transport Fever. Mods will be extracted to this folder. Please note, that a path like used on default on windows system like "C:\Spiele\Transport Fever" will **NOT** work. Use "/" or "\\" as delimiter between folders.

**deleteZips**: To install mods the zip files need to be downloaded. Those will be placed at the location of the "TPFMM.exe" inside of a tmp folder. If you switch this to *false* this folder will not be deleted, so you can use the downloaded zip files.

## Development
### Versioning
I will try to stick to Semantic Versioning 2.0.0 (http://semver.org/spec/v2.0.0.html).

### Used Tools
The code is written in "Xamarin Studio" (https://www.xamarin.com/studio).

I use "SourceTree" (https://www.sourcetreeapp.com/) to manage git.

### Used Third-Party Libraries
* Mono (http://www.mono-project.com/)
* F# Data (http://fsharp.github.io/FSharp.Data/)
