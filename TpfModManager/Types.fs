namespace TpfModManager

module Types =
    type TpfNetId = uint32
    let tpfNetId value = uint32 value
    let tpfNetIdFromString (value :string) = uint32 value

    type VersionNumber = uint16
    let versionNumber value = uint16 value
    let versionNumberFromString (value :string) = uint16 value

    type Author = {name: string; tpfNetId: TpfNetId option}
    type Folder = Folder of string